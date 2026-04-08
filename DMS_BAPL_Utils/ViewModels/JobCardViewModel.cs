using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class JobCardViewModel
    {
        public int JobTypeId { get; set; }
        public string? JobtypeName { get; set; }
    }

    public class ServiceDataViewModel
    {
        public string JobTypeName { get; set; }
        public string ServiceHead { get; set; }
        public string ServiceType { get; set; }
    }
    public class ServiceHeadViewModel
    {
        public int ServiceHeadId { get; set; }
        public string? ServiceHeadName { get; set; }
    }

    public class ServiceTypeViewModel
    {
        public int ServiceTypeId { get; set; }
        public string? ServiceTypeName { get; set; }
    }
    public class LotInspectionChassisVM
    {
        public string InvoiceNo { get; set; }
        public string ChassisNumber { get; set; }
        public string BatteryNumber { get; set; }
        public string ControllerNo  { get; set; }
        public string ChargerNumber { get; set; }
        public string BatteryMake { get; set; }
        public string BatteryCapacity { get; set; }
        public string BatteryChemestry { get; set; }
        public string ConverterNo { get; set; }
        public string MotorNo { get; set; }


    }
    public class JobSourceViewModel
    {
        public int JobSourceId { get; set; }
        public string? JobSourceName { get; set; }
    }
}
