using System;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleQuotationFilterViewModel
    {
        public string DealerCode { get; set; }

        public string QuotationNo { get; set; }

        public string CustomerName { get; set; }

        public string MobileNo { get; set; }

        public int? ModelId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool? IsApproved { get; set; }
    }
}