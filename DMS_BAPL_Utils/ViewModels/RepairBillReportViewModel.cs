using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class RepairBillReportFilterModel
    {
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? BillNo { get; set; }
        public int? JobNo { get; set; }
        public string? ChassisNo { get; set; }
        public string? PartyName { get; set; }
        public string? PartCode { get; set; }
        public string? LabourCode { get; set; }
        public string? JobStatus { get; set; }   // "Open" | "Closed"
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class RepairBillReportRowViewModel
    {
        public int SrNo { get; set; }

        public int RepairBillDetailId { get; set; }
        public int RepairBillId { get; set; }

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerLocation { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }

        public DateTime? JobDate { get; set; }
        public string? JobType { get; set; }
        public string? ServiceHead { get; set; }
        public string? ServiceType { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? ChassisNo { get; set; }
        public string? ModelDetails { get; set; }

        public string? JobStatus { get; set; }

        public string? RepairBillNo { get; set; }
        public DateTime? RepairBillDate { get; set; }

        // Populated only when ItemType == "Part"
        public string? PartCode { get; set; }
        public string? PartCodeDescription { get; set; }

        public int? IssueType { get; set; }

        public decimal ItemRate { get; set; }

        public decimal CgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalGstAmount { get; set; }

        public decimal Discount { get; set; }
        public string? DiscountType { get; set; }

        // Populated only when ItemType == "Labour"
        public string? LabourCode { get; set; }
        public string? LabourDescription { get; set; }

        public string? TechnicianName { get; set; }

        // "Part" or "Labour" — tells the frontend which set of fields above
        // is actually populated for this row.
        public string? ItemType { get; set; }
    }

    public class RepairBillReportPagedResponse
    {
        public List<RepairBillReportRowViewModel> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public decimal TotalItemRate { get; set; }
        public decimal TotalCgstAmount { get; set; }
        public decimal TotalSgstAmount { get; set; }
        public decimal TotalIgstAmount { get; set; }
        public decimal TotalGstAmount { get; set; }
        public decimal TotalDiscount { get; set; }
    }
}