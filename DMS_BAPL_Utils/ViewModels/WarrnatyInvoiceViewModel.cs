using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DMS_BAPL_Utils.ViewModels
{
    public class WarrantyInvoiceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Dealer code is required.")]
        public string? DealerCode { get; set; }

        [Required(ErrorMessage = "Date From is required.")]
        public DateTime? DateFrom { get; set; }

        [Required(ErrorMessage = "Date To is required.")]
        public DateTime? DateTo { get; set; }

        public string? BatchNo { get; set; }
        public DateTime? BatchDate { get; set; }

        public string? InvoicePrefix { get; set; }

        [Required(ErrorMessage = "Invoice No is required.")]
        public string? InvoiceNo { get; set; }

        [Required(ErrorMessage = "Invoice Date is required.")]
        public DateTime? InvoiceDate { get; set; }

        [Required(ErrorMessage = "Claim Type is required.")]
        public string? ClaimType { get; set; }

        [Required(ErrorMessage = "Supplier is required.")]
        public int? SupplierId { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public List<int> WarrantyOrderIds { get; set; } = new();

        public List<OrderApprovalViewModel> OrderApprovals { get; set; } = new();
        public List<WarrantyOrderSummaryViewModel> Orders { get; set; } = new();
    }

    public class OrderApprovalViewModel
    {
        public int OrderId { get; set; }
        public bool IsApproved { get; set; }
    }

    public class WarrantyOrderSummaryViewModel
    {
        public int Id { get; set; } 
        public string? OrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? BatchDate { get; set; }
        public string? Location { get; set; }
        public string? LocationName { get; set; }
        public string? ClaimType { get; set; }
        public int? SupplierId { get; set; }
        public string? PartyName { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalMrp { get; set; }
        public bool IsApproved { get; set; }
        public string? InvoicePrefix { get; set; }
        public string? InvoiceNo { get; set; }
        public string? InvoiceBatchNo { get; set; }
        public List<WarrantyJCClaimFullViewModel> Claims { get; set; } = new();
    }

    // Row shape for the Warranty Invoice List page.
    public class WarrantyInvoiceListViewModel
    {
        public int Id { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? BatchDate { get; set; }
        public string? InvoicePrefix { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? ClaimType { get; set; }
        public int? SupplierId { get; set; }
        public int TotalOrders { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? ClaimSubType { get; set; }
        public string? PackingSlipNo { get; set; }
        public DateTime? PackingSlipDate { get; set; }
        public string? ErpInvoiceNo { get; set; }
        public string? LocationName { get; set; }
    }

    public class WarrantyInvoiceSearchViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? BatchNo { get; set; }
        public string? DealerCode { get; set; }
        public string? InvoiceNo { get; set; }
        public string? ClaimInvoiceNo { get; set; }
        public string? ClaimType { get; set; }
        public int? SupplierId { get; set; }
        public bool? IsApproved { get; set; }
        public string? Location { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class WarrantyInvoiceSearchResultViewModel
    {
        public List<WarrantyInvoiceListViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}