using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.JobReportRepo
{
    public class JobReportRepo : IJobReportRepo
    {
        private readonly BapldmsvadContext _context;

        public JobReportRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(
    JobReportFilterModel filter)
        {
            try
            {
                // Step 1: Build base query on JobCardHeaders with joins (NO detail aggregation yet)
                var baseQuery = from jh in _context.JobCardHeaders
                                join jc in _context.JobCardCustomers
                                    on jh.Id equals jc.JobCardHeaderId
                                join jt in _context.JobTypes
                                    on jh.Jobtype equals jt.Id
                                join sh in _context.ServiceHeads
                                    on jh.Servicehead equals sh.Id
                                join st in _context.ServiceTypes
                                    on jh.Servicetype equals st.Id
                                join inv in _context.InvoiceHeaders
                                    on jh.Id equals inv.ReferenceId into invGroup
                                from invoice in invGroup.DefaultIfEmpty()
                                select new
                                {
                                    jh,
                                    jc,
                                    jt,
                                    sh,
                                    st,
                                    invoice
                                };

                // Step 2: Apply filters
                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    baseQuery = baseQuery.Where(x => x.jh.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                    baseQuery = baseQuery.Where(x =>
                        (x.invoice != null ? x.invoice.CreatedDate : x.jh.CreatedDate) >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    baseQuery = baseQuery.Where(x =>
                        (x.invoice != null ? x.invoice.CreatedDate : x.jh.CreatedDate) <= filter.ToDate.Value.Date.AddDays(1));

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

                // Step 3: Count total before pagination
                var totalRecords = await baseQuery.CountAsync();

                // Step 4: Paginate and fetch header data
                var pagedRows = await baseQuery
                    .OrderByDescending(x => x.invoice != null ? x.invoice.CreatedDate : x.jh.CreatedDate)
                    .ThenByDescending(x => x.jh.JobNo)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Step 5: Fetch invoice details for the paged invoice IDs
                var invoiceIds = pagedRows
                    .Where(r => r.invoice != null)
                    .Select(r => r.invoice!.Id)
                    .Distinct()
                    .ToList();

                var invoiceDetails = await _context.InvoiceDetails
                    .Where(d => invoiceIds.Contains(d.InvoiceId))
                    .ToListAsync();

                // Step 6: Map to ViewModel — join details in memory
                int srNo = (filter.PageIndex - 1) * filter.PageSize + 1;
                var data = pagedRows.Select(r =>
                {
                    var details = r.invoice != null
                        ? invoiceDetails.Where(d => d.InvoiceId == r.invoice.Id).ToList()
                        : new List<InvoiceDetail>();

                    var sparesAmount = details.Sum(d => (d.Quantity ?? 0) * (d.Rate ?? 0));
                    var taxAmount = r.invoice?.TaxAmount ?? 0;
                    var totalAmount = r.invoice?.TotalAmount ?? 0;

                    return new JobReportViewModel
                    {
                        SrNo = srNo++,
                        InvoiceNo = r.invoice != null ? Convert.ToInt32(r.invoice.DocumentNo) : 0,
                        InvoiceDate = r.invoice != null ? r.invoice.CreatedDate : r.jh.CreatedDate,
                        JobNo = r.jh.JobNo ?? 0,
                        PartyName = r.jc.CustomerName,
                        PartyMobileNo = r.jc.CustomerMobile,
                        RegNo = r.jc.RegisterNo,
                        MechanicName = r.jh.Technician,
                        InvoiceType = r.invoice?.InvoiceType ?? r.jt.JobTypeName,
                        InvoiceMode = r.invoice?.ServiceType ?? r.sh.ServiceHeadName,
                        JobType = r.jt.JobTypeName,
                        ServiceHead = r.sh.ServiceHeadName,
                        ServiceType = r.st.ServiceTypeName,
                        ChassisNo = r.jh.Chassisno,
                        DealerCode = r.jh.DealerCode,
                        ServiceLocation = r.jh.Serviceloc,
                        SparesAmount = sparesAmount,
                        AcsrAmount = 0,
                        OilAmount = 0,
                        LabourAmount = 0,
                        OutsideWorkAmount = 0,
                        TaxableAmount = totalAmount,
                        SGSTAmount = taxAmount / 2,
                        CGSTAmount = taxAmount / 2,
                        JobInDate = r.jh.JobinDate.HasValue
                                                ? r.jh.JobinDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        EstimatedDeliveryDate = r.jh.EstdelDate.HasValue
                                                ? r.jh.EstdelDate.Value.ToDateTime(TimeOnly.MinValue) : null
                    };
                }).ToList();

                // Step 7: Compute page-level totals from data
                return new JobReportPagedResponse<JobReportViewModel>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    TotalSpares = data.Sum(x => x.SparesAmount),
                    TotalAcsr = data.Sum(x => x.AcsrAmount),
                    TotalOil = data.Sum(x => x.OilAmount),
                    TotalLabour = data.Sum(x => x.LabourAmount),
                    TotalOutsideWork = data.Sum(x => x.OutsideWorkAmount),
                    TotalTaxable = data.Sum(x => x.TaxableAmount),
                    TotalSGST = data.Sum(x => x.SGSTAmount),
                    TotalCGST = data.Sum(x => x.CGSTAmount),
                    GrandTotal = data.Sum(x => x.TaxableAmount + x.SGSTAmount + x.CGSTAmount)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching job report: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Get Dealer Wise Job Card Summary Report
        /// </summary>
        public async Task<List<DealerWiseJobReportSummary>> GetDealerWiseJobReportAsync(
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var query = BuildJobReportQuery();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                    query = query.Where(x => x.DealerCode == dealerCode);

                if (fromDate.HasValue)
                    query = query.Where(x => x.InvoiceDate >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(x => x.InvoiceDate <= toDate.Value.Date.AddDays(1));

                var data = await query.ToListAsync();

                return data
                    .GroupBy(x => x.DealerCode)
                    .Select(g => new DealerWiseJobReportSummary
                    {
                        DealerCode = g.Key,
                        DealerName = g.First().PartyName,
                        TotalJobs = g.Count(),

                        TotalSpares = g.Sum(x => x.SparesAmount),
                        TotalLabour = g.Sum(x => x.LabourAmount),
                        TotalTaxable = g.Sum(x => x.TaxableAmount),
                        TotalSGST = g.Sum(x => x.SGSTAmount),
                        TotalCGST = g.Sum(x => x.CGSTAmount),

                        GrandTotal = g.Sum(x =>
                            x.TaxableAmount +
                            x.SGSTAmount +
                            x.CGSTAmount),

                        JobDetails = g.ToList()
                    })
                    .OrderByDescending(x => x.GrandTotal)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching dealer wise job report", ex);
            }
        }

        /// <summary>
        /// Get Job Report for specific dealer
        /// </summary>
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

        /// <summary>
        /// Get filtered report
        /// </summary>
        public async Task<JobReportPagedResponse<JobReportViewModel>> GetFilteredJobReportAsync(
            JobReportFilterModel filter)
        {
            return await GetJobReportAsync(filter);
        }

        /// <summary>
        /// Export Job Report
        /// </summary>
        public async Task<List<JobReportViewModel>> GetJobReportForExportAsync(
            string dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var query = BuildJobReportQuery();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                    query = query.Where(x => x.DealerCode == dealerCode);

                if (fromDate.HasValue)
                    query = query.Where(x => x.InvoiceDate >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(x => x.InvoiceDate <= toDate.Value.Date.AddDays(1));

                return await query
                    .OrderByDescending(x => x.InvoiceDate)
                    .ThenByDescending(x => x.JobNo)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error exporting report", ex);
            }
        }

        /// <summary>
        /// Base Query
        /// </summary>
        private IQueryable<JobReportViewModel> BuildJobReportQuery()
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

                // LEFT JOIN Invoice Header
                join inv in _context.InvoiceHeaders
                    on jh.Id equals inv.ReferenceId into invGroup

                from invoice in invGroup.DefaultIfEmpty()

                select new
                {
                    jh,
                    jc,
                    jt,
                    sh,
                    st,
                    invoice
                };

            var result = query
                .AsEnumerable()
                .Select(x => new JobReportViewModel
                {
                    // =========================
                    // BASIC DETAILS
                    // =========================

                    SrNo = x.jh.Id,

                    InvoiceNo = x.invoice != null
                        ? Convert.ToInt32(x.invoice.DocumentNo)
                        : 0,

                    InvoiceDate = x.invoice != null
                        ? x.invoice.CreatedDate
                        : x.jh.CreatedDate,

                    JobNo = x.jh.JobNo ?? 0,

                    PartyName = x.jc.CustomerName,

                    PartyMobileNo = x.jc.CustomerMobile,

                    RegNo = x.jc.RegisterNo,

                    MechanicName = x.jh.Technician,

                    // =========================
                    // JOB DETAILS
                    // =========================

                    InvoiceType = x.invoice != null
                        ? x.invoice.InvoiceType
                        : x.jt.JobTypeName,

                    InvoiceMode = x.invoice != null
                        ? x.invoice.ServiceType
                        : x.sh.ServiceHeadName,

                    JobType = x.jt.JobTypeName,

                    ServiceHead = x.sh.ServiceHeadName,

                    ServiceType = x.st.ServiceTypeName,

                    // =========================
                    // VEHICLE DETAILS
                    // =========================

                    ChassisNo = x.jh.Chassisno,

                    DealerCode = x.jh.DealerCode,

                    ServiceLocation = x.jh.Serviceloc,

                    // =========================
                    // AMOUNTS
                    // =========================

                    SparesAmount = x.invoice != null
                        ? _context.InvoiceDetails
                            .Where(d => d.InvoiceId == x.invoice.Id)
                            .Sum(d => ((d.Quantity ?? 0) * (d.Rate ?? 0)))
                        : 0,

                    AcsrAmount = 0,

                    OilAmount = 0,

                    LabourAmount = 0,

                    OutsideWorkAmount = 0,

                    TaxableAmount = x.invoice != null
                        ? x.invoice.TotalAmount ?? 0
                        : 0,

                    SGSTAmount = x.invoice != null
                        ? (x.invoice.TaxAmount ?? 0) / 2
                        : 0,

                    CGSTAmount = x.invoice != null
                        ? (x.invoice.TaxAmount ?? 0) / 2
                        : 0,

                    // =========================
                    // DATES
                    // =========================

                    JobInDate = x.jh.JobinDate.HasValue
                        ? x.jh.JobinDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,

                    EstimatedDeliveryDate = x.jh.EstdelDate.HasValue
                        ? x.jh.EstdelDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null
                });

            return result.AsQueryable();
        }
    }
}