using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    // Filter/paging parameters for the Invoice Dispatch master list.
    // NOTE: LocCode removed — InvoiceHeader has no location column.
    public class InvoiceDispatchViewModel
    {
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    // Generic paged wrapper so the Angular side can render pagination
    // without a separate count call.
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<T> Data { get; set; } = new();
    }

    // One row of the Invoice Dispatch master — Part tab.
    // Sourced from InvoiceHeader (InvoiceType == "PART") + InvoiceDetail.
    // InvoiceDetail is generic (no PartNo/HSN/GST split), so this only
    // carries what actually exists on that table.
    public class PartDispatchListViewModel
    {
        public int SrNo { get; set; }
        public int Id { get; set; }               // InvoiceDetail.Id
        public int InvoiceHeaderId { get; set; }   // InvoiceHeader.Id

        public string InvoiceNo { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }  // InvoiceHeader.CreatedDate (no dedicated InvoiceDate column found)

        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxPercent { get; set; }

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }

        public string Status { get; set; } = null!;   // InvoiceHeader.Status

        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }

    // One row of the Invoice Dispatch master — Vehicle tab.
    // Same shape as the Part tab — the only real difference server-side is
    // the InvoiceType filter, since InvoiceDetail carries no vehicle-specific
    // fields (ChasisNo, MotorNo, ColorCode, etc.).
    public class VehicleDispatchListViewModel
    {
        public int SrNo { get; set; }
        public int Id { get; set; }
        public int InvoiceHeaderId { get; set; }

        public string InvoiceNo { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }

        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxPercent { get; set; }

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
