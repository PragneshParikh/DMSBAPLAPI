using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels
{
    [Table("DMS_RepairBillItem")]
    public partial class DmsRepairBillItem
    {
        [Key]
        public int Id { get; set; }
        public int RepairBillId { get; set; }
        public long? SourceItemId { get; set; }
        public long? ItemIdno { get; set; }
        public string ItemCode { get; set; }
        public string ItemDesc { get; set; }
        public string ItemType { get; set; }
        public int? Qty { get; set; } 
        public decimal? Rate { get; set; }
        public string IssueType { get; set; }
        public decimal? SgstPer { get; set; }
        public decimal? SgstAmount { get; set; }
        public decimal? CgstPer { get; set; }
        public decimal? CgstAmount { get; set; }
        public decimal? IgstPer { get; set; }
        public decimal? IgstAmount { get; set; }
        public decimal? WavRate { get; set; }
        public decimal? TotAmnt { get; set; }
        public string MtrlIssue { get; set; }
    }
}