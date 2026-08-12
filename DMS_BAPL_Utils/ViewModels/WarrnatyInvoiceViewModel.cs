using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DMS_BAPL_Utils.ViewModels
{
    // Full header + linked-orders shape, used for Insert/Update payloads
    // and as the read response for GetWarrantyInvoiceById.
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

        // New, genuine prefix concept for invoices, mirroring how
        // ClaimPrefix works for Warranty Claims - fetched via
        // PrefixService, separate from the existing sequential InvoiceNo.
        public string? InvoicePrefix { get; set; }

        [Required(ErrorMessage = "Invoice No is required.")]
        public string? InvoiceNo { get; set; }

        [Required(ErrorMessage = "Invoice Date is required.")]
        public DateTime? InvoiceDate { get; set; }

        [Required(ErrorMessage = "Claim Type is required.")]
        public string? ClaimType { get; set; }

        [Required(ErrorMessage = "Supplier is required.")]
        public int? SupplierId { get; set; }

        // Set true to mark this invoice approved via the Save button - shows
        // up in the Approved Warranty Invoice List once saved.
        public bool IsApproved { get; set; } = false;

        // Populated only when reading (GetWarrantyInvoiceById) - lets the
        // frontend tell a soft-deleted invoice apart from an active one,
        // same purpose as WarrantyOrderViewModel.IsActive.
        public bool IsActive { get; set; } = true;

        // Ids of every WarrantyOrder this invoice batches.
        public List<int> WarrantyOrderIds { get; set; } = new();

        // Per-order approval state - mirrors ClaimApprovalViewModel's role
        // one level up (orders here instead of claims).
        public List<OrderApprovalViewModel> OrderApprovals { get; set; } = new();

        // Populated only on read - each linked order's resolved snapshot data.
        public List<WarrantyOrderSummaryViewModel> Orders { get; set; } = new();
    }

    public class OrderApprovalViewModel
    {
        public int OrderId { get; set; }
        public bool IsApproved { get; set; }
    }

    // One linked order's resolved display data - mirrors
    // WarrantyJCClaimFullViewModel's role one level up.
    public class WarrantyOrderSummaryViewModel
    {
        public int Id { get; set; } // the WarrantyOrder's own Id
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

        // The PARENT invoice's own identifying fields (not the order's) -
        // added so the grid can show "which invoice" each row belongs to
        // directly, without relying on frontend-side tagging alone.
        public string? InvoicePrefix { get; set; }
        public string? InvoiceNo { get; set; }
        public string? InvoiceBatchNo { get; set; }

        // Full claim/line-item detail for this order, read from the
        // order's own WarrantyOrderGridDetail snapshot (stable - only
        // changes if the order itself is re-saved). Same shape
        // GetWarrantyOrderById already returns for the Warranty Order page.
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

        // Added to match the reference layout's "Claim Date From | Date To"
        // column - these already exist on WarrantyInvoice itself, just
        // weren't in this particular list projection before.
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Derived from each linked line's confirmed ItemType (Part/Labour) -
        // "Warranty -Labour" if every line is Labour, "Warranty -Parts" if
        // every line is Part, "Warranty -Mixed" if both appear. Computed
        // rather than read from a new column, since no such column has
        // been confirmed to exist.
        public string? ClaimSubType { get; set; }

        // PLACEHOLDER - PackingSlipNo/PackingSlipDate genuinely not wired
        // up yet. Confirmed to live in a separate table, but the exact
        // table name and join key are still needed - see chat.
        public string? PackingSlipNo { get; set; }
        public DateTime? PackingSlipDate { get; set; }

        // PLACEHOLDER - confirmed to be a field on WarrantyInvoice itself,
        // but the exact property name is still needed - see chat.
        public string? ErpInvoiceNo { get; set; }

        // Shows the FIRST linked order's own location - an invoice can
        // technically batch orders from different locations, but the
        // established usage pattern is one order per invoice (same
        // convention already used for repairBillNo/repairBillDate on the
        // form page).
        public string? LocationName { get; set; }
    }

    public class WarrantyInvoiceSearchViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? BatchNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? ClaimType { get; set; }
        public int? SupplierId { get; set; }
        public bool? IsApproved { get; set; }

        // Filters to invoices that have at least one linked order at this
        // location - Location lives on each order (via the
        // WarrantyInvoiceGridDetail snapshot), not on the invoice header
        // itself, since one invoice can batch orders from different locations.
        public string? Location { get; set; }

        // Same purpose as WarrantyOrderSearchViewModel.IncludeInactive -
        // defaults false so the List page keeps excluding soft-deleted
        // invoices; the form page's "show latest" fallback opts in.
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