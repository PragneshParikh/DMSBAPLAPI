
using System;

namespace DMS_BAPL_Data.DBModels
{
    public partial class DispatchMaster
    {
        public int Id { get; set; }
        public string MasterType { get; set; }
        public string MasterName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}