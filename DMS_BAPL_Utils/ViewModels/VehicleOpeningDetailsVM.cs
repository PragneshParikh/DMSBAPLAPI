using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleOpeningDetailsVM
    {

        public int? VehicleSaleBillHeaderId { get; set; }
        public int? VehicleSaleBillDetailId { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime? SaleBillCreatedDate { get; set; }
        public string? ModelName { get; set; }
        public decimal? Rate { get; set; }
        public int? MfgYear { get; set; }
        public string? ColorName { get; set; }
        public string? ChassisNo { get; set; }
        public string? ServiceBookNo { get; set; }
        public string? MotorNo { get; set; }
        public string? BatteryMake { get; set; }
        public string? BatteryNo { get; set; }
        public string? ConverterNo { get; set; }
        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? BatteryChemical { get; set; }
        public string? BatteryCapacity { get; set; }
    }
}
