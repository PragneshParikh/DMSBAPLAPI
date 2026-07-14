using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.BgEmployeeMasterRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        // TSM ERP LOOKUP (GET, pull) — currently 404ing upstream,
        // route/casing unconfirmed. Kept for once the real route
        // is found.
        // =====================================================

        public async Task<TsmEntryViewModel?> FetchTsmDetailsAsync(string tsmCode)
        {
            if (string.IsNullOrWhiteSpace(tsmCode))
                return null;

            var client = _httpClientFactory.CreateClient("TsmApi");
            var response = await client.GetAsync($"api/erptsmmaster/TSMEntry?tsmcode={Uri.EscapeDataString(tsmCode)}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return System.Text.Json.JsonSerializer.Deserialize<TsmEntryViewModel>(json, options);
        }

        public async Task<List<(string Attempt, int StatusCode, string Body)>> FetchTsmRawAsync(string tsmCode)
        {
            var client = _httpClientFactory.CreateClient("TsmApi");
            var results = new List<(string, int, string)>();

            var attempts = new (string Label, HttpRequestMessage Request)[]
            {
                ("GET-query", new HttpRequestMessage(HttpMethod.Get,
                    $"api/erptsmmaster/TSMEntry?tsmcode={Uri.EscapeDataString(tsmCode)}")),
                ("GET-path", new HttpRequestMessage(HttpMethod.Get,
                    $"api/erptsmmaster/TSMEntry/{Uri.EscapeDataString(tsmCode)}")),
                ("POST-body", new HttpRequestMessage(HttpMethod.Post, "api/erptsmmaster/TSMEntry")
                {
                    Content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(new { tsmcode = tsmCode }),
                        Encoding.UTF8, "application/json")
                }),
            };

            foreach (var (label, request) in attempts)
            {
                try
                {
                    var response = await client.SendAsync(request);
                    var body = await response.Content.ReadAsStringAsync();
                    results.Add((label, (int)response.StatusCode, body));
                }
                catch (Exception ex)
                {
                    results.Add((label, 0, ex.Message));
                }
            }

            return results;
        }

        // =====================================================
        // TSM ENTRY CONSUMER (POST, push) — external TSM system
        // sends data here; we upsert BgEmployeeMaster keyed on TsmCode.
        // =====================================================

       
        public async Task<BgEmployeeMaster> ConsumeTsmEntryAsync(TsmEntryPayload payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.TsmCode))
                throw new ArgumentException("tsmcode is required.");

            var existing = (await _repo.Get())
                .FirstOrDefault(e => e.TsmCode == payload.TsmCode);

            var model = new BgEmployeeViewModel
            {
                Id = existing?.Id ?? 0,
                TsmCode = payload.TsmCode,
                AreaOfficeId = payload.AreaOfficeId,
                Gender = payload.Gender,
                Mobile = payload.MobileNo,
                EmailId = payload.Email?.Trim().ToLowerInvariant(),
                ReportingTo = payload.TsmHeadCode,
                IsActive = payload.EStatus != "N",
                ProfileImage = string.IsNullOrWhiteSpace(payload.Photo) ? existing?.ProfileImage : payload.Photo,
                CreatedBy = existing?.CreatedBy ?? "TSM-Sync",
                UpdatedBy = "TSM-Sync",
                CreatedDate = existing?.CreatedDate ?? DateTime.Now,
                UpdatedDate = DateTime.Now,
                MappedZones = existing?.MappedZones ?? string.Empty,
                MappedZoneIds = existing?.MappedZoneIds ?? string.Empty,
                MappedEmployeeIds = existing?.MappedEmployeeIds,
                MappedEmployees = existing?.MappedEmployees,
                Pincode = existing?.Pincode,
                DealerCode = existing?.DealerCode,
                LocationCode = existing?.LocationCode,
                Department = existing?.Department,
                ProfileId = existing?.ProfileId,
                EmployeeCode = existing?.EmployeeCode,
                Email = existing?.Email,
                Password = existing?.Password,
            };

            // Split tsmname -> firstName/lastName
            var fullName = (payload.TsmName ?? string.Empty).Trim();
            var spaceIdx = fullName.IndexOf(' ');
            if (spaceIdx > -1)
            {
                model.FirstName = fullName.Substring(0, spaceIdx);
                model.LastName = fullName.Substring(spaceIdx + 1);
            }
            else
            {
                model.FirstName = fullName;
                model.LastName = string.Empty;
            }

            // Dates — now safely nullable end-to-end, no sentinel value needed
            model.DateOfJoin = ParseDate(payload.Doa) ?? existing?.DateOfJoin;
            model.DateOfBirth = ParseDate(payload.Dob) ?? existing?.DateOfBirth;
            model.EffectiveDate = ParseDate(payload.Doe) ?? existing?.EffectiveDate;

            // State/City: payload sends names, entity wants int? —
            // no lookup table wired yet, so preserve existing FK.
            // TODO: resolve payload.State / payload.City by name once
            // a State/City lookup service is available.
            model.State = existing?.State;
            model.City = existing?.City;

            if (existing != null)
            {
                await Update(model);
                return (await _repo.GetById(existing.Id))!;
            }
            else
            {
                return await Create(model);
            }
        }

        
        public async Task<BgEmployeeMaster> UpdateTsmEntryAsync(TsmEntryPayload payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.TsmCode))
                throw new ArgumentException("tsmcode is required.");

            var existing = (await _repo.Get())
                .FirstOrDefault(e => e.TsmCode == payload.TsmCode);

            var model = new BgEmployeeViewModel
            {
                Id = existing?.Id ?? 0,
                TsmCode = payload.TsmCode,
                AreaOfficeId = payload.AreaOfficeId,
                Gender = payload.Gender,
                Mobile = payload.MobileNo,
                EmailId = payload.Email?.Trim().ToLowerInvariant(),
                ReportingTo = payload.TsmHeadCode,
                IsActive = payload.EStatus != "N",
                ProfileImage = string.IsNullOrWhiteSpace(payload.Photo) ? existing?.ProfileImage : payload.Photo,
                CreatedBy = existing?.CreatedBy ?? "TSM-Sync",
                UpdatedBy = "TSM-Sync",
                CreatedDate = existing?.CreatedDate ?? DateTime.Now,
                UpdatedDate = DateTime.Now,
                MappedZones = existing?.MappedZones ?? string.Empty,
                MappedZoneIds = existing?.MappedZoneIds ?? string.Empty,
                MappedEmployeeIds = existing?.MappedEmployeeIds,
                MappedEmployees = existing?.MappedEmployees,
                Pincode = existing?.Pincode,
                DealerCode = existing?.DealerCode,
                LocationCode = existing?.LocationCode,
                Department = existing?.Department,
                ProfileId = existing?.ProfileId,
                EmployeeCode = existing?.EmployeeCode,
                Email = existing?.Email,
                Password = existing?.Password,
            };

            // Split tsmname -> firstName/lastName
            var fullName = (payload.TsmName ?? string.Empty).Trim();
            var spaceIdx = fullName.IndexOf(' ');
            if (spaceIdx > -1)
            {
                model.FirstName = fullName.Substring(0, spaceIdx);
                model.LastName = fullName.Substring(spaceIdx + 1);
            }
            else
            {
                model.FirstName = fullName;
                model.LastName = string.Empty;
            }

            // Dates — now safely nullable end-to-end, no sentinel value needed
            model.DateOfJoin = ParseDate(payload.Doa) ?? existing?.DateOfJoin;
            model.DateOfBirth = ParseDate(payload.Dob) ?? existing?.DateOfBirth;
            model.EffectiveDate = ParseDate(payload.Doe) ?? existing?.EffectiveDate;

            // State/City: payload sends names, entity wants int? —
            // no lookup table wired yet, so preserve existing FK.
            // TODO: resolve payload.State / payload.City by name once
            // a State/City lookup service is available.
            model.State = existing?.State;
            model.City = existing?.City;

            if (existing != null)
            {
                await Update(model);
                return (await _repo.GetById(existing.Id))!;
            }
            else
            {
                return await Create(model);
            }
        }

        private static DateTime? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return DateTime.TryParseExact(value, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? result
                : (DateTime?)null;
        }

        // =====================================================
        // GET / GET BY ID
        // =====================================================

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

        // =====================================================
        // CREATE
        // =====================================================

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

        // =====================================================
        // UPDATE
        // =====================================================

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
                TsmCode = model.TsmCode,
                AreaOfficeId = model.AreaOfficeId,
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
                MappedEmployeeIds = model.MappedEmployeeIds,
                MappedEmployees = model.MappedEmployees,
                ProfileImage = model.ProfileImage,
                DealerCode = model.DealerCode,
                LocationCode = model.LocationCode,
                CreatedBy = model.CreatedBy,
                CreatedDate = model.CreatedDate,
                UpdatedBy = model.UpdatedBy,
                UpdatedDate = model.UpdatedDate,
            };
        }

        private async Task UnassignConflictingDealers(string? dealerCodesCsv, int excludeEmployeeId)
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
        // EXCEL EXPORT
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