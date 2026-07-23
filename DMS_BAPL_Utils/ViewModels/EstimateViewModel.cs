using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // ═════════════════════════════════════════════════════════════════
    // LINE ITEM (Parts & Labour)
    // ═════════════════════════════════════════════════════════════════
    public class EstimateDetailViewModel
    {
        public int Id { get; set; }
        public string ItemType { get; set; } = string.Empty; // "Part" or "Labour"
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public decimal Qty { get; set; } = 1;
        public decimal Rate { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal Amount { get; set; }
    }

    // ═════════════════════════════════════════════════════════════════
    // CREATE / UPDATE payload — what the frontend POSTs/PUTs
    // ═════════════════════════════════════════════════════════════════
    public class EstimateCreateViewModel
    {
        public string? EstimationNo { get; set; }
        public DateTime EstimateDate { get; set; }

        public string? ChassisNo { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPin { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }

        public int? Kms { get; set; }

        // FK to JobType.Id — resolved from the frontend dropdown,
        // NOT free text (matches JobCardHeader.Jobtype convention)
        public int? JobTypeId { get; set; }

        // ── Insurance ──
        // Property names match the Angular save payload exactly
        // (insuranceId, insDescription, surveyorName, contactNumber,
        // policyNo, insValidTill, zeroDepo) via default camelCase JSON.
        public int? InsuranceId { get; set; }
        public string? InsDescription { get; set; }
        public string? SurveyorName { get; set; }
        public string? ContactNumber { get; set; }
        public string? PolicyNo { get; set; }
        public DateTime? InsValidTill { get; set; }
        public bool ZeroDepo { get; set; }

        public string? DealerCode { get; set; }

        public List<EstimateDetailViewModel> Details { get; set; } = new();
    }

    // ═════════════════════════════════════════════════════════════════
    // RESPONSE payload — what the API returns
    // ═════════════════════════════════════════════════════════════════
    public class EstimateResponseViewModel
    {
        public int Id { get; set; }

        public string? EstimationNo { get; set; }
        public DateTime EstimateDate { get; set; }

        public string? ChassisNo { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPin { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }

        public int? Kms { get; set; }

        public int? JobTypeId { get; set; }
        public string? JobTypeName { get; set; }   // resolved via JobType lookup, read-only display value

        // ── Insurance ──
        // Only the raw FK/fields are returned here — the Angular edit form
        // resolves the party's display name itself by matching InsuranceId
        // against its own loaded insurance-ledger list, the same way it
        // already does for a freshly-selected party.
        public int? InsuranceId { get; set; }
        public string? InsDescription { get; set; }
        public string? SurveyorName { get; set; }
        public string? ContactNumber { get; set; }
        public string? PolicyNo { get; set; }
        public DateTime? InsValidTill { get; set; }
        public bool ZeroDepo { get; set; }

        // Job Card created from this estimate, if any (JobCardHeader.Jobestmate
        // is the FK back to this Estimate's Id). Null when no job card has
        // been created yet — matches the raw JobNo int, same convention
        // RepairBillRepo already uses for its own JobCardNo field.
        public int? JobCardNo { get; set; }

        public string? DealerCode { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<EstimateDetailViewModel> Details { get; set; } = new();
    }

    // ═════════════════════════════════════════════════════════════════
    // FILTER / PAGINATION
    // ═════════════════════════════════════════════════════════════════
    public class EstimateFilterModel
    {
        public string? DealerCode { get; set; }
        public string? ChassisNo { get; set; }
        public string? EstimationNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class EstimatePagedResponse
    {
        public List<EstimateResponseViewModel> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    // ═════════════════════════════════════════════════════════════════
    // JOB TYPE DROPDOWN
    // ═════════════════════════════════════════════════════════════════
    public class JobTypeDropdownItem
    {
        public int Id { get; set; }
        public string? JobTypeName { get; set; }
    }

    public class PartSearchResultViewModel
    {
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public decimal Rate { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal IgstPercent { get; set; }

        // ── Linked Labour ──
        // PartWiseLabourMaster pairs each part with one associated labour
        // operation (same table RepairBill uses to auto-generate labour
        // rows from a part via labourCodeDetailslist). When present, the
        // Estimate form auto-adds this labour to the Labour grid as soon
        // as the part is selected.
        public string? LinkedLabourCode { get; set; }
        public string? LinkedLabourDescription { get; set; }
        public decimal? LinkedLabourRate { get; set; }
        public decimal? LinkedLabourCgstPercent { get; set; }
        public decimal? LinkedLabourSgstPercent { get; set; }
        public decimal? LinkedLabourIgstPercent { get; set; }
    }

    public class LabourSearchResultViewModel
    {
        public string? LabourCode { get; set; }
        public string? LabourDescription { get; set; }
        public decimal Rate { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal IgstPercent { get; set; }
    }

    public class EstimatePrintLineViewModel
    {
        public string ItemType { get; set; } = string.Empty; // "Part" or "Labour"
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal Amount { get; set; }
    }

    public class EstimatePrintViewModel
    {
        public string? DealerCode { get; set; } // used for ownership check only, not printed directly
        public string? DealerName { get; set; }
        public string? DealerAddress { get; set; }
        public string? DealerPhone { get; set; }
        public string? DealerEmail { get; set; }
        public string? DealerGstin { get; set; }
        public string? DealerPan { get; set; }
        public string? DealerTradeCertNo { get; set; }

        public string? EstimationNo { get; set; }
        public DateTime EstimateDate { get; set; }

        public string? ChassisNo { get; set; }
        public int? Kms { get; set; }
        public string? JobTypeName { get; set; }

        // ── Insurance ──
        // Unlike EstimateResponseViewModel, this needs the resolved display
        // name — the PDF is generated entirely server-side, so there's no
        // Angular around at print time to resolve InsuranceId -> name.
        public string? InsuranceParty { get; set; }
        public string? InsDescription { get; set; }
        public string? SurveyorName { get; set; }
        public string? ContactNumber { get; set; }
        public string? PolicyNo { get; set; }
        public DateTime? InsValidTill { get; set; }
        public bool ZeroDepo { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }
        public string? CustomerPin { get; set; }
        public string? CustomerEmail { get; set; }

        public List<EstimatePrintLineViewModel> Parts { get; set; } = new();
        public List<EstimatePrintLineViewModel> Labour { get; set; } = new();

        public decimal TotalPartsAmount { get; set; }
        public decimal TotalLabourAmount { get; set; }
        public decimal GrandTotal { get; set; }
    }
}