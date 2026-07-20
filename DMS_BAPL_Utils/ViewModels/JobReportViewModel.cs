using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    /// <summary>
    /// Job Card Report — operational/customer view. No invoice or financial data.
    /// </summary>
    public class JobReportViewModel
    {
        public int SrNo { get; set; }

        public int JobNo { get; set; }

        public string? PartyName { get; set; }

        public string? PartyMobileNo { get; set; }

        public string? RegNo { get; set; }

        public string? MechanicName { get; set; }

        public string? ChassisNo { get; set; }

        public string? DealerCode { get; set; }

        public string? ServiceLocation { get; set; }

        public string? JobType { get; set; }

        public string? ServiceHead { get; set; }

        public string? ServiceType { get; set; }

        public DateTime? JobInDate { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        public string? DealerName { get; set; }

        public string? DealerLocation { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public int? Kms { get; set; }

        public string? MotorNo { get; set; }

        public string? BatteryNo { get; set; }

        public string? ChargerNo { get; set; }

        public string? CustomerVoice { get; set; }

        public string? CustomerCode { get; set; }

        public string? Observation { get; set; }

        public string? SupervisorComment { get; set; }

        public string? JobStatus { get; set; }

        public DateTime? SaleDate { get; set; }

        public string? SupervisorName { get; set; }

        public string? JobCreationSource { get; set; }
    }

    /// <summary>
    /// Pagination Response for Job Reports
    /// </summary>
    public class JobReportPagedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();

        public int TotalRecords { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    /// <summary>
    /// Job Report Filter Model
    /// </summary>
    public class JobReportFilterModel
    {
        public string? DealerCode { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? ServiceLocation { get; set; }

        public int? JobNo { get; set; }

        public string? PartyName { get; set; }

        public string? ChassisNo { get; set; }

        public string? RegNo { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Dealer Wise Summary Report — job counts only, no financial data
    /// </summary>
    public class DealerWiseJobReportSummary
    {
        public string? DealerCode { get; set; }

        public string? DealerName { get; set; }

        public int TotalJobs { get; set; }

        public List<JobReportViewModel> JobDetails { get; set; } = new List<JobReportViewModel>();
    }

    /// <summary>
    /// Excel Export Model for Job Report
    /// </summary>
    public class JobReportExportModel
    {
        public string? DealerCode { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<JobReportViewModel> ReportData { get; set; } = new List<JobReportViewModel>();
    }
}