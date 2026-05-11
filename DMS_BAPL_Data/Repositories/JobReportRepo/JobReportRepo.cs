using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.JobCardRepo
{
    public class JobReportRepo : IJobReportRepo
    {
        private readonly BapldmsvadContext _context;

        public JobReportRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get Job Card Report with pagination and filtering
        /// </summary>
        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(JobReportFilterModel filter)
        {
            try
            {
                var query = BuildJobReportQuery();

                // Apply Filters
                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    query = query.Where(x => x.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                    query = query.Where(x => x.InvoiceDate >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    query = query.Where(x => x.InvoiceDate <= filter.ToDate.Value.Date.AddDays(1));

                if (!string.IsNullOrWhiteSpace(filter.ServiceLocation))
                    query = query.Where(x => x.ServiceLocation == filter.ServiceLocation);

                if (filter.JobNo.HasValue)
                    query = query.Where(x => x.JobNo == filter.JobNo.Value);

                if (!string.IsNullOrWhiteSpace(filter.PartyName))
                    query = query.Where(x => x.PartyName.Contains(filter.PartyName));

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                    query = query.Where(x => x.ChassisNo.Contains(filter.ChassisNo));

                if (!string.IsNullOrWhiteSpace(filter.RegNo))
                    query = query.Where(x => x.RegNo.Contains(filter.RegNo));

                var totalRecords = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.InvoiceDate)
                    .ThenByDescending(x => x.JobNo)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();


                return new JobReportPagedResponse<JobReportViewModel>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,

                    TotalSpares = await query.SumAsync(x => x.SparesAmount),
                    TotalAcsr = await query.SumAsync(x => x.AcsrAmount),
                    TotalOil = await query.SumAsync(x => x.OilAmount),
                    TotalLabour = await query.SumAsync(x => x.LabourAmount),
                    TotalOutsideWork = await query.SumAsync(x => x.OutsideWorkAmount),
                    TotalTaxable = await query.SumAsync(x => x.TaxableAmount),
                    TotalSGST = await query.SumAsync(x => x.SGSTAmount),
                    TotalCGST = await query.SumAsync(x => x.CGSTAmount),

                    GrandTotal = await query.SumAsync(x =>
                        x.TaxableAmount +
                        x.SGSTAmount +
                        x.CGSTAmount),
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching job report", ex);
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
            return
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

                    // LEFT JOIN Invoice Details
                join invd in _context.InvoiceDetails
                    on invoice.Id equals invd.InvoiceId into detailGroup

                select new JobReportViewModel
                {
                    // =========================
                    // BASIC DETAILS
                    // =========================
                    SrNo = jh.Id,

                    InvoiceNo = invoice != null
                        ? Convert.ToInt32(invoice.DocumentNo)
                        : 0,

                    InvoiceDate = invoice != null
                        ? invoice.CreatedDate
                        : jh.CreatedDate,

                    JobNo = jh.JobNo ?? 0,

                    PartyName = jc.CustomerName,

                    PartyMobileNo = jc.CustomerMobile,

                    RegNo = jc.RegisterNo,

                    MechanicName = jh.Technician,

                    // =========================
                    // JOB DETAILS
                    // =========================
                    InvoiceType = invoice != null
                        ? invoice.InvoiceType
                        : jt.JobTypeName,

                    InvoiceMode = invoice != null
                        ? invoice.ServiceType
                        : sh.ServiceHeadName,

                    JobType = jt.JobTypeName,

                    ServiceHead = sh.ServiceHeadName,

                    ServiceType = st.ServiceTypeName,

                    // =========================
                    // VEHICLE DETAILS
                    // =========================
                    ChassisNo = jh.Chassisno,

                    DealerCode = jh.DealerCode,

                    ServiceLocation = jh.Serviceloc,

                    // =========================
                    // AMOUNTS
                    // =========================
                    SparesAmount = detailGroup.Sum(x =>
                        (decimal?)(
                            (x.Quantity ?? 0) *
                            (x.Rate ?? 0)
                        )) ?? 0,

                    AcsrAmount = 0,

                    OilAmount = 0,

                    LabourAmount = 0,

                    OutsideWorkAmount = 0,

                    TaxableAmount = invoice != null
                        ? invoice.TotalAmount ?? 0
                        : 0,

                    SGSTAmount = invoice != null
                        ? (invoice.TaxAmount ?? 0) / 2
                        : 0,

                    CGSTAmount = invoice != null
                        ? (invoice.TaxAmount ?? 0) / 2
                        : 0,

                    // =========================
                    // DATES
                    // =========================
                    JobInDate = jh.JobinDate.HasValue
                        ? jh.JobinDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,

                    EstimatedDeliveryDate = jh.EstdelDate.HasValue
                        ? jh.EstdelDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null
                };
        }
    }
}