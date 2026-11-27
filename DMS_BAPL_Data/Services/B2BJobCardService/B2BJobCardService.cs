using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.JobCardScanner;
using DMS_BAPL_Data.Services.JobCardServiceHistoryImportService;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS_BAPL_Data.Services.B2BJobCardService
{
    /// <summary>
    /// B2B Job Card (24.11.2026) - reads and writes the JobCardScanner database through JobCardScannerDbContext.
    ///
    /// Source rules reproduced (JobCardScanner backend/JobCardScanner.Api):
    ///  - Controllers/JobCardsController.cs Create: vehicle must exist; one open job card per chassis per dealer;
    ///    first active WorkflowStage (dealer-specific or global, lowest Seq); JobCardStageHistory row; vehicle
    ///    odometer = max(old, new); complaints saved as JobCardComplaints (IsCustomerVoice = true).
    ///  - Services/JobCardNumberingService.cs: JC/{last 3 digits of dealer code}/{FY yy-yy}/{0001} per dealer and
    ///    Indian financial year, counter dbo.Counters (CounterType "JobCard-FY{yy-yy}").
    ///  - web/mobile JobCardWizard: Job Type -> Service Head -> Priority from dbo.ServiceMenuMasters (posted priority =
    ///    PriorityLabel), complaints from dbo.ComplaintMasters, workshop from dbo.WorkshopLocations, ServiceType / Source
    ///    mapped from the DMS job type / job source name, odometer must be greater than the previous reading,
    ///    expected delivery required, at least one complaint.
    ///  - Controllers/JobCardsController.cs Update: only Priority, ExpectedDeliveryAt and AssignedTechnicianName.
    /// Not reproduced (stay in the JobCardScanner app): photos (blob upload), ERP push, SMS, workflow stage moves.
    /// </summary>
    public class B2BJobCardService : IB2BJobCardService
    {
        private const int MaxHistory = 200;
        private const int SuggestLimit = 20;
        private static readonly TimeSpan Ist = TimeSpan.FromMinutes(330);

        private readonly JobCardScannerDbContext _db;
        private readonly BapldmsvadContext _dms;
        private readonly IJobCardServiceHistoryReportService _history;
        private readonly ILogger<B2BJobCardService> _logger;

        public B2BJobCardService(JobCardScannerDbContext db, BapldmsvadContext dms, IJobCardServiceHistoryReportService history,
            ILogger<B2BJobCardService> logger)
        {
            _db = db;
            _dms = dms;
            _history = history;
            _logger = logger;
        }

        // ===================================================================== dealer
        private static string Norm(string? s) => Regex.Replace(s ?? "", @"[^A-Za-z0-9]", "").ToUpperInvariant();

        private async Task<JcsDealer?> FindDealerAsync(string code, CancellationToken ct)
        {
            code = code.Trim();
            var rows = await _db.Dealers.AsNoTracking()
                .Where(d => d.BaplDmsDealerCode == code || d.Code == code)
                .ToListAsync(ct);
            return rows.OrderBy(d => string.Equals(d.BaplDmsDealerCode, code, StringComparison.OrdinalIgnoreCase) ? 0 : 1).FirstOrDefault();
        }

        /// <summary>Dealer users always get their own dealer (token); Admin / SuperAdmin get the dealer they picked.</summary>
        private async Task<JcsDealer?> ResolveDealerAsync(B2BCaller caller, string? requested, bool required, CancellationToken ct)
        {
            var code = caller.IsAdmin ? requested : caller.DealerCode;
            if (string.IsNullOrWhiteSpace(code))
            {
                if (required) throw new B2BValidationException(caller.IsAdmin ? "Select the dealer." : "Your login has no dealer code.");
                return null;
            }
            var dealer = await FindDealerAsync(code, ct);
            if (dealer == null && required)
                throw new B2BValidationException($"Dealer {code.Trim()} is not set up in JobCardScanner (dbo.Dealers.BaplDmsDealerCode). Ask the JobCardScanner admin to import this dealer.");
            return dealer;
        }

        private static B2BDealerDto ToDto(JcsDealer d) => new()
        {
            Id = d.Id, Code = d.Code, Name = d.Name, DmsDealerCode = d.BaplDmsDealerCode, City = d.City
        };

        public async Task<List<B2BDealerDto>> GetDealersAsync(CancellationToken ct)
        {
            var rows = await _db.Dealers.AsNoTracking()
                .Where(d => d.BaplDmsDealerCode != null && d.BaplDmsDealerCode != "")
                .OrderBy(d => d.Name).ToListAsync(ct);
            return rows.Select(ToDto).ToList();
        }

        // ===================================================================== masters
        private async Task<List<JcsServiceMenuMaster>> MenuRowsAsync(CancellationToken ct) =>
            await _db.ServiceMenuMasters.AsNoTracking().Where(r => r.IsActive)
                .OrderBy(r => r.SortOrder).ThenBy(r => r.JobTypeName).ThenBy(r => r.ServiceHeadName).ThenBy(r => r.PriorityValue)
                .ToListAsync(ct);

        public async Task<List<B2BIdNameDto>> GetJobTypesAsync(CancellationToken ct) =>
            (await MenuRowsAsync(ct)).GroupBy(r => r.JobTypeId)
                .Select(g => new B2BIdNameDto { Id = g.Key, Name = g.First().JobTypeName }).ToList();

        public async Task<List<B2BIdNameDto>> GetServiceHeadsAsync(int jobTypeId, CancellationToken ct) =>
            (await MenuRowsAsync(ct)).Where(r => r.JobTypeId == jobTypeId).GroupBy(r => r.ServiceHeadId)
                .Select(g => new B2BIdNameDto { Id = g.Key, Name = g.First().ServiceHeadName }).ToList();

        public async Task<List<B2BPriorityDto>> GetPrioritiesAsync(int jobTypeId, int serviceHeadId, CancellationToken ct) =>
            (await MenuRowsAsync(ct)).Where(r => r.JobTypeId == jobTypeId && r.ServiceHeadId == serviceHeadId)
                .GroupBy(r => r.PriorityLabel, StringComparer.OrdinalIgnoreCase)
                .Select(g => new B2BPriorityDto { Value = g.First().PriorityLabel, Label = g.First().PriorityValue }).ToList();

        public async Task<List<B2BComplaintDto>> GetComplaintsAsync(CancellationToken ct) =>
            await _db.ComplaintMasters.AsNoTracking().Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.ComplaintText)
                .Select(c => new B2BComplaintDto { Id = c.Id, ComplaintText = c.ComplaintText, SortOrder = c.SortOrder })
                .ToListAsync(ct);

        private static int WorkshopNumber(string code)
        {
            var m = Regex.Match(code ?? "", @"W(\d+)$", RegexOptions.IgnoreCase);
            return m.Success && int.TryParse(m.Groups[1].Value, out var n) ? n : int.MaxValue;
        }

        private async Task<List<B2BWorkshopDto>> WorkshopsOfAsync(Guid dealerId, CancellationToken ct)
        {
            var rows = await _db.WorkshopLocations.AsNoTracking().Where(w => w.DealerId == dealerId && w.IsActive).ToListAsync(ct);
            return rows.OrderBy(w => WorkshopNumber(w.LocCode)).ThenBy(w => w.LocCode)
                .Select(w => new B2BWorkshopDto { LocCode = w.LocCode, LocName = w.LocName }).ToList();
        }

        public async Task<List<B2BWorkshopDto>> GetWorkshopsAsync(B2BCaller caller, string? dealerCode, CancellationToken ct)
        {
            var dealer = await ResolveDealerAsync(caller, dealerCode, true, ct);
            return await WorkshopsOfAsync(dealer!.Id, ct);
        }

        /// <summary>
        /// Supervisor/Advisor and Technician dropdowns, like the JobCardScanner wizard (TechniciansController):
        /// supervisors = active dbo.Users of the dealer with Role "Supervisor" whose WorkLocationCodes is empty or contains
        /// the workshop; technicians = active dbo.Technicians of the dealer at the workshop.
        /// </summary>
        public async Task<B2BStaffListsDto> GetStaffAsync(B2BCaller caller, string? dealerCode, string? locationCode, CancellationToken ct)
        {
            var dealer = await ResolveDealerAsync(caller, dealerCode, true, ct);
            var loc = (locationCode ?? "").Trim();

            var users = await _db.Users.AsNoTracking()
                .Where(u => u.DealerId == dealer!.Id && u.Role == "Supervisor" && u.Active)
                .OrderBy(u => u.Name)
                .Select(u => new { u.Id, u.Name, u.WorkLocationCodes })
                .ToListAsync(ct);
            // every active supervisor / technician of the dealer; the ones of the selected workshop first (AtLocation).
            // (The JobCardScanner job card page lists all of them too, so a technician set up at another workshop is still offered.)
            bool SupervisorAt(string? json)
            {
                if (loc.Length == 0 || string.IsNullOrWhiteSpace(json)) return true;
                try
                {
                    var codes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                    return codes.Count == 0 || codes.Contains(loc, StringComparer.OrdinalIgnoreCase);
                }
                catch { return true; }   // same tolerance as the source
            }
            var supervisors = users.Select(u => new B2BStaffDto { Id = u.Id, Name = u.Name, AtLocation = SupervisorAt(u.WorkLocationCodes) })
                .OrderByDescending(x => x.AtLocation).ThenBy(x => x.Name).ToList();

            var techRows = await _db.Technicians.AsNoTracking().Where(t => t.DealerId == dealer!.Id && t.Active)
                .OrderBy(t => t.Name)
                .Select(t => new { t.Id, t.Name, t.LocationCode, t.LocationName })
                .ToListAsync(ct);
            var technicians = techRows.Select(t => new B2BStaffDto
            {
                Id = t.Id,
                Name = t.Name,
                AtLocation = loc.Length == 0 || string.Equals((t.LocationCode ?? "").Trim(), loc, StringComparison.OrdinalIgnoreCase),
                LocationName = string.IsNullOrWhiteSpace(t.LocationName) ? t.LocationCode : t.LocationName
            }).OrderByDescending(x => x.AtLocation).ThenBy(x => x.Name).ToList();

            return new B2BStaffListsDto { Supervisors = supervisors, Technicians = technicians };
        }

        public async Task<B2BMastersDto> GetMastersAsync(B2BCaller caller, string? dealerCode, CancellationToken ct)
        {
            var result = new B2BMastersDto
            {
                ServiceMenu = (await MenuRowsAsync(ct)).Select(r => new B2BServiceMenuRowDto
                {
                    Id = r.Id, JobTypeId = r.JobTypeId, JobTypeName = r.JobTypeName, ServiceHeadId = r.ServiceHeadId,
                    ServiceHeadName = r.ServiceHeadName, PriorityValue = r.PriorityValue, PriorityLabel = r.PriorityLabel, SortOrder = r.SortOrder
                }).ToList(),
                Complaints = await GetComplaintsAsync(ct)
            };
            try
            {
                var dealer = await ResolveDealerAsync(caller, dealerCode, false, ct);
                if (dealer == null)
                {
                    var code = caller.IsAdmin ? dealerCode : caller.DealerCode;
                    result.DealerMessage = string.IsNullOrWhiteSpace(code)
                        ? "Select the dealer."
                        : $"Dealer {code} is not set up in JobCardScanner (dbo.Dealers.BaplDmsDealerCode).";
                    return result;
                }
                result.Dealer = ToDto(dealer);
                result.Workshops = await WorkshopsOfAsync(dealer.Id, ct);
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "B2B masters: WorkshopLocations could not be read");
                result.DealerMessage = "JobCardScanner WorkshopLocations could not be read: " + ex.Message;
            }
            return result;
        }

        // ===================================================================== vehicles
        /// <summary>
        /// Type-ahead like the JobCardScanner wizard (GET /api/jobcards/vehicle-suggestions): from 2 characters, "contains"
        /// on chassis or reg no. First the caller's dealer (JobCardScanner vehicles of the dealer + DMS sales of the dealer);
        /// when nothing matches there, every dealer (JobCardScanner vehicles + DMS sales + imported service history).
        /// </summary>
        public async Task<B2BVehicleSuggestResultDto> SuggestVehiclesAsync(B2BCaller caller, string? term, string? dealerCode, CancellationToken ct)
        {
            var t = Norm(term);
            var result = new B2BVehicleSuggestResultDto();
            if (t.Length < 2) return result;

            var dealer = await ResolveDealerAsync(caller, dealerCode, false, ct);
            var dmsDealerCode = (caller.IsAdmin ? dealerCode : caller.DealerCode)?.Trim();

            if (dealer != null || !string.IsNullOrWhiteSpace(dmsDealerCode))
            {
                result.Items = Merge(
                    dealer != null ? await JcsHitsAsync(t, dealer.Id, ct) : new List<B2BVehicleSuggestionDto>(),
                    !string.IsNullOrWhiteSpace(dmsDealerCode) ? await DmsSaleHitsAsync(t, dmsDealerCode, ct) : new List<B2BVehicleSuggestionDto>(),
                    new List<B2BVehicleSuggestionDto>());
                if (result.Items.Count > 0) { result.Scope = "dealer"; return result; }
            }

            result.Scope = "all";
            result.Items = Merge(await JcsHitsAsync(t, null, ct), await DmsSaleHitsAsync(t, null, ct), await HistoryHitsAsync(t, ct));
            return result;
        }

        private static List<B2BVehicleSuggestionDto> Merge(params List<B2BVehicleSuggestionDto>[] lists)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var outList = new List<B2BVehicleSuggestionDto>();
            foreach (var list in lists)
                foreach (var x in list)
                {
                    var key = Norm(x.ChassisNo);
                    if (key.Length == 0 || !seen.Add(key)) continue;
                    outList.Add(x);
                    if (outList.Count >= 30) return outList;
                }
            return outList;
        }

        private async Task<List<B2BVehicleSuggestionDto>> JcsHitsAsync(string t, Guid? dealerId, CancellationToken ct)
        {
            var q = _db.Vehicles.AsNoTracking().Where(v =>
                (v.Vin != null && v.Vin.Contains(t)) ||
                (v.RegNo != null && v.RegNo.Replace(" ", "").Replace("-", "").Contains(t)));
            if (dealerId.HasValue) q = q.Where(v => v.DealerId == dealerId.Value);
            return await q.OrderByDescending(v => v.CreatedAt).Take(SuggestLimit)
                .Select(v => new B2BVehicleSuggestionDto
                {
                    VehicleId = v.Id,
                    Source = "JobCardScanner",
                    ChassisNo = v.Vin,
                    RegNo = v.RegNo,
                    Model = v.Model,
                    CustomerName = v.Customer != null ? v.Customer.Name : null,
                    DealerCode = v.Dealer != null ? (v.Dealer.BaplDmsDealerCode ?? v.Dealer.Code) : null,
                    Jobs = _db.JobCards.Count(j => j.VehicleId == v.Id)
                })
                .ToListAsync(ct);
        }

        /// <summary>DMS vehicle sales (BAPLDMSvad VehicleSaleBillDetails + VehicleSaleBillHeader) - the DMS side of the wizard's DMS_SaleBill search</summary>
        private async Task<List<B2BVehicleSuggestionDto>> DmsSaleHitsAsync(string t, string? dmsDealerCode, CancellationToken ct)
        {
            try
            {
                var q = from d in _dms.VehicleSaleBillDetails.AsNoTracking()
                        join h in _dms.VehicleSaleBillHeaders.AsNoTracking() on d.VehicleSaleBillId equals h.Id
                        where !h.IsDeleted && (d.ChassisNo.Contains(t) || (d.RegNo != null && d.RegNo.Replace(" ", "").Replace("-", "").Contains(t)))
                        select new { d.ChassisNo, d.RegNo, d.ModelName, d.Colour, h.CustomerName, h.DealerCode, h.SaleDate, h.Id };
                if (!string.IsNullOrWhiteSpace(dmsDealerCode)) q = q.Where(x => x.DealerCode == dmsDealerCode);
                var rows = await q.OrderByDescending(x => x.Id).Take(SuggestLimit * 3).ToListAsync(ct);
                return rows.Select(x => new B2BVehicleSuggestionDto
                {
                    Source = "DMS sale",
                    ChassisNo = x.ChassisNo,
                    RegNo = x.RegNo,
                    Model = string.IsNullOrWhiteSpace(x.Colour) ? x.ModelName : $"{x.ModelName} ({x.Colour})",
                    CustomerName = x.CustomerName,
                    DealerCode = x.DealerCode,
                    SaleDate = x.SaleDate
                }).ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "B2B vehicle suggest: DMS sale search failed");
                return new List<B2BVehicleSuggestionDto>();
            }
        }

        /// <summary>Imported service history (BAPLDMSvad dbo.JobCardServiceHistory, every dealer)</summary>
        private async Task<List<B2BVehicleSuggestionDto>> HistoryHitsAsync(string t, CancellationToken ct)
        {
            try
            {
                var rows = await _history.SuggestAsync(t, ct);
                return rows.Where(r => !string.IsNullOrWhiteSpace(r.ChassisNo)).Select(r => new B2BVehicleSuggestionDto
                {
                    Source = "Service history",
                    ChassisNo = r.ChassisNo,
                    RegNo = r.RegNo,
                    Model = r.Model,
                    DealerCode = r.DealerCode,
                    Jobs = r.Jobs
                }).ToList();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "B2B vehicle suggest: service history search failed");
                return new List<B2BVehicleSuggestionDto>();
            }
        }

        private static B2BVehicleDto ToDto(JcsVehicle v) => new()
        {
            Id = v.Id, ChassisNo = v.Vin, RegNo = v.RegNo, Model = v.Model, Variant = v.Variant, Color = v.Color,
            BatteryNo = v.BatteryNo, MotorNo = v.MotorNo, ControllerNo = v.ControllerNo, ConverterNo = v.ConverterNo,
            ChargerNo = v.ChargerNo, PurchaseDate = v.PurchaseDate, NextServiceDueDate = v.NextServiceDueDate,
            InsuranceExpiry = v.InsuranceExpiry, Odometer = v.Odometer,
            DealerCode = v.Dealer != null ? (v.Dealer.BaplDmsDealerCode ?? v.Dealer.Code) : null
        };

        private static B2BCustomerDto ToDto(JcsCustomer? c) => c == null ? new B2BCustomerDto() : new()
        {
            Id = c.Id, Name = c.Name, Mobile = c.Mobile, Email = c.Email, Address = c.Address, City = c.City, State = c.State
        };

        /// <summary>All job cards (any dealer) of every vehicle row with this chassis, newest first.</summary>
        private async Task<List<B2BHistoryRowDto>> HistoryAsync(string chassis, CancellationToken ct)
        {
            var raw = await _db.JobCards.AsNoTracking()
                .Where(j => j.Vehicle!.Vin == chassis)
                .OrderByDescending(j => j.CreatedAt).Take(MaxHistory)
                .Select(j => new
                {
                    St = j.Status,
                    Pr = j.Priority,
                    Row = new B2BHistoryRowDto
                {
                    Id = j.Id,
                    JobCardNumber = j.JobCardNumber,
                    CreatedAt = j.CreatedAt,
                    RegNo = j.Vehicle!.RegNo,
                    OdometerAtCheckIn = j.OdometerAtCheckIn,
                    JobType = j.BaplJobType,
                    ServiceHead = j.BaplServiceHeadName,
                    ServiceLocation = j.BaplServiceLocation,
                    Stage = j.CurrentStage != null ? j.CurrentStage.Label : null,
                    DealerCode = j.Dealer != null ? (j.Dealer.BaplDmsDealerCode ?? j.Dealer.Code) : null,
                    DealerName = j.Dealer != null ? j.Dealer.Name : null
                }
                })
                .ToListAsync(ct);
            var rows = raw.Select(x => { x.Row.Status = x.St.ToString(); x.Row.Priority = x.Pr.ToString(); return x.Row; }).ToList();
            await FillComplaintsAsync(rows.Select(r => r.Id).ToList(), (id, text) => rows.First(r => r.Id == id).Complaints = text, ct);
            return rows;
        }

        private async Task FillComplaintsAsync(List<Guid> ids, Action<Guid, string> set, CancellationToken ct)
        {
            if (ids.Count == 0) return;
            var list = await _db.JobCardComplaints.AsNoTracking().Where(c => ids.Contains(c.JobCardId))
                .OrderBy(c => c.CreatedAt).Select(c => new { c.JobCardId, c.Description }).ToListAsync(ct);
            foreach (var g in list.GroupBy(c => c.JobCardId))
                set(g.Key, string.Join(", ", g.Select(c => c.Description)));
        }

        private async Task<double?> PreviousOdometerAsync(string chassis, CancellationToken ct)
        {
            var lastJob = await _db.JobCards.AsNoTracking().Where(j => j.Vehicle!.Vin == chassis)
                .OrderByDescending(j => j.CreatedAt).Select(j => (double?)j.OdometerAtCheckIn).FirstOrDefaultAsync(ct);
            var vehicleMax = await _db.Vehicles.AsNoTracking().Where(v => v.Vin == chassis)
                .Select(v => (double?)v.Odometer).MaxAsync(ct);
            var prev = Math.Max(lastJob ?? 0, vehicleMax ?? 0);
            return prev > 0 ? prev : (lastJob.HasValue ? 0 : null);
        }

        private Task<string?> OpenJobCardNumberAsync(Guid dealerId, string chassis, CancellationToken ct) =>
            _db.JobCards.AsNoTracking()
                .Where(j => j.DealerId == dealerId && j.Status != JcsJobCardStatus.Closed && j.Status != JcsJobCardStatus.Cancelled)
                .Where(j => j.Vehicle!.Vin == chassis)
                .Select(j => j.JobCardNumber).FirstOrDefaultAsync(ct);

        public async Task<B2BVehicleCardDto> GetVehicleAsync(B2BCaller caller, string? value, string? dealerCode, CancellationToken ct)
        {
            var v = Norm(value);
            if (v.Length < 3) throw new B2BValidationException("Enter the Chassis No / Reg No.");
            var dealer = await ResolveDealerAsync(caller, dealerCode, false, ct);

            var matches = await _db.Vehicles.AsNoTracking().Include(x => x.Customer).Include(x => x.Dealer)
                .Where(x => x.Vin == v || (x.RegNo != null && x.RegNo.Replace(" ", "").Replace("-", "") == v))
                .ToListAsync(ct);
            if (matches.Count == 0) return new B2BVehicleCardDto { Found = false };

            var vehicle = matches
                .OrderBy(x => dealer != null && x.DealerId == dealer.Id ? 0 : 1)
                .ThenByDescending(x => x.CreatedAt).First();
            var chassis = vehicle.Vin ?? "";

            var card = new B2BVehicleCardDto
            {
                Found = true,
                Vehicle = ToDto(vehicle),
                Customer = ToDto(vehicle.Customer)
            };
            if (chassis.Length > 0)
            {
                card.History = await HistoryAsync(chassis, ct);
                card.PreviousOdometer = await PreviousOdometerAsync(chassis, ct);
                if (dealer != null) card.OpenJobCardNumber = await OpenJobCardNumberAsync(dealer.Id, chassis, ct);
            }
            return card;
        }

        // ===================================================================== list / detail
        public async Task<B2BPagedDto<B2BJobCardListRowDto>> ListAsync(B2BCaller caller, B2BJobCardFilter f, CancellationToken ct)
        {
            var q = _db.JobCards.AsNoTracking().AsQueryable();
            if (!caller.IsAdmin || !string.IsNullOrWhiteSpace(f.DealerCode))
            {
                var dealer = await ResolveDealerAsync(caller, f.DealerCode, true, ct);
                q = q.Where(j => j.DealerId == dealer!.Id);
            }
            // dates are IST calendar days; JobCards.CreatedAt is UTC
            if (f.FromDate.HasValue) { var from = f.FromDate.Value.Date - Ist; q = q.Where(j => j.CreatedAt >= from); }
            if (f.ToDate.HasValue) { var to = f.ToDate.Value.Date.AddDays(1) - Ist; q = q.Where(j => j.CreatedAt < to); }
            if (!string.IsNullOrWhiteSpace(f.JobNo)) { var s = f.JobNo.Trim(); q = q.Where(j => j.JobCardNumber.Contains(s) || (j.BaplManualJobNo != null && j.BaplManualJobNo.Contains(s))); }
            if (!string.IsNullOrWhiteSpace(f.ChassisNo)) { var s = Norm(f.ChassisNo); q = q.Where(j => j.Vehicle!.Vin != null && j.Vehicle.Vin.Contains(s)); }
            if (!string.IsNullOrWhiteSpace(f.RegNo)) { var s = Norm(f.RegNo); q = q.Where(j => j.Vehicle!.RegNo != null && j.Vehicle.RegNo.Replace(" ", "").Replace("-", "").Contains(s)); }
            if (!string.IsNullOrWhiteSpace(f.CustomerName)) { var s = f.CustomerName.Trim(); q = q.Where(j => j.Customer!.Name.Contains(s)); }
            if (!string.IsNullOrWhiteSpace(f.ServiceLocationCode)) { var s = f.ServiceLocationCode.Trim(); q = q.Where(j => j.BaplServiceLocationCode == s); }
            if (!string.IsNullOrWhiteSpace(f.Status))
            {
                if (!Enum.TryParse<JcsJobCardStatus>(f.Status.Trim(), true, out var st)) throw new B2BValidationException($"Unknown status {f.Status}.");
                q = q.Where(j => j.Status == st);
            }

            var pageSize = Math.Clamp(f.PageSize <= 0 ? 10 : f.PageSize, 1, 500);
            var pageIndex = Math.Max(1, f.PageIndex);
            var total = await q.CountAsync(ct);
            var raw = await q.OrderByDescending(j => j.CreatedAt)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(j => new
                {
                    St = j.Status,
                    Pr = j.Priority,
                    Row = new B2BJobCardListRowDto
                {
                    Id = j.Id,
                    JobCardNumber = j.JobCardNumber,
                    CreatedAt = j.CreatedAt,
                    UpdatedAt = j.UpdatedAt,
                    Stage = j.CurrentStage != null ? j.CurrentStage.Label : null,
                    ManualJobNo = j.BaplManualJobNo,
                    ServiceLocation = j.BaplServiceLocation,
                    JobType = j.BaplJobType,
                    JobSource = j.BaplJobSourceName,
                    ServiceHead = j.BaplServiceHeadName,
                    Supervisor = j.BaplSupervisorName,
                    Technician = j.AssignedTechnicianName ?? j.BaplTechnicianName,
                    ChassisNo = j.Vehicle!.Vin,
                    RegNo = j.Vehicle.RegNo,
                    Model = j.Vehicle.Model,
                    CustomerName = j.Customer!.Name,
                    CustomerMobile = j.Customer.Mobile,
                    OdometerAtCheckIn = j.OdometerAtCheckIn,
                    DealerCode = j.Dealer != null ? (j.Dealer.BaplDmsDealerCode ?? j.Dealer.Code) : null
                }
                })
                .ToListAsync(ct);
            var rows = raw.Select(x => { x.Row.Status = x.St.ToString(); x.Row.Priority = x.Pr.ToString(); return x.Row; }).ToList();
            await FillComplaintsAsync(rows.Select(r => r.Id).ToList(), (id, text) => rows.First(r => r.Id == id).Complaints = text, ct);
            return new B2BPagedDto<B2BJobCardListRowDto> { Data = rows, TotalRecords = total, PageIndex = pageIndex, PageSize = pageSize };
        }

        private async Task<JcsJobCard> LoadForCallerAsync(B2BCaller caller, Guid id, bool tracking, CancellationToken ct)
        {
            var q = tracking ? _db.JobCards : _db.JobCards.AsNoTracking();
            var jc = await q.Include(j => j.Vehicle).ThenInclude(v => v!.Dealer)
                .Include(j => j.Customer).Include(j => j.Dealer).Include(j => j.CurrentStage).Include(j => j.Complaints)
                .FirstOrDefaultAsync(j => j.Id == id, ct);
            if (jc == null) throw new B2BNotFoundException("B2B job card not found.");
            if (!caller.IsAdmin)
            {
                var dealer = await ResolveDealerAsync(caller, null, true, ct);
                if (jc.DealerId != dealer!.Id) throw new B2BNotFoundException("B2B job card not found.");
            }
            return jc;
        }

        private static bool Editable(JcsJobCardStatus s) => s != JcsJobCardStatus.Closed && s != JcsJobCardStatus.Cancelled;

        public async Task<B2BJobCardDetailDto> GetAsync(B2BCaller caller, Guid id, CancellationToken ct)
        {
            var j = await LoadForCallerAsync(caller, id, false, ct);
            return new B2BJobCardDetailDto
            {
                Id = j.Id,
                JobCardNumber = j.JobCardNumber,
                Status = j.Status.ToString(),
                Stage = j.CurrentStage?.Label,
                CanEdit = Editable(j.Status),
                DealerCode = j.Dealer?.BaplDmsDealerCode ?? j.Dealer?.Code,
                DealerName = j.Dealer?.Name,
                JobTypeId = j.BaplJobTypeId,
                JobTypeName = j.BaplJobType,
                ServiceHeadId = j.BaplServiceHeadId,
                ServiceHeadName = j.BaplServiceHeadName,
                Priority = j.Priority.ToString(),
                ServiceLocationCode = j.BaplServiceLocationCode,
                ServiceLocationName = j.BaplServiceLocation,
                JobSourceId = j.BaplJobSourceId,
                JobSourceName = j.BaplJobSourceName,
                ManualJobNo = j.BaplManualJobNo,
                CouponNo = j.BaplCouponNo,
                SupervisorName = j.BaplSupervisorName,
                TechnicianName = j.BaplTechnicianName,
                AssignedTechnicianName = j.AssignedTechnicianName,
                OdometerAtCheckIn = j.OdometerAtCheckIn,
                BatteryLevelAtCheckIn = j.BatteryLevelAtCheckIn,
                ExpectedDeliveryAt = j.ExpectedDeliveryAt,
                CustomerVoice = j.CustomerConsentNotes,
                CreatedAt = j.CreatedAt,
                UpdatedAt = j.UpdatedAt,
                Complaints = j.Complaints.OrderBy(c => c.CreatedAt).Select(c => c.Description).ToList(),
                Photos = await PhotosOfAsync(j.Id, ct),
                Vehicle = j.Vehicle != null ? ToDto(j.Vehicle) : new B2BVehicleDto(),
                Customer = ToDto(j.Customer)
            };
        }

        // ===================================================================== create
        /// <summary>JobCardScanner web/mobile wizard mapBaplJobTypeToServiceType</summary>
        private static JcsServiceType MapJobType(string name) => name.Trim().ToLowerInvariant() switch
        {
            "pdi" => JcsServiceType.Pdi,
            "accidental" => JcsServiceType.AccidentRepair,
            "in warranty period" => JcsServiceType.Warranty,
            _ => JcsServiceType.PaidService
        };

        /// <summary>JobCardScanner web/mobile wizard mapBaplJobSourceToSource</summary>
        private static JcsJobCardSource MapJobSource(string? name) => (name ?? "").Trim().ToLowerInvariant() switch
        {
            "rsa" => JcsJobCardSource.Breakdown,
            "mega camp" => JcsJobCardSource.Scheduled,
            _ => JcsJobCardSource.WalkIn
        };

        private static string? Clean(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length > max ? s.Substring(0, max) : s;
        }

        private static DateTime IndianNow() => DateTime.UtcNow + Ist;

        private static string FyLabel()
        {
            var now = IndianNow();
            var start = now.Month >= 4 ? now.Year : now.Year - 1;
            return $"{start % 100:D2}-{(start + 1) % 100:D2}";
        }

        /// <summary>
        /// Same number as JobCardNumberingService.NextJobCardNumberAsync, but the counter row is incremented under an
        /// update lock inside the caller's transaction, so two saves cannot get the same number.
        /// </summary>
        private async Task<string> NextJobCardNumberAsync(JcsDealer dealer, CancellationToken ct)
        {
            var dealerCode = dealer.BaplDmsDealerCode ?? dealer.Code ?? "000";
            var digits = new string(dealerCode.Where(char.IsDigit).ToArray());
            var src = digits.Length > 0 ? digits : dealerCode;
            var last3 = src.Length >= 3 ? src[^3..] : src.PadLeft(3, '0');
            var fy = FyLabel();
            var counterType = $"JobCard-FY{fy}";

            for (var attempt = 0; attempt < 3; attempt++)
            {
                var updated = await _db.Database.SqlQueryRaw<int>(
                    "UPDATE [dbo].[Counters] WITH (UPDLOCK, ROWLOCK) SET [CurrentValue] = [CurrentValue] + 1 " +
                    "OUTPUT inserted.[CurrentValue] AS [Value] WHERE [DealerId] = @d AND [CounterType] = @t",
                    new SqlParameter("@d", dealer.Id), new SqlParameter("@t", counterType)).ToListAsync(ct);
                if (updated.Count > 0) return $"JC/{last3}/{fy}/{updated[0]:D4}";
                try
                {
                    await _db.Database.ExecuteSqlRawAsync(
                        "INSERT INTO [dbo].[Counters] ([Id], [DealerId], [CounterType], [Prefix], [CurrentValue]) VALUES (NEWID(), @d, @t, @p, 1)",
                        new object[] { new SqlParameter("@d", dealer.Id), new SqlParameter("@t", counterType), new SqlParameter("@p", last3) }, ct);
                    return $"JC/{last3}/{fy}/{1:D4}";
                }
                catch (SqlException ex) when (ex.Number is 2601 or 2627)
                {
                    // another save created the counter row at the same moment - increment it instead
                }
            }
            throw new InvalidOperationException("Could not allocate a job card number. Please try again.");
        }

        private static bool IsDuplicateJobCardNumber(Exception ex) =>
            ex.GetBaseException() is SqlException sql && (sql.Number == 2601 || sql.Number == 2627)
            && sql.Message.Contains("JobCardNumber", StringComparison.OrdinalIgnoreCase);

        public async Task<B2BSaveResultDto> CreateAsync(B2BCaller caller, B2BJobCardCreateRequest req, CancellationToken ct)
        {
            if (req == null) throw new B2BValidationException("Nothing to save.");
            var dealer = (await ResolveDealerAsync(caller, req.DealerCode, true, ct))!;

            // ---- vehicle / chassis
            var chassis = Norm(req.ChassisNo);
            if (chassis.Length < 5) throw new B2BValidationException("Chassis No is required.");
            if (chassis.Length > 50) throw new B2BValidationException("Chassis No is too long.");

            // ---- service details (Service Menu Master cascade)
            if (req.JobTypeId is null or <= 0) throw new B2BValidationException("Job Type is required.");
            if (req.ServiceHeadId is null or <= 0) throw new B2BValidationException("Service Head is required.");
            if (string.IsNullOrWhiteSpace(req.Priority)) throw new B2BValidationException("Priority is required.");
            var menu = await _db.ServiceMenuMasters.AsNoTracking()
                .Where(r => r.IsActive && r.JobTypeId == req.JobTypeId && r.ServiceHeadId == req.ServiceHeadId)
                .ToListAsync(ct);
            if (menu.Count == 0) throw new B2BValidationException("This Job Type / Service Head combination is not in the Service Menu Master.");
            var menuRow = menu.FirstOrDefault(r => string.Equals(r.PriorityLabel, req.Priority.Trim(), StringComparison.OrdinalIgnoreCase));
            if (menuRow == null) throw new B2BValidationException("Priority is not valid for this Job Type / Service Head.");
            var priority = Enum.TryParse<JcsJobCardPriority>(menuRow.PriorityLabel, true, out var p) ? p : JcsJobCardPriority.Normal;

            // ---- workshop
            var locCode = Clean(req.ServiceLocationCode, 20);
            if (locCode == null) throw new B2BValidationException("Service Location (workshop) is required.");
            var workshops = await WorkshopsOfAsync(dealer.Id, ct);
            string? locName;
            if (workshops.Count > 0)
            {
                var w = workshops.FirstOrDefault(x => string.Equals(x.LocCode, locCode, StringComparison.OrdinalIgnoreCase));
                if (w == null) throw new B2BValidationException("This Service Location is not a workshop of the dealer.");
                locCode = w.LocCode;
                locName = w.LocName;
            }
            else
            {
                // dealer has no JobCardScanner WorkshopLocations yet: the UI offers the dealer's DMS workshops
                locName = Clean(req.ServiceLocationName, 200) ?? locCode;
            }

            // ---- odometer / delivery / complaints
            if (req.OdometerAtCheckIn is null || req.OdometerAtCheckIn < 0) throw new B2BValidationException("Odometer (km) is required.");
            var previous = await PreviousOdometerAsync(chassis, ct);
            if (previous.HasValue && previous.Value > 0 && req.OdometerAtCheckIn.Value <= previous.Value)
                throw new B2BValidationException($"Odometer must be greater than the previous reading ({previous.Value:0} km).");
            if (req.BatteryLevelAtCheckIn is < 0 or > 100) throw new B2BValidationException("Battery level must be 0 to 100 %.");
            if (req.ExpectedDeliveryAt is null) throw new B2BValidationException("Expected delivery is required.");
            var complaints = (req.Complaints ?? new List<string>())
                .Select(c => (c ?? "").Trim()).Where(c => c.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (complaints.Count == 0) throw new B2BValidationException("Add at least one complaint.");
            if (complaints.Any(c => c.Length > 500)) throw new B2BValidationException("A complaint can have at most 500 characters.");

            // ---- one open job card per chassis per dealer (JobCardsController.Create)
            var open = await OpenJobCardNumberAsync(dealer.Id, chassis, ct);
            if (open != null)
                throw new B2BValidationException($"This chassis already has an open job card ({open}). It must be closed before a new job card can be created for it.");

            var existingVehicleId = await _db.Vehicles.AsNoTracking()
                .Where(v => v.DealerId == dealer.Id && v.Vin == chassis)
                .OrderByDescending(v => v.CreatedAt).Select(v => (Guid?)v.Id).FirstOrDefaultAsync(ct);
            if (existingVehicleId == null)
            {
                if (string.IsNullOrWhiteSpace(req.Model)) throw new B2BValidationException("Model is required for a new vehicle.");
                if (string.IsNullOrWhiteSpace(req.CustomerName)) throw new B2BValidationException("Customer name is required for a new vehicle.");
                var mobile = Regex.Replace(req.CustomerMobile ?? "", @"\D", "");
                if (mobile.Length != 10) throw new B2BValidationException("Customer mobile must have 10 digits.");
            }

            // ---- save (retried if JobCardScanner itself took the same number at the same moment)
            for (var attempt = 1; ; attempt++)
            {
                _db.ChangeTracker.Clear();
                await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
                try
                {
                    JcsVehicle vehicle;
                    if (existingVehicleId.HasValue)
                    {
                        vehicle = await _db.Vehicles.FirstAsync(v => v.Id == existingVehicleId.Value, ct);
                        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == vehicle.CustomerId, ct);
                        if (customer == null) throw new B2BValidationException("Customer details are missing for this vehicle.");
                        if (string.IsNullOrWhiteSpace(customer.State))
                        {
                            var customerState = Clean(req.CustomerState, 100);
                            if (string.IsNullOrWhiteSpace(customerState))
                                throw new B2BValidationException("Customer State is required for tax calculation.");
                            customer.State = customerState;
                        }

                        // fill only empty details from the form (never overwrite JobCardScanner data)
                        vehicle.RegNo ??= Clean(req.RegNo, 30)?.ToUpperInvariant();
                        vehicle.BatteryNo ??= Clean(req.BatteryNo, 50);
                        vehicle.MotorNo ??= Clean(req.MotorNo, 50);
                        vehicle.ControllerNo ??= Clean(req.ControllerNo, 50);
                        vehicle.ConverterNo ??= Clean(req.ConverterNo, 50);
                        vehicle.ChargerNo ??= Clean(req.ChargerNo, 50);
                        vehicle.PurchaseDate ??= req.PurchaseDate;
                    }
                    else
                    {
                        var customerState = Clean(req.CustomerState, 100);
                        if (string.IsNullOrWhiteSpace(customerState))
                            throw new B2BValidationException("Customer State is required for tax calculation.");

                        var customer = new JcsCustomer
                        {
                            Name = Clean(req.CustomerName, 200)!,
                            Mobile = Regex.Replace(req.CustomerMobile ?? "", @"\D", ""),
                            Email = Clean(req.CustomerEmail, 200),
                            Address = Clean(req.CustomerAddress, 300),
                            City = Clean(req.CustomerCity, 100),
                            State = customerState,
                            DealerId = dealer.Id
                        };
                        _db.Customers.Add(customer);
                        vehicle = new JcsVehicle
                        {
                            CustomerId = customer.Id,
                            DealerId = dealer.Id,
                            Model = Clean(req.Model, 100)!,
                            Variant = Clean(req.Variant, 60),
                            Color = Clean(req.Color, 40),
                            RegNo = Clean(req.RegNo, 30)?.ToUpperInvariant(),
                            Vin = chassis,
                            BatteryNo = Clean(req.BatteryNo, 50),
                            MotorNo = Clean(req.MotorNo, 50),
                            ControllerNo = Clean(req.ControllerNo, 50),
                            ConverterNo = Clean(req.ConverterNo, 50),
                            ChargerNo = Clean(req.ChargerNo, 50),
                            PurchaseDate = req.PurchaseDate,
                            Odometer = 0
                        };
                        _db.Vehicles.Add(vehicle);
                    }

                    var firstStage = await _db.WorkflowStages.AsNoTracking()
                        .Where(s => (s.DealerId == null || s.DealerId == dealer.Id) && s.Active)
                        .OrderBy(s => s.Seq).FirstOrDefaultAsync(ct);

                    var now = DateTime.UtcNow;
                    var jobCard = new JcsJobCard
                    {
                        JobCardNumber = await NextJobCardNumberAsync(dealer, ct),
                        DealerId = dealer.Id,
                        CustomerId = vehicle.CustomerId,
                        VehicleId = vehicle.Id,
                        Status = JcsJobCardStatus.Open,
                        ServiceType = MapJobType(menuRow.JobTypeName),
                        Source = MapJobSource(req.JobSourceName),
                        Priority = priority,
                        CurrentStageId = firstStage?.Id,
                        OdometerAtCheckIn = req.OdometerAtCheckIn.Value,
                        BatteryLevelAtCheckIn = req.BatteryLevelAtCheckIn,
                        ExpectedDeliveryAt = req.ExpectedDeliveryAt.Value.Kind == DateTimeKind.Local
                            ? req.ExpectedDeliveryAt.Value.ToUniversalTime() : req.ExpectedDeliveryAt.Value,
                        CustomerConsentNotes = Clean(req.CustomerVoice, 2000),
                        BaplJobType = menuRow.JobTypeName,
                        BaplJobTypeId = menuRow.JobTypeId,
                        BaplServiceHeadId = menuRow.ServiceHeadId,
                        BaplServiceHeadName = menuRow.ServiceHeadName,
                        BaplServiceLocationCode = locCode,
                        BaplServiceLocation = locName,
                        BaplJobSourceId = req.JobSourceId,
                        BaplJobSourceName = Clean(req.JobSourceName, 60),
                        BaplManualJobNo = Clean(req.ManualJobNo, 40),
                        BaplCouponNo = Clean(req.CouponNo, 100),
                        BaplSupervisorName = Clean(req.SupervisorName, 120),
                        BaplTechnicianName = Clean(req.TechnicianName, 120),
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatedById = null   // DMS users are not JobCardScanner Users (FK); see the stage-history note
                    };
                    foreach (var c in complaints)
                        jobCard.Complaints.Add(new JcsJobCardComplaint { JobCardId = jobCard.Id, Description = c, IsCustomerVoice = true, CreatedAt = now });
                    _db.JobCards.Add(jobCard);
                    await _db.SaveChangesAsync(ct);

                    if (firstStage != null)
                        _db.JobCardStageHistories.Add(new JcsJobCardStageHistory
                        {
                            JobCardId = jobCard.Id,
                            StageId = firstStage.Id,
                            EnteredAt = now,
                            Notes = Clean($"Created from BAPL DMS (B2B Job Card) by DMS user {caller.UserId}", 500)
                        });
                    vehicle.Odometer = Math.Max(vehicle.Odometer, req.OdometerAtCheckIn.Value);
                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    _logger.LogInformation("B2B job card {No} created for dealer {Dealer} by DMS user {User}", jobCard.JobCardNumber, dealer.Code, caller.UserId);
                    return new B2BSaveResultDto { Id = jobCard.Id, JobCardNumber = jobCard.JobCardNumber, Message = $"B2B Job Card {jobCard.JobCardNumber} created." };
                }
                catch (Exception ex) when (attempt < 3 && IsDuplicateJobCardNumber(ex))
                {
                    await tx.RollbackAsync(ct);
                    _logger.LogWarning("B2B job card number clash for dealer {Dealer}, retrying ({Attempt})", dealer.Code, attempt);
                }
            }
        }

        // ===================================================================== update (JobCardsController.Update fields)
        public async Task<B2BSaveResultDto> UpdateAsync(B2BCaller caller, Guid id, B2BJobCardUpdateRequest req, CancellationToken ct)
        {
            if (req == null) throw new B2BValidationException("Nothing to save.");
            var jc = await LoadForCallerAsync(caller, id, true, ct);
            if (!Editable(jc.Status)) throw new B2BValidationException($"Job card {jc.JobCardNumber} is {jc.Status} and cannot be changed.");

            if (!string.IsNullOrWhiteSpace(req.Priority))
            {
                var label = req.Priority.Trim();
                if (jc.BaplJobTypeId.HasValue && jc.BaplServiceHeadId.HasValue)
                {
                    var ok = await _db.ServiceMenuMasters.AsNoTracking().AnyAsync(r => r.IsActive
                        && r.JobTypeId == jc.BaplJobTypeId && r.ServiceHeadId == jc.BaplServiceHeadId && r.PriorityLabel == label, ct);
                    if (!ok) throw new B2BValidationException("Priority is not valid for this job card's Job Type / Service Head.");
                }
                if (!Enum.TryParse<JcsJobCardPriority>(label, true, out var pr)) throw new B2BValidationException("Priority is not valid.");
                jc.Priority = pr;
            }
            if (req.ExpectedDeliveryAt.HasValue)
                jc.ExpectedDeliveryAt = req.ExpectedDeliveryAt.Value.Kind == DateTimeKind.Local ? req.ExpectedDeliveryAt.Value.ToUniversalTime() : req.ExpectedDeliveryAt.Value;
            if (req.AssignedTechnicianName != null) jc.AssignedTechnicianName = Clean(req.AssignedTechnicianName, 120);
            if (jc.Customer != null && string.IsNullOrWhiteSpace(jc.Customer.State))
            {
                var customerState = Clean(req.CustomerState, 100);
                if (!string.IsNullOrWhiteSpace(customerState))
                    jc.Customer.State = customerState;
            }
            jc.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return new B2BSaveResultDto { Id = jc.Id, JobCardNumber = jc.JobCardNumber, Message = $"B2B Job Card {jc.JobCardNumber} updated." };
        }

        // ===================================================================== photos (JobCardScanner dbo.JobCardPhotos)
        private const int MaxPhotos = 10;

        private Task<List<B2BPhotoDto>> PhotosOfAsync(Guid jobCardId, CancellationToken ct) =>
            _db.JobCardPhotos.AsNoTracking().Where(p => p.JobCardId == jobCardId).OrderBy(p => p.CreatedAt)
                .Select(p => new B2BPhotoDto { Id = p.Id, Url = p.Url, Caption = p.Caption, Stage = p.Stage, CreatedAt = p.CreatedAt })
                .ToListAsync(ct);

        /// <summary>a photo URL produced by the DMS API (wwwroot/uploads/jobcard-photos, same layout as JobCardScanner)</summary>
        private static bool IsPhotoUrl(string? url) =>
            !string.IsNullOrWhiteSpace(url) && url.Length <= 500 && url.Contains("/uploads/jobcard-photos/", StringComparison.OrdinalIgnoreCase)
            && Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeHttp);

        /// <summary>Check-in photos of a job card - same table / Stage the JobCardScanner wizard writes after creating a card.</summary>
        public async Task<List<B2BPhotoDto>> AddPhotosAsync(B2BCaller caller, Guid id, B2BSavePhotosRequest req, CancellationToken ct)
        {
            var photos = req?.Photos ?? new List<B2BPhotoInput>();
            if (photos.Count == 0) throw new B2BValidationException("No photos to save.");
            if (photos.Any(p => !IsPhotoUrl(p.BlobUrl))) throw new B2BValidationException("Invalid photo URL.");
            if (photos.Any(p => !string.IsNullOrEmpty(p.ContentType) && !p.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)))
                throw new B2BValidationException("Only images can be attached.");
            var jc = await LoadForCallerAsync(caller, id, false, ct);
            if (!Editable(jc.Status)) throw new B2BValidationException($"Job card {jc.JobCardNumber} is {jc.Status}; photos cannot be added.");
            var existing = await _db.JobCardPhotos.CountAsync(p => p.JobCardId == id, ct);
            if (existing + photos.Count > MaxPhotos) throw new B2BValidationException($"A job card can have at most {MaxPhotos} photos.");
            var now = DateTime.UtcNow;
            foreach (var p in photos)
                _db.JobCardPhotos.Add(new JcsJobCardPhoto { JobCardId = id, Stage = "CheckIn", Url = p.BlobUrl!, Caption = Clean(p.Caption, 200), CreatedAt = now });
            await _db.SaveChangesAsync(ct);
            return await PhotosOfAsync(id, ct);
        }

        // ===================================================================== connectivity
        public async Task<object> PingAsync(CancellationToken ct)
        {
            var started = DateTime.UtcNow;
            var canConnect = await _db.Database.CanConnectAsync(ct);
            if (!canConnect) return new { connected = false };
            return new
            {
                connected = true,
                database = _db.Database.GetDbConnection().Database,
                dealers = await _db.Dealers.CountAsync(ct),
                serviceMenuRows = await _db.ServiceMenuMasters.CountAsync(r => r.IsActive, ct),
                complaints = await _db.ComplaintMasters.CountAsync(c => c.IsActive, ct),
                jobCards = await _db.JobCards.CountAsync(ct),
                ms = (int)(DateTime.UtcNow - started).TotalMilliseconds
            };
        }
    }
}
