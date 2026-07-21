using System;
namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleQuotationViewModel
    {
        public long VehicleQuotationId { get; set; }
        public string QuotationNo { get; set; }
        public DateTime QuotationDate { get; set; }
        public long? DealerId { get; set; }
        public int? StateId { get; set; }
        public string StateName { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string Status { get; set; }
        public string DealerCode { get; set; }
        public string DealerName { get; set; }
        public long? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string Address { get; set; }
        public string? CustomerGSTNo { get; set; }
        public string? CustomerPanNo { get; set; }
        public long ModelId { get; set; }
        public string ModelName { get; set; }
        public long VariantId { get; set; }
        public string VariantName { get; set; }
        public long? ColorId { get; set; }
        public string ColorName { get; set; }
        public decimal CustPrice { get; set; }
        public decimal Fame2Amount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ExShowroomPrice { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal RegistrationAmount { get; set; }
        public decimal AccessoriesAmount { get; set; }
        public decimal ExtendedWarrantyAmount { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal? HypothecationAmount { get; set; }
        public decimal? PlateAmount { get; set; }
        public decimal? HandlingCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ValidTill { get; set; }
        public string Remarks { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // FIX: these were never returned by GetAll/GetById at all — meaning
        // reopening a saved quotation for edit silently blanked out Exchange
        // Amount and the entire Finance section, even though both were
        // correctly persisted. The Angular edit-load code was already
        // written expecting financeCompanyId; it just never received it.
        public bool IsExchange { get; set; }
        public decimal ExchangeAmount { get; set; }
        public bool IsFinance { get; set; }
        public long? FinanceCompanyId { get; set; }
        public decimal? LoanAmount { get; set; }
        public decimal? DownPayment { get; set; }

        // NEW — captured inside the Exchange section
        public string? OldCompanyName { get; set; }
        public string? OldModelName { get; set; }
    }
}