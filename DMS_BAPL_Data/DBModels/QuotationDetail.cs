using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class QuotationDetail
{
    public int Id { get; set; }

    public int QuotationId { get; set; }

    public string ItemCode { get; set; } = null!;

    public decimal? Rate { get; set; }

    public decimal? Gstper { get; set; }

    public decimal? Igstper { get; set; }

    public decimal? Cgstamnt { get; set; }

    public decimal? Igstamnt { get; set; }

    public decimal? Sgstamnt { get; set; }

    public decimal? Hsrpcharges { get; set; }

    public decimal? InsuranceAmnt { get; set; }

    public decimal? Rtoamnt { get; set; }

    public decimal? AccessoryAmnt { get; set; }

    public decimal? OtherAmnt { get; set; }

    public decimal? WarrantyAmnt { get; set; }

    public decimal? Discount { get; set; }

    public decimal? GoodLifeAmount { get; set; }

    public decimal? Hpaamount { get; set; }

    public decimal? Institute { get; set; }

    public decimal? Connectivity4G { get; set; }

    public decimal? MunicipalTaxAmnt { get; set; }

    public decimal? Afee { get; set; }

    public decimal? HandlingAmount { get; set; }

    public decimal? SubsidyAmnt { get; set; }

    public decimal? StateSubsidyAmnt { get; set; }

    public decimal? CoatingAmnt { get; set; }

    public decimal? TotalAmnt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual QuotationHeader Quotation { get; set; } = null!;
}
