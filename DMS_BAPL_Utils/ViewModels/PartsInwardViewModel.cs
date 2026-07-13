using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PartsInwardViewModel
    {
        public int id { get; set; }

        public string? invoice_date { get; set; }

        public string invoice_no { get; set; } = null!;

        public string part_no { get; set; } = null!;

        public int item_idno { get; set; }

        public string item_hsncode { get; set; } = null!;

        public decimal item_rate { get; set; }

        public decimal item_mrp { get; set; }

        public int item_qty { get; set; }

        public decimal sgst { get; set; }

        public decimal cgst { get; set; }

        public decimal igst { get; set; }

        public decimal ugst { get; set; }

        public decimal item_disc { get; set; }

        public string discount_type { get; set; } = null!;

        public string loc_code { get; set; } = null!;

        public string dealer_code { get; set; } = null!;

        public bool? is_accepted { get; set; }
    }
}
