using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;

namespace DMS_BAPL_Data.Repositories.ReportRepo
{
    public class ReportRepo : IReportRepo
    {
        private readonly BapldmsvadContext _context;

        public ReportRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        private async Task<Dictionary<int, string>> GetFinancierNameLookupAsync()
        {
            var financiers = await _context.LedgerMasters
                .AsNoTracking()
                .Where(x => x.LedgerType != null && x.LedgerType.ToLower() == "financier")
                .ToListAsync();

            return financiers
                .GroupBy(f => f.Id)
                .ToDictionary(g => g.Key, g => g.First().LedgerName);
        }

        private static bool HasDealerFilter(string? dealerCode) =>
                !string.IsNullOrWhiteSpace(dealerCode)
                && !dealerCode.Equals("undefined", StringComparison.OrdinalIgnoreCase)
                && !dealerCode.Equals("null", StringComparison.OrdinalIgnoreCase);

        private static int SafeParseInvoiceNo(string? documentNo)
        {
            if (string.IsNullOrWhiteSpace(documentNo))
                return 0;

            if (int.TryParse(documentNo, out var direct))
                return direct;

            var digitsOnly = new string(documentNo.Where(char.IsDigit).ToArray());
            return int.TryParse(digitsOnly, out var parsed) ? parsed : 0;
        }
        // ═════════════════════════════════════════════════════════════════════
        // NORMALIZED NAME MATCHING 
        // ═════════════════════════════════════════════════════════════════════
        private static string NormalizeForMatch(string value) =>
            Regex.Replace(value.Trim(), @"\s+", " ").ToUpperInvariant();

        private static Dictionary<string, string> BuildNormalizedLookup(IEnumerable<string> canonicalNames)
        {
            return canonicalNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .GroupBy(n => NormalizeForMatch(n))
                .ToDictionary(g => g.Key, g => g.First());
        }

        private static string? ResolveCanonicalName(string? rawName, Dictionary<string, string> normalizedLookup)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return null;

            return normalizedLookup.TryGetValue(NormalizeForMatch(rawName), out var canonical)
                ? canonical
                : null;
        }

        private async Task<Dictionary<string, ChassisDetailsD2dhistory>> GetLatestD2dHistoryByChassisAsync(IEnumerable<string?> chassisNos)
        {
            var codes = chassisNos.Where(c => !string.IsNullOrEmpty(c)).Cast<string>().Distinct().ToList();
            if (codes.Count == 0) return new Dictionary<string, ChassisDetailsD2dhistory>();

            var histories = await _context.ChassisDetailsD2dhistories
                .AsNoTracking()
                .Where(h => codes.Contains(h.ChassisNo) && !h.IsDeleted)
                .ToListAsync();

            return histories
                .GroupBy(h => h.ChassisNo)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedDate).First());
        }

        private async Task<Dictionary<string, DealerMaster>> GetDealerLookupAsync()
        {
            var dealers = await _context.DealerMasters.AsNoTracking().ToListAsync();
            return dealers
                .Where(d => d.Dealercode != null)
                .GroupBy(d => d.Dealercode!)
                .ToDictionary(g => g.Key, g => g.First());
        }

        public async Task<List<object>> GetUnmappedModelDiagnosticsAsync(
            DateTime? fromDate, DateTime? toDate, string? dealerCode)
        {
            var allCanonical = await _context.OemmodelMasters
                .Select(x => new { x.ModelName, x.IsActive })
                .ToListAsync();

            var activeNormalizedSet = allCanonical
                .Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.ModelName))
                .Select(x => NormalizeForMatch(x.ModelName))
                .ToHashSet();
            var inactiveNormalizedSet = allCanonical
                .Where(x => !x.IsActive && !string.IsNullOrWhiteSpace(x.ModelName))
                .Select(x => NormalizeForMatch(x.ModelName))
                .ToHashSet();

            var query =
                from d in _context.VehicleSaleBillDetails
                join h in _context.VehicleSaleBillHeaders on d.VehicleSaleBillId equals h.Id
                join im in _context.ItemMasters on d.ItemCode equals im.Itemcode into imJoin
                from im in imJoin.DefaultIfEmpty()
                where
                    (!fromDate.HasValue || h.SaleDate.Date >= fromDate.Value.Date)
                    && (!toDate.HasValue || h.SaleDate.Date <= toDate.Value.Date)
                    && (!HasDealerFilter(dealerCode) || h.DealerCode == dealerCode)
                select new
                {
                    h.DealerCode,
                    d.ChassisNo,
                    d.ItemCode,
                    ItemFound = im != null,
                    RawOemModelName = im != null ? im.Oemmodelname : null
                };

            var rows = await query.ToListAsync();

            return rows
                .Select(x =>
                {
                    string reason;

                    if (!x.ItemFound)
                    {
                        reason = "ItemCode has no matching row in ItemMasters";
                    }
                    else if (string.IsNullOrWhiteSpace(x.RawOemModelName))
                    {
                        reason = "ItemMasters.Oemmodelname is blank";
                    }
                    else
                    {
                        var normalized = NormalizeForMatch(x.RawOemModelName);

                        if (activeNormalizedSet.Contains(normalized))
                            reason = "MATCHED"; // not actually unmapped — filtered out below
                        else if (inactiveNormalizedSet.Contains(normalized))
                            reason = "Matches an OemmodelMasters row, but that row has IsActive = false";
                        else
                            reason = "Oemmodelname has no corresponding row in OemmodelMasters at all — needs a new master record, or a spelling fix in ItemMasters";
                    }

                    return new
                    {
                        x.DealerCode,
                        x.ChassisNo,
                        x.ItemCode,
                        x.RawOemModelName,
                        Reason = reason
                    };
                })
                .Where(x => x.Reason != "MATCHED")
                .Cast<object>()
                .ToList();
        }

        private async Task<Dictionary<string, VehicleInward>> GetLatestVehicleInwardByChassisAsync(IEnumerable<string?> chassisNos)
        {
            var codes = chassisNos.Where(c => !string.IsNullOrEmpty(c)).Cast<string>().Distinct().ToList();
            if (codes.Count == 0) return new Dictionary<string, VehicleInward>();

            var inwards = await _context.VehicleInwards
                .AsNoTracking()
                .Where(vi => vi.ChasisNo != null && codes.Contains(vi.ChasisNo))
                .ToListAsync();

            return inwards
                .GroupBy(vi => vi.ChasisNo!)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.Id).First());
        }


        private async Task<Dictionary<string, LocationMaster>> GetLocationLookupAsync()
        {
            var locations = await _context.LocationMasters.AsNoTracking().ToListAsync();
            return locations.Where(l => l.Loccode != null)
                .GroupBy(l => l.Loccode!)
                .ToDictionary(g => g.Key, g => g.First());
        }

        private async Task<Dictionary<string, ColorMaster>> GetColorLookupAsync()
        {
            var colors = await _context.ColorMasters.AsNoTracking().ToListAsync();
            return colors.Where(c => c.Colorcode != null)
                .GroupBy(c => c.Colorcode!)
                .ToDictionary(g => g.Key, g => g.First());
        }


        // ═════════════════════════════════════════════════════════════════════
        // STOCK REPORT
        // ═════════════════════════════════════════════════════════════════════

        private async Task<HashSet<string>> GetInvoicedChassisSetAsync(IEnumerable<string?> chassisNos)
        {
            var codes = chassisNos.Where(c => !string.IsNullOrEmpty(c)).Cast<string>().Distinct().ToList();
            if (codes.Count == 0) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var saleDetails = await _context.VehicleSaleBillDetails
                .AsNoTracking()
                .Where(d => d.ChassisNo != null && codes.Contains(d.ChassisNo))
                .ToListAsync();

            var saleBillIds = saleDetails.Select(d => d.VehicleSaleBillId).Distinct().ToList();

            var saleHeaders = await _context.VehicleSaleBillHeaders
                .AsNoTracking()
                .Where(h => saleBillIds.Contains(h.Id))
                .ToListAsync();

            var invoicedHeaderIds = saleHeaders
                .Where(h => h.Status == "Invoiced")
                .Select(h => h.Id)
                .ToHashSet();

            return saleDetails
                .Where(d => d.ChassisNo != null && invoicedHeaderIds.Contains(d.VehicleSaleBillId))
                .Select(d => d.ChassisNo!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<List<StockReportViewModel>> GetDealerWiseStockReportAsync(
            string? dealerCode = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                var fromDateOnly = fromDate.HasValue ? DateOnly.FromDateTime(fromDate.Value.Date) : (DateOnly?)null;
                var toDateOnly = toDate.HasValue ? DateOnly.FromDateTime(toDate.Value.Date) : (DateOnly?)null;

                var vehicleInwards = await _context.VehicleInwards
                    .AsNoTracking()
                    .Where(vi =>
                        vi.DealerCode != null &&
                        (!HasDealerFilter(dealerCode) || vi.DealerCode == dealerCode) &&
                        (!fromDateOnly.HasValue || (vi.InvoiceDate.HasValue && vi.InvoiceDate.Value >= fromDateOnly.Value)) &&
                        (!toDateOnly.HasValue || (vi.InvoiceDate.HasValue && vi.InvoiceDate.Value <= toDateOnly.Value)))
                    .ToListAsync();

                var invoicedChassisSet = await GetInvoicedChassisSetAsync(vehicleInwards.Select(vi => vi.ChasisNo));

                vehicleInwards = vehicleInwards
                    .Where(vi => vi.ChasisNo == null || !invoicedChassisSet.Contains(vi.ChasisNo))
                    .ToList();

                var dealers = await _context.DealerMasters
                    .AsNoTracking()
                    .ToListAsync();

                var items = await _context.ItemMasters
                    .AsNoTracking()
                    .ToListAsync();

                var colors = await _context.ColorMasters
                    .AsNoTracking()
                    .ToListAsync();

                var result = (
                    from vi in vehicleInwards
                    join dm in dealers
                        on vi.DealerCode equals dm.Dealercode into dealerGroup
                    from dm in dealerGroup.DefaultIfEmpty()
                    join im in items
                        on vi.ItemCode equals im.Itemcode into itemGroup
                    from im in itemGroup.DefaultIfEmpty()
                    join cm in colors
                        on vi.ColrCode equals cm.Colorcode into colorGroup
                    from cm in colorGroup.DefaultIfEmpty()
                    group new { vi, dm, im, cm }
                        by new
                        {
                            DealerCode = vi.DealerCode ?? "Unknown",
                            DealerName = dm != null ? dm.Compname : (vi.DealerCode ?? "Unknown"),
                            Model = im != null ? (im.Oemmodelname ?? im.Itemcode ?? "Unknown")
                                                    : (vi.ItemCode ?? "Unknown"),
                            Colour = cm != null ? cm.Colorname
                                                    : (vi.ColrCode ?? "Unknown")
                        }
                    into g
                    orderby g.Key.DealerName, g.Key.Model, g.Key.Colour
                    select new StockReportViewModel
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        Model = g.Key.Model,
                        Colour = g.Key.Colour,
                        TotalQty = g.Count()
                    }
                ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetDealerWiseStockReportAsync: " + ex.Message, ex);
            }
        }

        public async Task<List<StockReportViewModel>> GetColourWiseStockReportAsync()
        {
            try
            {
                var vehicleInwards = await _context.VehicleInwards
                    .AsNoTracking()
                    .Where(vi => vi.DealerCode != null)
                    .ToListAsync();

                var invoicedChassisSet = await GetInvoicedChassisSetAsync(vehicleInwards.Select(vi => vi.ChasisNo));

                vehicleInwards = vehicleInwards
                    .Where(vi => vi.ChasisNo == null || !invoicedChassisSet.Contains(vi.ChasisNo))
                    .ToList();

                var dealers = await _context.DealerMasters
                    .AsNoTracking()
                    .ToListAsync();

                var items = await _context.ItemMasters
                    .AsNoTracking()
                    .ToListAsync();

                var colors = await _context.ColorMasters
                    .AsNoTracking()
                    .ToListAsync();

                var result = (
                    from vi in vehicleInwards
                    join dm in dealers
                        on vi.DealerCode equals dm.Dealercode into dealerGroup
                    from dm in dealerGroup.DefaultIfEmpty()
                    join im in items
                        on vi.ItemCode equals im.Itemcode into itemGroup
                    from im in itemGroup.DefaultIfEmpty()
                    join cm in colors
                        on vi.ColrCode equals cm.Colorcode into colorGroup
                    from cm in colorGroup.DefaultIfEmpty()
                    group new { vi, dm, im, cm }
                        by new
                        {
                            DealerCode = vi.DealerCode ?? "Unknown",
                            DealerName = dm != null ? dm.Compname : (vi.DealerCode ?? "Unknown"),
                            Model = im != null ? (im.Oemmodelname ?? im.Itemcode ?? "Unknown")
                                                    : (vi.ItemCode ?? "Unknown"),
                            Colour = cm != null ? cm.Colorname
                                                    : (vi.ColrCode ?? "Unknown")
                        }
                    into g
                    orderby g.Key.Colour, g.Key.DealerName, g.Key.Model
                    select new StockReportViewModel
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        Model = g.Key.Model,
                        Colour = g.Key.Colour,
                        TotalQty = g.Count()
                    }
                ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetColourWiseStockReportAsync: " + ex.Message, ex);
            }
        }


        // ═════════════════════════════════════════════════════════════════════
        // JOB REPORT  (unchanged from JobReportRepo)
        // ═════════════════════════════════════════════════════════════════════

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(
    JobReportFilterModel filter)
        {
            try
            {
                var baseQuery = from jh in _context.JobCardHeaders
                                join jc in _context.JobCardCustomers
                                    on jh.Id equals jc.JobCardHeaderId
                                join jt in _context.JobTypes
                                    on jh.Jobtype equals jt.Id
                                join sh in _context.ServiceHeads
                                    on jh.Servicehead equals sh.Id
                                join st in _context.ServiceTypes
                                    on jh.Servicetype equals st.Id
                                join dm in _context.DealerMasters
                                    on jh.DealerCode equals dm.Dealercode into dmGroup
                                from dm in dmGroup.DefaultIfEmpty()
                                join loc in _context.LocationMasters
                                    on jh.Serviceloc equals loc.Loccode into locGroup
                                from loc in locGroup.DefaultIfEmpty()
                                join js in _context.JobSources
                                    on jh.JobSource equals js.Id into jsGroup
                                from js in jsGroup.DefaultIfEmpty()
                                select new { jh, jc, jt, sh, st, dm, loc, js };

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    baseQuery = baseQuery.Where(x => x.jh.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                {
                    var from = DateOnly.FromDateTime(filter.FromDate.Value.Date);
                    baseQuery = baseQuery.Where(x => x.jh.JobinDate >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var to = DateOnly.FromDateTime(filter.ToDate.Value.Date);
                    baseQuery = baseQuery.Where(x => x.jh.JobinDate <= to);
                }

                if (!string.IsNullOrWhiteSpace(filter.ServiceLocation))
                    baseQuery = baseQuery.Where(x => x.jh.Serviceloc == filter.ServiceLocation);

                if (filter.JobNo.HasValue)
                    baseQuery = baseQuery.Where(x => x.jh.JobNo == filter.JobNo.Value);

                if (!string.IsNullOrWhiteSpace(filter.PartyName))
                    baseQuery = baseQuery.Where(x => x.jc.CustomerName.Contains(filter.PartyName));

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                    baseQuery = baseQuery.Where(x => x.jh.Chassisno.Contains(filter.ChassisNo));

                if (!string.IsNullOrWhiteSpace(filter.RegNo))
                    baseQuery = baseQuery.Where(x => x.jc.RegisterNo.Contains(filter.RegNo));

                var totalRecords = await baseQuery.CountAsync();

                var pagedRows = await baseQuery
                    .OrderByDescending(x => x.jh.JobinDate)
                    .ThenByDescending(x => x.jh.JobNo)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var headerIds = pagedRows.Select(r => r.jh.Id).ToList();
                var chassisNos = pagedRows.Select(r => r.jh.Chassisno).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();

                var batteryRows = await _context.JobCardBatteryDetails
                    .Where(b => headerIds.Contains(b.JobCardHeaderId))
                    .ToListAsync();
                var batteryLookup = batteryRows
                    .GroupBy(b => b.JobCardHeaderId)
                    .ToDictionary(g => g.Key, g => g.First().ChargerNo);

                var complaintRows = await _context.JobCardComplaints
                    .Where(c => headerIds.Contains(c.JobCardHeaderId))
                    .ToListAsync();
                var complaintLookup = complaintRows
                    .GroupBy(c => c.JobCardHeaderId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var repairBillRows = await _context.RepairBillHeaders
                    .Where(r => headerIds.Contains(r.JobId))
                    .ToListAsync();
                var repairBillLookup = repairBillRows
                    .GroupBy(r => r.JobId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedDate).First());

                var ffirRows = await _context.Ffirheaders
                    .Where(f => chassisNos.Contains(f.FfirchassisNo))
                    .ToListAsync();
                var ffirLookup = ffirRows
                    .GroupBy(f => f.FfirchassisNo)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(f => f.CreatedDate).First());

                int srNo = (filter.PageIndex - 1) * filter.PageSize + 1;
                var data = pagedRows.Select(r =>
                {
                    var complaints = complaintLookup.TryGetValue(r.jh.Id, out var cList) ? cList : new List<JobCardComplaint>();
                    var rb = repairBillLookup.TryGetValue(r.jh.Id, out var rbVal) ? rbVal : null;
                    var fr = r.jh.Chassisno != null && ffirLookup.TryGetValue(r.jh.Chassisno, out var frVal) ? frVal : null;

                    string jobStatus =
                        rb != null && rb.RepairbillStatus == "Billed" ? "Closed"
                        : rb != null && rb.TotalNetAmount > 0 ? "Complete"
                        : r.jh.IsMaterialTransfer == true ? "Material Transfer"
                        : fr != null && fr.Ffirstatus == "Closed" ? "FFIR Closed"
                        : fr != null ? "FFIR Created"
                        : "Open";

                    return new JobReportViewModel
                    {
                        SrNo = srNo++,
                        JobNo = r.jh.JobNo ?? 0,
                        PartyName = r.jc.CustomerName,
                        PartyMobileNo = r.jc.CustomerMobile,
                        RegNo = r.jc.RegisterNo,
                        MechanicName = r.jh.Technician,
                        JobType = r.jt.JobTypeName,
                        ServiceHead = r.sh.ServiceHeadName,
                        ServiceType = r.st.ServiceTypeName,
                        ChassisNo = r.jh.Chassisno,
                        DealerCode = r.jh.DealerCode,
                        ServiceLocation = r.jh.Serviceloc,
                        JobInDate = r.jh.JobinDate.HasValue
                            ? r.jh.JobinDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        EstimatedDeliveryDate = r.jh.EstdelDate.HasValue
                            ? r.jh.EstdelDate.Value.ToDateTime(TimeOnly.MinValue) : null,

                        DealerName = r.dm != null ? r.dm.Compname : null,
                        DealerLocation = r.loc != null ? r.loc.Locname : r.jh.Serviceloc,
                        City = r.dm != null ? r.dm.City : null,
                        State = r.dm != null ? r.dm.State : null,
                        Kms = r.jh.Vehiclekms,
                        MotorNo = r.jc.MotorNo,
                        BatteryNo = r.jc.BatteryNo,
                        ChargerNo = batteryLookup.TryGetValue(r.jh.Id, out var chargerNo) ? chargerNo : null,
                        CustomerVoice = complaints.Any()
                            ? string.Join(", ", complaints.Select(c => c.CustomerVoice).Where(v => !string.IsNullOrWhiteSpace(v)))
                            : null,
                        CustomerCode = complaints.Any()
                            ? string.Join(", ", complaints.Select(c => c.ComplaintCode).Where(v => !string.IsNullOrWhiteSpace(v)))
                            : null,
                        Observation = r.jh.Observation,
                        SupervisorComment = r.jh.SupervisorComment,
                        JobStatus = jobStatus,
                        SaleDate = r.jc.SaleDate,
                        SupervisorName = r.jh.Supervisor,
                        JobCreationSource = r.js != null ? r.js.JobSourceName : null
                    };
                }).ToList();

                return new JobReportPagedResponse<JobReportViewModel>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching job report: " + ex.Message, ex);
            }
        }

        public async Task<List<DealerWiseJobReportSummary>> GetDealerWiseJobReportAsync(
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var allData = await BuildJobReportQueryAsync();

                IEnumerable<JobReportViewModel> query = allData;

                if (!string.IsNullOrWhiteSpace(dealerCode))
                    query = query.Where(x => x.DealerCode == dealerCode);

                if (fromDate.HasValue)
                    query = query.Where(x => x.JobInDate >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(x => x.JobInDate <= toDate.Value.Date.AddDays(1));

                var data = query.ToList();

                return data
                    .GroupBy(x => x.DealerCode)
                    .Select(g => new DealerWiseJobReportSummary
                    {
                        DealerCode = g.Key,
                        DealerName = g.First().DealerName,
                        TotalJobs = g.Count(),
                        JobDetails = g.ToList()
                    })
                    .OrderByDescending(x => x.TotalJobs)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching dealer wise job report: " + ex.Message, ex);
            }
        }

        // ── RESTORED — pure wrapper, unchanged from the original file ──
        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportByDealerAsync(
            string dealerCode,
            int pageIndex,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var filter = new JobReportFilterModel
            {
                DealerCode = dealerCode,
                PageIndex = pageIndex,
                PageSize = pageSize,
                FromDate = fromDate,
                ToDate = toDate
            };

            return await GetJobReportAsync(filter);
        }

        // ── RESTORED — pure wrapper, unchanged from the original file ──
        public async Task<JobReportPagedResponse<JobReportViewModel>> GetFilteredJobReportAsync(
            JobReportFilterModel filter)
        {
            return await GetJobReportAsync(filter);
        }

        public async Task<List<JobReportViewModel>> GetJobReportForExportAsync(
            string dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var allData = await BuildJobReportQueryAsync();

                IEnumerable<JobReportViewModel> query = allData;

                if (!string.IsNullOrWhiteSpace(dealerCode))
                    query = query.Where(x => x.DealerCode == dealerCode);

                if (fromDate.HasValue)
                    query = query.Where(x => x.JobInDate >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(x => x.JobInDate <= toDate.Value.Date.AddDays(1));

                return query
                    .OrderByDescending(x => x.JobInDate)
                    .ThenByDescending(x => x.JobNo)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error exporting report: " + ex.Message, ex);
            }
        }

        private async Task<List<JobReportViewModel>> BuildJobReportQueryAsync()
        {
            var query =
                from jh in _context.JobCardHeaders
                join jc in _context.JobCardCustomers
                    on jh.Id equals jc.JobCardHeaderId
                join jt in _context.JobTypes
                    on jh.Jobtype equals jt.Id
                join sh in _context.ServiceHeads
                    on jh.Servicehead equals sh.Id
                join st in _context.ServiceTypes
                    on jh.Servicetype equals st.Id
                join dm in _context.DealerMasters
                    on jh.DealerCode equals dm.Dealercode into dmGroup
                from dm in dmGroup.DefaultIfEmpty()
                join loc in _context.LocationMasters
                    on jh.Serviceloc equals loc.Loccode into locGroup
                from loc in locGroup.DefaultIfEmpty()
                join js in _context.JobSources
                    on jh.JobSource equals js.Id into jsGroup
                from js in jsGroup.DefaultIfEmpty()
                select new { jh, jc, jt, sh, st, dm, loc, js };

            var rows = await query.ToListAsync();

            var headerIds = rows.Select(r => r.jh.Id).ToList();
            var chassisNos = rows.Select(r => r.jh.Chassisno).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();

            var batteryRows = await _context.JobCardBatteryDetails
                .Where(b => headerIds.Contains(b.JobCardHeaderId))
                .ToListAsync();
            var batteryLookup = batteryRows
                .GroupBy(b => b.JobCardHeaderId)
                .ToDictionary(g => g.Key, g => g.First().ChargerNo);

            var complaintRows = await _context.JobCardComplaints
                .Where(c => headerIds.Contains(c.JobCardHeaderId))
                .ToListAsync();
            var complaintLookup = complaintRows
                .GroupBy(c => c.JobCardHeaderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var repairBillRows = await _context.RepairBillHeaders
                .Where(r => headerIds.Contains(r.JobId))
                .ToListAsync();
            var repairBillLookup = repairBillRows
                .GroupBy(r => r.JobId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedDate).First());

            var ffirRows = await _context.Ffirheaders
                .Where(f => chassisNos.Contains(f.FfirchassisNo))
                .ToListAsync();
            var ffirLookup = ffirRows
                .GroupBy(f => f.FfirchassisNo)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(f => f.CreatedDate).First());

            var result = rows.Select(x =>
            {
                var complaints = complaintLookup.TryGetValue(x.jh.Id, out var cList) ? cList : new List<JobCardComplaint>();
                var rb = repairBillLookup.TryGetValue(x.jh.Id, out var rbVal) ? rbVal : null;
                var fr = x.jh.Chassisno != null && ffirLookup.TryGetValue(x.jh.Chassisno, out var frVal) ? frVal : null;

                string jobStatus =
                    rb != null && rb.RepairbillStatus == "Billed" ? "Closed"
                    : rb != null && rb.TotalNetAmount > 0 ? "Complete"
                    : x.jh.IsMaterialTransfer == true ? "Material Transfer"
                    : fr != null && fr.Ffirstatus == "Closed" ? "FFIR Closed"
                    : fr != null ? "FFIR Created"
                    : "Open";

                return new JobReportViewModel
                {
                    SrNo = x.jh.Id,
                    JobNo = x.jh.JobNo ?? 0,
                    PartyName = x.jc.CustomerName,
                    PartyMobileNo = x.jc.CustomerMobile,
                    RegNo = x.jc.RegisterNo,
                    MechanicName = x.jh.Technician,
                    JobType = x.jt.JobTypeName,
                    ServiceHead = x.sh.ServiceHeadName,
                    ServiceType = x.st.ServiceTypeName,
                    ChassisNo = x.jh.Chassisno,
                    DealerCode = x.jh.DealerCode,
                    ServiceLocation = x.jh.Serviceloc,
                    JobInDate = x.jh.JobinDate.HasValue
                        ? x.jh.JobinDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    EstimatedDeliveryDate = x.jh.EstdelDate.HasValue
                        ? x.jh.EstdelDate.Value.ToDateTime(TimeOnly.MinValue) : null,

                    DealerName = x.dm != null ? x.dm.Compname : null,
                    DealerLocation = x.loc != null ? x.loc.Locname : x.jh.Serviceloc,
                    City = x.dm != null ? x.dm.City : null,
                    State = x.dm != null ? x.dm.State : null,
                    Kms = x.jh.Vehiclekms,
                    MotorNo = x.jc.MotorNo,
                    BatteryNo = x.jc.BatteryNo,
                    ChargerNo = batteryLookup.TryGetValue(x.jh.Id, out var chargerNo) ? chargerNo : null,
                    CustomerVoice = complaints.Any()
                        ? string.Join(", ", complaints.Select(c => c.CustomerVoice).Where(v => !string.IsNullOrWhiteSpace(v)))
                        : null,
                    CustomerCode = complaints.Any()
                        ? string.Join(", ", complaints.Select(c => c.ComplaintCode).Where(v => !string.IsNullOrWhiteSpace(v)))
                        : null,
                    Observation = x.jh.Observation,
                    SupervisorComment = x.jh.SupervisorComment,
                    JobStatus = jobStatus,
                    SaleDate = x.jc.SaleDate,
                    SupervisorName = x.jh.Supervisor,
                    JobCreationSource = x.js != null ? x.js.JobSourceName : null
                };
            }).ToList();

            return result;
        }

        // ═════════════════════════════════════════════════════════════════════
        // VEHICLE SALE REPORT
        // ═════════════════════════════════════════════════════════════════════

        public async Task<List<VehicleSaleReportViewModel>> GetVehicleSaleReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode)
        {
            var baseQuery =
                from h in _context.VehicleSaleBillHeaders

                join d in _context.VehicleSaleBillDetails
                    on h.Id equals d.VehicleSaleBillId

                join im in _context.ItemMasters
                    on d.ItemCode equals im.Itemcode into imJoin
                from im in imJoin.DefaultIfEmpty()

                join dm in _context.DealerMasters
                    on h.DealerCode equals dm.Dealercode into dmJoin
                from dm in dmJoin.DefaultIfEmpty()

                join lm in _context.LedgerMasters
                    on h.LedgerId equals lm.Id into lmJoin
                from lm in lmJoin.DefaultIfEmpty()

                join city in _context.Cities
                    on lm.City equals city.CityId into cityJoin
                from city in cityJoin.DefaultIfEmpty()

                join state in _context.States
                    on lm.State equals state.StateId into stateJoin
                from state in stateJoin.DefaultIfEmpty()

                join om in _context.OccupationMasters
                    on lm.OccupationId equals om.Id into omJoin
                from om in omJoin.DefaultIfEmpty()

                where
                    (!fromDate.HasValue || h.SaleDate.Date >= fromDate.Value.Date)
                    && (!toDate.HasValue || h.SaleDate.Date <= toDate.Value.Date)
                    && (!HasDealerFilter(dealerCode) || h.DealerCode == dealerCode)

                select new { h, d, im, dm, lm, city, state, om };

            var rawRows = await baseQuery.ToListAsync();

            var chassisNos = rawRows.Select(x => x.d.ChassisNo).ToList();
            var inwardLookup = await GetLatestVehicleInwardByChassisAsync(chassisNos);
            var locationLookup = await GetLocationLookupAsync();
            var colorLookup = await GetColorLookupAsync();
            var financierLookup = await GetFinancierNameLookupAsync();

            var rows = rawRows.Select(x =>
            {
                var vi = x.d.ChassisNo != null && inwardLookup.TryGetValue(x.d.ChassisNo, out var vinw) ? vinw : null;
                var clr = vi?.ColrCode != null && colorLookup.TryGetValue(vi.ColrCode, out var c) ? c : null;
                var locM = vi?.LocCode != null && locationLookup.TryGetValue(vi.LocCode, out var l) ? l : null;

                return new VehicleSaleReportViewModel
                {
                    SrNo = x.d.Id,
                    ModelCode = x.im?.Itemcode,
                    ModelDescription = x.im?.Itemname,
                    OemModelName = x.im?.Oemmodelname,
                    VehicleGroup = x.im?.Itemdesc,
                    ColorCode = clr?.Colorcode,
                    Colour = clr?.Colorname,
                    ChasisNo = x.d.ChassisNo,
                    RegNo = x.d.RegNo,
                    BillingName = x.h.BillingName,
                    Hsn = x.im?.Hsncode,
                    MfgYear = x.d.MfgYear ?? vi?.MfgYear,
                    SaleType = x.h.SaleType,
                    Status = x.h.Status,
                    DealerCode = x.dm?.Dealercode,
                    DealerName = x.dm?.Compname,
                    DealerCity = x.dm?.City,
                    DealerState = x.dm?.State,
                    Location = locM != null ? locM.Locname : x.h.Location,
                    LocCode = vi?.LocCode,
                    LocCity = locM != null ? locM.City : x.dm?.City,
                    Name = x.h.CustomerName,
                    Address1 = x.lm?.Address,
                    Address2 = "",
                    CustomerState = x.state?.StateName,
                    CustomerCity = x.city?.CityName,
                    Pin = x.lm?.Pin,
                    Email = x.lm?.EMail,
                    MobileNo = x.lm?.MobileNumber,
                    Type = x.h.CustomerType,
                    BookingId = x.h.BookingId,
                    DispatchDate = vi != null && vi.InvoiceDate.HasValue
                        ? vi.InvoiceDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                    SaleDate = x.h.SaleDate,
                    InvoiceNo = x.h.SaleBillNo,
                    SaleBillNo = x.h.SaleBillNo,
                    BillType = x.h.BillType,
                    FinancierId = x.h.Financier,
                    FinanceBy = x.h.Financier.HasValue && financierLookup.TryGetValue(x.h.Financier.Value, out var finName)
                        ? finName : null,
                    FinancerCode = "",
                    FinancerCategory = "",
                    ExecutiveName = x.h.SalesExecutive,
                    ProspectName = x.h.RefName,
                    ProspectMobNo = "",
                    MotorNumber = vi?.MotorNo,
                    BatteryNo = vi?.BatteryNo,
                    ChargerNo = vi?.ChargerNo,
                    ControllerNo = vi?.ControllerNo,
                    BatteryNo2 = vi?.BatteryNo2,
                    BatteryNo3 = vi?.BatteryNo3,
                    BatteryNo4 = vi?.BatteryNo4,
                    BatteryNo5 = vi?.BatteryNo5,
                    BatteryNo6 = vi?.BatteryNo6,
                    BatteryCapacity = vi?.BatteryCapacity,
                    SubsidyAmount = x.im?.Fame2amount,
                    FameIIRequired = x.im?.Fame2amount > 0,
                    TotalAmount = x.h.TotalAmount,
                    BillDate = x.h.CreatedDate,
                    ItemRate = x.d.ItemRate,
                    PreGstDiscount = x.d.PreGstDiscount ?? 0,
                    TaxableAmount = x.d.ItemRate - (x.d.PreGstDiscount ?? 0),
                    SgstPer = x.d.Sgstper ?? 0,
                    SgstAmount = x.d.Sgstamnt ?? 0,
                    CgstPer = x.d.Cgstper ?? 0,
                    CgstAmount = x.d.Cgstamnt ?? 0,
                    IgstPer = x.d.Igstper ?? 0,
                    IgstAmount = x.d.Igstamnt ?? 0,
                    TotalGstAmount = (x.d.Sgstamnt ?? 0) + (x.d.Cgstamnt ?? 0) + (x.d.Igstamnt ?? 0),
                    FinalAmount = x.d.FinalAmount,
                    FameIIDiscount = x.d.FameIi ?? 0,
                    RegAmount = x.d.RegAmount ?? 0,
                    InsuranceAmount = x.d.InsuranceAmount ?? 0,
                    PostGstDiscount = x.d.PostGstDisc ?? 0,
                    Vcu = x.d.Vcu,

                    Gender = x.lm?.Gender,
                    Dob = x.lm != null && x.lm.DateOfBirth.HasValue
                        ? x.lm.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                    AccountType = x.lm?.LedgerType,
                    PartyEmail = x.lm?.EMail,
                    Occupation = x.om != null ? x.om.OccupationName : null,
                    ItemCode = x.d.ItemCode,
                    Battery = x.d.Battery,
                    BatteryMake = vi?.BatteryMake,
                    BatteryType = vi?.BatteryChemistry
                };
            })
            .OrderByDescending(x => x.BillDate)
            .ToList();

            return rows;
        }

        // ═════════════════════════════════════════════════════════════════════
        // TOTAL SALE REPORT (DEALER-WISE MAPPING)
        // ═════════════════════════════════════════════════════════════════════
        public async Task<TotalSaleReportDealerWiseResponse> GetTotalSaleReportDealerWiseAsync(
            DateTime? fromDate, DateTime? toDate, string? dealerCode)
        {
            try
            {
                var query =
                    from d in _context.VehicleSaleBillDetails

                    join h in _context.VehicleSaleBillHeaders
                        on d.VehicleSaleBillId equals h.Id

                    join dm in _context.DealerMasters
                        on h.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    where
                        (!fromDate.HasValue || h.SaleDate.Date >= fromDate.Value.Date)
                        && (!toDate.HasValue || h.SaleDate.Date <= toDate.Value.Date)
                        && (string.IsNullOrEmpty(dealerCode) || h.DealerCode == dealerCode)

                    select new
                    {
                        DealerCode = h.DealerCode ?? "Unknown",
                        DealerName = dm != null ? dm.Compname : (h.DealerCode ?? "Unknown"),
                        DealerCity = dm != null ? dm.City : null,
                        DealerState = dm != null ? dm.State : null,
                        h.SaleType,
                        d.ItemRate,
                        PreGstDiscount = d.PreGstDiscount ?? 0,
                        TaxableAmount = d.ItemRate - (d.PreGstDiscount ?? 0),
                        SgstAmount = d.Sgstamnt ?? 0,
                        CgstAmount = d.Cgstamnt ?? 0,
                        IgstAmount = d.Igstamnt ?? 0,
                        FameIIDiscount = d.FameIi ?? 0,
                        RegAmount = d.RegAmount ?? 0,
                        InsuranceAmount = d.InsuranceAmount ?? 0,
                        PostGstDiscount = d.PostGstDisc ?? 0,
                        FinalAmount = d.FinalAmount,
                        TotalAmount = h.TotalAmount ?? 0
                    };

                var rawRows = await query.ToListAsync();

                var rows = rawRows
                    .GroupBy(x => new { x.DealerCode, x.DealerName, x.DealerCity, x.DealerState })
                    .Select(g => new TotalSaleReportDealerWiseViewModel
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        DealerCity = g.Key.DealerCity,
                        DealerState = g.Key.DealerState,
                        TotalUnitsSold = g.Count(),
                        CashCount = g.Count(x => string.Equals(x.SaleType, "Cash", StringComparison.OrdinalIgnoreCase)),
                        CreditCount = g.Count(x => string.Equals(x.SaleType, "Credit", StringComparison.OrdinalIgnoreCase)),
                        TotalItemRate = g.Sum(x => x.ItemRate),
                        TotalPreGstDiscount = g.Sum(x => x.PreGstDiscount),
                        TotalTaxableAmount = g.Sum(x => x.TaxableAmount),
                        TotalSgstAmount = g.Sum(x => x.SgstAmount),
                        TotalCgstAmount = g.Sum(x => x.CgstAmount),
                        TotalIgstAmount = g.Sum(x => x.IgstAmount),
                        TotalFameIIDiscount = g.Sum(x => x.FameIIDiscount),
                        TotalRegAmount = g.Sum(x => x.RegAmount),
                        TotalInsuranceAmount = g.Sum(x => x.InsuranceAmount),
                        TotalPostGstDiscount = g.Sum(x => x.PostGstDiscount),
                        TotalFinalAmount = g.Sum(x => x.FinalAmount),
                        TotalAmount = g.Sum(x => x.TotalAmount)
                    })
                    .OrderByDescending(x => x.TotalFinalAmount)
                    .ToList();

                var grandTotal = new TotalSaleReportDealerWiseViewModel
                {
                    DealerCode = "",
                    DealerName = "Grand Total",
                    TotalUnitsSold = rows.Sum(x => x.TotalUnitsSold),
                    CashCount = rows.Sum(x => x.CashCount),
                    CreditCount = rows.Sum(x => x.CreditCount),
                    TotalItemRate = rows.Sum(x => x.TotalItemRate),
                    TotalPreGstDiscount = rows.Sum(x => x.TotalPreGstDiscount),
                    TotalTaxableAmount = rows.Sum(x => x.TotalTaxableAmount),
                    TotalSgstAmount = rows.Sum(x => x.TotalSgstAmount),
                    TotalCgstAmount = rows.Sum(x => x.TotalCgstAmount),
                    TotalIgstAmount = rows.Sum(x => x.TotalIgstAmount),
                    TotalFameIIDiscount = rows.Sum(x => x.TotalFameIIDiscount),
                    TotalRegAmount = rows.Sum(x => x.TotalRegAmount),
                    TotalInsuranceAmount = rows.Sum(x => x.TotalInsuranceAmount),
                    TotalPostGstDiscount = rows.Sum(x => x.TotalPostGstDiscount),
                    TotalFinalAmount = rows.Sum(x => x.TotalFinalAmount),
                    TotalAmount = rows.Sum(x => x.TotalAmount)
                };

                return new TotalSaleReportDealerWiseResponse
                {
                    Rows = rows,
                    GrandTotal = grandTotal
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching total sale report (dealer wise): " + ex.Message, ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // MODEL WISE SALE REPORT (COUNT-WISE) 
        // ═════════════════════════════════════════════════════════════════════
        public async Task<ModelWiseSalePivotResponse> GetModelWiseSaleCountReportAsync(
     DateTime? fromDate, DateTime? toDate, string? dealerCode)
        {
            try
            {
                var canonicalModels = await _context.OemmodelMasters
                    .Where(x => x.IsActive)
                    .Select(x => x.ModelName)
                    .OrderBy(m => m)
                    .ToListAsync();

                var canonicalLookup = BuildNormalizedLookup(canonicalModels);

                var query =
                    from d in _context.VehicleSaleBillDetails

                    join h in _context.VehicleSaleBillHeaders
                        on d.VehicleSaleBillId equals h.Id

                    join im in _context.ItemMasters
                        on d.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join dm in _context.DealerMasters
                        on h.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    where
                        (!fromDate.HasValue || h.SaleDate.Date >= fromDate.Value.Date)
                        && (!toDate.HasValue || h.SaleDate.Date <= toDate.Value.Date)
                        && (!HasDealerFilter(dealerCode) || h.DealerCode == dealerCode)
                        && h.Status != null && h.Status.Trim().ToLower() == "invoiced"

                    select new
                    {
                        DealerCode = h.DealerCode ?? "Unknown",
                        DealerName = dm != null ? dm.Compname : (h.DealerCode ?? "Unknown"),
                        OemModelName = im != null ? im.Oemmodelname : null
                    };

                var rawRows = await query.ToListAsync();

                const string UnmappedModel = "Unmapped";

                var mapped = rawRows
                    .Select(x => new
                    {
                        x.DealerCode,
                        x.DealerName,
                        ModelName = ResolveCanonicalName(x.OemModelName, canonicalLookup) ?? UnmappedModel
                    })
                    .ToList();

                var actualModels = mapped.Select(x => x.ModelName).Distinct().ToList();
                bool hasUnmapped = actualModels.Contains(UnmappedModel);

                var modelNames = HasDealerFilter(dealerCode)
                    ? actualModels
                        .Where(m => m != UnmappedModel)
                        .OrderBy(m => m)
                        .Concat(hasUnmapped ? new[] { UnmappedModel } : Array.Empty<string>())
                        .ToList()
                    : canonicalModels
                        .Concat(hasUnmapped ? new[] { UnmappedModel } : Array.Empty<string>())
                        .ToList();

                var rows = mapped
                    .GroupBy(x => new { x.DealerCode, x.DealerName })
                    .Select(g => new ModelWiseSalePivotRow
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        ModelCounts = modelNames.ToDictionary(
                            m => m,
                            m => g.Count(x => x.ModelName == m)),
                        Total = g.Count()
                    })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                var columnTotals = modelNames.ToDictionary(
                    m => m,
                    m => rows.Sum(r => r.ModelCounts.TryGetValue(m, out var c) ? c : 0));

                return new ModelWiseSalePivotResponse
                {
                    ModelNames = modelNames,
                    Rows = rows,
                    ColumnTotals = columnTotals,
                    GrandTotal = rows.Sum(r => r.Total)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching model wise sale count report: " + ex.Message, ex);
            }
        }

        public async Task<PagedResponse<CurrentStockReportViewModel>> GetCurrentStockReportAsync(CurrentStockFilterModel filter)
        {
            try
            {
                static bool IsInvoiced(VehicleSaleBillHeader hdr) =>
                    hdr != null && hdr.Status == "Invoiced";

                var query =
                    from vi in _context.VehicleInwards.AsNoTracking()

                    join dm in _context.DealerMasters.AsNoTracking()
                        on vi.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    join im in _context.ItemMasters.AsNoTracking()
                        on vi.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join cm in _context.ColorMasters.AsNoTracking()
                        on vi.ColrCode equals cm.Colorcode into cmJoin
                    from cm in cmJoin.DefaultIfEmpty()

                    join lm in _context.LocationMasters.AsNoTracking()
                        on vi.LocCode equals lm.Loccode into lmJoin
                    from lm in lmJoin.DefaultIfEmpty()

                    join vsd in _context.VehicleSaleBillDetails.AsNoTracking()
                        on vi.ChasisNo equals vsd.ChassisNo into saleDetailJoin
                    from vsd in saleDetailJoin.DefaultIfEmpty()

                    join vsh in _context.VehicleSaleBillHeaders.AsNoTracking()
                        on vsd.VehicleSaleBillId equals vsh.Id into saleHeaderJoin
                    from vsh in saleHeaderJoin.DefaultIfEmpty()

                    select new
                    {
                        Vehicle = vi,
                        Dealer = dm,
                        Item = im,
                        Color = cm,
                        Sale = vsd,
                        SaleHdr = vsh,
                        LocationMaster = lm
                    };

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                {
                    query = query.Where(x =>
                        x.Vehicle.DealerCode == filter.DealerCode);
                }

                if (!string.IsNullOrWhiteSpace(filter.ModelCode))
                {
                    query = query.Where(x =>
                        x.Vehicle.ItemCode == filter.ModelCode);
                }

                if (!string.IsNullOrWhiteSpace(filter.ColorCode))
                {
                    query = query.Where(x =>
                        x.Vehicle.ColrCode == filter.ColorCode);
                }

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                {
                    query = query.Where(x =>
                        x.Vehicle.ChasisNo != null &&
                        x.Vehicle.ChasisNo.Contains(filter.ChassisNo));
                }

                var rawData = await query.ToListAsync();

                rawData = rawData
                    .Where(x => !IsInvoiced(x.SaleHdr))
                    .ToList();

                var result = rawData.Select((x, index) =>
                {
                    DateTime? invoiceDate = null;

                    if (x.Vehicle.InvoiceDate.HasValue)
                    {
                        invoiceDate =
                            x.Vehicle.InvoiceDate.Value
                                .ToDateTime(TimeOnly.MinValue);
                    }

                    string vehicleStatus =
                        x.SaleHdr != null ? "Allocated" : "Available";

                    return new CurrentStockReportViewModel
                    {
                        SrNo = index + 1,
                        DealerCode = x.Vehicle.DealerCode,
                        DealerName = x.Dealer != null ? x.Dealer.Compname : "",
                        ModelCode = x.Vehicle.ItemCode,
                        ModelName = x.Item != null ? x.Item.Itemname : "",
                        OEMModelName = x.Item != null ? x.Item.Oemmodelname : "",
                        ColorCode = x.Vehicle.ColrCode,
                        ColorName = x.Color != null ? x.Color.Colorname : "",
                        ChassisNo = x.Vehicle.ChasisNo,
                        MotorNo = x.Vehicle.MotorNo,
                        BatteryNo = x.Vehicle.BatteryNo,
                        BatteryNo2 = x.Vehicle.BatteryNo2,
                        BatteryNo3 = x.Vehicle.BatteryNo3,
                        BatteryNo4 = x.Vehicle.BatteryNo4,
                        BatteryNo5 = x.Vehicle.BatteryNo5,
                        BatteryNo6 = x.Vehicle.BatteryNo6,
                        ChargerNo = x.Vehicle.ChargerNo,
                        ControllerNo = x.Vehicle.ControllerNo,
                        RegisterNo = x.Sale != null ? x.Sale.RegNo : "",
                        InvoiceNo = x.Vehicle.InvoiceNo,
                        DispatchDate = invoiceDate,
                        ReceiveDate = invoiceDate,
                        VehicleStatus = vehicleStatus,
                        StockStatus = x.SaleHdr != null ? "Allocated" : "In Stock",
                        LocationCode = x.Vehicle.LocCode,
                        LocationName = x.LocationMaster != null ? x.LocationMaster.Locname : "",
                        CurrentLocation = x.LocationMaster != null ? x.LocationMaster.Locname : x.Vehicle.LocCode,
                        PurchaseRate = 0,
                        EstimatedSaleRate = 0,
                        IsBilled = false,
                        DaysInStock = invoiceDate.HasValue ? (DateTime.Now - invoiceDate.Value).Days : 0
                    };
                });

                if (!string.IsNullOrWhiteSpace(filter.StockStatus))
                {
                    result = result.Where(x => x.VehicleStatus == filter.StockStatus);
                }

                if (filter.FromDate.HasValue)
                {
                    result = result.Where(x =>
                        x.ReceiveDate.HasValue &&
                        x.ReceiveDate.Value.Date >= filter.FromDate.Value.Date);
                }

                if (filter.ToDate.HasValue)
                {
                    result = result.Where(x =>
                        x.ReceiveDate.HasValue &&
                        x.ReceiveDate.Value.Date <= filter.ToDate.Value.Date);
                }

                var totalRecords = result.Count();

                var data = result
                    .OrderByDescending(x => x.ReceiveDate)
                    .ThenBy(x => x.DealerName)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                return new PagedResponse<CurrentStockReportViewModel>
                {
                    Data = data,
                    TotalRecords = totalRecords
                };
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error fetching current stock report: "
                    + ex.Message,
                    ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // MODEL-WISE CURRENT STOCK (COUNT-WISE)
        // ═════════════════════════════════════════════════════════════════════
        public async Task<ModelWiseStockPivotResponse> GetModelWiseStockCountReportAsync(
    string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                static bool IsInvoiced(VehicleSaleBillHeader hdr) =>
                    hdr != null && hdr.Status == "Invoiced";

                var canonicalModels = await _context.OemmodelMasters
                    .Where(x => x.IsActive)
                    .Select(x => x.ModelName)
                    .OrderBy(m => m)
                    .ToListAsync();

                var canonicalLookup = BuildNormalizedLookup(canonicalModels);

                var query =
                    from vi in _context.VehicleInwards.AsNoTracking()

                    join dm in _context.DealerMasters.AsNoTracking()
                        on vi.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    join im in _context.ItemMasters.AsNoTracking()
                        on vi.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join vsd in _context.VehicleSaleBillDetails.AsNoTracking()
                        on vi.ChasisNo equals vsd.ChassisNo into saleDetailJoin
                    from vsd in saleDetailJoin.DefaultIfEmpty()

                    join vsh in _context.VehicleSaleBillHeaders.AsNoTracking()
                        on vsd.VehicleSaleBillId equals vsh.Id into saleHeaderJoin
                    from vsh in saleHeaderJoin.DefaultIfEmpty()

                    select new { Vehicle = vi, Dealer = dm, Item = im, SaleHdr = vsh };

                if (HasDealerFilter(dealerCode))
                    query = query.Where(x => x.Vehicle.DealerCode == dealerCode);

                if (fromDate.HasValue)
                {
                    var fromDateOnly = DateOnly.FromDateTime(fromDate.Value.Date);
                    query = query.Where(x =>
                        x.Vehicle.InvoiceDate.HasValue &&
                        x.Vehicle.InvoiceDate.Value >= fromDateOnly);
                }

                if (toDate.HasValue)
                {
                    var toDateOnly = DateOnly.FromDateTime(toDate.Value.Date);
                    query = query.Where(x =>
                        x.Vehicle.InvoiceDate.HasValue &&
                        x.Vehicle.InvoiceDate.Value <= toDateOnly);
                }

                var rawRows = await query.ToListAsync();

                rawRows = rawRows.Where(x => !IsInvoiced(x.SaleHdr)).ToList();

                var mapped = rawRows
                    .Where(x => x.Item != null && !string.IsNullOrWhiteSpace(x.Item.Oemmodelname))
                    .Select(x => new
                    {
                        DealerCode = x.Vehicle.DealerCode ?? "Unknown",
                        DealerName = x.Dealer != null ? x.Dealer.Compname : (x.Vehicle.DealerCode ?? "Unknown"),
                        ModelName = ResolveCanonicalName(x.Item!.Oemmodelname, canonicalLookup)
                    })
                    .Where(x => x.ModelName != null)
                    .Select(x => new { x.DealerCode, x.DealerName, ModelName = x.ModelName! })
                    .ToList();

                var modelNames = HasDealerFilter(dealerCode)
                               ? mapped.Select(x => x.ModelName).Distinct().OrderBy(m => m).ToList()
                               : canonicalModels;

                var rows = mapped
                    .GroupBy(x => new { x.DealerCode, x.DealerName })
                    .Select(g => new ModelWiseStockPivotRow
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        ModelCounts = modelNames.ToDictionary(
                            m => m,
                            m => g.Count(x => x.ModelName == m)),
                        Total = g.Count()
                    })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                var columnTotals = modelNames.ToDictionary(
                    m => m,
                    m => rows.Sum(r => r.ModelCounts.TryGetValue(m, out var c) ? c : 0));

                return new ModelWiseStockPivotResponse
                {
                    ModelNames = modelNames,
                    Rows = rows,
                    ColumnTotals = columnTotals,
                    GrandTotal = rows.Sum(r => r.Total)
                };
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error fetching model-wise current stock count report: "
                    + ex.Message,
                    ex);
            }
        }


        public async Task<PagedResponse<POTrackingReportViewModel>> GetPOTrackingReportAsync(POTrackingFilterModel filter)
        {
            try
            {
                var query =
                    from po in _context.PurchaseOrders

                    join dealer in _context.DealerMasters
                        on po.CustomerCode equals dealer.Dealercode
                        into dealerGroup

                    from dealer in dealerGroup.DefaultIfEmpty()

                    join loc in _context.LocationMasters
                        on po.LocCode equals loc.Loccode
                        into locationGroup

                    from loc in locationGroup.DefaultIfEmpty()

                    select new
                    {
                        po,
                        dealer,
                        loc
                    };

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                {
                    query = query.Where(x => x.po.CustomerCode == filter.DealerCode);
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(x => x.po.PurchaseDate >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(x => x.po.PurchaseDate <= filter.ToDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.POType))
                {
                    query = query.Where(x => x.po.OrderType == filter.POType);
                }

                if (!string.IsNullOrWhiteSpace(filter.POStatus))
                {
                    bool isActive = filter.POStatus == "Active";
                    query = query.Where(x => x.po.Status == isActive);
                }

                var totalRecords = await query.CountAsync();

                var rawData = await query
                    .OrderByDescending(x => x.po.PurchaseDate)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                int srNo = ((filter.PageIndex - 1) * filter.PageSize) + 1;

                var result = rawData.Select(x =>
                {
                    var poDetails = _context.PurchaseOrderDetails
                        .Where(d => d.Ponumber == x.po.Ponumber)
                        .ToList();

                    decimal poQty = poDetails.Sum(d => d.Qty ?? 0);
                    decimal poPrice = poDetails.Sum(d => d.LineAmount ?? 0);
                    decimal billedQty = 0;
                    decimal billedPrice = 0;
                    decimal pendingQty = poQty - billedQty;
                    decimal pendingPrice = poPrice - billedPrice;

                    return new POTrackingReportViewModel
                    {
                        SrNo = srNo++,
                        DealerName = x.dealer != null ? x.dealer.Compname : "",
                        DealerCode = x.po.CustomerCode,
                        LocationName = x.loc != null ? x.loc.Locname : "",
                        OrderNumber = x.po.Ponumber,
                        OrderDate = x.po.PurchaseDate,
                        SubmitToERPDate = x.po.UpdatedDate,
                        POType = x.po.OrderType,
                        POQty = poQty,
                        BilledQty = billedQty,
                        PendingQty = pendingQty,
                        Archived = 0,
                        POPrice = poPrice,
                        BilledPrice = billedPrice,
                        PendingPOPrice = pendingPrice,
                        ArchivedPriceExclGST = 0,
                        POStatus = x.po.Status ? "Active" : "Inactive",
                        UniqueId = x.po.Id.ToString(),
                        DealerPONo = x.po.Ponumber,
                        WalletDebit = 0,
                        PGDebit = 0,
                        PGStatus = "",
                        PaymentLink = "",
                        PaymentType = "",
                        TempPONo = x.po.Ponumber,
                        MerchantOrderNo = "",
                        MerchantOrderStatus = ""
                    };
                })
                .ToList();

                return new PagedResponse<POTrackingReportViewModel>
                {
                    Data = result,
                    TotalRecords = totalRecords
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching PO Tracking Report", ex);
            }
        }

        public async Task<List<string>> GetPOTypeDropdownAsync()
        {
            return await _context.PurchaseOrders
                .Where(x => x.OrderType != null && x.OrderType != "")
                .Select(x => x.OrderType!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<List<string>> GetPOStatusDropdownAsync()
        {
            return await Task.FromResult(new List<string>
                {
                    "Active",
                    "Inactive"
                });
        }


        // ═════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ═════════════════════════════════════════════════════════════════════
        public async Task<List<PartsDispatchReportViewModel>> GetPartsDispatchReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode)
        {
            try
            {
                var query = await (
                    from vd in _context.VehicleInwards.AsNoTracking()

                    join dm in _context.DealerMasters.AsNoTracking()
                        on vd.DealerCode equals dm.Dealercode into dealerGroup
                    from dm in dealerGroup.DefaultIfEmpty()

                    join im in _context.ItemMasters.AsNoTracking()
                        on vd.ItemCode equals im.Itemcode into itemGroup
                    from im in itemGroup.DefaultIfEmpty()

                    join jc in _context.JobCardCustomers.AsNoTracking()
                        on vd.ChasisNo equals jc.ChassisNo into customerGroup
                    from jc in customerGroup.DefaultIfEmpty()

                    join jh in _context.JobCardHeaders.AsNoTracking()
                        on jc.JobCardHeaderId equals jh.Id into jobGroup
                    from jh in jobGroup.DefaultIfEmpty()

                    select new PartsDispatchReportViewModel
                    {
                        DealerName = dm != null ? dm.Compname : null,
                        DealerCode = dm != null ? dm.Dealercode : null,
                        CustomerName = jc != null ? jc.CustomerName : null,
                        MobileNo = jc != null ? jc.CustomerMobile : null,
                        City = dm != null ? dm.City : null,
                        State = dm != null ? dm.State : null,
                        VehicleModel = im != null ? im.Itemname : null,
                        VehicleVIN = vd.ChasisNo,
                        BatteryMasterName = vd.BatteryMake,
                        DateOfSale = jc != null ? jc.SaleDate : null,
                        PartName = im != null ? im.Itemname : null,
                        DeviceGroup = im != null ? im.Itemtype.ToString() : null,
                        DeviceType = im != null ? im.Vehtype.ToString() : null,
                        ItemDescription = im != null ? im.Itemdesc : null,
                        VehicleStandardWarrantyMonths = 36,
                        VehicleStandardWarrantyODOReading = 30000,
                        VehicleExtendedWarrantyMonths = 12,
                        VehicleExtendedWarrantyODOReading = 10000,
                        StandardWarrantyExpiryDate =
                            jc != null && jc.SaleDate.HasValue ? jc.SaleDate.Value.AddMonths(36) : null,
                        ExtendedWarrantyExpiryDate =
                            jc != null && jc.SaleDate.HasValue ? jc.SaleDate.Value.AddMonths(48) : null,
                        LastODOReadingDate = jh != null ? jh.CreatedDate : null,
                        ODOReadingLastDate = jh != null ? (jh.Vehiclekms ?? 0) : 0,
                        CurrentWarrantyStatusDate =
                            jc != null && jc.SaleDate.HasValue && jc.SaleDate.Value.AddMonths(36) >= DateTime.Now
                                ? "In Warranty" : "Expired",
                        CurrentWarrantyStatusODO =
                            jh != null && (jh.Vehiclekms ?? 0) <= 30000 ? "In Warranty" : "Expired",
                        FinalWarrantyStatus =
                            jc != null && jc.SaleDate.HasValue && jc.SaleDate.Value.AddMonths(36) >= DateTime.Now
                            && jh != null && (jh.Vehiclekms ?? 0) <= 30000 ? "Active" : "Expired"
                    }).ToListAsync();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    query = query.Where(x => x.DealerCode == dealerCode).ToList();
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.DateOfSale.HasValue && x.DateOfSale.Value.Date >= fromDate.Value.Date).ToList();
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.DateOfSale.HasValue && x.DateOfSale.Value.Date <= toDate.Value.Date).ToList();
                }

                int srNo = 1;

                query = query.OrderByDescending(x => x.DateOfSale).ToList();

                query.ForEach(x => { x.SrNo = srNo++; });

                return query;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching Parts Dispatch Report", ex);
            }
        }

        public async Task<List<object>> GetDealerListAsync()
        {
            try
            {
                return await _context.DealerMasters
                    .AsNoTracking()
                    .Select(x => new
                    {
                        dealerCode = x.Dealercode,
                        dealerName = x.Compname
                    })
                    .OrderBy(x => x.dealerName)
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching dealer list", ex);
            }
        }

        public async Task<List<object>> GetModelListAsync()
        {
            try
            {
                var data = await (
                    from vi in _context.VehicleInwards
                    join im in _context.ItemMasters
                        on vi.ItemCode equals im.Itemcode
                    where
                        !string.IsNullOrEmpty(im.Itemname)
                        && !im.Itemname.Contains("BRAKE")
                        && !im.Itemname.Contains("LEVER")
                        && !im.Itemname.Contains("BUZZER")
                        && !im.Itemname.Contains("CONTROLLER")
                        && !im.Itemname.Contains("HARNESS")
                        && !im.Itemname.Contains("CABLE")
                        && !im.Itemname.Contains("SWITCH")
                        && !im.Itemname.Contains("FOOTREST")
                        && !im.Itemname.Contains("DRUM")
                        && !im.Itemname.Contains("TOOL")
                        && !im.Itemname.Contains("PART")
                        && !im.Itemname.Contains("KIT")
                    select new
                    {
                        modelCode = im.Itemcode,
                        modelName = im.Itemname
                    }
                )
                .Distinct()
                .OrderBy(x => x.modelName)
                .ToListAsync();

                return data.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching model list", ex);
            }
        }

        public async Task<List<object>> GetModelListByDealerAsync(string dealerCode)
        {
            try
            {
                var data = await (
                    from vi in _context.VehicleInwards
                    join im in _context.ItemMasters
                        on vi.ItemCode equals im.Itemcode
                    where vi.DealerCode == dealerCode
                    select new
                    {
                        modelCode = im.Itemcode,
                        modelName = im.Itemname
                    }
                )
                .Distinct()
                .OrderBy(x => x.modelName)
                .ToListAsync();

                return data.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching model list", ex);
            }
        }

        public async Task<List<string>> GetChassisListAsync()
        {
            try
            {
                return await _context.VehicleInwards
                    .AsNoTracking()
                    .Where(x => x.ChasisNo != null)
                    .Select(x => x.ChasisNo!)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching chassis list", ex);
            }
        }

        public async Task<List<PartDispatchKitReportViewModel>> GetPartDispatchKitReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode)
        {
            try
            {
                var query =
                    from po in _context.PurchaseOrders
                    join dealer in _context.DealerMasters
                        on po.CustomerCode equals dealer.Dealercode
                        into dealerGroup
                    from dealer in dealerGroup.DefaultIfEmpty()
                    where po.Id > 0
                    select new PartDispatchKitReportViewModel
                    {
                        PONumber = po.Ponumber,
                        PODate = po.PurchaseDate,
                        SubmitToERPDate = po.UpdatedDate,
                        POType = (po.TransactionType ?? "") + "-" + (po.OrderType ?? ""),
                        CompanyName = dealer != null ? dealer.Compname : "",
                        MobileNo = dealer != null ? dealer.Mobile : "",
                        DealerCode = dealer != null ? dealer.Dealercode : "",
                        DealerCity = dealer != null ? dealer.City : "",
                        DealerState = dealer != null ? dealer.State : "",
                        LocationCode = po.LocCode,
                        LocationName = dealer != null ? dealer.Compname : "",
                        LocationCity = dealer != null ? dealer.City : ""
                    };

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    query = query.Where(x => x.DealerCode == dealerCode);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.PODate.HasValue && x.PODate.Value.Date >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.PODate.HasValue && x.PODate.Value.Date <= toDate.Value.Date);
                }

                var result = await query.OrderByDescending(x => x.PODate).ToListAsync();

                for (int i = 0; i < result.Count; i++)
                {
                    result[i].SrNo = i + 1;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching Part Dispatch Kit Report", ex);
            }
        }

        public async Task<List<string>> GetPartDispatchKitPOTypeDropdownAsync()
        {
            try
            {
                return await _context.PurchaseOrders
                    .Where(x => x.TransactionType != null && x.OrderType != null)
                    .Select(x => x.TransactionType + "-" + x.OrderType)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo)
        {
            try
            {
                var data = await (
                    from vi in _context.VehicleInwards
                    join im in _context.ItemMasters
                        on vi.ItemCode equals im.Itemcode
                    join f2 in _context.Form22Masters
                        on (im.Oemmodelname ?? "").Trim().ToLower()
                        equals (f2.OemModelName ?? "").Trim().ToLower()
                        into formGroup
                    from f2 in formGroup.DefaultIfEmpty()
                    where vi.ChasisNo == chassisNo
                    select new Form22SlipViewModel
                    {
                        ChassisNo = chassisNo,
                        TypeApprovalCertNo = f2.ApprovalCertificateNo,
                        BrandName = im.Itemname,
                        MotorNo = _context.ChassisBatteryDetails
                            .Where(x => x.ChassisNo == vi.ChasisNo && x.MotorOrderNo == 1)
                            .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                            .Select(x => x.MotorNo)
                            .FirstOrDefault(),
                        Emission = "",
                        SoundLevelHorn = f2.SoundLevelHorn,
                        NoiseLevel = f2.PassbyNoiseLevel
                    }
                ).FirstOrDefaultAsync();

                return data;
            }
            catch
            {
                throw;
            }
        }

        public async Task<VehicleSaleBillReportResponse> GetVehicleSaleBillReportAsync(VehicleSaleBillReportFilterModel filter)
        {
            try
            {
                var baseQuery =
                    from vd in _context.VehicleSaleBillDetails

                    join vh in _context.VehicleSaleBillHeaders
                        on vd.VehicleSaleBillId equals vh.Id

                    join im in _context.ItemMasters
                        on vd.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join dm in _context.DealerMasters
                        on vh.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    join cust in _context.LedgerMasters
                        on vh.LedgerId equals cust.Id into custJoin
                    from cust in custJoin.DefaultIfEmpty()

                    join city in _context.Cities
                        on cust.City equals city.CityId into cityJoin
                    from city in cityJoin.DefaultIfEmpty()

                    join state in _context.States
                        on cust.State equals state.StateId into stateJoin
                    from state in stateJoin.DefaultIfEmpty()

                    join occ in _context.OccupationMasters
                        on cust.OccupationId equals occ.Id into occJoin
                    from occ in occJoin.DefaultIfEmpty()

                    select new { vd, vh, im, dm, cust, city, state, occ };
                if (HasDealerFilter(filter.DealerCode))
                    baseQuery = baseQuery.Where(x => x.vh.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                    baseQuery = baseQuery.Where(x => x.vh.SaleDate.Date >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    baseQuery = baseQuery.Where(x => x.vh.SaleDate.Date <= filter.ToDate.Value.Date);

                if (!string.IsNullOrWhiteSpace(filter.SaleType))
                    baseQuery = baseQuery.Where(x => x.vh.SaleType == filter.SaleType);

                if (!string.IsNullOrWhiteSpace(filter.CustomerType))
                    baseQuery = baseQuery.Where(x => x.vh.CustomerType == filter.CustomerType);

                if (filter.BillType.HasValue)
                    baseQuery = baseQuery.Where(x => x.vh.BillType == filter.BillType.Value);

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    baseQuery = baseQuery.Where(x => x.vh.Status == filter.Status);

                if (!string.IsNullOrWhiteSpace(filter.SaleBillNo))
                    baseQuery = baseQuery.Where(x => x.vh.SaleBillNo != null && x.vh.SaleBillNo.Contains(filter.SaleBillNo));

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                    baseQuery = baseQuery.Where(x => x.vd.ChassisNo != null && x.vd.ChassisNo.Contains(filter.ChassisNo));

                var rawRows = await baseQuery.ToListAsync();
                var financierLookup = await GetFinancierNameLookupAsync();
                var chassisNos = rawRows.Select(x => x.vd.ChassisNo).ToList();
                var inwardLookup = await GetLatestVehicleInwardByChassisAsync(chassisNos);
                var locationLookup = await GetLocationLookupAsync();
                var colorLookup = await GetColorLookupAsync();


                var rows = rawRows.Select(x =>
                {
                    var vi = x.vd.ChassisNo != null && inwardLookup.TryGetValue(x.vd.ChassisNo, out var vinw) ? vinw : null;
                    var clr = vi?.ColrCode != null && colorLookup.TryGetValue(vi.ColrCode, out var c) ? c : null;
                    var locM = vi?.LocCode != null && locationLookup.TryGetValue(vi.LocCode, out var l) ? l : null;


                    return new VehicleSaleBillReportViewModel
                    {
                        SaleBillId = x.vh.Id,
                        SaleBillNo = x.vh.SaleBillNo,
                        SaleDate = x.vh.SaleDate,
                        Status = x.vh.Status,
                        Location = locM != null ? locM.Locname : x.vh.Location,
                        DealerCode = x.vh.DealerCode,
                        DealerName = x.dm?.Compname,
                        CustomerName = x.vh.CustomerName ?? x.cust?.LedgerName,
                        BillingName = x.vh.BillingName,
                        CustomerType = x.vh.CustomerType,
                        SaleType = x.vh.SaleType,
                        BillType = x.vh.BillType,
                        Financier = x.vh.Financier.HasValue && financierLookup.TryGetValue(x.vh.Financier.Value, out var finName)
                            ? finName : null,
                        FinancierId = x.vh.Financier,
                        SalesExecutive = x.vh.SalesExecutive,
                        CustomerMobile = x.cust?.MobileNumber,
                        CustomerCity = x.city?.CityName,
                        CustomerState = x.state?.StateName,

                        ChassisNo = x.vd.ChassisNo,
                        MotorNo = vi?.MotorNo,
                        ItemCode = x.vd.ItemCode,
                        ModelName = x.im?.Itemname ?? x.vd.ModelName,
                        OemModelName = x.im?.Oemmodelname,
                        Colour = clr?.Colorname ?? x.vd.Colour,
                        Hsn = x.im?.Hsncode,
                        MfgYear = x.vd.MfgYear ?? vi?.MfgYear,
                        RegNo = x.vd.RegNo,
                        InsNo = x.vd.InsNo,
                        ItemRate = x.vd.ItemRate,
                        PreGstDiscount = x.vd.PreGstDiscount ?? 0,
                        TaxableAmount = x.vd.ItemRate - (x.vd.PreGstDiscount ?? 0),
                        SgstPer = x.vd.Sgstper ?? 0,
                        SgstAmount = x.vd.Sgstamnt ?? 0,
                        CgstPer = x.vd.Cgstper ?? 0,
                        CgstAmount = x.vd.Cgstamnt ?? 0,
                        IgstPer = x.vd.Igstper ?? 0,
                        IgstAmount = x.vd.Igstamnt ?? 0,
                        FameIIDiscount = x.vd.FameIi ?? 0,
                        RegAmount = x.vd.RegAmount ?? 0,
                        InsuranceAmount = x.vd.InsuranceAmount ?? 0,
                        PostGstDiscount = x.vd.PostGstDisc ?? 0,
                        FinalAmount = x.vd.FinalAmount,
                        Battery = x.vd.Battery,
                        ChargerNo = x.vd.ChargerNo,
                        ControllerNo = x.vd.ControllerNo,
                        Vcu = x.vd.Vcu,
                        PartyEmail = x.cust?.EMail,
                        Gender = x.cust?.Gender,
                        Dob = x.cust != null && x.cust.DateOfBirth.HasValue
                            ? x.cust.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue) : null,
                        AccountType = x.cust?.LedgerType,
                        Occupation = x.occ != null ? x.occ.OccupationName : null,
                        BatteryMake = vi?.BatteryMake,
                        BatteryType = vi?.BatteryChemistry,
                        DealerCity = x.dm?.City,
                        DealerState = x.dm?.State,
                        LocCode = vi?.LocCode,
                        LocCity = locM != null ? locM.City : x.dm?.City,
                        Pin = x.cust?.Pin,
                        PartyAddress = x.cust?.Address,
                        ColorCode = clr?.Colorcode,
                        VehicleGroup = x.im?.Itemdesc,
                        SubsidyAmnt = x.im?.Fame2amount ?? 0,
                        FameIIRequired = (x.im?.Fame2amount ?? 0) > 0,
                        ProspectName = x.vh.RefName,
                        BookingId = x.vh.BookingId,
                        TotalAmount = x.vh.TotalAmount ?? 0,
                        DispatchDate = vi != null && vi.InvoiceDate.HasValue
                            ? vi.InvoiceDate.Value.ToDateTime(TimeOnly.MinValue) : null
                    };
                }).ToList();

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var s = filter.Search.Trim().ToLower();
                    rows = rows.Where(x =>
                        (x.SaleBillNo ?? "").ToLower().Contains(s) ||
                        (x.CustomerName ?? "").ToLower().Contains(s) ||
                        (x.BillingName ?? "").ToLower().Contains(s) ||
                        (x.ChassisNo ?? "").ToLower().Contains(s) ||
                        (x.ModelName ?? "").ToLower().Contains(s) ||
                        (x.RegNo ?? "").ToLower().Contains(s)
                    ).ToList();
                }

                rows = rows.OrderByDescending(x => x.SaleDate).ThenByDescending(x => x.SaleBillNo).ToList();

                var response = new VehicleSaleBillReportResponse
                {
                    TotalRecords = rows.Count,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    TotalItemRate = rows.Sum(x => x.ItemRate),
                    TotalTaxable = rows.Sum(x => x.TaxableAmount),
                    TotalSgst = rows.Sum(x => x.SgstAmount),
                    TotalCgst = rows.Sum(x => x.CgstAmount),
                    TotalIgst = rows.Sum(x => x.IgstAmount),
                    TotalFameII = rows.Sum(x => x.FameIIDiscount),
                    TotalRegistration = rows.Sum(x => x.RegAmount),
                    TotalInsurance = rows.Sum(x => x.InsuranceAmount),
                    GrandTotal = rows.Sum(x => x.FinalAmount)
                };

                var paged = rows.Skip((filter.PageIndex - 1) * filter.PageSize).Take(filter.PageSize).ToList();

                int srNo = ((filter.PageIndex - 1) * filter.PageSize) + 1;
                foreach (var r in paged) r.SrNo = srNo++;

                response.Data = paged;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching vehicle sale bill report: " + ex.Message, ex);
            }
        }

        public async Task<List<VehicleSaleBillReportViewModel>> GetVehicleSaleBillReportForExportAsync(
            string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var filter = new VehicleSaleBillReportFilterModel
                {
                    DealerCode = dealerCode,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PageIndex = 1,
                    PageSize = int.MaxValue
                };

                var result = await GetVehicleSaleBillReportAsync(filter);
                return result.Data;
            }
            catch (Exception ex)
            {
                throw new Exception("Error exporting vehicle sale bill report: " + ex.Message, ex);
            }
        }

        public async Task<List<string>> GetSaleTypeDropdownAsync()
        {
            try
            {
                return await _context.VehicleSaleBillHeaders
                    .AsNoTracking()
                    .Where(x => x.SaleType != null && x.SaleType != "")
                    .Select(x => x.SaleType!)
                    .Distinct().OrderBy(x => x).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching sale type dropdown: " + ex.Message, ex);
            }
        }

        public async Task<List<string>> GetSaleBillStatusDropdownAsync()
        {
            try
            {
                return await _context.VehicleSaleBillHeaders
                    .AsNoTracking()
                    .Where(x => x.Status != null && x.Status != "")
                    .Select(x => x.Status!)
                    .Distinct().OrderBy(x => x).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching status dropdown: " + ex.Message, ex);
            }
        }

        public async Task<CounterBillPrintViewModel?> GetCounterBillPrintById(int id)
        {
            try
            {
                var header = await _context.CounterBillHeaders
                    .Include(x => x.CounterBillDetails)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                string? cityName = null;

                if (header == null)
                    return null;

                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(x => x.Dealercode == header.DealerCode);

                LedgerMaster? customer = null;

                if (header.CustomerLedgerId.HasValue)
                {
                    customer = await _context.LedgerMasters
                        .FirstOrDefaultAsync(x => x.Id == header.CustomerLedgerId.Value);
                    if (customer != null)
                    {
                        cityName = await _context.Cities
                            .Where(i => i.CityId == customer.City)
                            .Select(i => i.CityName)
                            .FirstOrDefaultAsync();
                    }
                }

                var stateName = await _context.States
                    .Where(x => x.StateId == header.PartyState)
                    .Select(x => x.StateName)
                    .FirstOrDefaultAsync();

                var partCodes = header.CounterBillDetails
                    .Select(x => x.PartCode)
                    .Distinct()
                    .ToList();

                var itemLookup = await _context.ItemMasters
                            .ToDictionaryAsync(
                                x => x.Itemcode,
                                x => new
                                {
                                    x.Itemname,
                                    x.Hsncode,
                                });

                return new CounterBillPrintViewModel
                {
                    DealerCode = header.DealerCode,
                    DealerName = dealer?.Compname,
                    DealerAddress1 = dealer?.Adress1,
                    DealerAddress2 = dealer?.Adress2,
                    Pin = dealer?.Pin,
                    PhoneNo1 = dealer?.Mobile,
                    PhoneNo2 = dealer?.PhoneOff,
                    PanNo = dealer?.Pan,
                    GSTNo = dealer?.CompgstinNo,
                    ChassisNo = header?.ChassisNo,
                    BillType = header?.BillType,
                    City = cityName,
                    Remarks = header.Remarks,
                    InvoiceNo = header.BillNo,
                    InvoiceDate = header.BillDate,
                    CustomerName = header.PartyName,
                    CustomerMobile = header.MobileNo,
                    CustomerAddress = customer?.Address,
                    CustomerGST = customer?.Gstno,
                    State = stateName,
                    TermsAndConditions = await _context.TermandConditionMasters
                                        .Where(i => i.ConditionModule == 7 &&
                                                    i.ConditionEffectiveDate <= header.BillDate)
                                        .OrderBy(i => i.Id)
                                        .Select(i => new SalesConditionViewModel
                                        {
                                            Id = i.Id,
                                            SrNo = null,
                                            ConditionText = i.TermCondition
                                        })
                                        .ToListAsync(),
                    Details = header.CounterBillDetails
                            .Select(d =>
                            {
                                itemLookup.TryGetValue(d.PartCode, out var item);

                                return new CounterBillDetailsViewModel
                                {
                                    PartCode = d.PartCode,
                                    PartName = item?.Itemname,
                                    HSNCode = item?.Hsncode,
                                    SaleType = d.SaleType,
                                    Qty = d.Qty,
                                    Rate = d.Rate,
                                    DiscType = d.DiscType,
                                    Discount = d.Discount,
                                    Mrp = d.Mrp,
                                    Igstper = d.Igstper,
                                    Igstamnt = d.Igstamnt,
                                    Cgstper = d.Cgstper,
                                    Cgstamnt = d.Cgstamnt,
                                    Sgstper = d.Sgstper,
                                    Sgstamnt = d.Sgstamnt
                                };
                            })
                            .ToList()
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<VehicleSaleBillReportResponse> GetVehicleSaleBillOnlyReportAsync(VehicleSaleBillReportFilterModel filter)
        {
            try
            {
                var baseQuery =
                    from vd in _context.VehicleSaleBillDetails.AsNoTracking()

                    join vh in _context.VehicleSaleBillHeaders.AsNoTracking()
                        on vd.VehicleSaleBillId equals vh.Id

                    join im in _context.ItemMasters.AsNoTracking()
                        on vd.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join dm in _context.DealerMasters.AsNoTracking()
                        on vh.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    join cust in _context.LedgerMasters.AsNoTracking()
                        on vh.LedgerId equals cust.Id into custJoin
                    from cust in custJoin.DefaultIfEmpty()

                    join city in _context.Cities.AsNoTracking()
                        on cust.City equals city.CityId into cityJoin
                    from city in cityJoin.DefaultIfEmpty()

                    join state in _context.States.AsNoTracking()
                        on cust.State equals state.StateId into stateJoin
                    from state in stateJoin.DefaultIfEmpty()

                    join occ in _context.OccupationMasters.AsNoTracking()
                        on cust.OccupationId equals occ.Id into occJoin
                    from occ in occJoin.DefaultIfEmpty()

                    select new { vd, vh, im, dm, cust, city, state, occ };

                if (HasDealerFilter(filter.DealerCode))
                    baseQuery = baseQuery.Where(x => x.vh.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                    baseQuery = baseQuery.Where(x => x.vh.SaleDate.Date >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    baseQuery = baseQuery.Where(x => x.vh.SaleDate.Date <= filter.ToDate.Value.Date);

                if (!string.IsNullOrWhiteSpace(filter.SaleType))
                    baseQuery = baseQuery.Where(x => x.vh.SaleType == filter.SaleType);

                if (!string.IsNullOrWhiteSpace(filter.CustomerType))
                    baseQuery = baseQuery.Where(x => x.vh.CustomerType == filter.CustomerType);

                if (filter.BillType.HasValue)
                    baseQuery = baseQuery.Where(x => x.vh.BillType == filter.BillType.Value);

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    baseQuery = baseQuery.Where(x => x.vh.Status == filter.Status);

                if (!string.IsNullOrWhiteSpace(filter.SaleBillNo))
                    baseQuery = baseQuery.Where(x => x.vh.SaleBillNo != null && x.vh.SaleBillNo.Contains(filter.SaleBillNo));

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                    baseQuery = baseQuery.Where(x => x.vd.ChassisNo != null && x.vd.ChassisNo.Contains(filter.ChassisNo));

                var rawRows = await baseQuery.ToListAsync();
                var financierLookup = await GetFinancierNameLookupAsync();

                var chassisNos = rawRows.Select(x => x.vd.ChassisNo).ToList();
                var inwardLookup = await GetLatestVehicleInwardByChassisAsync(chassisNos);
                var locationLookup = await GetLocationLookupAsync();
                var colorLookup = await GetColorLookupAsync();


                var rows = rawRows.Select(x =>
                {
                    var vi = x.vd.ChassisNo != null && inwardLookup.TryGetValue(x.vd.ChassisNo, out var vinw) ? vinw : null;
                    var clr = vi?.ColrCode != null && colorLookup.TryGetValue(vi.ColrCode, out var c) ? c : null;
                    var locM = vi?.LocCode != null && locationLookup.TryGetValue(vi.LocCode, out var l) ? l : null;


                    return new VehicleSaleBillReportViewModel
                    {
                        SaleBillId = x.vh.Id,
                        SaleBillNo = x.vh.SaleBillNo,
                        SaleDate = x.vh.SaleDate,
                        Status = x.vh.Status,
                        Location = locM != null ? locM.Locname : x.vh.Location,
                        DealerCode = x.vh.DealerCode,
                        DealerName = x.dm?.Compname,
                        CustomerName = x.vh.CustomerName ?? x.cust?.LedgerName,
                        BillingName = x.vh.BillingName,
                        CustomerType = x.vh.CustomerType,
                        SaleType = x.vh.SaleType,
                        BillType = x.vh.BillType,
                        Financier = x.vh.Financier.HasValue && financierLookup.TryGetValue(x.vh.Financier.Value, out var finName)
                            ? finName : null,
                        FinancierId = x.vh.Financier,
                        SalesExecutive = x.vh.SalesExecutive,
                        CustomerMobile = x.cust?.MobileNumber,
                        CustomerCity = x.city?.CityName,
                        CustomerState = x.state?.StateName,

                        ChassisNo = x.vd.ChassisNo,
                        MotorNo = vi?.MotorNo,
                        ItemCode = x.vd.ItemCode,
                        ModelName = x.im?.Itemname ?? x.vd.ModelName,
                        OemModelName = x.im?.Oemmodelname,
                        Colour = clr?.Colorname ?? x.vd.Colour,
                        Hsn = x.im?.Hsncode,
                        MfgYear = x.vd.MfgYear ?? vi?.MfgYear,
                        RegNo = x.vd.RegNo,
                        InsNo = x.vd.InsNo,
                        ItemRate = x.vd.ItemRate,
                        PreGstDiscount = x.vd.PreGstDiscount ?? 0,
                        TaxableAmount = x.vd.ItemRate - (x.vd.PreGstDiscount ?? 0),
                        SgstPer = x.vd.Sgstper ?? 0,
                        SgstAmount = x.vd.Sgstamnt ?? 0,
                        CgstPer = x.vd.Cgstper ?? 0,
                        CgstAmount = x.vd.Cgstamnt ?? 0,
                        IgstPer = x.vd.Igstper ?? 0,
                        IgstAmount = x.vd.Igstamnt ?? 0,
                        FameIIDiscount = x.vd.FameIi ?? 0,
                        RegAmount = x.vd.RegAmount ?? 0,
                        InsuranceAmount = x.vd.InsuranceAmount ?? 0,
                        PostGstDiscount = x.vd.PostGstDisc ?? 0,
                        FinalAmount = x.vd.FinalAmount,
                        Battery = x.vd.Battery,
                        ChargerNo = x.vd.ChargerNo,
                        ControllerNo = x.vd.ControllerNo,
                        Vcu = x.vd.Vcu,
                        PartyEmail = x.cust?.EMail,
                        Gender = x.cust?.Gender,
                        Dob = x.cust != null && x.cust.DateOfBirth.HasValue
                            ? x.cust.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue) : null,
                        AccountType = x.cust?.LedgerType,
                        Occupation = x.occ != null ? x.occ.OccupationName : null,
                        BatteryMake = vi?.BatteryMake,
                        BatteryType = vi?.BatteryChemistry
                    };
                }).ToList();

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var s = filter.Search.Trim().ToLower();
                    rows = rows.Where(x =>
                        (x.SaleBillNo ?? "").ToLower().Contains(s) ||
                        (x.CustomerName ?? "").ToLower().Contains(s) ||
                        (x.BillingName ?? "").ToLower().Contains(s) ||
                        (x.ChassisNo ?? "").ToLower().Contains(s) ||
                        (x.ModelName ?? "").ToLower().Contains(s) ||
                        (x.RegNo ?? "").ToLower().Contains(s)
                    ).ToList();
                }

                rows = rows.OrderByDescending(x => x.SaleDate).ThenByDescending(x => x.SaleBillNo).ToList();

                var response = new VehicleSaleBillReportResponse
                {
                    TotalRecords = rows.Count,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    TotalItemRate = rows.Sum(x => x.ItemRate),
                    TotalTaxable = rows.Sum(x => x.TaxableAmount),
                    TotalSgst = rows.Sum(x => x.SgstAmount),
                    TotalCgst = rows.Sum(x => x.CgstAmount),
                    TotalIgst = rows.Sum(x => x.IgstAmount),
                    TotalFameII = rows.Sum(x => x.FameIIDiscount),
                    TotalRegistration = rows.Sum(x => x.RegAmount),
                    TotalInsurance = rows.Sum(x => x.InsuranceAmount),
                    GrandTotal = rows.Sum(x => x.FinalAmount)
                };

                var paged = rows.Skip((filter.PageIndex - 1) * filter.PageSize).Take(filter.PageSize).ToList();

                int srNo = ((filter.PageIndex - 1) * filter.PageSize) + 1;
                foreach (var r in paged) r.SrNo = srNo++;

                response.Data = paged;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching vehicle sale bill only report: " + ex.Message, ex);
            }
        }

        public async Task<VehicleInwardReportResponse> GetVehicleInwardReportAsync(VehicleInwardReportFilterModel filter)
        {
            try
            {
                var query =
                    from vi in _context.VehicleInwards.AsNoTracking()
                    join dm in _context.DealerMasters.AsNoTracking()
                        on vi.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()
                    join loc in _context.LocationMasters.AsNoTracking()
                        on vi.LocCode equals loc.Loccode into locJoin
                    from loc in locJoin.DefaultIfEmpty()
                    join im in _context.ItemMasters.AsNoTracking()
                        on vi.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()
                    join clr in _context.ColorMasters.AsNoTracking()
                        on vi.ColrCode equals clr.Colorcode into clrJoin
                    from clr in clrJoin.DefaultIfEmpty()
                    join lih in _context.LotinspectionHeaders.AsNoTracking()
                        on vi.InvoiceNo equals lih.InvoiceNo into lihJoin
                    from lih in lihJoin.DefaultIfEmpty()
                    select new { vi, dm, loc, im, clr, lih };

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    query = query.Where(x => x.vi.DealerCode == filter.DealerCode);

                if (!string.IsNullOrWhiteSpace(filter.LocationCode))
                    query = query.Where(x => x.vi.LocCode == filter.LocationCode);

                if (!string.IsNullOrWhiteSpace(filter.InvoiceNo))
                    query = query.Where(x => x.vi.InvoiceNo != null && x.vi.InvoiceNo.Contains(filter.InvoiceNo));

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                    query = query.Where(x => x.vi.ChasisNo != null && x.vi.ChasisNo.Contains(filter.ChassisNo));

                if (!string.IsNullOrWhiteSpace(filter.MotorNo))
                    query = query.Where(x => x.vi.MotorNo != null && x.vi.MotorNo.Contains(filter.MotorNo));

                if (!string.IsNullOrWhiteSpace(filter.BatteryNo))
                    query = query.Where(x => x.vi.BatteryNo != null && x.vi.BatteryNo.Contains(filter.BatteryNo));

                var rawData = await query.ToListAsync();

                var withDates = rawData.Select(x => new
                {
                    x.vi,
                    x.dm,
                    x.loc,
                    x.im,
                    x.clr,
                    x.lih,
                    InvoiceDateTime = x.vi.InvoiceDate.HasValue
                        ? x.vi.InvoiceDate.Value.ToDateTime(TimeOnly.MinValue)
                        : (DateTime?)null
                }).AsEnumerable();

                if (filter.FromDate.HasValue)
                    withDates = withDates.Where(x =>
                        x.InvoiceDateTime.HasValue && x.InvoiceDateTime.Value.Date >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    withDates = withDates.Where(x =>
                        x.InvoiceDateTime.HasValue && x.InvoiceDateTime.Value.Date <= filter.ToDate.Value.Date);

                var filteredList = withDates
                    .OrderByDescending(x => x.InvoiceDateTime)
                    .ThenByDescending(x => x.vi.Id)
                    .ToList();

                var totalRecords = filteredList.Count;

                var allMapped = filteredList.Select(x =>
                {
                    var rate = x.vi.Dlrprice ?? 0;
                    var sgstPer = x.im?.Sgst ?? 0;
                    var cgstPer = x.im?.Cgst ?? 0;
                    var igstPer = x.im?.Igst ?? 0;
                    var ugstPer = x.im?.Ugst ?? 0;
                    var invoiceNo = x.vi.InvoiceNo ?? "";
                    var partyName = invoiceNo.StartsWith("BG", StringComparison.OrdinalIgnoreCase)
                        ? "BGauss"
                        : (x.dm != null ? x.dm.Compname : x.vi.DealerCode);

                    return new VehicleInwardReportViewModel
                    {
                        ReceivingDate = x.InvoiceDateTime,
                        InvoiceDate = x.InvoiceDateTime,
                        DealerCode = x.vi.DealerCode,
                        DealerName = x.dm != null ? x.dm.Compname : x.vi.DealerCode,
                        BgInvoiceNo = x.vi.InvoiceNo,
                        LotInspectionNo = x.lih != null ? x.lih.LotNo : (int?)null,
                        PartyName = partyName,
                        PurchaseReceivingLocation = x.loc != null ? x.loc.Locname : x.vi.LocCode,
                        ModelName = x.im != null
                            ? (x.im.Oemmodelname ?? x.im.Itemname ?? x.vi.ItemCode)
                            : x.vi.ItemCode,
                        Quantity = 1,
                        ChassisNo = x.vi.ChasisNo,
                        MotorNo = x.vi.MotorNo,
                        Colour = x.clr != null ? x.clr.Colorname : x.vi.ColrCode,
                        MfgYear = x.vi.MfgYear,
                        BatteryNo = x.vi.BatteryNo,
                        BatteryMake = x.vi.BatteryMake,
                        BatteryCapacity = x.vi.BatteryCapacity,
                        BatteryChemical = x.vi.BatteryChemistry,
                        ChargerNo = x.vi.ChargerNo,
                        ControllerNo = x.vi.ControllerNo,
                        Rate = rate,
                        SubsidyAmountFame2 = x.vi.Fame2Discount ?? 0,
                        Sgst = Math.Round(rate * sgstPer / 100m, 2),
                        Cgst = Math.Round(rate * cgstPer / 100m, 2),
                        Igst = Math.Round(rate * igstPer / 100m, 2),
                        Hst = Math.Round(rate * ugstPer / 100m, 2),
                    };
                }).ToList();

                var paged = allMapped
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                int srNo = (filter.PageIndex - 1) * filter.PageSize + 1;
                foreach (var row in paged)
                    row.SrNo = srNo++;

                return new VehicleInwardReportResponse
                {
                    TotalRecords = totalRecords,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    TotalQuantity = allMapped.Sum(x => x.Quantity),
                    TotalRate = allMapped.Sum(x => x.Rate),
                    TotalSubsidy = allMapped.Sum(x => x.SubsidyAmountFame2),
                    TotalSgst = allMapped.Sum(x => x.Sgst),
                    TotalCgst = allMapped.Sum(x => x.Cgst),
                    TotalIgst = allMapped.Sum(x => x.Igst),
                    TotalHst = allMapped.Sum(x => x.Hst),
                    GrandTotal = allMapped.Sum(x => x.Rate + x.Sgst + x.Cgst + x.Igst + x.Hst - x.SubsidyAmountFame2),
                    Data = paged
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching vehicle inward report: " + ex.Message, ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // MODEL-WISE VARIANT STOCK (COUNT-WISE) — pivoted Model x Colour Variant
        // ═════════════════════════════════════════════════════════════════════
        public async Task<ModelWiseVariantStockPivotResponse> GetModelWiseVariantStockCountReportAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                static bool IsInvoiced(VehicleSaleBillHeader hdr) =>
                    hdr != null && hdr.Status == "Invoiced";

                var canonicalModels = await _context.OemmodelMasters
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.ModelName)
                    .Select(x => x.ModelName)
                    .ToListAsync();

                var canonicalVariants = await _context.ColorMasters
                    .AsNoTracking()
                    .OrderBy(c => c.Colorname)
                    .Select(c => c.Colorname)
                    .ToListAsync();

                var canonicalModelLookup = BuildNormalizedLookup(canonicalModels);
                var canonicalVariantLookup = BuildNormalizedLookup(canonicalVariants);

                var query =
                    from vi in _context.VehicleInwards.AsNoTracking()

                    join im in _context.ItemMasters.AsNoTracking()
                        on vi.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join cm in _context.ColorMasters.AsNoTracking()
                        on vi.ColrCode equals cm.Colorcode into cmJoin
                    from cm in cmJoin.DefaultIfEmpty()

                    join vsd in _context.VehicleSaleBillDetails.AsNoTracking()
                        on vi.ChasisNo equals vsd.ChassisNo into saleDetailJoin
                    from vsd in saleDetailJoin.DefaultIfEmpty()

                    join vsh in _context.VehicleSaleBillHeaders.AsNoTracking()
                        on vsd.VehicleSaleBillId equals vsh.Id into saleHeaderJoin
                    from vsh in saleHeaderJoin.DefaultIfEmpty()

                    select new { Vehicle = vi, Item = im, Color = cm, SaleHdr = vsh };

                if (HasDealerFilter(dealerCode))
                    query = query.Where(x => x.Vehicle.DealerCode == dealerCode);

                if (fromDate.HasValue)
                {
                    var fromDateOnly = DateOnly.FromDateTime(fromDate.Value.Date);
                    query = query.Where(x =>
                        x.Vehicle.InvoiceDate.HasValue &&
                        x.Vehicle.InvoiceDate.Value >= fromDateOnly);
                }

                if (toDate.HasValue)
                {
                    var toDateOnly = DateOnly.FromDateTime(toDate.Value.Date);
                    query = query.Where(x =>
                        x.Vehicle.InvoiceDate.HasValue &&
                        x.Vehicle.InvoiceDate.Value <= toDateOnly);
                }

                var rawRows = await query.ToListAsync();
                rawRows = rawRows.Where(x => !IsInvoiced(x.SaleHdr)).ToList();

                const string UnmappedVariant = "Unmapped";

                var mapped = rawRows
                    .Select(x => new
                    {
                        ModelName = ResolveCanonicalName(x.Item?.Oemmodelname, canonicalModelLookup),
                        VariantName = ResolveCanonicalName(x.Color?.Colorname, canonicalVariantLookup) ?? UnmappedVariant
                    })
                    .Where(x => x.ModelName != null)
                    .Select(x => new { ModelName = x.ModelName!, x.VariantName })
                    .ToList();

                var actualVariants = mapped.Select(x => x.VariantName).Distinct().ToList();
                bool hasUnmapped = actualVariants.Contains(UnmappedVariant);


                var variantNames = HasDealerFilter(dealerCode)
                    ? actualVariants
                        .Where(v => v != UnmappedVariant)
                        .OrderBy(v => v)
                        .Concat(hasUnmapped ? new[] { UnmappedVariant } : Array.Empty<string>())
                        .ToList()
                    : canonicalVariants
                        .Concat(hasUnmapped ? new[] { UnmappedVariant } : Array.Empty<string>())
                        .ToList();

                var modelNames = mapped
                    .Select(x => x.ModelName)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                var rows = modelNames
                    .Select(model =>
                    {
                        var itemsForModel = mapped.Where(x => x.ModelName == model).ToList();
                        return new ModelWiseVariantStockPivotRow
                        {
                            ModelName = model,
                            VariantCounts = variantNames.ToDictionary(
                                v => v,
                                v => itemsForModel.Count(x => x.VariantName == v)),
                            Total = itemsForModel.Count
                        };
                    })
                    .Where(r => r.Total > 0)
                    .OrderByDescending(x => x.Total)
                    .ToList();

                var columnTotals = variantNames.ToDictionary(
                    v => v,
                    v => rows.Sum(r => r.VariantCounts.TryGetValue(v, out var c) ? c : 0));

                return new ModelWiseVariantStockPivotResponse
                {
                    VariantNames = variantNames,
                    Rows = rows,
                    ColumnTotals = columnTotals,
                    GrandTotal = rows.Sum(r => r.Total)
                };
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error fetching model-wise variant stock count report: "
                    + ex.Message,
                    ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // D2D (DOOR-TO-DOOR) REPORT
        // ═════════════════════════════════════════════════════════════════════
        public async Task<D2DReportResponse> GetD2DReportAsync(D2DReportFilterModel filter)
        {
            try
            {
                var allMapped = await BuildD2DReportRowsAsync(filter);

                var totalRecords = allMapped.Count;

                var paged = allMapped
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                int srNo = (filter.PageIndex - 1) * filter.PageSize + 1;
                foreach (var row in paged) row.SrNo = srNo++;

                return new D2DReportResponse
                {
                    TotalRecords = totalRecords,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    Data = paged
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching D2D report: " + ex.Message, ex);
            }
        }

        private async Task<List<D2DReportViewModel>> BuildD2DReportRowsAsync(D2DReportFilterModel filter)
        {
            var query =
                from vi in _context.VehicleInwards.AsNoTracking()
                where vi.IsD2d == true
                join dm in _context.DealerMasters.AsNoTracking()
                    on vi.DealerCode equals dm.Dealercode into dmJoin
                from dm in dmJoin.DefaultIfEmpty()
                join loc in _context.LocationMasters.AsNoTracking()
                    on vi.LocCode equals loc.Loccode into locJoin
                from loc in locJoin.DefaultIfEmpty()
                join im in _context.ItemMasters.AsNoTracking()
                    on vi.ItemCode equals im.Itemcode into imJoin
                from im in imJoin.DefaultIfEmpty()
                join clr in _context.ColorMasters.AsNoTracking()
                    on vi.ColrCode equals clr.Colorcode into clrJoin
                from clr in clrJoin.DefaultIfEmpty()
                join vsd in _context.VehicleSaleBillDetails.AsNoTracking()
                    on vi.ChasisNo equals vsd.ChassisNo into vsdJoin
                from vsd in vsdJoin.DefaultIfEmpty()
                join vsh in _context.VehicleSaleBillHeaders.AsNoTracking()
                    on vsd.VehicleSaleBillId equals vsh.Id into vshJoin
                from vsh in vshJoin.DefaultIfEmpty()
                select new { vi, dm, loc, im, clr, vsh };

            if (HasDealerFilter(filter.DealerCode))
                query = query.Where(x => x.vi.DealerCode == filter.DealerCode);

            if (!string.IsNullOrWhiteSpace(filter.LocationCode))
                query = query.Where(x => x.vi.LocCode == filter.LocationCode);

            if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                query = query.Where(x => x.vi.ChasisNo != null && x.vi.ChasisNo.Contains(filter.ChassisNo));

            if (!string.IsNullOrWhiteSpace(filter.MotorNo))
                query = query.Where(x => x.vi.MotorNo != null && x.vi.MotorNo.Contains(filter.MotorNo));

            if (!string.IsNullOrWhiteSpace(filter.BatteryNo))
                query = query.Where(x => x.vi.BatteryNo != null && x.vi.BatteryNo.Contains(filter.BatteryNo));

            if (!string.IsNullOrWhiteSpace(filter.ChargerNo))
                query = query.Where(x => x.vi.ChargerNo != null && x.vi.ChargerNo.Contains(filter.ChargerNo));

            if (!string.IsNullOrWhiteSpace(filter.ControllerNo))
                query = query.Where(x => x.vi.ControllerNo != null && x.vi.ControllerNo.Contains(filter.ControllerNo));

            var rawData = await query.ToListAsync();

            var chassisNosForHistory = rawData.Select(x => x.vi.ChasisNo).ToList();
            var d2dHistoryLookup = await GetLatestD2dHistoryByChassisAsync(chassisNosForHistory);
            var dealerLookup = await GetDealerLookupAsync();

            var withDates = rawData.Select(x => new
            {
                x.vi,
                x.dm,
                x.loc,
                x.im,
                x.clr,
                x.vsh,
                InvoiceDateTime = x.vi.InvoiceDate.HasValue
                    ? x.vi.InvoiceDate.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null
            }).AsEnumerable();

            if (filter.FromDate.HasValue)
                withDates = withDates.Where(x =>
                    x.InvoiceDateTime.HasValue && x.InvoiceDateTime.Value.Date >= filter.FromDate.Value.Date);

            if (filter.ToDate.HasValue)
                withDates = withDates.Where(x =>
                    x.InvoiceDateTime.HasValue && x.InvoiceDateTime.Value.Date <= filter.ToDate.Value.Date);

            var filteredList = withDates
                .OrderByDescending(x => x.InvoiceDateTime)
                .ThenByDescending(x => x.vi.Id)
                .ToList();

            var allMapped = filteredList.Select(x =>
            {
                var stockStatus = x.vsh == null
                    ? "In Stock"
                    : (x.vsh.Status == "Invoiced" ? "Sold" : "Allocated");

                var history = x.vi.ChasisNo != null && d2dHistoryLookup.TryGetValue(x.vi.ChasisNo, out var hist)
                    ? hist : null;

                DealerMaster? fromDealer = history?.IssueingDealerCode != null
                    && dealerLookup.TryGetValue(history.IssueingDealerCode, out var fd)
                    ? fd : null;

                return new D2DReportViewModel
                {
                    ReceivingDate = x.InvoiceDateTime,
                    InvoiceDate = x.InvoiceDateTime,
                    DealerCode = x.vi.DealerCode,
                    DealerName = x.dm != null ? x.dm.Compname : x.vi.DealerCode,
                    DealerCity = x.dm != null ? x.dm.City : null,
                    DealerState = x.dm != null ? x.dm.State : null,
                    FromDealerCode = history?.IssueingDealerCode,
                    FromDealerName = fromDealer?.Compname,
                    FromDealerCity = fromDealer?.City,
                    FromDealerState = fromDealer?.State,
                    BgInvoiceNo = x.vi.InvoiceNo,
                    LocationCode = x.vi.LocCode,
                    PurchaseReceivingLocation = x.loc != null ? x.loc.Locname : x.vi.LocCode,
                    LocationCity = x.loc != null ? x.loc.City : null,
                    ModelCode = x.vi.ItemCode,
                    ModelName = x.im != null
                        ? (x.im.Oemmodelname ?? x.im.Itemname ?? x.vi.ItemCode)
                        : x.vi.ItemCode,
                    OemModelName = x.im != null ? x.im.Oemmodelname : null,
                    ChassisNo = x.vi.ChasisNo,
                    MotorNo = x.vi.MotorNo,
                    Colour = x.clr != null ? x.clr.Colorname : x.vi.ColrCode,
                    MfgYear = x.vi.MfgYear,
                    BatteryNo = x.vi.BatteryNo,
                    BatteryMake = x.vi.BatteryMake,
                    BatteryCapacity = x.vi.BatteryCapacity,
                    BatteryChemical = x.vi.BatteryChemistry,
                    ChargerNo = x.vi.ChargerNo,
                    ControllerNo = x.vi.ControllerNo,
                    StockStatus = stockStatus,
                    IsD2D = x.vi.IsD2d == true
                };
            }).ToList();

            allMapped = allMapped
                   .Where(x => !string.IsNullOrWhiteSpace(x.FromDealerCode))
                   .Where(x => !string.Equals(x.FromDealerCode, x.DealerCode, StringComparison.OrdinalIgnoreCase))
                   .ToList();

            // NEW — "From Dealer" filter: the dealer that originally issued/sent the vehicle,
            // independent of filter.DealerCode (which only matches the current/receiving dealer).
            if (HasDealerFilter(filter.FromDealerCode))
            {
                allMapped = allMapped
                    .Where(x => string.Equals(x.FromDealerCode, filter.FromDealerCode, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.StockStatus))
            {
                allMapped = allMapped
                    .Where(x => string.Equals(x.StockStatus, filter.StockStatus, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                allMapped = allMapped.Where(x =>
                    (x.ChassisNo ?? "").ToLower().Contains(s) ||
                    (x.MotorNo ?? "").ToLower().Contains(s) ||
                    (x.DealerName ?? "").ToLower().Contains(s) ||
                    (x.ModelName ?? "").ToLower().Contains(s) ||
                    (x.BgInvoiceNo ?? "").ToLower().Contains(s)
                ).ToList();
            }

            return allMapped;
        }

        public async Task<List<D2DReportViewModel>> GetD2DReportForExportAsync(D2DReportFilterModel filter)
        {
            try
            {
                var allMapped = await BuildD2DReportRowsAsync(filter);

                int srNo = 1;
                foreach (var row in allMapped) row.SrNo = srNo++;

                return allMapped;
            }
            catch (Exception ex)
            {
                throw new Exception("Error exporting D2D report: " + ex.Message, ex);
            }
        }
    }
}