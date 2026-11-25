using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels
{
    [Table("NewZeroHoursJobHeader")]
    public partial class NewZeroHoursJobHeader
    {
        public int Id { get; set; }
        public int? JobCardHeaderId { get; set; }
        public int? RepairBillHeaderId { get; set; }

        // Company / Dealer / Location / State
        public string? CompName { get; set; }
        public string DealerCode { get; set; } = null!;
        public string? DealerState { get; set; }
        public string? DealerType { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }

        // Job & Document Details
        public DateOnly? JobDate { get; set; }
        public string? JobNo { get; set; }
        public string? JobCategory { get; set; }
        public string? DocNo { get; set; }
        public DateOnly? DocDate { get; set; }
        public string? JobStatus { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public DateOnly? CloseDate { get; set; }
        public DateOnly? SaleDate { get; set; }
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public string? InTime { get; set; }
        public string? CloseTime { get; set; }

        // Vehicle & Customer Details
        public string? Model { get; set; }
        public string? BrandName { get; set; }
        public string? PartyName { get; set; }
        public string? PartyMobile { get; set; }
        public string? Supervisor { get; set; }
        public string? Technician { get; set; }
        public decimal? KMS { get; set; }
        public string? ServiceHead { get; set; }
        public string? JobType { get; set; }
        public string? RegNo { get; set; }
        public string? MotorNo { get; set; }
        public string? ChassisNo { get; set; }
        public string? CopnNo { get; set; }
        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? ConverterNo { get; set; }
        public string? CreateThrough { get; set; }

        // Financial Summary
        public decimal EstimatedJobExpenses { get; set; }
        public decimal PartsAmount { get; set; }
        public decimal LabourAmount { get; set; }
        public decimal TotalWoTax { get; set; }
        public decimal GstAmnt { get; set; }
        public decimal IgstAmnt { get; set; }
        public decimal NetTotal { get; set; }

        // Audit & Flags
        public bool IsActive { get; set; } = true;
        public bool IsDelete { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual ICollection<NewZeroHoursJobDetail> NewZeroHoursJobDetails { get; set; }= new List<NewZeroHoursJobDetail>();
    }
}