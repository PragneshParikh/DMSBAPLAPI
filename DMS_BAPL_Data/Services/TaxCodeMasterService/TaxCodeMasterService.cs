using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.TaxCodeMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.TaxCodeMasterService
{
    public class TaxCodeMasterService : ITaxCodeMasterService
    {

        private readonly ITaxCodeMasterRepo _taxCodeMasterRepo;
        private readonly IExcelService _excelService;

        public TaxCodeMasterService(ITaxCodeMasterRepo taxCodeMasterRepo, IExcelService excelService)
        {
            _taxCodeMasterRepo = taxCodeMasterRepo;
            _excelService = excelService;
        }

        public async Task<IEnumerable<TaxCodeMaster>> GetAllTaxCodes()
        {
            IEnumerable<TaxCodeMaster> taxCodeMasterList = await _taxCodeMasterRepo.GetAllTaxCodes();
            return taxCodeMasterList;
        }

        public async Task<TaxCodeMaster?> GetTaxCodeById(int id)
        {
            TaxCodeMaster? taxCodeMaster = await _taxCodeMasterRepo.GetTaxCodeById(id);
            return taxCodeMaster;
        }

        public async Task<int> AddTaxCode(TaxCodeMasterViewModel taxCodeMasterViewModel)
        {
            int taxCodeMasterId = await _taxCodeMasterRepo.AddTaxCode(taxCodeMasterViewModel);
            return taxCodeMasterId;
        }

        public async Task<int> UpdateTaxCode(TaxCodeMasterViewModel taxCodeMasterViewModel)
        {
            int affectedRows = await _taxCodeMasterRepo.UpdateTaxCode(taxCodeMasterViewModel);
            return affectedRows;
        }
        public async Task<byte[]> DownloadTaxCodeExcel()
        {
            try
            {
                var data = await _taxCodeMasterRepo.GetAllTaxCodes();

                var columns = new List<string>
        {
            "Id",
            "TaxCode",
            "Description",
            "TaxRate",
            "EffectiveDate",
            "CreatedBy",
            "CreatedDate",
            "UpdatedBy",
            "UpdatedDate"
        };

                var rows = data.Select(taxCodeMaster =>
                {
                    var dictionary = new Dictionary<string, object>();

                    dictionary["Id"] = taxCodeMaster.Id;
                    dictionary["TaxCode"] = taxCodeMaster.TaxCode;
                    dictionary["Description"] = taxCodeMaster.Description;
                    dictionary["TaxRate"] = taxCodeMaster.TaxRate;
                    dictionary["EffectiveDate"] = taxCodeMaster.EffectiveDate?.ToString("yyyy-MM-dd");
                    dictionary["CreatedBy"] = taxCodeMaster.CreatedBy;
                    dictionary["CreatedDate"] = taxCodeMaster.CreatedDate.ToString("yyyy-MM-dd HH:mm");
                    dictionary["UpdatedBy"] = taxCodeMaster.UpdatedBy;
                    dictionary["UpdatedDate"] = taxCodeMaster.UpdatedDate?.ToString("yyyy-MM-dd HH:mm");

                    return dictionary;

                }).ToList();

                var excelExportViewModel = new ExcelExportViewModel
                {
                    SheetName = "TaxCodeMaster",
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(excelExportViewModel);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }
        public async Task<TaxCodeImportResultViewModel> ImportTaxCodeExcelAsync(IFormFile file)
        {
            var result = new TaxCodeImportResultViewModel();
            var rows = ReadTaxCodesFromExcel(file);
            result.TotalRows = rows.Count;

            if (rows.Count == 0)
                throw new InvalidOperationException("The uploaded file has no data rows — nothing was imported.");

            var existing = await GetAllTaxCodes();
            var existingKeys = new HashSet<string>(
                existing.Select(t => BuildKey(t.TaxCode, t.EffectiveDate)),
                StringComparer.OrdinalIgnoreCase);

            foreach (var (rowNumber, taxCode, description, taxRateText, effectiveDateText) in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(taxCode))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new TaxCodeImportRowError
                        {
                            RowNumber = rowNumber,
                            TaxCode = taxCode,
                            Message = "Tax Code is required."
                        });
                        continue;
                    }

                    if (!decimal.TryParse(taxRateText, out var taxRate))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new TaxCodeImportRowError
                        {
                            RowNumber = rowNumber,
                            TaxCode = taxCode,
                            Message = "Tax Rate is required and must be a number."
                        });
                        continue;
                    }

                    var effectiveDate = ParseExcelDate(effectiveDateText);
                    if (effectiveDate == null)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new TaxCodeImportRowError
                        {
                            RowNumber = rowNumber,
                            TaxCode = taxCode,
                            Message = "Effective Date is required and must be a valid date."
                        });
                        continue;
                    }

                    var key = BuildKey(taxCode, effectiveDate);
                    if (existingKeys.Contains(key))
                    {
                        result.SkippedCount++;
                        result.Errors.Add(new TaxCodeImportRowError
                        {
                            RowNumber = rowNumber,
                            TaxCode = taxCode,
                            Message = "A row with this exact Tax Code and Effective Date already exists — skipped."
                        });
                        continue;
                    }

                    var payload = new TaxCodeMasterViewModel
                    {
                        Id = 0,
                        TaxCode = taxCode,
                        Description = description,
                        TaxRate = taxRate,
                        EffectiveDate = effectiveDate,
                        CreatedBy = "Excel Import",
                        CreatedDate = DateTime.Now
                    };

                    await AddTaxCode(payload);
                    existingKeys.Add(key); // guards against duplicate rows within the same sheet
                    result.InsertedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(new TaxCodeImportRowError
                    {
                        RowNumber = rowNumber,
                        TaxCode = taxCode,
                        Message = ex.Message
                    });
                }
            }

            return result;
        }

        private static string BuildKey(string? taxCode, DateTime? effectiveDate) =>
            $"{(taxCode ?? string.Empty).Trim().ToUpperInvariant()}|{effectiveDate?.Date:yyyy-MM-dd}";

        /// <summary>
        /// Parses a cell's raw text as a date. Genuine Excel date cells store a serial
        /// day-count (e.g. "45896"), not a human-readable string, so a pure-numeric value
        /// is converted via DateTime.FromOADate before falling back to normal text parsing
        /// (covers a Text-formatted date cell or an ISO date string).
        /// </summary>
        private static DateTime? ParseExcelDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            if (double.TryParse(raw, out var serial) && serial > 0)
            {
                try { return DateTime.FromOADate(serial); }
                catch { /* not actually a serial date — fall through to text parsing */ }
            }

            return DateTime.TryParse(raw, out var parsed) ? parsed : (DateTime?)null;
        }

        /// <summary>
        /// Reads Tax Code rows from an uploaded .xlsx file. The first row is treated as
        /// the header; columns are matched by header text against a per-field alias list,
        /// so column order doesn't matter. Expected headers: "Tax Code", "Description",
        /// "Tax Rate", "Effective Date".
        /// </summary>
        private List<(int RowNumber, string TaxCode, string Description, string TaxRate, string EffectiveDate)> ReadTaxCodesFromExcel(IFormFile file)
        {
            var rows = new List<(int, string, string, string, string)>();

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

            // A1-style cell reference ("C2") -> 0-based column index. OpenXML omits
            // genuinely empty cells from a row entirely, so reading by position-in-list
            // would silently shift later columns on any sparse row — reading by the cell's
            // own reference avoids that regardless of gaps.
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

            // Normalizes header text so "Tax Code", "TaxCode", "Tax_Code" etc. all compare
            // equal — strips whitespace/underscores/hyphens and lowercases.
            static string Normalize(string s) =>
                new string(s.Where(c => !char.IsWhiteSpace(c) && c != '_' && c != '-').ToArray()).ToLowerInvariant();

            var allRows = sheetData.Elements<Row>().ToList();
            if (allRows.Count < 2) return rows; // header only, or empty sheet

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

            var taxCodeCol = FindColumn("TaxCode", "Tax Code");
            var descriptionCol = FindColumn("Description");
            var taxRateCol = FindColumn("TaxRate", "Tax Rate");
            var effectiveDateCol = FindColumn("EffectiveDate", "Effective Date");

            for (int r = 1; r < allRows.Count; r++)
            {
                var cellsByColumn = allRows[r].Elements<Cell>().ToDictionary(ColumnIndexOf, c => c);
                if (cellsByColumn.Count == 0) continue;

                string ValueAt(int? colIndex) =>
                    colIndex.HasValue && cellsByColumn.TryGetValue(colIndex.Value, out var cell)
                        ? GetCellText(cell)
                        : string.Empty;

                var taxCode = ValueAt(taxCodeCol).Trim();
                var description = ValueAt(descriptionCol).Trim();
                var taxRate = ValueAt(taxRateCol).Trim();
                var effectiveDate = ValueAt(effectiveDateCol).Trim();

                if (string.IsNullOrWhiteSpace(taxCode) &&
                    string.IsNullOrWhiteSpace(taxRate) &&
                    string.IsNullOrWhiteSpace(effectiveDate))
                    continue; // fully blank row

                rows.Add((r + 1, taxCode, description, taxRate, effectiveDate)); // +1 so row numbers match Excel (header = row 1)
            }

            return rows;
        }
    }
}
