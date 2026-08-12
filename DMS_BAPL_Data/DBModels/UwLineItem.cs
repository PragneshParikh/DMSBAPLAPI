using System;

namespace DMS_BAPL_Data.DBModels
{
    public partial class UwLineItem
    {
        public int Id { get; set; }
        public int WarrantyJcclaimId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected
        public string? RejectionReason { get; set; }
        public string? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual WarrantyJcclaim WarrantyJcclaim { get; set; } = null!;
    }
}