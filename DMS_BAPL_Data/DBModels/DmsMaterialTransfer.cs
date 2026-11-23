using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels
{
    [Table("DMS_MaterialTransfer")]
    public partial class DmsMaterialTransfer
    {
        [Key]
        public int Id { get; set; }
        public string? DealerName { get; set; }
        public string? DealerCode { get; set; }
        public long? SourceUniqueId { get; set; }
        public long? SourceJobId { get; set; }
        public int? DocNo { get; set; }
        public DateTime? DocDate { get; set; }
        public string? DocType { get; set; }
        public string? Location { get; set; }
        public string? LocCode { get; set; }
        public string? TechnicianName { get; set; }
        public string UniqueKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Action { get; set; }
    }
}