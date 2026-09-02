using Azure.Core;
using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DealerMasterService
{
    public class DealerMasterService : IDealerMasterService
    {
        private readonly IDealerMasterRepo _dealerMasterRepo;
        private readonly IExcelService _excelService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DealerMasterService(IDealerMasterRepo dealerMasterRepo, IExcelService excelService, UserManager<ApplicationUser> userManager)
        {
            _dealerMasterRepo = dealerMasterRepo;
            _excelService = excelService;
            _userManager = userManager;
        }

        // Create dealer and corresponding identity user
        public async Task<DealerMaster?> AddDealerAsync(DealerMasterViewModel dealer, string userId)
        {
            await _dealerMasterRepo.BeginTransactionAsync();

            try
            {
                var existingDealer = await _dealerMasterRepo.GetDealerByCode(dealer.Dealercode);

                DealerMaster result;

                if (existingDealer != null)
                {
                    var _existingDealer = new DealerMasterViewModel
                    {
                        Compname = dealer.Compname,
                        Compcode = dealer.Compcode,
                        Adress1 = dealer.Adress1,
                        Adress2 = dealer.Adress2,
                        City = dealer.City,
                        State = dealer.State,
                        Pin = dealer.Pin,
                        Pan = dealer.Pan,
                        PhoneOff = dealer.PhoneOff,
                        Mobile = dealer.Mobile,
                        Email = dealer.Email,
                        Contactperson = dealer.Contactperson,
                        RegDate = dealer.RegDate,
                        TradCert = dealer.TradCert,
                        CompgstinNo = dealer.CompgstinNo,
                        BrandName = dealer.BrandName,
                        CompImage = dealer.CompImage,
                        Dealercode = dealer.Dealercode,
                        Areaofficeid = dealer.Areaofficeid,
                        CinNo = dealer.CinNo,
                        VatNo = dealer.VatNo,
                        IsTcs = dealer.IsTcs,
                        TcsPercent = dealer.TcsPercent,
                        FameiiCode = dealer.FameiiCode,
                        CeditLimit = dealer.CeditLimit,
                        RegAddress = dealer.RegAddress,
                        B2b = dealer.B2b,
                        CreatedBy = dealer.CreatedBy,
                        CreatedDate = dealer.CreatedDate,
                        UpdatedBy = dealer.UpdatedBy,
                        UpdatedDate = dealer.UpdatedDate
                    };

                    result = await _dealerMasterRepo.UpdateDealerAsync(existingDealer.Id, _existingDealer, userId);
                }
                else
                {
                    result = await _dealerMasterRepo.AddDealerAsync(dealer, userId);

                    await _dealerMasterRepo.AddDealerToLedgerAsync(dealer, userId);
                }

                await _dealerMasterRepo.SaveAsync();

                var existingUser = await _userManager.FindByNameAsync(dealer.Email);

                if (existingUser == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = result.Email,
                        Email = result.Email,
                        EmailConfirmed = true,
                        DealerCode = result.Dealercode
                    };

                    var userResult = await _userManager.CreateAsync(
                        newUser,
                        StringConstants.DealerDefaultPassword
                    );

                    if (!userResult.Succeeded)
                        throw new Exception(string.Join(", ", userResult.Errors.Select(e => e.Description)));

                    var roleResult = await _userManager.AddToRoleAsync(newUser, StringConstants.DealerText);

                    if (!roleResult.Succeeded)
                        throw new Exception("Role assignment failed");
                }

                await _dealerMasterRepo.CommitTransactionAsync();

                return result;
            }
            catch
            {
                await _dealerMasterRepo.RollbackTransactionAsync();
                throw;
            }
        }

        // Get all dealers with optional search
        public async Task<List<DealerMaster>> GetAllDealersAsync(string? search)
        {
            try
            {
                return await _dealerMasterRepo.GetAllDealersAsync(search);
            }
            catch
            {
                throw;
            }
        }

        // Get dealer by ID
        public async Task<DealerMaster> GetDealerById(int id)
        {
            try
            {
                return await _dealerMasterRepo.GetDealerById(id);
            }
            catch
            {
                throw;
            }
        }

        // Update dealer details
        public async Task<DealerMaster?> UpdateDealerAsync(int id, DealerMasterViewModel dealer, string userId)
        {
            try
            {
                return await _dealerMasterRepo.UpdateDealerAsync(id, dealer, userId);
            }
            catch
            {
                throw;
            }
        }

        // Export dealer list to Excel
        public async Task<byte[]> DownloadDealerExcel()
        {
            try
            {
                var data = await _dealerMasterRepo.GetAllDealersAsync(null);

                var properties = typeof(DealerMasterViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);
                        dict[prop.Name] = entityProp != null
                            ? entityProp.GetValue(d)
                            : null;
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
                Console.WriteLine(ex.ToString()); // optional logging
                throw;
            }
        }

        // Get dealer dropdown list
        public async Task<List<DealerDropdownViewModel>> GetDealerDropdown(string? dealerCode)
        {
            try
            {
                return await _dealerMasterRepo.GetDealerDropdown(dealerCode);
            }
            catch
            {
                throw;
            }
        }

        // Get dealer by dealer code
        public async Task<DealerMaster> GetDealerByCode(string dealerCode)
        {
            try
            {
                return await _dealerMasterRepo.GetDealerByCode(dealerCode);
            }
            catch
            {
                throw;
            }
        }

        //Update Trade Certificate
        public async Task<DealerMaster> EditTradeCertificate(string dealerCode, string tradeCertificate)
        {
            try
            {
                return await _dealerMasterRepo.EditTradeCertificate(dealerCode, tradeCertificate);
            }
            catch
            {
                throw;
            }


        }

        public async Task<object> SyncAllDealerLoginsAsync()
        {
            var dealers = await _dealerMasterRepo.GetAllDealersAsync(null);

            var created = new List<string>();
            var alreadyLinked = new List<string>();
            var skippedNoEmail = new List<string>();
            var failed = new List<object>();

            foreach (var dealer in dealers)
            {
                if (string.IsNullOrWhiteSpace(dealer.Dealercode))
                    continue;

                if (string.IsNullOrWhiteSpace(dealer.Email))
                {
                    skippedNoEmail.Add(dealer.Dealercode);
                    continue;
                }

                try
                {
                    // Match by DealerCode first (authoritative), fall back to email
                    var existingUser = _userManager.Users.FirstOrDefault(u => u.DealerCode == dealer.Dealercode)
                                        ?? await _userManager.FindByEmailAsync(dealer.Email);

                    if (existingUser != null)
                    {
                        bool needsUpdate = false;

                        if (string.IsNullOrEmpty(existingUser.DealerCode))
                        {
                            existingUser.DealerCode = dealer.Dealercode;
                            needsUpdate = true;
                        }

                        if (needsUpdate)
                            await _userManager.UpdateAsync(existingUser);

                        if (!await _userManager.IsInRoleAsync(existingUser, StringConstants.DealerText))
                            await _userManager.AddToRoleAsync(existingUser, StringConstants.DealerText);

                        alreadyLinked.Add(dealer.Dealercode);
                        continue;
                    }

                    var newUser = new ApplicationUser
                    {
                        UserName = dealer.Email,
                        Email = dealer.Email,
                        EmailConfirmed = true,
                        DealerCode = dealer.Dealercode
                    };

                    var userResult = await _userManager.CreateAsync(newUser, StringConstants.DealerDefaultPassword);

                    if (!userResult.Succeeded)
                    {
                        failed.Add(new { dealer.Dealercode, dealer.Email, errors = userResult.Errors.Select(e => e.Description) });
                        continue;
                    }

                    var roleResult = await _userManager.AddToRoleAsync(newUser, StringConstants.DealerText);

                    if (!roleResult.Succeeded)
                    {
                        failed.Add(new { dealer.Dealercode, dealer.Email, errors = roleResult.Errors.Select(e => e.Description) });
                        continue;
                    }

                    created.Add(dealer.Dealercode);
                }
                catch (Exception ex)
                {
                    failed.Add(new { dealer.Dealercode, dealer.Email, error = ex.Message });
                }
            }

            return new
            {
                totalDealers = dealers.Count,
                createdCount = created.Count,
                alreadyLinkedCount = alreadyLinked.Count,
                skippedNoEmailCount = skippedNoEmail.Count,
                failedCount = failed.Count,
                created,
                skippedNoEmail,
                failed
            };
        }

        public async Task<ApplicationUser> EnsureDealerUserFromEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var dealer = await _dealerMasterRepo.GetDealerByEmail(email);

            if (dealer == null || string.IsNullOrWhiteSpace(dealer.Dealercode))
                return null; // no matching dealer — caller stays Unauthorized

            // Double-check no user already exists for this dealer/email (race-condition safety)
            var existingUser = _userManager.Users.FirstOrDefault(u => u.DealerCode == dealer.Dealercode)
                                ?? await _userManager.FindByEmailAsync(dealer.Email);

            if (existingUser != null)
            {
                if (!await _userManager.IsInRoleAsync(existingUser, StringConstants.DealerText))
                    await _userManager.AddToRoleAsync(existingUser, StringConstants.DealerText);

                return existingUser;
            }

            var newUser = new ApplicationUser
            {
                UserName = dealer.Email,
                Email = dealer.Email,
                EmailConfirmed = true,
                DealerCode = dealer.Dealercode
            };

            var userResult = await _userManager.CreateAsync(newUser, StringConstants.DealerDefaultPassword);

            if (!userResult.Succeeded)
                throw new Exception(string.Join(", ", userResult.Errors.Select(e => e.Description)));

            var roleResult = await _userManager.AddToRoleAsync(newUser, StringConstants.DealerText);

            if (!roleResult.Succeeded)
                throw new Exception("Role assignment failed for auto-provisioned dealer login");

            return newUser;
        }

        public Task<object> UpdateByDealerCode(string userId, DealerMasterViewModel dealerMasterViewModel) => _dealerMasterRepo.UpdateByDealerCode(userId, dealerMasterViewModel);
        Task<PagedResponse<DealerMaster>> IDealerMasterService.GetDealerByPaged(string? searchTerm, int pageIndex, int pageSize, string? dealerCode) => _dealerMasterRepo.GetDealerByPaged(searchTerm, pageIndex, pageSize, dealerCode);

        public async Task<DealerImportResultViewModel> ImportDealerExcelAsync(IFormFile file, string userId)
        {
            var result = new DealerImportResultViewModel();

            List<(int RowNumber, DealerMasterViewModel Dealer)> rows;

            try
            {
                rows = ReadDealersFromExcel(file);
            }
            catch (Exception ex)
            {
                // Header/format problems in the uploaded file — surfaced as a
                // clean 400 by the controller instead of an unhandled 500.
                throw new InvalidOperationException($"Could not read the Excel file: {ex.Message}", ex);
            }

            result.TotalRows = rows.Count;

            foreach (var (rowNumber, dealerVm) in rows)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dealerVm.Dealercode))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new DealerImportRowError
                        {
                            RowNumber = rowNumber,
                            DealerCode = dealerVm.Dealercode,
                            Message = "Dealer Code is required."
                        });
                        continue;
                    }

                    var existing = await GetDealerByCode(dealerVm.Dealercode);

                    if (existing != null)
                    {
                        var updated = await UpdateByDealerCode(userId, dealerVm);
                        if (updated == null)
                        {
                            result.FailedCount++;
                            result.Errors.Add(new DealerImportRowError
                            {
                                RowNumber = rowNumber,
                                DealerCode = dealerVm.Dealercode,
                                Message = "Update failed for this dealer code."
                            });
                            continue;
                        }

                        result.UpdatedCount++;
                    }
                    else
                    {
                        await AddDealerAsync(dealerVm, userId);
                        result.InsertedCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(new DealerImportRowError
                    {
                        RowNumber = rowNumber,
                        DealerCode = dealerVm.Dealercode,
                        Message = ex.Message
                    });
                }
            }

            return result;
        }
        private List<(int RowNumber, DealerMasterViewModel Dealer)> ReadDealersFromExcel(IFormFile file)
        {
            var rows = new List<(int, DealerMasterViewModel)>();

            using var stream = file.OpenReadStream();
            using var document = SpreadsheetDocument.Open(stream, false);

            var workbookPart = document.WorkbookPart!;

            var sheet = workbookPart.Workbook
                .Descendants<Sheet>()
                .First();

            var worksheetPart =
                (WorksheetPart)workbookPart.GetPartById(sheet.Id!);

            var sheetData =
                worksheetPart.Worksheet
                    .Elements<SheetData>()
                    .First();

            var sharedStrings =
                workbookPart.SharedStringTablePart?.SharedStringTable;


            // ============================================================
            // GET CELL VALUE
            // ============================================================

            string GetCellValue(Cell? cell)
            {
                if (cell?.CellValue == null)
                    return string.Empty;

                var value = cell.CellValue.InnerText;

                if (cell.DataType != null &&
                    cell.DataType.Value == CellValues.SharedString &&
                    sharedStrings != null)
                {
                    if (int.TryParse(value, out int index) &&
                        index >= 0 &&
                        index < sharedStrings.Count())
                    {
                        return sharedStrings
                            .ElementAt(index)
                            .InnerText
                            .Trim();
                    }
                }

                return value?.Trim() ?? string.Empty;
            }


            // ============================================================
            // GET COLUMN INDEX FROM EXCEL CELL REFERENCE
            // Example:
            // A1 -> 0
            // B1 -> 1
            // C1 -> 2
            // AA1 -> 26
            // ============================================================

            int GetColumnIndex(string? cellReference)
            {
                if (string.IsNullOrWhiteSpace(cellReference))
                    return -1;

                int columnIndex = 0;

                foreach (char c in cellReference)
                {
                    if (!char.IsLetter(c))
                        break;

                    columnIndex =
                        columnIndex * 26 +
                        (char.ToUpper(c) - 'A' + 1);
                }

                return columnIndex - 1;
            }


            // ============================================================
            // NORMALIZE HEADER
            // ============================================================

            string NormalizeHeader(string? header)
            {
                if (string.IsNullOrWhiteSpace(header))
                    return string.Empty;

                return new string(
                    header
                        .Trim()
                        .ToLowerInvariant()
                        .Where(char.IsLetterOrDigit)
                        .ToArray()
                );
            }


            var allRows =
                sheetData
                    .Elements<Row>()
                    .ToList();

            if (allRows.Count < 2)
                return rows;


            // ============================================================
            // HEADER MAP
            // ============================================================

            var headerMap =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase
                );

            var headerCells =
                allRows[0]
                    .Elements<Cell>()
                    .ToList();

            foreach (var cell in headerCells)
            {
                var header = GetCellValue(cell);

                if (string.IsNullOrWhiteSpace(header))
                    continue;

                var columnIndex =
                    GetColumnIndex(cell.CellReference?.Value);

                if (columnIndex < 0)
                    continue;

                var normalizedHeader =
                    NormalizeHeader(header);

                if (!headerMap.ContainsKey(normalizedHeader))
                {
                    headerMap[normalizedHeader] = columnIndex;
                }
            }


            // ============================================================
            // HEADER ALIASES
            // ============================================================

            string? FindColumn(params string[] aliases)
            {
                foreach (var alias in aliases)
                {
                    var normalized =
                        NormalizeHeader(alias);

                    if (headerMap.ContainsKey(normalized))
                        return normalized;
                }

                return null;
            }


            var companyNameColumn =
                FindColumn(
                    "Company Name",
                    "Compname",
                    "CompanyName"
                );

            var companyCodeColumn =
                FindColumn(
                    "Company Code",
                    "Compcode",
                    "CompanyCode"
                );

            var dealerCodeColumn =
                FindColumn(
                    "Dealer Code",
                    "DealerCode",
                    "Dealercode",
                    "Dealer Code "
                );

            var address1Column =
                FindColumn(
                    "Address 1",
                    "Adress1"
                );

            var address2Column =
                FindColumn(
                    "Address 2",
                    "Adress2"
                );

            var cityColumn =
                FindColumn("City");

            var stateColumn =
                FindColumn("State");

            var pinColumn =
                FindColumn(
                    "Pin",
                    "PIN",
                    "Pincode",
                    "Pin Code"
                );

            var panColumn =
                FindColumn("PAN");

            var phoneOfficeColumn =
                FindColumn(
                    "Phone Office",
                    "PhoneOff",
                    "Office Phone"
                );

            var mobileColumn =
                FindColumn("Mobile");

            var emailColumn =
                FindColumn("Email");

            var contactPersonColumn =
                FindColumn(
                    "Contact Person",
                    "Contactperson"
                );

            var tradeCertificateColumn =
                FindColumn(
                    "Trade Certificate",
                    "TradCert"
                );

            var gstinColumn =
                FindColumn(
                    "GSTIN",
                    "CompgstinNo",
                    "GSTIN No"
                );

            var brandNameColumn =
                FindColumn("Brand Name");

            var cinColumn =
                FindColumn(
                    "CIN No",
                    "CinNo",
                    "CIN"
                );

            var vatColumn =
                FindColumn(
                    "VAT No",
                    "VatNo",
                    "VAT"
                );

            var fameiiColumn =
                FindColumn(
                    "FameII Code",
                    "FameiiCode",
                    "FAMEII Code"
                );

            var registrationAddressColumn =
                FindColumn(
                    "Registration Address",
                    "RegAddress"
                );

            var regDateColumn =
                FindColumn(
                    "Reg Date",
                    "RegDate",
                    "Registration Date"
                );

            var isTcsColumn =
                FindColumn(
                    "Is TCS",
                    "IsTcs"
                );

            var tcsPercentColumn =
                FindColumn(
                    "TCS Percent",
                    "TcsPercent"
                );

            var creditLimitColumn =
                FindColumn(
                    "Credit Limit",
                    "CeditLimit",
                    "CreditLimit"
                );

            var b2bColumn =
                FindColumn("B2B");

            var activeColumn =
                FindColumn(
                    "Active",
                    "IsActive"
                );



            // ============================================================
            // DEBUG: CHECK DEALER CODE COLUMN
            // ============================================================

            if (dealerCodeColumn == null)
            {
                throw new InvalidOperationException(
                    "Dealer Code column was not found in the Excel file. " +
                    "Expected column name: 'Dealer Code'."
                );
            }


            // ============================================================
            // GET VALUE BY COLUMN
            // ============================================================

            string Value(
                List<Cell> cells,
                string? normalizedColumn)
            {
                if (string.IsNullOrWhiteSpace(normalizedColumn))
                    return string.Empty;

                if (!headerMap.TryGetValue(
                        normalizedColumn,
                        out var columnIndex))
                {
                    return string.Empty;
                }

                foreach (var cell in cells)
                {
                    var currentColumn =
                        GetColumnIndex(
                            cell.CellReference?.Value
                        );

                    if (currentColumn == columnIndex)
                    {
                        return GetCellValue(cell);
                    }
                }

                return string.Empty;
            }


            // ============================================================
            // READ DATA ROWS
            // ============================================================

            for (int r = 1; r < allRows.Count; r++)
            {
                var excelRow =
                    allRows[r];

                var cells =
                    excelRow
                        .Elements<Cell>()
                        .ToList();

                if (cells.Count == 0)
                    continue;


                // --------------------------------------------------------
                // READ DEALER CODE FIRST
                // --------------------------------------------------------

                var dealerCode =
                    Value(
                        cells,
                        dealerCodeColumn
                    ).Trim();


                // --------------------------------------------------------
                // SKIP COMPLETELY EMPTY ROW
                // --------------------------------------------------------

                if (string.IsNullOrWhiteSpace(dealerCode) &&
                    cells.All(c =>
                        string.IsNullOrWhiteSpace(
                            GetCellValue(c))))
                {
                    continue;
                }


                // --------------------------------------------------------
                // CREATE DEALER MODEL
                // --------------------------------------------------------

                var dealer =
                    new DealerMasterViewModel
                    {
                        Compname =
                            Value(cells, companyNameColumn),

                        Compcode =
                            Value(cells, companyCodeColumn),

                        Dealercode =
                            dealerCode,

                        Adress1 =
                            Value(cells, address1Column),

                        Adress2 =
                            Value(cells, address2Column),

                        City =
                            Value(cells, cityColumn),

                        State =
                            Value(cells, stateColumn),

                        Pin =
                            Value(cells, pinColumn),

                        Pan =
                            Value(cells, panColumn),

                        PhoneOff =
                            Value(cells, phoneOfficeColumn),

                        Mobile =
                            Value(cells, mobileColumn),

                        Email =
                            Value(cells, emailColumn),

                        Contactperson =
                            Value(cells, contactPersonColumn),

                        TradCert =
                            Value(cells, tradeCertificateColumn),

                        CompgstinNo =
                            Value(cells, gstinColumn),

                        BrandName =
                            Value(cells, brandNameColumn),

                        CinNo =
                            Value(cells, cinColumn),

                        VatNo =
                            Value(cells, vatColumn),

                        FameiiCode =
                            Value(cells, fameiiColumn),

                        RegAddress =
                            Value(cells, registrationAddressColumn),

                        RegDate =
                            Value(cells, regDateColumn),

                        IsTcs =
                            string.Equals(
                                Value(cells, isTcsColumn),
                                "Yes",
                                StringComparison.OrdinalIgnoreCase
                            ),

                        B2b =
                            string.Equals(
                                Value(cells, b2bColumn),
                                "Yes",
                                StringComparison.OrdinalIgnoreCase
                            ),

                        IsActive =
                            !string.Equals(
                                Value(cells, activeColumn),
                                "No",
                                StringComparison.OrdinalIgnoreCase
                            )
                    };


                // --------------------------------------------------------
                // DECIMAL VALUES
                // --------------------------------------------------------

                decimal.TryParse(
                    Value(cells, tcsPercentColumn),
                    out var tcsPercent
                );

                dealer.TcsPercent =
                    tcsPercent;


                decimal.TryParse(
                    Value(cells, creditLimitColumn),
                    out var creditLimit
                );

                dealer.CeditLimit =
                    creditLimit;


                // --------------------------------------------------------
                // AREA OFFICE
                // --------------------------------------------------------

                var areaOfficeText =
                    Value(
                        cells,
                        FindColumn(
                            "Area Office Id",
                            "Areaofficeid",
                            "Area Office"
                        )
                    );

                int.TryParse(
                    areaOfficeText,
                    out var areaOfficeId
                );

                dealer.Areaofficeid =
                    areaOfficeId;


                // --------------------------------------------------------
                // ADD ROW
                // --------------------------------------------------------

                rows.Add(
                    (
                        r + 1,
                        dealer
                    )
                );
            }


            return rows;
        }
    }
}
