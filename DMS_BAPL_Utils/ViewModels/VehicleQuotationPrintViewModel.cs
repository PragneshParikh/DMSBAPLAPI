using System;
namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleQuotationPrintViewModel
    {
        // Quotation
        public long Id { get; set; }
        public string QuotationNo { get; set; }
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidTill { get; set; }
        public string Status { get; set; }
        // Dealer
        public long DealerId { get; set; }
        public string DealerCode { get; set; }
        public string DealerName { get; set; }
        public string DealerAddress { get; set; }
        public string DealerMobile { get; set; }
        public string DealerEmail { get; set; }
        public string DealerGSTNo { get; set; }
        // Customer
        public long? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string Address { get; set; }
        public int? StateId { get; set; }
        public string StateName { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string CustomerGSTNo { get; set; }   // NEW
        public string CustomerPanNo { get; set; }   // NEW
        // Vehicle
        public long ModelId { get; set; }
        public string ModelName { get; set; }
        public long VariantId { get; set; }
        public string VariantName { get; set; }
        public long? ColorId { get; set; }
        public string ColorName { get; set; }
        // ===== Values from Item Master =====
        public decimal CustPrice { get; set; }
        public decimal Fame2Amount { get; set; }
        // GST %
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
        public decimal IGST { get; set; }
        // GST Amount
        public decimal SGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal TaxAmount { get; set; }
        // ===== Quotation Charges =====
        public decimal ExShowroomPrice { get; set; }
        public decimal RTOCharges { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal AccessoriesAmount { get; set; }
        public decimal ExtendedWarrantyAmount { get; set; }
        public decimal AmcAmount { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ExchangeAmount { get; set; }
        public decimal HypothecationAmount { get; set; }
        public decimal PlateAmount { get; set; }
        public decimal HandlingCharges { get; set; }
        // Finance
        public bool IsFinance { get; set; }
        public long? FinanceCompanyId { get; set; }
        public string FinanceCompanyName { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal DownPayment { get; set; }
        // Grand Total
        public decimal TotalAmount { get; set; }
        public string Remarks { get; set; }
    }
}