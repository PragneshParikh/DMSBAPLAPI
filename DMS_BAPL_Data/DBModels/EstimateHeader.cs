using System;
using System.Collections.Generic;
namespace DMS_BAPL_Data.DBModels
{
    public class EstimateHeader
    {
        public int Id { get; set; }
        public string? EstimationNo { get; set; }
        public DateTime EstimateDate { get; set; }
        public string? ChassisNo { get; set; }
        // Customer details — snapshotted at time of estimate creation
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPin { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }
        public int? Kms { get; set; }
        public int? JobTypeId { get; set; }
        public int? InsuranceId { get; set; }
        public string? InsDescription { get; set; }
        public string? SurveyorName { get; set; }
        public string? ContactNumber { get; set; }
        public string? PolicyNo { get; set; }
        public DateTime? InsValidTill { get; set; }
        public bool ZeroDepo { get; set; }

        public string? DealerCode { get; set; }
        public string Status { get; set; } = "Open";
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        // Navigation properties
        public JobType? JobType { get; set; }
        public ICollection<EstimateDetail> EstimateDetails { get; set; } = new List<EstimateDetail>();
    }
}