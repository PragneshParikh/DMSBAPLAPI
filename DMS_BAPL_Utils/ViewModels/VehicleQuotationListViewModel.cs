using System;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleQuotationListViewModel
    {
        public int VehicleQuotationId { get; set; }

        public string QuotationNo { get; set; }

        public DateTime QuotationDate { get; set; }

        public string CustomerName { get; set; }

        public string MobileNo { get; set; }

        public string ModelName { get; set; }

        public string VariantName { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime ValidTill { get; set; }

        public bool IsApproved { get; set; }

        public bool IsActive { get; set; }
    }
}