using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace DMS_BAPL_Utils.ViewModels
{
    public class AddVehicleQuotationViewModel
    {
        public long Id { get; set; }
        public string QuotationNo { get; set; }
        public DateTime QuotationDate { get; set; }
        public long DealerId { get; set; }
        public long? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }
        public string? EmailId { get; set; }
        public string? Address { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public string? CustomerGSTNo { get; set; }
        public string? CustomerPanNo { get; set; }
        public long ModelId { get; set; }
        public long VariantId { get; set; }
        public long? ColorId { get; set; }
        public decimal CustPrice { get; set; }
        public decimal Fame2Amount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal ExShowroomPrice { get; set; }
        public decimal RTOCharges { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal AccessoriesAmount { get; set; }
        public decimal ExtendedWarrantyAmount { get; set; }
        public decimal AMCAmount { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsExchange { get; set; }
        public decimal ExchangeAmount { get; set; }
        // NEW — captured inside the Exchange section
        public string? OldCompanyName { get; set; }
        public string? OldModelName { get; set; }
        public bool IsFinance { get; set; }
        public long? FinanceCompanyId { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal DownPayment { get; set; }
        public string Status { get; set; }
        public string? Remarks { get; set; }
        public decimal HypothecationAmount { get; set; }
        public decimal PlateAmount { get; set; }
        public decimal HandlingCharges { get; set; }
        public DateTime? ValidTillDate { get; set; }
        [BindNever]
        public string CreatedBy { get; set; } = string.Empty;
    }
}