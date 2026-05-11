using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    /// <summary>
    /// Job Card Report - Dealer Wise Table Format
    /// </summary>
    public class JobReportViewModel
    {
        public int SrNo { get; set; }

        public int InvoiceNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public int JobNo { get; set; }

        public string? PartyName { get; set; }

        public string? PartyMobileNo { get; set; }

        public string? RegNo { get; set; }

        public string? MechanicName { get; set; }

        public string? InvoiceType { get; set; }

        public string? InvoiceMode { get; set; }

        public decimal SparesAmount { get; set; }

        public decimal AcsrAmount { get; set; }

        public decimal OilAmount { get; set; }

        public decimal LabourAmount { get; set; }

        public decimal OutsideWorkAmount { get; set; }

        public decimal TaxableAmount { get; set; }

        public decimal SGSTAmount { get; set; }

        public decimal CGSTAmount { get; set; }

        public string? ChassisNo { get; set; }

        public string? DealerCode { get; set; }

        public string? ServiceLocation { get; set; }

        public string? JobType { get; set; }

        public string? ServiceHead { get; set; }

        public string? ServiceType { get; set; }

        public DateTime? JobInDate { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
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

        public decimal TotalSpares { get; set; }

        public decimal TotalAcsr { get; set; }

        public decimal TotalOil { get; set; }

        public decimal TotalLabour { get; set; }

        public decimal TotalOutsideWork { get; set; }

        public decimal TotalTaxable { get; set; }

        public decimal TotalSGST { get; set; }

        public decimal TotalCGST { get; set; }

        public decimal GrandTotal { get; set; }
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
    /// Dealer Wise Summary Report
    /// </summary>
    public class DealerWiseJobReportSummary
    {
        public string? DealerCode { get; set; }

        public string? DealerName { get; set; }

        public int TotalJobs { get; set; }

        public decimal TotalSpares { get; set; }

        public decimal TotalLabour { get; set; }

        public decimal TotalTaxable { get; set; }

        public decimal TotalSGST { get; set; }

        public decimal TotalCGST { get; set; }

        public decimal GrandTotal { get; set; }

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