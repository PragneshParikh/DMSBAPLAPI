using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class DealerMasterViewModel
    {
        public string Compname { get; set; } = null!;

        public string Compcode { get; set; } = null!;

        public string Adress1 { get; set; } = null!;

        public string Adress2 { get; set; } = null!;

        public string City { get; set; } = null!;

        public string State { get; set; } = null!;

        public string Pin { get; set; } = null!;

        public string Pan { get; set; } = null!;

        public string? PhoneOff { get; set; } = null!;

        public string Mobile { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Contactperson { get; set; } = null!;

        [JsonPropertyName("reg_date")]
        public string RegDate { get; set; } = null!;

        public string? TradCert { get; set; } = null!;

        public string? CompgstinNo { get; set; } = null!;

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

       

    }
}
