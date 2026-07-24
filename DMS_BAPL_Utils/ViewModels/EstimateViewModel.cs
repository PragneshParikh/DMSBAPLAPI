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
        public int? JobTypeId { get; set; }
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
        public string? JobTypeName { get; set; }  
        public int? InsuranceId { get; set; }
        public string? InsDescription { get; set; }
        public string? SurveyorName { get; set; }
        public string? ContactNumber { get; set; }
        public string? PolicyNo { get; set; }
        public DateTime? InsValidTill { get; set; }
        public bool ZeroDepo { get; set; }
        public int? JobCardNo { get; set; }
        public DateTime? JobCardCreatedDate { get; set; }
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