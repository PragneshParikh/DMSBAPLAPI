using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DMS_BAPL_Data.Services.AgreetaxcodeService
{
    public class AgreegateTaxcodeService : IAgreegateTaxcodeService
    {
        private readonly IAgreetaxcodeRepo _agreeTaxcodeRepo;
        public AgreegateTaxcodeService(IAgreetaxcodeRepo agreeTaxcodeRepo)
        {
            _agreeTaxcodeRepo = agreeTaxcodeRepo;
        }
        public async Task<AgreeTaxCodeViewModel> InsertAgreeTaxcodeAsync(AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        {
            return await _agreeTaxcodeRepo.InsertAgreeTaxcodeAsync(agreeTaxCodeViewModel);
        }
        public async Task<List<AggregateTaxCode>> GetAggregateTaxcodesAsync(string? search)
        {
            return await _agreeTaxcodeRepo.GetAggregateTaxcodesAsync(search);
        }
        public async Task<List<AggregateTaxCode>> GetAggregateTaxDetailsAsync(string ataxCode)
        {
            return await _agreeTaxcodeRepo.GetAggregateTaxDetailsAsync(ataxCode);
        }
        public async Task<AggregateTaxCode> GetAggregateTaxcodeByIdAsync(int id)
        {
            return await _agreeTaxcodeRepo.GetAggregateTaxcodeByIdAsync(id);
        }
        public async Task<AggregateTaxCode> UpdateAgreeTaxcodeAsync(int id, AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        {
            return await _agreeTaxcodeRepo.UpdateAgreeTaxcodeAsync(id, agreeTaxCodeViewModel);
        }
        public async Task<List<TaxCodeWithRateViewModel>> GetTaxCodeWithRate()
        {
            return await _agreeTaxcodeRepo.GetTaxCodeWithRate();
        }

        // Used only by Excel import — bypasses TaxCodeMasters validation so the sheet's
        // own TaxCode/TaxRate values are trusted and saved as-is.
        public async Task<AgreeTaxCodeViewModel> InsertAgreeTaxcodeNoValidationAsync(AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        {
            return await _agreeTaxcodeRepo.InsertAgreeTaxcodeNoValidationAsync(agreeTaxCodeViewModel);
        }

        // Used only by Excel import — bypasses TaxCodeMasters validation.
        public async Task<AggregateTaxCode> UpdateAgreeTaxcodeNoValidationAsync(int id, AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        {
            return await _agreeTaxcodeRepo.UpdateAgreeTaxcodeNoValidationAsync(id, agreeTaxCodeViewModel);
        }

        /// <summary>
        /// UPSERT (no delete) — imports the uploaded .xlsx file by updating rows that
        /// already match on (AtaxCode, TaxCode) and inserting rows that don't. Existing
        /// data is never removed; a row that can't be matched is skipped and reported,
        /// leaving everything else untouched.
        ///
        /// Per your request, this import path does NOT validate TaxCode/TaxRate against
        /// TaxCodeMasters — the sheet's own values are trusted and saved as-is via
        /// InsertAgreeTaxcodeNoValidationAsync / UpdateAgreeTaxcodeNoValidationAsync.
        /// Manual add/update through the UI (InsertAgreeTaxcodeAsync / UpdateAgreeTaxcodeAsync)
        /// still validates against TaxCodeMasters as before — only import skips it.
        ///
        /// AggregateTaxCode has no "header-only" row: TaxCode/TaxRate are required, real
        /// columns on every row. A row with no TaxCode is skipped rather than sent through
        /// with an empty TaxDetails list — the insert's `foreach` over an empty list would
        /// otherwise silently persist nothing and still report success.
        ///
        /// Existing rows for each AtaxCode are fetched once (via GetAggregateTaxDetailsAsync)
        /// and cached for the rest of the run, since a sheet can have several rows — one per
        /// TaxCode — sharing the same AtaxCode.
        /// </summary>
        public async Task<AggregateTaxImportResultViewModel> ImportAggregateTaxCodeExcelAsync(IFormFile file)
        {
            var result = new AggregateTaxImportResultViewModel();
            var rows = ReadAggregateTaxRowsFromExcel(file);
            result.TotalRows = rows.Count;

            if (rows.Count == 0)
                throw new InvalidOperationException("The uploaded file has no data rows — nothing was imported.");

            var existingByCode = new Dictionary<string, List<AggregateTaxCode>>(StringComparer.OrdinalIgnoreCase);
            var nextSrNoByCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var (rowNumber, ataxCode, description, taxCode, taxRate, srNo) in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(ataxCode))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new AggregateTaxImportRowError
                        {
                            RowNumber = rowNumber,
                            AtaxCode = ataxCode,
                            Message = "Aggregate Tax Code is required."
                        });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(taxCode))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new AggregateTaxImportRowError
                        {
                            RowNumber = rowNumber,
                            AtaxCode = ataxCode,
                            Message = "TaxCode is required for this row. Add a TaxCode column value for this row."
                        });
                        continue;
                    }

                    if (!existingByCode.TryGetValue(ataxCode, out var existingRows))
                    {
                        existingRows = await GetAggregateTaxDetailsAsync(ataxCode);
                        existingByCode[ataxCode] = existingRows;
                    }

                    var existingRow = existingRows.FirstOrDefault(
                        r => string.Equals(r.TaxCode, taxCode, StringComparison.OrdinalIgnoreCase));

                    if (!nextSrNoByCode.TryGetValue(ataxCode, out var autoSrNo))
                        autoSrNo = existingRows.Count > 0 ? existingRows.Max(r => r.SrNo) + 1 : 1;
                    nextSrNoByCode[ataxCode] = autoSrNo + 1;

                    decimal.TryParse(taxRate, out var parsedRate);

                    var detail = new TaxDetailViewModel
                    {
                        SrNo = srNo ?? (existingRow?.SrNo ?? autoSrNo),
                        TaxCode = taxCode,
                        TaxRate = parsedRate
                    };

                    if (existingRow != null)
                    {
                        var updatePayload = new AgreeTaxCodeViewModel
                        {
                            AtaxCode = ataxCode,
                            Description = description,
                            UpdatedBy = "Excel Import",
                            TaxDetails = new List<TaxDetailViewModel> { detail }
                        };

                        var updated = await UpdateAgreeTaxcodeNoValidationAsync(existingRow.Id, updatePayload);
                        if (updated == null)
                        {
                            result.FailedCount++;
                            result.Errors.Add(new AggregateTaxImportRowError
                            {
                                RowNumber = rowNumber,
                                AtaxCode = ataxCode,
                                Message = "Update failed for this row."
                            });
                            continue;
                        }

                        // Keep the cache in sync in case this AtaxCode/TaxCode combo
                        // appears again later in the same sheet.
                        existingRow.Description = updated.Description;
                        existingRow.TaxRate = updated.TaxRate;
                        existingRow.SrNo = updated.SrNo;

                        result.UpdatedCount++;
                    }
                    else
                    {
                        var insertPayload = new AgreeTaxCodeViewModel
                        {
                            AtaxCode = ataxCode,
                            Description = description,
                            CreatedBy = "Excel Import",
                            TaxDetails = new List<TaxDetailViewModel> { detail }
                        };

                        await InsertAgreeTaxcodeNoValidationAsync(insertPayload);

                        // Re-fetch so the cached copy carries the real DB-generated Id.
                        // Without this, existingRow.Id would default to 0, and a second
                        // occurrence of this AtaxCode/TaxCode later in the same sheet
                        // would try to UpdateAgreeTaxcodeNoValidationAsync(0, ...) and fail
                        // with a misleading "Aggregate Tax Code not found" error instead of
                        // updating the row that was just inserted.
                        var freshRows = await GetAggregateTaxDetailsAsync(ataxCode);
                        var insertedRow = freshRows.FirstOrDefault(
                            r => string.Equals(r.TaxCode, taxCode, StringComparison.OrdinalIgnoreCase));

                        existingRows.Add(insertedRow ?? new AggregateTaxCode
                        {
                            AtaxCode = ataxCode,
                            Description = description,
                            SrNo = detail.SrNo,
                            TaxCode = taxCode,
                            TaxRate = parsedRate
                        });

                        result.InsertedCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(new AggregateTaxImportRowError
                    {
                        RowNumber = rowNumber,
                        AtaxCode = ataxCode,
                        Message = ex.Message
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Reads Aggregate Tax Code rows from an uploaded .xlsx file. The first row is
        /// treated as the header; columns are matched by header text against a per-field
        /// alias list, so column order doesn't matter. AtaxCode/Description are required
        /// columns; TaxCode/TaxRate are required per row for a save to succeed (see
        /// ImportAggregateTaxCodeExcelAsync); SrNo is optional and auto-increments.
        /// </summary>
        private List<(int RowNumber, string AtaxCode, string Description, string TaxCode, string TaxRate, int? SrNo)> ReadAggregateTaxRowsFromExcel(IFormFile file)
        {
            var rows = new List<(int, string, string, string, string, int?)>();

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

            // Normalizes header text so "ATaxCode", "Aggregate Tax Code", "Sr No", "SrNo"
            // etc. can be compared against alias lists — strips whitespace/underscores/
            // hyphens and lowercases.
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

            // "ATaxCode"/"ATaxDescription" match this app's own naming exactly (confirmed
            // against a real export); the other aliases cover the more conversational
            // labels used in the UI ("Aggregate Tax Code").
            var ataxCodeCol = FindColumn("ATaxCode", "AtaxCode", "Aggregate Tax Code", "Aggregate TaxCode");
            var descriptionCol = FindColumn("ATaxDescription", "Aggregate Tax Description", "Description");
            var taxCodeCol = FindColumn("TaxCode", "Tax Code");
            var taxRateCol = FindColumn("TaxRate", "Tax Rate");
            var srNoCol = FindColumn("SrNo", "Sr No", "Sr No.");

            for (int r = 1; r < allRows.Count; r++)
            {
                var cellsByColumn = allRows[r].Elements<Cell>().ToDictionary(ColumnIndexOf, c => c);
                if (cellsByColumn.Count == 0) continue;

                string ValueAt(int? colIndex) =>
                    colIndex.HasValue && cellsByColumn.TryGetValue(colIndex.Value, out var cell)
                        ? GetCellText(cell)
                        : string.Empty;

                var ataxCode = ValueAt(ataxCodeCol).Trim();
                var description = ValueAt(descriptionCol).Trim();
                var taxCode = ValueAt(taxCodeCol).Trim();
                var taxRate = ValueAt(taxRateCol).Trim();
                var srNoText = ValueAt(srNoCol).Trim();

                if (string.IsNullOrWhiteSpace(ataxCode) &&
                    string.IsNullOrWhiteSpace(description) &&
                    string.IsNullOrWhiteSpace(taxCode))
                    continue; // fully blank row

                int? srNo = int.TryParse(srNoText, out var parsedSrNo) ? parsedSrNo : (int?)null;

                rows.Add((r + 1, ataxCode, description, taxCode, taxRate, srNo)); // +1 so row numbers match Excel (header = row 1)
            }

            return rows;
        }
    }
}