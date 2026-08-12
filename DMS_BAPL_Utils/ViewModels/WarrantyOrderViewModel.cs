using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DMS_BAPL_Utils.ViewModels 
{ 
    public class WarrantyOrderViewModel
    {
        public int Id { get; set; }
        public string? DealerCode { get; set; }

        [Required(ErrorMessage = "Date From is required.")]
        public DateTime? DateFrom { get; set; }

        [Required(ErrorMessage = "Date To is required.")]
        public DateTime? DateTo { get; set; }

        [Required(ErrorMessage = "Batch No is required.")]
        public string? BatchNo { get; set; }

        [Required(ErrorMessage = "Batch Date is required.")]
        public DateTime? BatchDate { get; set; }

        [Required(ErrorMessage = "Order No is required.")]
        public string? OrderNo { get; set; }

        [Required(ErrorMessage = "Order Date is required.")]
        public DateTime? OrderDate { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public string? Location { get; set; }

        public string? LocationName { get; set; }

        [Required(ErrorMessage = "Claim Type is required.")]
        public string? ClaimType { get; set; }

        [Required(ErrorMessage = "Supplier is required.")]
        public int? SupplierId { get; set; }
        public bool IsApproved { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public List<int> WarrantyClaimIds { get; set; } = new List<int>();

        public List<ClaimApprovalViewModel>? ClaimApprovals { get; set; }
        public List<WarrantyJCClaimFullViewModel>? Claims { get; set; }
    }

    public class ClaimApprovalViewModel
    {
        public int ClaimId { get; set; }
        public bool IsApproved { get; set; }
    }

    public class WarrantyOrderSearchViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? BatchDate { get; set; }
        public string? OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Location { get; set; }
        public string? ClaimType { get; set; }
        public int? SupplierId { get; set; }
        public bool? IsApproved { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class WarrantyOrderListViewModel
    {
        public int Id { get; set; }
        public string BatchNo { get; set; } = null!;
        public DateTime BatchDate { get; set; }
        public string OrderNo { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public string Location { get; set; } = null!;
        public string ClaimType { get; set; } = null!;
        public int SupplierId { get; set; }
        public decimal TotalMrp { get; set; }
        public int TotalClaims { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
    }
    public class WarrantyOrderSearchResultViewModel
    {
        public List<WarrantyOrderListViewModel> Items { get; set; } = new List<WarrantyOrderListViewModel>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }

    public class NextOrderNumberViewModel
    {
        public string BatchNo { get; set; } = null!;
        public string OrderNo { get; set; } = null!;
    }

    public class WarrantyJCClaimFullViewModel
    {
        public int Id { get; set; }
        public string? ClaimPrefix { get; set; }
        public int? ClaimNo { get; set; }
        public DateTime? ClaimDate { get; set; }
        public string? ChassisNo { get; set; }
        public string? JobCardNo { get; set; }
        public DateTime? JobCardDate { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? ServiceHead { get; set; }
        public decimal? Kms { get; set; }
        public string? MotorNo { get; set; }
        public string? PartyName { get; set; }
        public int? SupplierId { get; set; }
        public string? ServiceLocation { get; set; }
        public string? LocationName { get; set; }
        public bool IsApproved { get; set; } = false;

        public List<WarrantyJCClaimDetailLineViewModel> Details { get; set; } = new List<WarrantyJCClaimDetailLineViewModel>();
    }

    public class WarrantyJCClaimDetailLineViewModel
    {
        public int Id { get; set; }

        public string? ItemType { get; set; }
        public string? PartCode { get; set; }
        public string? PartName { get; set; }
        public string? PartDescription { get; set; }
        public string? LabourCode { get; set; }
        public string? LabourDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
        public decimal Mrp { get; set; }
        public string? DealerObservation { get; set; }
        public string? RootCauseAnalysis { get; set; }
    }

    public class WarrantyJCClaimUpdateViewModel
    {
        public int ClaimId { get; set; }
        public List<WarrantyJCClaimLineUpdateViewModel> Lines { get; set; } = new();
    }

    public class WarrantyJCClaimLineUpdateViewModel
    {
        public int DetailId { get; set; }
        public string? DealerObservation { get; set; }
        public string? RootCauseAnalysis { get; set; }
    }
}