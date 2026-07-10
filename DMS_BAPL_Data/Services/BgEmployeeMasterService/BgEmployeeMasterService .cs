using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.BgEmployeeMasterRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.BgEmployeeMasterService
{
    public class BgEmployeeMasterService : IBgEmployeeMasterService
    {
        private readonly IBgEmployeeMasterRepo _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Now read from appsettings.json ("TsmApi:BaseUrl") instead of being
        // hardcoded — lets the URL be corrected without a rebuild once the
        // real path is confirmed. Falls back to the original assumed URL
        // only if the config key is missing entirely.
        private string TsmEntryUrl =>
            _configuration["TsmApi:BaseUrl"]
            ?? "https://bapldmsai-e6f0hzhmg4achue9.centralindia-01.azurewebsites.net/api/erptsmmaster/TSMEntry";

        public BgEmployeeMasterService(
            IBgEmployeeMasterRepo repo,
            UserManager<ApplicationUser> userManager,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _repo = repo;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // =====================================================
        // TSM ERP LOOKUP — NEW. Proxies the external ERP TSM API so the
        // frontend never calls a third-party domain directly.
        // =====================================================

        public async Task<TsmEntryViewModel?> FetchTsmDetailsAsync(string tsmCode)
        {
            if (string.IsNullOrWhiteSpace(tsmCode))
                return null;

            var client = _httpClientFactory.CreateClient();

            var payload = new { tsmcode = tsmCode };
            var response = await client.PostAsJsonAsync(TsmEntryUrl, payload);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<TsmEntryViewModel>();
        }

        // ── TEMPORARY DIAGNOSTIC — tries several plausible calling
        // conventions against the external TSM API in one shot, since the
        // first assumption (POST + {tsmcode} body) came back 404 from
        // their server directly. Remove once the real convention is confirmed.
        public async Task<List<(string Attempt, int StatusCode, string Body)>> FetchTsmRawAsync(string tsmCode)
        {
            var client = _httpClientFactory.CreateClient();
            var results = new List<(string, int, string)>();

            async Task TryCall(string label, Func<Task<HttpResponseMessage>> call)
            {
                try
                {
                    var resp = await call();
                    var body = await resp.Content.ReadAsStringAsync();
                    results.Add((label, (int)resp.StatusCode, body));
                }
                catch (Exception ex)
                {
                    results.Add((label, -1, $"Exception: {ex.Message}"));
                }
            }

            // 1. Original assumption: POST with { tsmcode } in the body
            await TryCall(
                "POST TSMEntry + {tsmcode} body",
                () => client.PostAsJsonAsync(TsmEntryUrl, new { tsmcode = tsmCode }));

            // 2. GET with tsmcode as a query string
            await TryCall(
                "GET TSMEntry?tsmcode=...",
                () => client.GetAsync($"{TsmEntryUrl}?tsmcode={Uri.EscapeDataString(tsmCode)}"));

            // 3. GET with the code as part of the URL path
            await TryCall(
                "GET TSMEntry/{code}",
                () => client.GetAsync($"{TsmEntryUrl}/{Uri.EscapeDataString(tsmCode)}"));

            // 4. POST with the code as part of the URL path, empty body
            await TryCall(
                "POST TSMEntry/{code} (no body)",
                () => client.PostAsync($"{TsmEntryUrl}/{Uri.EscapeDataString(tsmCode)}", null));

            return results;
        }

        public async Task<IEnumerable<BgEmployeeMaster>> Get()
        {
            try { return await _repo.Get(); }
            catch { throw; }
        }

        public async Task<BgEmployeeMaster?> GetById(int id)
        {
            try { return await _repo.GetById(id); }
            catch { throw; }
        }

        public async Task<BgEmployeeMaster> Create(BgEmployeeViewModel model)
        {
            try
            {
                model.EmailId = model.EmailId?.Trim().ToLowerInvariant();

                var existingEmployee = await _repo.GetByEmail(model.EmailId);
                if (existingEmployee != null)
                    throw new InvalidOperationException("An employee with this email already exists.");

                await UnassignConflictingDealers(model.DealerCode, excludeEmployeeId: 0);

                var entity = MapToEntity(model);
                entity.CreatedBy = model.CreatedBy ?? "admin";
                entity.CreatedDate = DateTime.Now;
                entity.UpdatedBy = model.CreatedBy ?? "admin";
                entity.UpdatedDate = DateTime.Now;

                var savedEmployee = await _repo.Create(entity);

                if (model.RoleMappings?.Any() == true)
                    await _repo.SaveRoleMappings(savedEmployee.Id, model.RoleMappings);

                if (!string.IsNullOrWhiteSpace(savedEmployee.EmailId))
                {
                    try
                    {
                        var existingUser = await _userManager.FindByNameAsync(savedEmployee.EmailId);
                        if (existingUser == null)
                        {
                            var newUser = new ApplicationUser
                            {
                                UserName = savedEmployee.EmailId,
                                Email = savedEmployee.EmailId,
                                EmailConfirmed = true
                            };
                            var passwordToUse = !string.IsNullOrWhiteSpace(model.Password)
                                ? model.Password : StringConstants.BgEmployeeDefaultPassword;

                            var userResult = await _userManager.CreateAsync(newUser, passwordToUse);
                            if (userResult.Succeeded)
                                await _userManager.AddToRoleAsync(newUser, StringConstants.BgEmployeeText);
                            else
                                Console.WriteLine($"Login creation failed: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Login creation exception: {ex.Message}");
                    }
                }

                return savedEmployee;
            }
            catch { throw; }
        }

        public async Task<int> Update(BgEmployeeViewModel model)
        {
            try
            {
                await UnassignConflictingDealers(model.DealerCode, excludeEmployeeId: model.Id);

                var entity = MapToEntity(model);
                entity.UpdatedBy = model.UpdatedBy ?? "admin";
                entity.UpdatedDate = DateTime.Now;

                var result = await _repo.Update(entity);

                await _repo.SaveRoleMappings(model.Id, model.RoleMappings ?? new List<RoleMappingDto>());

                return result;
            }
            catch { throw; }
        }

        // =====================================================
        // UPDATE STATUS ONLY — NEW. Straight passthrough; no role
        // mapping or entity mapping involved at all.
        // =====================================================

        public async Task<int> UpdateStatus(int id, bool isActive)
            => await _repo.UpdateStatus(id, isActive);

        public async Task<int> Delete(int id)
        {
            try { return await _repo.Delete(id); }
            catch { throw; }
        }

        public Task<BgEmployeeMaster?> GetByEmail(string email) => _repo.GetByEmail(email);

        private static BgEmployeeMaster MapToEntity(BgEmployeeViewModel model)
        {
            return new BgEmployeeMaster
            {
                Id = model.Id,
                EmployeeCode = model.EmployeeCode,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                Mobile = model.Mobile,
                State = model.State,
                City = model.City,
                Pincode = model.Pincode,
                DateOfBirth = model.DateOfBirth,
                DateOfJoin = model.DateOfJoin,
                EffectiveDate = model.EffectiveDate,
                ReportingTo = model.ReportingTo,
                IsActive = model.IsActive,
                Department = model.Department,
                ProfileId = model.ProfileId,
                EmailId = model.EmailId,
                Email = model.Email,
                Password = model.Password,
                MappedZones = model.MappedZones,
                MappedZoneIds = model.MappedZoneIds,
                ProfileImage = model.ProfileImage,
                DealerCode = model.DealerCode,
                LocationCode = model.LocationCode,
                CreatedBy = model.CreatedBy,
                CreatedDate = model.CreatedDate,
                UpdatedBy = model.UpdatedBy,
                UpdatedDate = model.UpdatedDate,
            };
        }

        private async Task UnassignConflictingDealers(string dealerCodesCsv, int excludeEmployeeId)
        {
            if (string.IsNullOrWhiteSpace(dealerCodesCsv)) return;

            var incomingCodes = dealerCodesCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!incomingCodes.Any()) return;

            var others = (await _repo.Get())
                .Where(e => e.IsActive
                         && e.Id != excludeEmployeeId
                         && !string.IsNullOrWhiteSpace(e.DealerCode))
                .ToList();

            foreach (var other in others)
            {
                var existingCodes = (other.DealerCode ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();

                var hasConflict = existingCodes.Any(c => incomingCodes.Contains(c));
                if (!hasConflict) continue;

                var remaining = existingCodes
                    .Where(c => !incomingCodes.Contains(c))
                    .ToList();

                other.DealerCode = string.Join(",", remaining);
                other.UpdatedDate = DateTime.Now;
                other.UpdatedBy = "system";

                await _repo.Update(other);
            }
        }

        public Task<IEnumerable<BgEmployeeRoleMapping>> GetRoleMappings(int employeeId)
            => _repo.GetRoleMappings(employeeId);

        public Task<IEnumerable<AssignedDealerInfo>> GetAssignedDealerCodes(int excludeEmployeeId)
            => _repo.GetAssignedDealerCodes(excludeEmployeeId);

        public Task<IEnumerable<BgEmployeeListItemViewModel>> GetEmployeeListView()
            => _repo.GetEmployeeListView();

        // =====================================================
        // EXCEL EXPORT — Uses the same already-resolved
        // list-view data the grid displays (dealer names, zones,
        // job roles, reporting-to names — not raw ids), same
        // OpenXML approach as DealerMasterController/EmployeeController.
        // =====================================================

        public async Task<byte[]> DownloadBgEmployeeExcel()
        {
            var employees = (await _repo.GetEmployeeListView()).ToList();

            using var stream = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(stream, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "BgEmployees"
                });

                var headerRow = new Row();
                string[] headers =
                {
                    "Employee Code", "Employee Name", "Dealer Code", "Dealer Name",
                    "Zone", "Job Roles", "Reporting To", "Created By",
                    "Created On", "Updated On", "Status"
                };
                foreach (var h in headers)
                    headerRow.Append(CreateTextCell(h));
                sheetData.Append(headerRow);

                foreach (var e in employees)
                {
                    var row = new Row();
                    row.Append(CreateTextCell(e.EmployeeCode));
                    row.Append(CreateTextCell(e.EmployeeName));
                    row.Append(CreateTextCell(e.DealerCode));
                    row.Append(CreateTextCell(e.DealerName));
                    row.Append(CreateTextCell(e.Zone));
                    row.Append(CreateTextCell(e.JobRoles));
                    row.Append(CreateTextCell(e.ReportingTo));
                    row.Append(CreateTextCell(e.CreatedBy));
                    row.Append(CreateTextCell(e.CreatedDate.ToString("yyyy-MM-dd")));
                    row.Append(CreateTextCell(e.UpdatedDate?.ToString("yyyy-MM-dd") ?? ""));
                    row.Append(CreateTextCell(e.IsActive ? "Active" : "Inactive"));
                    sheetData.Append(row);
                }

                workbookPart.Workbook.Save();
            }

            return stream.ToArray();
        }

        private static Cell CreateTextCell(string? value)
        {
            return new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(value ?? string.Empty)
            };
        }
    }
}