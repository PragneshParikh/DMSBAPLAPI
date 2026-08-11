using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels
{
    public partial class EbwInvoiceDetail
    {
        public int Id { get; set; }
        public int EbwInvoiceHeaderId { get; set; }

        public string ItemCode { get; set; } = null!;
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public string? HsnCode { get; set; }

        public int Qty { get; set; }

        public decimal ItemMrp { get; set; }
        public decimal BaseItemRate { get; set; }
        public decimal ItemRate { get; set; }

        public decimal Discount { get; set; }
        public string DiscountType { get; set; } = "Value";

        public decimal IgstPer { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal CgstPer { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPer { get; set; }
        public decimal SgstAmount { get; set; }

        public decimal Amount { get; set; }

        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual EbwInvoiceHeader EbwInvoiceHeader { get; set; } = null!;
    }
}
