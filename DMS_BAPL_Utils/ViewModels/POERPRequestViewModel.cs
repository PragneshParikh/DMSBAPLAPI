using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class POERPRequestViewModel
    {
        public SOHeader soHeader { get; set; }
        public List<SOLine> soLine { get; set; }

    }
    public class SOHeader
    {
        public string refno { get; set; }
        public string pordrdate { get; set; }
        public string pordr_type { get; set; }
        public string ordrtype { get; set; }
        public string testcertificate { get; set; }
        public string consigneecode { get; set; }
        public string customercode { get; set; }
        public string amount { get; set; }
        public string FameIIFlag { get; set; }
        public string? TransactionType { get; set; }

    }

    public class SOLine
    {
        public string Itemname { get; set; }
        public string modlname { get; set; }
        public string descriptions { get; set; }
        public string Unit { get; set; }
        public string qty { get; set; }
        public string itemmodelname { get; set; }
        public string colridno { get; set; }
        public string colrcode { get; set; }
        public string dmspordridno { get; set; }
    }
}
