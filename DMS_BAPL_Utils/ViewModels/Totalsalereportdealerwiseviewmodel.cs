using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // One row per dealer — full financial rollup, mirroring the amount fields
    // already on VehicleSaleReportViewModel/VehicleSaleBillReportViewModel.
    // Percentage fields (SGST%/CGST%/IGST%) are deliberately NOT included here
    // since summing a percentage across many transactions isn't meaningful —
    // only the amount fields are aggregated.
    public class TotalSaleReportDealerWiseViewModel
    {
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerCity { get; set; }
        public string? DealerState { get; set; }

        public int TotalUnitsSold { get; set; }
        public int CashCount { get; set; }
        public int CreditCount { get; set; }

        public decimal TotalItemRate { get; set; }
        public decimal TotalPreGstDiscount { get; set; }
        public decimal TotalTaxableAmount { get; set; }
        public decimal TotalSgstAmount { get; set; }
        public decimal TotalCgstAmount { get; set; }
        public decimal TotalIgstAmount { get; set; }
        public decimal TotalFameIIDiscount { get; set; }
        public decimal TotalRegAmount { get; set; }
        public decimal TotalInsuranceAmount { get; set; }
        public decimal TotalPostGstDiscount { get; set; }
        public decimal TotalFinalAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Backs GET api/Report/total-sale-dealer-wise.
    public class TotalSaleReportDealerWiseResponse
    {
        public List<TotalSaleReportDealerWiseViewModel> Rows { get; set; } = new();
        public TotalSaleReportDealerWiseViewModel? GrandTotal { get; set; }
    }
}