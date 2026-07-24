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
        public int? IssueType { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class MaterialTransferReportRowViewModel
    {
        public int SrNo { get; set; }

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerLocation { get; set; }
        public string? LocationName { get; set; }

        public string? DealerCity { get; set; }
        public string? DealerState { get; set; }

        public int JobId { get; set; }
        public int? JobNo { get; set; }

        // REMOVED: JobInvoiceNo
        // REMOVED: RegisterNo

        public string? ChassisNo { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }

        // REMOVED: ServiceLocationCode / ServiceLocationName
        // (replaced by DealerLocation above)

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

        // NEW — MRP from ItemMaster
        public decimal Mrp { get; set; }

        // NEW — GST calculation, sourced from ItemMaster.Cgst/Sgst/Igst rate
        // fields (same table Repair Bill Report already uses for Part
        // rows). See caveat in accompanying reply re: potential
        // simultaneous CGST+SGST+IGST double-counting if all three rates
        // are non-zero on the same master record.
        public decimal CgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalGstAmount { get; set; }

        public string? SerialNo { get; set; }
        public string? Remarks { get; set; }

        // REMOVED: ItemReceived
        // REMOVED: ValidDays

        public int? RackNo { get; set; }
        public int? Bin { get; set; }

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
        public decimal TotalMrp { get; set; }
        public decimal TotalCgstAmount { get; set; }
        public decimal TotalSgstAmount { get; set; }
        public decimal TotalIgstAmount { get; set; }
        public decimal TotalGstAmount { get; set; }
    }
}