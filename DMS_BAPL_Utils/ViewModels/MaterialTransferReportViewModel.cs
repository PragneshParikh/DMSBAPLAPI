using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class MaterialTransferReportFilterModel
    {
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? JobNo { get; set; }
        public string? ChassisNo { get; set; }
        public string? PartyName { get; set; }
        public string? ItemCode { get; set; }
        public int? IssueType { get; set; }   // NEW
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class MaterialTransferReportRowViewModel
    {
        public int SrNo { get; set; }

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerCity { get; set; }
        public string? DealerState { get; set; }

        public int JobId { get; set; }
        public int? JobNo { get; set; }
        public string? JobInvoiceNo { get; set; }
        public string? ChassisNo { get; set; }
        public string? RegisterNo { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? ServiceLocationCode { get; set; }
        public string? ServiceLocationName { get; set; }

        public string? MaterialPrefix { get; set; }
        public int MaterialIssueNumber { get; set; }
        public DateTime TransferDate { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDesc { get; set; }
        public string? Hsncode { get; set; }

        public int Quantity { get; set; }
        public decimal ItemRate { get; set; }
        public decimal Amount { get; set; }

        public string? SerialNo { get; set; }
        public string? Remarks { get; set; }
        public string? ItemReceived { get; set; }
        public int? ValidDays { get; set; }
        public int? RackNo { get; set; }
        public int? Bin { get; set; }

        // Raw pass-through — no employee/lookup table was available anywhere
        // in the shared codebase to resolve these to a display name/label.
        public int TechnicianId { get; set; }
        public int IssueType { get; set; }

        public string JobCardStatus { get; set; } = "Open";

        public string? PreparedByDealerCode { get; set; }
        public string? ModifiedByDealerCode { get; set; }
    }

    public class MaterialTransferReportPagedResponse
    {
        public List<MaterialTransferReportRowViewModel> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}