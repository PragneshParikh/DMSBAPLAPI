using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels
{
    [Table("DMS_RepairBill")]
    public partial class DmsRepairBill
    {
        [Key]
        public int Id { get; set; }
        public string DealerName { get; set; }
        public string DealerCode { get; set; }
        public long? UniqueKey { get; set; }
        public long? UniqueId { get; set; }
        public long? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string Location { get; set; }
        public string PartyName { get; set; }
        public int? BillType { get; set; } 
        public string CashType { get; set; }
        public string CashAccount { get; set; }
        public string RegNo { get; set; }
        public string ChassisNo { get; set; }
        public string ComputedUniqueKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}