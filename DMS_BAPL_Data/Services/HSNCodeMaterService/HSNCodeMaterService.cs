using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.HSNCodeMaterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DMS_BAPL_Data.Services.HSNCodeMaterService
{
    public class HSNCodeMaterService : IHSNCodeMaterService
    {
        private readonly IHSNCodeMaterRepo _hSNCodeMaterRepo;
        private readonly IExcelService _excelService;
        public HSNCodeMaterService(IHSNCodeMaterRepo hSNCodeMaterRepo, IExcelService excelService)
        {
            _hSNCodeMaterRepo = hSNCodeMaterRepo;
            _excelService = excelService;
        }
        public async Task<List<HsncodeMaster>> GetAllHSNCodeListAsync(string? search)
        {
            return await _hSNCodeMaterRepo.GetAllHSNCodeListAsync(search);
        }
        public async Task<HsncodeMaster?> GetByIdAsync(int id)
        {
            try
            {
                return await _hSNCodeMaterRepo.GetByIdAsync(id);
            }
            catch
            {
                throw;
            }
        }
        public async Task<HsncodeMaster> AddAsync(HSNCodeMasterViewModel entity)
        {
            try
            {
                return await _hSNCodeMaterRepo.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> UpdateAsync(int id, HSNCodeMasterViewModel entity)
        {
            try
            {
                return await _hSNCodeMaterRepo.UpdateAsync(id, entity);
            }
            catch
            {
                throw;
            }
        }
        public async Task<byte[]> downloadHSNCodeExcel()
        {
            try
            {
                var data = await _hSNCodeMaterRepo.GetAllHSNCodeListAsync(null);
                // Get all DTO properties for columns
                var properties = typeof(HSNCodeMasterViewModel)
                    .GetProperties()
                    .ToList();
                var columns = properties.Select(p => p.Name).ToList();
                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);
                        if (entityProp != null)
                            dict[prop.Name] = entityProp.GetValue(d);
                        else
                            dict[prop.Name] = null;
                    }
                    return dict;
                }).ToList();
                var model = new ExcelExportViewModel
                {
                    SheetName = StringConstants.HSNCodeExcelSheetName,
                    Columns = columns,
                    Rows = rows
                };
                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Imports HSN/SAC code rows from an uploaded .xlsx file. Existing HSN codes
        /// are skipped (there's no update-by-code path today, only AddAsync/UpdateAsync
        /// by numeric Id) — new codes are inserted via the existing AddAsync.
        /// </summary>
        public async Task<HSNImportResultViewModel> ImportHSNCodeExcelAsync(IFormFile file)
        {
            var result = new HSNImportResultViewModel();
            var rows = ReadHsnCodesFromExcel(file);
            result.TotalRows = rows.Count;

            // Loaded once up front so each row can be checked for duplicates without a
            // per-row DB round trip.
            var existingCodes = (await GetAllHSNCodeListAsync(null))
                .Select(h => h.Hsncode)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var (rowNumber, item) in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(item.Hsncode))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new HSNImportRowError
                        {
                            RowNumber = rowNumber,
                            HsnCode = item.Hsncode,
                            Message = "HSN Code is required."
                        });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.Type) ||
                        !(string.Equals(item.Type, "HSN", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(item.Type, "SAC", StringComparison.OrdinalIgnoreCase)))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new HSNImportRowError
                        {
                            RowNumber = rowNumber,
                            HsnCode = item.Hsncode,
                            Message = "Type must be either 'HSN' or 'SAC'."
                        });
                        continue;
                    }

                    if (existingCodes.Contains(item.Hsncode))
                    {
                        result.SkippedCount++;
                        result.Errors.Add(new HSNImportRowError
                        {
                            RowNumber = rowNumber,
                            HsnCode = item.Hsncode,
                            Message = "HSN Code already exists — skipped."
                        });
                        continue;
                    }

                    await AddAsync(item);
                    existingCodes.Add(item.Hsncode); // guards against duplicate codes within the same sheet
                    result.InsertedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(new HSNImportRowError
                    {
                        RowNumber = rowNumber,
                        HsnCode = item.Hsncode,
                        Message = ex.Message
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Reads HSN/SAC code rows from an uploaded .xlsx file. The first row is treated
        /// as the header; columns are matched by header text against a per-field alias
        /// list (see <c>FindColumn</c> calls below), so column order doesn't matter and
        /// different export sources can use different header wording — e.g. this app's
        /// own "Download Excel" uses the raw property name ("Hsncode"), while an ERP-style
        /// import file may instead use "HSNCode" / "HSNDescription" / "CodeFlag".
        /// </summary>
        private List<(int RowNumber, HSNCodeMasterViewModel Item)> ReadHsnCodesFromExcel(IFormFile file)
        {
            var rows = new List<(int, HSNCodeMasterViewModel)>();

            using var stream = file.OpenReadStream();
            using var document = SpreadsheetDocument.Open(stream, false);

            var workbookPart = document.WorkbookPart!;
            var sheet = workbookPart.Workbook.Descendants<Sheet>().First();
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

            string GetCellText(Cell? cell)
            {
                if (cell?.CellValue == null) return string.Empty;
                var value = cell.CellValue.InnerText;

                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && sharedStrings != null)
                    return sharedStrings.ElementAt(int.Parse(value)).InnerText;

                return value;
            }

            // A1-style cell reference ("C2") -> 0-based column index (A=0, B=1, ... Z=25,
            // AA=26...). OpenXML omits genuinely empty cells from a row entirely, so reading
            // by position-in-list would silently shift later columns on any sparse row —
            // reading by the cell's own reference avoids that regardless of gaps.
            static int ColumnIndexOf(Cell cell)
            {
                var reference = cell.CellReference?.Value ?? string.Empty;
                int index = 0;
                foreach (var c in reference)
                {
                    if (!char.IsLetter(c)) break;
                    index = index * 26 + (char.ToUpperInvariant(c) - 'A' + 1);
                }
                return index - 1;
            }

            // Normalizes header text so "HSN Code", "HSNCode", "Hsncode", "HSN_Code" etc.
            // all compare equal — strips whitespace/underscores/hyphens and lowercases.
            static string Normalize(string s) =>
                new string(s.Where(c => !char.IsWhiteSpace(c) && c != '_' && c != '-').ToArray()).ToLowerInvariant();

            var allRows = sheetData.Elements<Row>().ToList();
            if (allRows.Count < 2) return rows; // header only, or empty sheet

            // Maps normalized header text -> actual column index, built from row 1.
            var headerMap = new Dictionary<string, int>();
            foreach (var cell in allRows[0].Elements<Cell>())
            {
                var header = GetCellText(cell).Trim();
                if (string.IsNullOrEmpty(header)) continue;

                var key = Normalize(header);
                if (!headerMap.ContainsKey(key))
                    headerMap[key] = ColumnIndexOf(cell);
            }

            int? FindColumn(params string[] candidates)
            {
                foreach (var candidate in candidates)
                {
                    if (headerMap.TryGetValue(Normalize(candidate), out var idx))
                        return idx;
                }
                return null;
            }

            var hsnCodeCol = FindColumn("HSN Code", "HSNCode", "Hsncode");
            var descriptionCol = FindColumn("Description", "HSN Description", "HSNDescription");
            var typeCol = FindColumn("Type", "Code Flag", "CodeFlag");

            for (int r = 1; r < allRows.Count; r++)
            {
                var cellsByColumn = allRows[r].Elements<Cell>().ToDictionary(ColumnIndexOf, c => c);
                if (cellsByColumn.Count == 0) continue;

                string ValueAt(int? colIndex) =>
                    colIndex.HasValue && cellsByColumn.TryGetValue(colIndex.Value, out var cell)
                        ? GetCellText(cell)
                        : string.Empty;

                var item = new HSNCodeMasterViewModel
                {
                    Hsncode = ValueAt(hsnCodeCol).Trim(),
                    Description = ValueAt(descriptionCol).Trim(),
                    Type = ValueAt(typeCol).Trim().ToUpperInvariant()
                };

                // Skip fully blank rows (e.g. trailing empty rows in the sheet)
                if (string.IsNullOrWhiteSpace(item.Hsncode) &&
                    string.IsNullOrWhiteSpace(item.Description) &&
                    string.IsNullOrWhiteSpace(item.Type))
                    continue;

                rows.Add((r + 1, item)); // +1 so row numbers match what the user sees in Excel (1-based, header = row 1)
            }

            return rows;
        }
    }
}