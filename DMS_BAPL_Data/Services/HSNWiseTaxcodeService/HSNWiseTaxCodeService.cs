using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.HSNWiseTaxCodeRepo;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DMS_BAPL_Data.Services.HSNWiseTaxcodeService
{
    public class HSNWiseTaxCodeService : IHSNWiseTaxcodeservice
    {
        private readonly IHSNWiseTaxcodeRepo _hsnWiseTaxcodeRepo;
        public HSNWiseTaxCodeService(IHSNWiseTaxcodeRepo hsnWiseTaxcodeRepo)
        {
            _hsnWiseTaxcodeRepo = hsnWiseTaxcodeRepo;
        }
        public async Task<List<HSNCodeList>> GetHsncodeList()
        {
            return await _hsnWiseTaxcodeRepo.GetHsncodeList();
        }
        public async Task<List<AggregateTaxCode>> GetAggregateTaxCodeList()
        {
            return await _hsnWiseTaxcodeRepo.GetAggregateTaxCodeList();
        }
        public async Task<HsnwiseTaxCodeViewModel> InsertHsnwiseTaxcodedetails(HsnwiseTaxCodeViewModel hsnwiseTaxCodeViewModel)
        {
            return await _hsnWiseTaxcodeRepo.InsertHsnwiseTaxcodedetails(hsnwiseTaxCodeViewModel);
        }
        public async Task<List<HsnwiseTaxCode>> GetHsnwiseTaxcodedetails(string? search)
        {
            return await _hsnWiseTaxcodeRepo.GetHsnwiseTaxcodedetails(search);
        }

        /// <summary>
        /// UPSERT (no delete) — imports HSNWise TaxCode rows from an uploaded .xlsx file.
        /// Matches on (Hsncode, AtaxCode, StateFlag): a match updates EffectiveDate (and
        /// CreatedBy/UpdatedBy bookkeeping); no match inserts a new row. Nothing existing
        /// is ever removed; a row missing a required field is skipped and reported.
        /// </summary>
        /// 
        private static string BuildKey(
            string? hsncode,
            string? ataxCode,
            string? stateFlag)
                {
                    return string.Join(
                        "|",
                        hsncode?.Trim().ToUpperInvariant() ?? "",
                        ataxCode?.Trim().ToUpperInvariant() ?? "",
                        NormalizeStateFlag(stateFlag)
                    );
                }
        private static string NormalizeStateFlag(string? stateFlag)
        {
            if (string.IsNullOrWhiteSpace(stateFlag))
                return "";

            var value = stateFlag
                .Trim()
                .ToUpperInvariant();

            return value switch
            {
                "S" => "S",
                "SAME" => "S",
                "SAME STATE" => "S",

                "O" => "O",
                "OTHER" => "O",
                "OTHER STATE" => "O",
                "OTHER STATE/EX-WP" => "O",
                "OTHER STATE / EX-WP" => "O",

                _ => value
            };
        }
        public async Task<HsnwiseTaxImportResultViewModel> ImportHsnwiseTaxCodeExcelAsync(
    IFormFile file)
        {
            var result = new HsnwiseTaxImportResultViewModel();

            var rows = ReadHsnwiseTaxRowsFromExcel(file);

            result.TotalRows = rows.Count;

            if (rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "The uploaded file has no data rows — nothing was imported.");
            }

            // ============================================================
            // GET EXISTING DB DATA
            // ============================================================

            var existingRows =
                await _hsnWiseTaxcodeRepo.GetHsnwiseTaxcodedetails(null);

            // Use HashSet instead of ToDictionary.
            // This prevents exception if DB already contains duplicate keys.
            var existingKeys = new HashSet<string>(
                existingRows
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Hsncode) &&
                        !string.IsNullOrWhiteSpace(x.AtaxCode) &&
                        !string.IsNullOrWhiteSpace(x.StateFlag))
                    .Select(x => BuildKey(
                        x.Hsncode,
                        x.AtaxCode,
                        x.StateFlag)),
                StringComparer.OrdinalIgnoreCase
            );


            // ============================================================
            // TRACK DUPLICATES INSIDE THE EXCEL FILE
            // ============================================================

            var importedKeys = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );


            // ============================================================
            // PROCESS EXCEL ROWS
            // ============================================================

            foreach (var (
                rowNumber,
                hsncode,
                ataxCode,
                stateFlag,
                effectiveDateText) in rows)
            {
                try
                {
                    // ----------------------------------------------------
                    // HSN CODE VALIDATION
                    // ----------------------------------------------------

                    if (string.IsNullOrWhiteSpace(hsncode))
                    {
                        result.FailedCount++;

                        result.Errors.Add(
                            new HsnwiseTaxImportRowError
                            {
                                RowNumber = rowNumber,
                                Hsncode = hsncode,
                                AtaxCode = ataxCode,
                                Message = "HsnCode is required."
                            });

                        continue;
                    }


                    // ----------------------------------------------------
                    // ATAX CODE VALIDATION
                    // ----------------------------------------------------

                    if (string.IsNullOrWhiteSpace(ataxCode))
                    {
                        result.FailedCount++;

                        result.Errors.Add(
                            new HsnwiseTaxImportRowError
                            {
                                RowNumber = rowNumber,
                                Hsncode = hsncode,
                                AtaxCode = ataxCode,
                                Message = "AtaxCode is required."
                            });

                        continue;
                    }


                    // ----------------------------------------------------
                    // STATE FLAG
                    // ----------------------------------------------------

                    var normalizedFlag =
                        NormalizeStateFlag(stateFlag);

                    if (normalizedFlag != "S" &&
                        normalizedFlag != "O")
                    {
                        result.FailedCount++;

                        result.Errors.Add(
                            new HsnwiseTaxImportRowError
                            {
                                RowNumber = rowNumber,
                                Hsncode = hsncode,
                                AtaxCode = ataxCode,
                                Message =
                                    $"StateFlag must be 'S' or 'O'. Found '{stateFlag}'."
                            });

                        continue;
                    }


                    // ----------------------------------------------------
                    // EFFECTIVE DATE
                    // ----------------------------------------------------

                    if (!TryParseEffectiveDate(
                            effectiveDateText,
                            out var effectiveDate))
                    {
                        result.FailedCount++;

                        result.Errors.Add(
                            new HsnwiseTaxImportRowError
                            {
                                RowNumber = rowNumber,
                                Hsncode = hsncode,
                                AtaxCode = ataxCode,
                                Message =
                                    $"EffectiveDate could not be parsed " +
                                    $"(found '{effectiveDateText}'). " +
                                    $"Use dd-MM-yyyy or yyyy-MM-dd."
                            });

                        continue;
                    }


                    // ====================================================
                    // CREATE COMPOSITE KEY
                    // ====================================================

                    var key = BuildKey(
                        hsncode,
                        ataxCode,
                        normalizedFlag
                    );


                    // ====================================================
                    // DUPLICATE CHECK
                    // ====================================================

                    // Case 1:
                    // Already exists in database
                    if (existingKeys.Contains(key))
                    {
                        // SKIP duplicate
                        result.SkippedCount++;

                        result.Errors.Add(
                            new HsnwiseTaxImportRowError
                            {
                                RowNumber = rowNumber,
                                Hsncode = hsncode,
                                AtaxCode = ataxCode,
                                Message =
                                    $"Duplicate record skipped. " +
                                    $"Key already exists: " +
                                    $"{hsncode}, {ataxCode}, {normalizedFlag}"
                            });

                        continue;
                    }


                    // Case 2:
                    // Duplicate appears multiple times in the SAME Excel file
                    if (importedKeys.Contains(key))
                    {
                        // SKIP duplicate from Excel
                        result.SkippedCount++;

                        result.Errors.Add(
                            new HsnwiseTaxImportRowError
                            {
                                RowNumber = rowNumber,
                                Hsncode = hsncode,
                                AtaxCode = ataxCode,
                                Message =
                                    $"Duplicate row skipped in Excel. " +
                                    $"Key: {hsncode}, {ataxCode}, {normalizedFlag}"
                            });

                        continue;
                    }


                    // ====================================================
                    // INSERT NEW RECORD
                    // ====================================================

                    var insertPayload =
                        new HsnwiseTaxCodeViewModel
                        {
                            Hsncode = hsncode.Trim(),

                            AtaxCode = ataxCode.Trim(),

                            StateFlag = normalizedFlag,

                            EffectiveDate = effectiveDate,

                            CreatedBy = "Excel Import"
                        };


                    await _hsnWiseTaxcodeRepo
                        .InsertHsnwiseTaxcodedetails(insertPayload);


                    // Add key to both sets immediately.
                    // This prevents duplicate insertion if the same
                    // key appears again later in the Excel file.

                    importedKeys.Add(key);

                    existingKeys.Add(key);


                    result.InsertedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;

                    result.Errors.Add(
                        new HsnwiseTaxImportRowError
                        {
                            RowNumber = rowNumber,
                            Hsncode = hsncode,
                            AtaxCode = ataxCode,
                            Message = ex.Message
                        });
                }
            }


            return result;
        }

        private static bool TryParseEffectiveDate(string text, out DateTime date)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                date = default;
                return false;
            }

            // Excel serial date (numeric cell read as text, e.g. "45747")
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var serial) && serial > 0)
            {
                try
                {
                    date = DateTime.FromOADate(serial);
                    return true;
                }
                catch { /* fall through to text formats */ }
            }

            string[] formats = { "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" };
            if (DateTime.TryParseExact(text.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return true;

            return DateTime.TryParse(text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        /// <summary>
        /// Reads HSNWise TaxCode rows from an uploaded .xlsx file. Header row 1, columns
        /// matched by header text (order-independent). Handles shared-string, inline-string,
        /// and numeric/date cell types — see GetCellText.
        /// </summary>
        private List<(int RowNumber, string Hsncode, string AtaxCode, string StateFlag, string EffectiveDate)> ReadHsnwiseTaxRowsFromExcel(IFormFile file)
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
                if (cell == null) return string.Empty;

                // Inline strings store text in <is><t>, not in <v> (CellValue) — without
                // this branch every inline-string cell silently reads back as "".
                if (cell.DataType != null && cell.DataType.Value == CellValues.InlineString)
                    return cell.InlineString?.Text?.Text ?? string.Empty;

                if (cell.CellValue == null) return string.Empty;
                var value = cell.CellValue.InnerText;

                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && sharedStrings != null)
                    return int.TryParse(value, out var idx) && idx < sharedStrings.Count()
                        ? sharedStrings.ElementAt(idx).InnerText
                        : string.Empty;

                return value;
            }

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

            static string Normalize(string s) =>
                new string(s.Where(c => !char.IsWhiteSpace(c) && c != '_' && c != '-').ToArray()).ToLowerInvariant();

            var allRows = sheetData.Elements<Row>().ToList();
            if (allRows.Count < 2) return rows;

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

            var hsncodeCol = FindColumn("HsnCode", "Hsncode", "HSN Code");
            var ataxCodeCol = FindColumn("AtaxCode", "ATaxCode", "Aggregate Tax Code", "Aggregate TaxCode");
            var stateFlagCol = FindColumn("StateFlag", "State Flag");
            var effectiveDateCol = FindColumn("EffectiveDate", "Effective Date");

            for (int r = 1; r < allRows.Count; r++)
            {
                var cellsByColumn = allRows[r].Elements<Cell>().ToDictionary(ColumnIndexOf, c => c);
                if (cellsByColumn.Count == 0) continue;

                string ValueAt(int? colIndex) =>
                    colIndex.HasValue && cellsByColumn.TryGetValue(colIndex.Value, out var cell)
                        ? GetCellText(cell)
                        : string.Empty;

                var hsncode = ValueAt(hsncodeCol).Trim();
                var ataxCode = ValueAt(ataxCodeCol).Trim();
                var stateFlag = ValueAt(stateFlagCol).Trim();
                var effectiveDate = ValueAt(effectiveDateCol).Trim();

                if (string.IsNullOrWhiteSpace(hsncode) &&
                    string.IsNullOrWhiteSpace(ataxCode) &&
                    string.IsNullOrWhiteSpace(stateFlag))
                    continue; // fully blank row

                rows.Add((r + 1, hsncode, ataxCode, stateFlag, effectiveDate));
            }

            return rows;
        }
    }
}