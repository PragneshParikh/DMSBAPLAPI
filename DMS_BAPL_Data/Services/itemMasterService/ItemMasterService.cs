using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Data.Services.TaxServices;
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

namespace DMS_BAPL_Data.Services.itemMasterService
{
    public class ItemMasterService : IitemMasterService
    {

        private readonly IitemMasterRepo _itemMasterRepo;
        private readonly IExcelService _excelService;
        private readonly ITaxServices _taxService;

        public ItemMasterService(IitemMasterRepo itemMasterRepo, IExcelService excelService, ITaxServices taxServices)
        {
            _itemMasterRepo = itemMasterRepo;
            _excelService = excelService;
            _taxService = taxServices;
        }

        // add  itemserice to the database

        public async Task<insertItemMasterViewModel> InsertItemAsync(insertItemMasterViewModel item, string userId)
        {
            return await _itemMasterRepo.InsertItemAsync(item, userId);
        }
        // get all itemservice from the database
        public async Task<List<ItemMasterViewModel>> GetAllItemMastersAsync(int? grpidno, string? search)
        {
            return await _itemMasterRepo.GetAllItemsAsync(grpidno, search);
        }

        // update itemservice to the database

        public async Task<ItemMaster> UpdateItemAsync(ItemMaster item)
        {
            return await _itemMasterRepo.UpdateItemAsync(item);
        }

        public async Task<byte[]> DownloadItemMasterExcel()
        {
            try
            {
                var data = await _itemMasterRepo.GetAllExcelItemsAsync();

                // Get all DTO properties for columns
                var properties = typeof(ItemMaster)
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
                    SheetName = StringConstants.DealerExcelSheetName,
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
        /// Get PurchaseDetails By ModelNo 
        /// </summary>
        public async Task<ItemMasterViewModel> GetPurchaseDetailsByModelNo(string modelNo)
        {
            return await _itemMasterRepo.GetPurchaseDetailsByModelNo(modelNo);
        }
        /// <summary>
        /// Get Purchase Details With HsnTax By ModelNo
        /// </summary>
        /// <param name="modelNo"></param>
        /// <returns></returns>
        public async Task<ItemMasterViewModel> GetPurchaseDetailsWithHsnTaxByModelNo(string modelNo)
        {
            return await _itemMasterRepo.GetPurchaseDetailsWithHsnTaxByModelNo(modelNo);
        }

        public Task<IEnumerable<ItemMaster>> GetItemByItemType(int itemType) => _itemMasterRepo.GetItemByItemType(itemType);
        public Task<IEnumerable<ItemMaster>> GetItemsByOEMModel(int id) => _itemMasterRepo.GetItemsByOEMModel(id);

        public Task<object> UpdateByItemCode(string userId, insertItemMasterViewModel insertItemMasterViewModel) => _itemMasterRepo.UpdateByItemCode(userId, insertItemMasterViewModel);
        public Task<IEnumerable<object>> GetItemsWithHSNTaxGroupId(int? groupId, string? dealerCode) => _itemMasterRepo.GetItemsWithHSNTaxGroupId(groupId, dealerCode);

        public async Task<List<ItemPartsByLocationViewModel>> GetItemsByLocation(string dealerLocation, string customerLocation)
        {
            try
            {

                return await _itemMasterRepo.GetItemsByLocation(dealerLocation, customerLocation);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<ItemMasterViewModel>> GetItemModelist()
        {
            try
            {
                return await _itemMasterRepo.GetItemModelist();
            }
            catch
            {
                throw;
            }

        }

        // ============================================================
        // EXCEL IMPORT (Spares / Parts)
        // ============================================================

        public async Task<ItemImportResultViewModel> ImportItemExcelAsync(IFormFile file, string userId)
        {
            var result = new ItemImportResultViewModel();

            List<(int RowNumber, insertItemMasterViewModel Item, string? SupplierCode)> rows;

            try
            {
                rows = ReadItemsFromExcel(file);
            }
            catch (Exception ex)
            {
                // Header/format problems in the uploaded file — surfaced as a
                // clean 400 by the controller instead of an unhandled 500.
                throw new InvalidOperationException($"Could not read the Excel file: {ex.Message}", ex);
            }

            result.TotalRows = rows.Count;

            foreach (var (rowNumber, itemVm, supplierCode) in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(itemVm.Itemcode))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ItemImportRowError
                        {
                            RowNumber = rowNumber,
                            Itemcode = itemVm.Itemcode,
                            Message = "Part Code is required."
                        });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(itemVm.Itemname))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ItemImportRowError
                        {
                            RowNumber = rowNumber,
                            Itemcode = itemVm.Itemcode,
                            Message = "Part No is required."
                        });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(itemVm.Itemdesc))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ItemImportRowError
                        {
                            RowNumber = rowNumber,
                            Itemcode = itemVm.Itemcode,
                            Message = "Item Description is required."
                        });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(itemVm.Hsncode))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new ItemImportRowError
                        {
                            RowNumber = rowNumber,
                            Itemcode = itemVm.Itemcode,
                            Message = "HSN Code is required."
                        });
                        continue;
                    }

                    // Empty string means "not specified" here, not "explicitly no dealer" —
                    // normalize to null so it matches the "available to all dealers" check
                    // used by GetItemsWithHSNTaxGroupId (IM.DealerCode == null).
                    if (string.IsNullOrWhiteSpace(itemVm.Dealercode))
                        itemVm.Dealercode = null;

                    // Resolve Supplier Code -> SupplierId (Excel sheets carry a
                    // human-readable code, ItemMaster stores the numeric id).
                    if (!string.IsNullOrWhiteSpace(supplierCode))
                        itemVm.SupplierId = await _itemMasterRepo.GetSupplierIdByCodeAsync(supplierCode);

                    var existing = await _itemMasterRepo.GetItemByCodeAsync(itemVm.Itemcode);
                    var isNew = existing == null;

                    // UpdateByItemCode's insert branch stamps CreatedBy from the view
                    // model itself (not from the userId parameter it receives), so it
                    // must be set explicitly here or every newly-inserted row would
                    // get a null CreatedBy.
                    if (isNew)
                        itemVm.CreatedBy = userId;

                    await _itemMasterRepo.UpdateByItemCode(userId, itemVm);

                    if (isNew) result.InsertedCount++;
                    else result.UpdatedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(new ItemImportRowError
                    {
                        RowNumber = rowNumber,
                        Itemcode = itemVm.Itemcode,
                        Message = ex.Message
                    });
                }
            }

            return result;
        }

        private List<(int RowNumber, insertItemMasterViewModel Item, string? SupplierCode)> ReadItemsFromExcel(IFormFile file)
        {
            var rows = new List<(int, insertItemMasterViewModel, string?)>();

            using var stream = file.OpenReadStream();
            using var document = SpreadsheetDocument.Open(stream, false);

            var workbookPart = document.WorkbookPart!;
            var sheet = workbookPart.Workbook.Descendants<Sheet>().First();
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

            string GetCellValue(Cell? cell)
            {
                if (cell?.CellValue == null) return string.Empty;
                var value = cell.CellValue.InnerText;

                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && sharedStrings != null)
                {
                    if (int.TryParse(value, out int index) && index >= 0 && index < sharedStrings.Count())
                        return sharedStrings.ElementAt(index).InnerText.Trim();
                }

                return value?.Trim() ?? string.Empty;
            }

            int GetColumnIndex(string? cellReference)
            {
                if (string.IsNullOrWhiteSpace(cellReference)) return -1;
                int columnIndex = 0;
                foreach (char c in cellReference)
                {
                    if (!char.IsLetter(c)) break;
                    columnIndex = columnIndex * 26 + (char.ToUpper(c) - 'A' + 1);
                }
                return columnIndex - 1;
            }

            // Strips everything but letters/digits and lowercases — used both for
            // matching header aliases and for matching dropdown-text values like
            // "Battery/E-Device" or "Outside Work" against a fixed id.
            string Normalize(string? text) =>
                new string((text ?? "").Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

            var allRows = sheetData.Elements<Row>().ToList();
            if (allRows.Count < 2) return rows;

            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in allRows[0].Elements<Cell>())
            {
                var header = GetCellValue(cell);
                if (string.IsNullOrWhiteSpace(header)) continue;

                var columnIndex = GetColumnIndex(cell.CellReference?.Value);
                if (columnIndex < 0) continue;

                var normalizedHeader = Normalize(header);
                if (!headerMap.ContainsKey(normalizedHeader))
                    headerMap[normalizedHeader] = columnIndex;
            }

            string? FindColumn(params string[] aliases)
            {
                foreach (var alias in aliases)
                {
                    var normalized = Normalize(alias);
                    if (headerMap.ContainsKey(normalized)) return normalized;
                }
                return null;
            }

            var itemTypeColumn = FindColumn("Item Type", "Itemtype");
            var itemNameColumn = FindColumn("Part No", "Itemname", "Part Number");
            var itemCodeColumn = FindColumn("Part Code", "Itemcode", "Item Code");
            var itemDescColumn = FindColumn("Item Description", "Itemdesc", "Description");
            var statusColumn = FindColumn("Status", "Active");
            var hsnColumn = FindColumn("HSN Code", "Hsncode", "HSNCode");
            var listPriceColumn = FindColumn("List Price", "Dlrprice");
            var saleRateColumn = FindColumn("Sale Rate", "Custprice", "MRP", "Item MRP");
            var moqColumn = FindColumn("MOQ", "Moq");
            var boqColumn = FindColumn("BOQ", "Boq");
            var sgstColumn = FindColumn("SGST", "Sgst");
            var cgstColumn = FindColumn("CGST", "Cgst");
            var igstColumn = FindColumn("IGST", "Igst");
            var ugstColumn = FindColumn("UGST", "Ugst");
            var groupColumn = FindColumn("Select Group", "Group", "Grpidno", "Item Group");
            var purchaseRateColumn = FindColumn("Purchase Rate", "Ipurrate");
            var isOemPartColumn = FindColumn("Is OEM Part", "Iselectric");
            var vehTypeColumn = FindColumn("Vehicle Type", "Vehtype");
            var noOfBatteriesColumn = FindColumn("No Of Batteries", "Noofbatteries");
            var colorCodeColumn = FindColumn("Color Code", "Colorcode");
            var rrgItemIdColumn = FindColumn("ERP Item Id", "Rrgitemidno");
            var itemCcColumn = FindColumn("Item CC", "Itemcc");
            var batteryTypeIdColumn = FindColumn("Battery Type Id", "Batterytypeidno");
            var fame2Column = FindColumn("FAME II Amount", "Fame2amount", "Fame2 Amount");
            var compCodeColumn = FindColumn("Company Code", "Compcode");
            var displayNameColumn = FindColumn("Display Name", "Displayname");
            var oemModelNameColumn = FindColumn("Vehicle Model Name", "Oemmodelname", "OEM Model Name");
            var dealerCodeColumn = FindColumn("Dealer Code", "Dealercode");
            var uomColumn = FindColumn("UOM");
            var minBillQtyColumn = FindColumn("Min Bill Qty", "MinBillQty");
            var minOrderQtyColumn = FindColumn("Min Order Qty", "MinOrderQty");
            var supplierCodeColumn = FindColumn("Supplier Code", "Supplier");

            if (itemCodeColumn == null)
                throw new InvalidOperationException("Part Code column was not found in the Excel file. Expected column name: 'Part Code' or 'Item Code'.");

            string Value(List<Cell> cells, string? normalizedColumn)
            {
                if (string.IsNullOrWhiteSpace(normalizedColumn)) return string.Empty;
                if (!headerMap.TryGetValue(normalizedColumn, out var columnIndex)) return string.Empty;

                foreach (var cell in cells)
                {
                    if (GetColumnIndex(cell.CellReference?.Value) == columnIndex)
                        return GetCellValue(cell);
                }
                return string.Empty;
            }

            // "Parts" screen default: unresolvable/blank Item Type falls back to
            // 2 (Parts), matching the Add-Item modal's default selection.
            int ResolveItemType(string raw)
            {
                var byName = Normalize(raw) switch
                {
                    "vehicle" => (int?)1,
                    "parts" => 2,
                    "labour" => 3,
                    "accessory" => 4,
                    "outsidework" => 6,
                    "complain" => 7,
                    "tyre" => 8,
                    "oil" => 9,
                    "gift" => 10,
                    "batteryedevice" => 12,
                    _ => null
                };
                if (byName.HasValue) return byName.Value;
                return int.TryParse(raw, out var n) && n > 0 ? n : 2;
            }

            // Blank/unresolvable Group falls back to 1 (Spares), since this import
            // targets the Spares/Parts screen specifically.
            int ResolveGroupId(string raw)
            {
                var byName = Normalize(raw) switch
                {
                    "spares" => (int?)1,
                    "tools" => 2,
                    "accessories" => 3,
                    "fg" => 6,
                    "parts" => 100,
                    _ => null
                };
                if (byName.HasValue) return byName.Value;
                return int.TryParse(raw, out var n) && n > 0 ? n : 1;
            }

            int? ResolveUom(string raw)
            {
                var byName = Normalize(raw) switch
                {
                    "kit" => (int?)39,
                    "make" => 40,
                    "pcs" => 42,
                    "unit" => 56,
                    _ => null
                };
                if (byName.HasValue) return byName.Value;
                return int.TryParse(raw, out var n) ? n : (int?)null;
            }

            decimal ParseDecimal(string raw) => decimal.TryParse(raw, out var d) ? d : 0m;
            int ParseInt(string raw) => int.TryParse(raw, out var i) ? i : 0;

            for (int r = 1; r < allRows.Count; r++)
            {
                var cells = allRows[r].Elements<Cell>().ToList();
                if (cells.Count == 0) continue;

                var itemCode = Value(cells, itemCodeColumn).Trim();

                if (string.IsNullOrWhiteSpace(itemCode) && cells.All(c => string.IsNullOrWhiteSpace(GetCellValue(c))))
                    continue;

                var rawStatus = Value(cells, statusColumn);
                var isOemRaw = Value(cells, isOemPartColumn);

                var item = new insertItemMasterViewModel
                {
                    Itemtype = ResolveItemType(Value(cells, itemTypeColumn)),
                    Itemname = Value(cells, itemNameColumn),
                    Itemcode = itemCode,
                    Itemdesc = Value(cells, itemDescColumn),
                    Status = !(string.Equals(rawStatus, "No", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(rawStatus, "Inactive", StringComparison.OrdinalIgnoreCase)),
                    Hsncode = Value(cells, hsnColumn),
                    Dlrprice = ParseDecimal(Value(cells, listPriceColumn)),
                    Custprice = ParseDecimal(Value(cells, saleRateColumn)),
                    Moq = ParseInt(Value(cells, moqColumn)),
                    Boq = ParseInt(Value(cells, boqColumn)),
                    Sgst = ParseDecimal(Value(cells, sgstColumn)),
                    Cgst = ParseDecimal(Value(cells, cgstColumn)),
                    Igst = ParseDecimal(Value(cells, igstColumn)),
                    Ugst = ParseDecimal(Value(cells, ugstColumn)),
                    Grpidno = ResolveGroupId(Value(cells, groupColumn)),
                    Ipurrate = ParseDecimal(Value(cells, purchaseRateColumn)),
                    Iselectric = string.Equals(isOemRaw, "Yes", StringComparison.OrdinalIgnoreCase),
                    Vehtype = ParseInt(Value(cells, vehTypeColumn)),
                    Noofbatteries = ParseInt(Value(cells, noOfBatteriesColumn)),
                    Colorcode = Value(cells, colorCodeColumn),
                    Rrgitemidno = ParseInt(Value(cells, rrgItemIdColumn)),
                    Itemcc = ParseInt(Value(cells, itemCcColumn)),
                    Batterytypeidno = ParseInt(Value(cells, batteryTypeIdColumn)),
                    Fame2amount = ParseDecimal(Value(cells, fame2Column)),
                    Compcode = Value(cells, compCodeColumn),
                    Displayname = Value(cells, displayNameColumn),
                    Oemmodelname = Value(cells, oemModelNameColumn),
                    Dealercode = Value(cells, dealerCodeColumn), // blank -> normalized to null in ImportItemExcelAsync
                    UOM = ResolveUom(Value(cells, uomColumn)),
                    MinBillQty = ParseInt(Value(cells, minBillQtyColumn)),
                    MinOrderQty = ParseInt(Value(cells, minOrderQtyColumn))
                };

                var supplierCodeRaw = Value(cells, supplierCodeColumn);

                rows.Add((r + 1, item, string.IsNullOrWhiteSpace(supplierCodeRaw) ? null : supplierCodeRaw));
            }

            return rows;
        }
    }
}