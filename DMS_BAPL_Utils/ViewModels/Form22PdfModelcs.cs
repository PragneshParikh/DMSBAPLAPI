using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class Form22PdfModel
    {
        // ── Dealer / issuer header ─────────────────────────────────────
        public string? DealerName { get; set; }
        public string? DealerAddress { get; set; }
        public string? DealerPhone { get; set; }
        public string? DealerGstin { get; set; }
        public string? DealerTradeCertNo { get; set; }

        public string? SaleBillNo { get; set; }
        public DateTime SaleDate { get; set; }

        public string? OwnerName { get; set; }
        public string? OwnerAddress { get; set; }
        public string? OwnerMobile { get; set; }

        public List<Form22VehicleLine> Vehicles { get; set; } = new();
    }
}
