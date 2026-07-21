using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ComparisonReportFilterModel
    {
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? ChassisNo { get; set; }
        public string? CustomerName { get; set; }
        public bool? PerformaCreated { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ComparisonReportRowViewModel
    {
        public int SrNo { get; set; }

        public int VehicleSaleBillId { get; set; }

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerLocation { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? ChassisNo { get; set; }
        public bool IsPerformaCreated { get; set; }
        public DateTime? PerformaCreatedDate { get; set; }
        public bool IsSaleBillCreated { get; set; } = true;
        public DateTime? SaleBillCreatedDate { get; set; }
    }

    public class ComparisonReportPagedResponse
    {
        public List<ComparisonReportRowViewModel> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalWithPerforma { get; set; }
        public int TotalSaleBillCreated { get; set; }
    }
}