using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LocationMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LocationMasterService
{
    public class LocationMasterService : ILocationMasterService
    {
        private readonly ILocationMasterRepo _locationMasterRepo;
        private readonly IExcelService _excelService;
        public LocationMasterService(ILocationMasterRepo locationMasterRepo, IExcelService excelService)
        {
            _locationMasterRepo = locationMasterRepo;
            _excelService = excelService;
        }
        public async Task<List<LocationMasterViewModel>> GetAllLocationMaster()
        {
            return await _locationMasterRepo.GetAllLocationMaster();
        }
        public async Task<LocationMasterViewModel> GetLocationMasterById(int id)
        {
            return await _locationMasterRepo.GetLocationMasterById(id);
        }

        public async Task<bool> AddLocationMaster(LocationMasterViewModel model)
        {
            return await _locationMasterRepo.AddLocationMaster(model);
        }

        public async Task<bool> UpdateLocationMaster(LocationMasterViewModel model)
        {
            return await _locationMasterRepo.UpdateLocationMaster(model);
        }
        public async Task<byte[]> DownloadLocationMasterExcel()
        {
            try
            {
                var data = await _locationMasterRepo.GetAllLocationMaster();

                var columns = new List<string>
                {
                    "Loccode",
                    "Locname",
                    "LocationArea",
                    "Add1",
                    "Add2",
                    "State",
                    "City",
                    "Pincode",
                    "Gstinno",
                    "Email",
                    "Mobileno",
                    "Contpername1",
                    "Contpername2",
                    "Contpermob1",
                    "Contpermob2",
                    "Contperemail1",
                    "Contperemail2",
                    "Formtype",
                    "Dealercode",
                    "Rrglocationidno",
                    "Active"
                };

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    dict["Loccode"] = d.Loccode;
                    dict["Locname"] = d.Locname;
                    //dict["Locareaidno"] = d.Locareaidno;
                    dict["LocationArea"] = d.Locareaidno == 1 ? "Showroom"
                        : d.Locareaidno == 2 ? "Workshop"
                        : d.Locareaidno == 3 ? "Yard"
                        : "";
                    dict["Add1"] = d.Add1;
                    dict["Add2"] = d.Add2;
                    dict["State"] = d.State;
                    dict["City"] = d.City;
                    dict["Pincode"] = d.Pincode;
                    dict["Gstinno"] = d.Gstinno;
                    dict["Email"] = d.Email;
                    dict["Mobileno"] = d.Mobileno;
                    dict["Contpername1"] = d.Contpername1;
                    dict["Contpername2"] = d.Contpername2;
                    dict["Contpermob1"] = d.Contpermob1;
                    dict["Contpermob2"] = d.Contpermob2;
                    dict["Contperemail1"] = d.Contperemail1;
                    dict["Contperemail2"] = d.Contperemail2;
                    dict["Formtype"] = d.Formtype;
                    dict["Dealercode"] = d.Dealercode;
                    dict["Rrglocationidno"] = d.Rrglocationidno;
                    dict["Active"] = d.Active;

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = "LocationMaster",
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

        public async Task<List<LocationNameViewModel>> GetLocationByDealerCode(string dealerCode)
        {
            return await _locationMasterRepo.GetLocationByDealerCode(dealerCode);
        }
        public async Task<List<LocationTypewiseNameViewModel>> GetLocationNameTypewiseListAsync(string? dealerCode)
        {
            return await _locationMasterRepo.GetLocationNameTypewiseListAsync(dealerCode);
        }
        public Task<(LocationMaster Location, bool IsNew)> UpdateByLocationCode(string userId, LocationMasterViewModel locationMasterViewModel) => _locationMasterRepo.UpdateByLocationCode(userId, locationMasterViewModel);
        public Task<IEnumerable<LocationNameViewModel>> GetLocationByDealerByAreaId(string? dealerCode, int areaId) => _locationMasterRepo.GetLocationByDealerByAreaId(dealerCode, areaId);
        Task<IEnumerable<object>> ILocationMasterService.GetDealerPrimaryLocationByAreaId(int areaId, string locCode, string? dealerCode) => _locationMasterRepo.GetDealerPrimaryLocationByAreaId(areaId, locCode, dealerCode);

        public async Task<List<LocationNameViewModel>> GetAllLocationByDealerCode(string dealerCode)=> await _locationMasterRepo.GetAllLocationByDealerCode(dealerCode);
        public async Task<IEnumerable<LocationMasterViewModel>> GetLocationDropdownByDealerCode(string? dealerCode)
        {
            try
            {
                return await _locationMasterRepo.GetLocationDropdownByDealerCode(dealerCode);
            }
            catch
            {
                throw;
            }
        }

        public Task<(string? RoleId, string? RoleName)> GetRoleByDealerAndLocationCodeAsync(string? dealerCode, string? locationCode)
    => _locationMasterRepo.GetRoleByDealerAndLocationCodeAsync(dealerCode, locationCode);

        public async Task<LocationImportResultViewModel> ImportLocationExcelAsync(IFormFile file, string userId)
        {
            var result = new LocationImportResultViewModel();

            List<(int RowNumber, LocationMasterViewModel Location)> rows;

            try
            {
                rows = ReadLocationsFromExcel(file);
            }
            catch (Exception ex)
            {
                // Header/format problems in the uploaded file — surfaced as a
                // clean 400 by the controller instead of an unhandled 500.
                throw new InvalidOperationException($"Could not read the Excel file: {ex.Message}", ex);
            }

            result.TotalRows = rows.Count;

            foreach (var (rowNumber, locationVm) in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(locationVm.Loccode))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new LocationImportRowError
                        {
                            RowNumber = rowNumber,
                            Loccode = locationVm.Loccode,
                            Message = "Location Code is required."
                        });
                        continue;
                    }

                    var (_, isNew) = await _locationMasterRepo.UpdateByLocationCode(userId, locationVm);

                    if (isNew) result.InsertedCount++;
                    else result.UpdatedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(new LocationImportRowError
                    {
                        RowNumber = rowNumber,
                        Loccode = locationVm.Loccode,
                        Message = ex.Message
                    });
                }
            }

            return result;
        }

        private List<(int RowNumber, LocationMasterViewModel Location)> ReadLocationsFromExcel(IFormFile file)
        {
            var rows = new List<(int, LocationMasterViewModel)>();

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

            string NormalizeHeader(string? header)
            {
                if (string.IsNullOrWhiteSpace(header)) return string.Empty;
                return new string(header.Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
            }

            var allRows = sheetData.Elements<Row>().ToList();
            if (allRows.Count < 2) return rows;

            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in allRows[0].Elements<Cell>())
            {
                var header = GetCellValue(cell);
                if (string.IsNullOrWhiteSpace(header)) continue;

                var columnIndex = GetColumnIndex(cell.CellReference?.Value);
                if (columnIndex < 0) continue;

                var normalizedHeader = NormalizeHeader(header);
                if (!headerMap.ContainsKey(normalizedHeader))
                    headerMap[normalizedHeader] = columnIndex;
            }

            string? FindColumn(params string[] aliases)
            {
                foreach (var alias in aliases)
                {
                    var normalized = NormalizeHeader(alias);
                    if (headerMap.ContainsKey(normalized)) return normalized;
                }
                return null;
            }

            var loccodeColumn = FindColumn("Loc Code", "Loccode", "Location Code");
            var locnameColumn = FindColumn("Location Name", "Locname");
            var locAreaTextColumn = FindColumn("Location Area", "LocationArea");
            var locAreaIdColumn = FindColumn("Locareaidno", "Location Area Id");
            var add1Column = FindColumn("Address 1", "Add1");
            var add2Column = FindColumn("Address 2", "Add2");
            var stateColumn = FindColumn("State");
            var cityColumn = FindColumn("City");
            var pincodeColumn = FindColumn("Pincode", "Pin Code", "Pin");
            var gstinColumn = FindColumn("GSTIN", "Gstinno", "GSTIN No");
            var emailColumn = FindColumn("Email");
            var mobileColumn = FindColumn("Mobile No", "Mobileno", "Mobile");
            var contPerson1Column = FindColumn("Contact Person Name", "Contpername1");
            var contPerson2Column = FindColumn("Contpername2");
            var contMobile1Column = FindColumn("Contpermob1");
            var contMobile2Column = FindColumn("Contpermob2");
            var contEmail1Column = FindColumn("Contperemail1");
            var contEmail2Column = FindColumn("Contperemail2");
            var formTypeColumn = FindColumn("Source", "Formtype");
            var dealerCodeColumn = FindColumn("Dealer Code", "Dealercode");
            var supplierCodeColumn = FindColumn("Supplier Code", "Rrglocationidno");
            var activeColumn = FindColumn("Active");

            if (loccodeColumn == null)
                throw new InvalidOperationException("Location Code column was not found in the Excel file. Expected column name: 'Loc Code' or 'Location Code'.");

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

            int ParseLocationArea(string text, string numericFallback)
            {
                var normalized = text?.Trim().ToLowerInvariant();
                return normalized switch
                {
                    "showroom" => 1,
                    "workshop" => 2,
                    "yard" => 3,
                    _ => int.TryParse(numericFallback, out var n) ? n : 0
                };
            }

            for (int r = 1; r < allRows.Count; r++)
            {
                var cells = allRows[r].Elements<Cell>().ToList();
                if (cells.Count == 0) continue;

                var loccode = Value(cells, loccodeColumn).Trim();

                if (string.IsNullOrWhiteSpace(loccode) && cells.All(c => string.IsNullOrWhiteSpace(GetCellValue(c))))
                    continue;

                var location = new LocationMasterViewModel
                {
                    Loccode = loccode,
                    Locname = Value(cells, locnameColumn),
                    Locareaidno = ParseLocationArea(Value(cells, locAreaTextColumn), Value(cells, locAreaIdColumn)),
                    Add1 = Value(cells, add1Column),
                    Add2 = Value(cells, add2Column),
                    State = Value(cells, stateColumn),
                    City = Value(cells, cityColumn),
                    Pincode = Value(cells, pincodeColumn),
                    Gstinno = Value(cells, gstinColumn),
                    Email = Value(cells, emailColumn),
                    Mobileno = Value(cells, mobileColumn),
                    Contpername1 = Value(cells, contPerson1Column),
                    Contpername2 = Value(cells, contPerson2Column),
                    Contpermob1 = Value(cells, contMobile1Column),
                    Contpermob2 = Value(cells, contMobile2Column),
                    Contperemail1 = Value(cells, contEmail1Column),
                    Contperemail2 = Value(cells, contEmail2Column),
                    Formtype = Value(cells, formTypeColumn),
                    Dealercode = Value(cells, dealerCodeColumn),
                    Active = string.Equals(Value(cells, activeColumn), "No", StringComparison.OrdinalIgnoreCase) ? "N" : "Y"
                };

                int.TryParse(Value(cells, supplierCodeColumn), out var supplierCode);
                location.Rrglocationidno = supplierCode;

                rows.Add((r + 1, location));
            }

            return rows;
        }
    }
}
