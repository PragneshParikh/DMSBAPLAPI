using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class DealerMaster
{
    public int Id { get; set; }

    public string Compname { get; set; } = null!;

    public string Compcode { get; set; } = null!;

    public string Adress1 { get; set; } = null!;

    public string Adress2 { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Pin { get; set; } = null!;

    public string Pan { get; set; } = null!;

    public string PhoneOff { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Contactperson { get; set; } = null!;

    public DateTime RegDate { get; set; }

    public string TradCert { get; set; } = null!;

    public string CompgstinNo { get; set; } = null!;

    public string? BrandName { get; set; }

    public string? CompImage { get; set; }

    public string Dealercode { get; set; } = null!;

    public int Areaofficeid { get; set; }

    public string? CinNo { get; set; }

    public string? VatNo { get; set; }

    public bool IsTcs { get; set; }

    public decimal TcsPercent { get; set; }

    public string? FameiiCode { get; set; }

    public decimal CeditLimit { get; set; }

    public string? RegAddress { get; set; }

    public bool B2b { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<LmsleadMaster> LmsleadMasters { get; set; } = new List<LmsleadMaster>();
}
