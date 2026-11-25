using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels
{
    [Table("NewZeroHoursJobDetail")]
    public partial class NewZeroHoursJobDetail
    {
        public int Id { get; set; }
        public int ZeroHoursHeaderId { get; set; }
        public int? JobCardHeaderId { get; set; }
        public int? RepairBillId { get; set; }
        public int? MaterialTransferId { get; set; }

        public string SourceType { get; set; } = null!;
        public string? ItemCode { get; set; }
        public string? ItemDesc { get; set; }
        public string? IssueType { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal NetAmount { get; set; }

        public bool IsDelete { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual NewZeroHoursJobHeader ZeroHoursHeader { get; set; } = null!;
    }
}