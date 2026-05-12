using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }

        public string InvoiceType { get; set; } = null!;

        public string ServiceType { get; set; } = null!;

        public string DocumentNo { get; set; } = null!;

        public int? ReferenceId { get; set; }

        public bool? IsFinalized { get; set; }

        public int? CustomerId { get; set; }

        public decimal? TotalAmount { get; set; }

        public decimal? TaxAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? NetAmount { get; set; }

        public string? Status { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? DealerCode { get; set; }

        public List<InvoiceDetailVM> InvoiceDetails { get; set; }

    }

    public class InvoiceDetailVM
    {
        public int Id { get; set; }

        public int? ItemId { get; set; }

        public string? Description { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? Rate { get; set; }

        public decimal? TaxPercent { get; set; }

        public decimal? Amount { get; set; }
    }
}
