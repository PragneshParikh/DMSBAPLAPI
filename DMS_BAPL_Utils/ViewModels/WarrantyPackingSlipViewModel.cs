using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // One row in the "packable lines" grid.
    public class PackingSlipLineViewModel
    {
        public int WarrantyOrderGridDetailId { get; set; }
        public string? ClaimNo { get; set; }
        public string? ItemType { get; set; }
        public string? PartCode { get; set; }
        public string? PartDescription { get; set; }
        public decimal InvoicedQty { get; set; }
        public decimal AlreadyPackedQty { get; set; }
        public decimal RemainingQty { get; set; }
    }

    public class WarrantyPackingSlipViewModel
    {
        public int Id { get; set; }
        public string DealerCode { get; set; } = null!;
        public int WarrantyInvoiceHeaderId { get; set; }
        public string? SlipPrefix { get; set; }
        public string SlipNo { get; set; } = null!;
        public DateTime SlipDate { get; set; }
        public List<WarrantyPackingSlipBoxViewModel> Boxes { get; set; } = new();
    }

    public class WarrantyPackingSlipBoxViewModel
    {
        public string BoxNumber { get; set; } = null!;
        public string? BoxType { get; set; }

        public string Length { get; set; } = "0";
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public List<WarrantyPackingSlipDetailViewModel> Details { get; set; } = new();
    }

    public class WarrantyPackingSlipDetailViewModel
    {
        public int WarrantyOrderGridDetailId { get; set; }
        public string? PrnNo { get; set; }
        public decimal Qty { get; set; }
    }

    // --- Search / List page ---

    public class WarrantyPackingSlipSearchViewModel
    {
        public string? DealerCode { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SlipNo { get; set; }
        public string? InvoiceNo { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class WarrantyPackingSlipListViewModel
    {
        public int Id { get; set; }
        public string? SlipPrefix { get; set; }
        public string SlipNo { get; set; } = null!;
        public DateTime SlipDate { get; set; }
        public int WarrantyInvoiceHeaderId { get; set; }
        public string? InvoicePrefix { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int TotalBoxes { get; set; }
        public decimal TotalQty { get; set; }
        public bool IsActive { get; set; }
    }

    public class WarrantyPackingSlipSearchResultViewModel
    {
        public List<WarrantyPackingSlipListViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // --- Used by the read-only "View" modal on the list page ---

    public class WarrantyPackingSlipDetailsViewModel
    {
        public int Id { get; set; }
        public string DealerCode { get; set; } = null!;
        public int WarrantyInvoiceHeaderId { get; set; }
        public string? InvoicePrefix { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? SlipPrefix { get; set; }
        public string SlipNo { get; set; } = null!;
        public DateTime SlipDate { get; set; }
        public List<WarrantyPackingSlipBoxDetailsViewModel> Boxes { get; set; } = new();
    }

    public class WarrantyPackingSlipBoxDetailsViewModel
    {
        public string BoxNumber { get; set; } = null!;
        public string? BoxType { get; set; }
        public string? Length { get; set; }
        public List<WarrantyPackingSlipLineDetailsViewModel> Lines { get; set; } = new();
    }

    public class WarrantyPackingSlipLineDetailsViewModel
    {
        public int WarrantyOrderGridDetailId { get; set; }
        public string? ItemType { get; set; }
        public string? ClaimNo { get; set; }
        public string? PartCode { get; set; }
        public string? PartDescription { get; set; }
        public decimal Qty { get; set; }
    }

    public class WarrantyPackingSlipLineSearchViewModel
    {
        public string? DealerCode { get; set; }
        public DateTime? DateFrom { get; set; }       
        public DateTime? DateTo { get; set; }
        public string? InvoiceNo { get; set; }        
        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }
        public string? SlipNo { get; set; }           
        public string? SearchText { get; set; }        
        public bool IncludeInactive { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WarrantyPackingSlipLineListViewModel
    {
        public int WarrantyPackingSlipHeaderId { get; set; }   
        public int DetailId { get; set; }
        public string? ClaimNo { get; set; }
        public string? SlipPrefix { get; set; }
        public string SlipNo { get; set; } = null!;
        public DateTime SlipDate { get; set; }
        public string? InvoicePrefix { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string BoxNumber { get; set; } = null!;
        public string? BoxType { get; set; }
        public string? ItemType { get; set; }
        public string? PartsNumber { get; set; }
        public string? PartsDescription { get; set; }
        public decimal Qty { get; set; }
        public string? Dimension { get; set; }
    }

    public class WarrantyPackingSlipLineSearchResultViewModel
    {
        public List<WarrantyPackingSlipLineListViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}