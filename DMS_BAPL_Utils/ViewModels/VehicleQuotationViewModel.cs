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
        public long ModelId { get; set; }
        public string ModelName { get; set; }         
        public long VariantId { get; set; }
        public string VariantName { get; set; }       
        public long? ColorId { get; set; }
        public string ColorName { get; set; }          
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
    }
}