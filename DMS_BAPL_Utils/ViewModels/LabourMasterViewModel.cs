using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class LabourMasterViewModel
    {
        public IFormFile File { get; set; }

        public DateTime effectiveDate { get; set; }

        // public string rateType { get; set; }

        public string oemmodelname { get; set; }
    }
    public class LabourMasteUpdateViewModel
    {
        public int Id { get; set; }
        public string LabourCode { get; set; }
        public string LabourDescription { get; set; }
        public decimal? LabourRate { get; set; }
        public string? HSNCode { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public decimal? Cgst { get; set; }
        public decimal? Sgst { get; set; }
        public decimal? Igst { get; set; }
        public string OemModelName { get; set; }
        public int? CityTier { get; set; }
        public bool? IsLabourRateActive { get; set; }
        public int? JobType { get; set; }
        public int? ServiceHead { get; set; }
        public int? Servicetype { get; set; }

        public string? JobTypeName { get; set; }
        public string? ServiceHeadName { get; set; }
        public string? ServicetypeName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class PartWiseLabourMasterRateViewModel
    {
        public int Id { get; set; }
        public string? LabourCode { get; set; }
        public string? LabourName { get; set; }
        public string? PartCode { get; set; }
        public string? PartDescription { get; set; }
        public string? oemModelName { get; set; }
        public int? CityTier { get; set; }
        public decimal? LabourRate { get; set; }
        public decimal? LabourHours { get; set; }
        public decimal? Cgst { get; set; }
        public decimal? Sgst { get; set; }
        public decimal? Igst { get; set; }
        public int? JobType { get; set; }
        public string? DealerCode { get; set; }
        public string? HSNCode { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int? ServiceHead { get; set; }
        public int? Servicetype { get; set; }
        public bool? IsActive { get; set; }
        public string? JobTypeName { get; set; }
        public string? ServiceHeadName { get; set; }
        public string? ServicetypeName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

    }

    public class LabourRateDropDown
    {
        public int LabourId { get; set; }
        public string? LabourCode { get; set; }
        public string? LabourName { get; set; }
        public string? LabourDescription { get; set; }
        public string? OemModelName { get; set; }
        public decimal? LabourRate { get; set; }
        public decimal? Cgst { get; set; }
        public decimal? Sgst { get; set; }
        public decimal? Igst { get; set; }

        public string? custState { get; set; }
        public string? DealerState { get; set; }

    }
}
