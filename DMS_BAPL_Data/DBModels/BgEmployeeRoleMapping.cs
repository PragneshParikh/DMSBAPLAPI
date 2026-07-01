using System;

namespace DMS_BAPL_Data.DBModels
{
    public class BgEmployeeRoleMapping
    {
        public int Id { get; set; }
        public int BgEmployeeId { get; set; }
        public string Category { get; set; }
        public string RoleName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public BgEmployeeMaster BgEmployee { get; set; }
    }
}