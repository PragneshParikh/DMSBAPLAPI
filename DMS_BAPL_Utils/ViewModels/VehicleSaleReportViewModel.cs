namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleReportViewModel
    {
        public int SrNo { get; set; }
        public string? ModelCode { get; set; }
        public string? ModelDescription { get; set; }
        public string? OemModelName { get; set; }
        public string? VehicleGroup { get; set; }
        public string? ColorCode { get; set; }
        public string? ChasisNo { get; set; }
        public string? RegNo { get; set; }
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerCity { get; set; }
        public string? DealerState { get; set; }
        public string? Location { get; set; }
        public string? LocCode { get; set; }
        public string? LocCity { get; set; }
        public string? Name { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? CustomerState { get; set; }
        public string? CustomerCity { get; set; }
        public string? Pin { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? Type { get; set; }
        public string? BookingId { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? SaleDate { get; set; }
        public string? InvoiceNo { get; set; }
        public int? BillType { get; set; }
        public string? FinanceBy { get; set; }

        // ── NEW: raw Financier ledger ID, for diagnosing join-vs-data gaps ──
        public int? FinancierId { get; set; }

        public string? FinancerCode { get; set; }
        public string? FinancerCategory { get; set; }
        public string? ExecutiveName { get; set; }
        public string? ProspectName { get; set; }
        public string? ProspectMobNo { get; set; }
        public string? MotorNumber { get; set; }
        public string? BatteryNo { get; set; }
        public string? BatteryNo2 { get; set; }
        public string? BatteryNo3 { get; set; }
        public string? BatteryNo4 { get; set; }
        public string? BatteryNo5 { get; set; }
        public string? BatteryNo6 { get; set; }
        public string? BatteryCapacity { get; set; }
        public decimal? SubsidyAmount { get; set; }
        public bool? FameIIRequired { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? BillDate { get; set; }
        public string? SaleBillNo { get; set; }
        public decimal ItemRate { get; set; }
        public decimal PreGstDiscount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal Sgstper { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal Cgstper { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal Igstper { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalGstAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? BillingName { get; set; }
        public string? Hsn { get; set; }
        public int? MfgYear { get; set; }
        public string? SaleType { get; set; }
        public string? Status { get; set; }
        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }
    }
}