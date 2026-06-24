using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PendingApprovalJobCardViewModel
    {
        public int JobCardId { get; set; }
        public int? JobNo { get; set; }
        public DateOnly? JobDate { get; set; }
        public int RepairBillNo { get; set; }
        public DateTime? RepairBillDate { get; set; }
        public string? RegistrationNo { get; set; }
        public string ChassisNo { get; set; }
        public string MotorNo { get; set; }
        public int? VehicleKMs { get; set; }
        public string PartyName { get; set; }
        public string MobileNo { get; set; }
        public DateTime SaleDate { get; set; }
        public int? Days { get; set; }
        public string Supervisor { get; set; }
        public string Technician { get; set; }
        public string ModelName { get; set; }
        public string CouponNo { get; set; }
        public string ServiceHead { get; set; }
        public string ServiceType { get; set; }
        public string? DealerCode { get; set; }
        public decimal? Rate { get; set; }
        public decimal? GstAmount { get; set; }
        public bool? IsApproved { get; set; }
    }
}
