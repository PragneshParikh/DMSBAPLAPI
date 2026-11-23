using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels
{
    [Table("DMS_MaterialTransferItem")]
    public partial class DmsMaterialTransferItem
    {
        [Key]
        public int Id { get; set; }
        public int MaterialTransferId { get; set; }
        public long? SourceLineId { get; set; }
        public long? ItemIdno { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public string? ItemType { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal SgstPer { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal CgstPer { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal IgstPer { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Mrp { get; set; }
    }
}