using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class VehicleQuotation
{
    public long Id { get; set; }

    public string QuotationNo { get; set; } = null!;

    public DateTime QuotationDate { get; set; }

    public long DealerId { get; set; }

    public long? CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? MobileNo { get; set; }

    public string? EmailId { get; set; }

    public string? Address { get; set; }

    public long ModelId { get; set; }

    public long VariantId { get; set; }

    public long? ColorId { get; set; }

    public decimal ExShowroomPrice { get; set; }

    public decimal Rtocharges { get; set; }

    public decimal InsuranceAmount { get; set; }

    public decimal AccessoriesAmount { get; set; }

    public decimal ExtendedWarrantyAmount { get; set; }

    public decimal Amcamount { get; set; }

    public decimal OtherCharges { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsExchange { get; set; }

    public decimal ExchangeAmount { get; set; }

    public bool IsFinance { get; set; }

    public long? FinanceCompanyId { get; set; }

    public decimal? LoanAmount { get; set; }

    public decimal? DownPayment { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public decimal? HypothecationAmount { get; set; }

    public decimal? PlateAmount { get; set; }

    public decimal? HandlingCharges { get; set; }

    public DateOnly? ValidTillDate { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }
}
