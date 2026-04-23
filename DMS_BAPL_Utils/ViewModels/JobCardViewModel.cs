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
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerAltMobile { get; set; }
        public string? ModelName { get; set; }
        public string? RegisterNo { get; set; }
        public DateOnly? SaleDate { get; set; }
        public DateOnly? InsuranceExpDate { get; set; }
        public DateOnly? NextserviceDueDate { get; set; }
        public DateOnly? RsarenewalDate { get; set; }
        public string BatteryNumber { get; set; }
        public string ControllerNo { get; set; }
        public string ChargerNumber { get; set; }
        public string BatteryMake { get; set; }
        public string BatteryCapacity { get; set; }
        public string BatteryChemestry { get; set; }
        public string ConverterNo { get; set; }
        public string MotorNo { get; set; }
        public decimal? OdoReading { get; set; }

        public decimal? Duration { get; set; }

        public string DurationType { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public DateOnly? ExpireWarrentyDate { get; set; }

    }
    public class JobSourceViewModel
    {
        public int JobSourceId { get; set; }
        public string? JobSourceName { get; set; }
    }

    public class JobCardDetailsViewModel
    {
        // Define properties for job card details here

        public string Jobtype { get; set; }
        public string Jobsource { get; set; }
        public string serviceHead { get; set; }
        public string serviceType { get; set; }
        public string Complaint { get; set; }

        public JobCardHeaderVM JobCardHeader { get; set; }
        public JobCardBatteryVM JobCardBattery { get; set; }
        public JobCardCustomerVM JobCardCustomer { get; set; }
        public List<JobCardComplaintVM> JobCardComplaint { get; set; }

        public List<PdiChecklistChassiWiseVM> PdiChecklistChassiWise { get; set; }
    }
    public class JobCardHeaderVM
    {
        public int Id { get; set; }

        public int? Jobtype { get; set; }

        public string? InvoiceNo { get; set; }

        public string DealerCode { get; set; }

        public string? Chassisno { get; set; }

        public int? Vehiclekms { get; set; }

        public int? Servicehead { get; set; }

        public int? Servicetype { get; set; }

        public string? Serviceloc { get; set; }

        public string? Couponno { get; set; }

        public string? Jobprefix { get; set; }

        public DateOnly? JobinDate { get; set; }

        public string? JobinTime { get; set; }

        public int? JobNo { get; set; }

        public int? ManualjobNo { get; set; }

        public DateOnly? EstdelDate { get; set; }

        public string? EstdelTime { get; set; }

        public int? JobSource { get; set; }

        public string? Supervisor { get; set; }

        public string? Technician { get; set; }

        public int? Jobestmate { get; set; }

        public int? AirpressureRearTyre { get; set; }

        public int? AirpressurefrontTyre { get; set; }

        public string? Observation { get; set; }

        public string? SupervisorComment { get; set; }

        public bool? IsPdiSuccess { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class JobCardBatteryVM
    {
        public int Id { get; set; }

        public string DealerCode { get; set; }
        public int JobCardHeaderId { get; set; }

        public string? BatteryMake { get; set; }

        public string? BatterySerialNo { get; set; }

        public string? BatteryOcv { get; set; }

        public string? BatteryCcv { get; set; }

        public string? BatteryDischarge { get; set; }

        public string? BatteryCapacityAh { get; set; }

        public string? BatteryVoltage { get; set; }

        public string? MotorDrawing { get; set; }

        public string? ChargerMake { get; set; }

        public string? ChargerNo { get; set; }

        public string? ConverterNo { get; set; }

        public string? ControllerNo { get; set; }

        public string? BatteryChemical { get; set; }

        public string? BatteryCapacity { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class JobCardCustomerVM
    {
        public int Id { get; set; }

        public int JobCardHeaderId { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerMobile { get; set; }

        public string? CustomerAltMobile { get; set; }

        public string? ModelName { get; set; }

        public string? ChassisNo { get; set; }

        public string? RegisterNo { get; set; }

        public string? MotorNo { get; set; }

        public string? BatteryNo { get; set; }

        public DateOnly? SaleDate { get; set; }

        public DateOnly? InsuranceExpDate { get; set; }

        public DateOnly? NextserviceDueDate { get; set; }

        public DateOnly? RsarenewalDate { get; set; }

        public string? Remarks { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class JobCardComplaintVM
    {
        public int Id { get; set; }
        public int JobCardHeaderId { get; set; }
        public string? CustomerVoice { get; set; }
        public string? ComplaintCode { get; set; }
        public string? Complaint { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class PdiChecklistChassiWiseVM
    {
        public int Id { get; set; }

        public int PdichecklistMasterId { get; set; }

        public int JobCardMasterId { get; set; }

        public bool? IsStatus { get; set; }

        public string? Remarks { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

    }

    public class JobCardListViewModel
    {
        public int? JobNo { get; set; }
        public DateOnly? JobInDate { get; set; }
        public DateOnly? JobStatus { get; set; }
        public string InvoiceNo { get; set; }
        public int? ManualJobNo { get; set; }
        public string Joblocation { get; set; }
        public string Jobtype { get; set; }
        public string Jobsource { get; set; }

        public string Complaint { get; set; }

        public string Supervisor { get; set; }

        public string RegisterNo { get; set; }
        public string ChassisNo { get; set; }

        public string ModelName { get; set; }
        public string ModelType { get; set; }

        public string serviceHead { get; set; }
        public string serviceType { get; set; }

        public string CustomerName { get; set; }
        public string MobileNo { get; set; }

    }

    public class JobCardWarrentydetailsVM
    {

        public decimal? OdoReading { get; set; }

        public decimal? Duration { get; set; }

        public string DurationType { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public DateOnly? ExpireWarrentyDate { get; set; }

    }

    public class UpdateJobCardVM
    {
        public JobCardHeaderVM JobCardHeader { get; set; }
        public JobCardBatteryVM JobCardBattery { get; set; }
        public JobCardCustomerVM JobCardCustomer { get; set; }
        public List<JobCardComplaintVM> JobCardComplaint { get; set; }

        public List<PdiChecklistChassiWiseVM> PdiChecklistChassiWise { get; set; }
    }

    public class UpdateSaleDetailsVM
    {
        public string ChassisNo { get; set; }
        public string? RegisterNo { get; set; }
        public DateOnly? SaleDate { get; set; }

        public DateOnly? InsuranceExpDate { get; set; }
    }

    public class JobCardSearchModel
    {
        public string? DealerCode { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? ServiceLocation { get; set; }
        public int? JobNo { get; set; }
        public string? CustomerName { get; set; }
        public string? ChassisNo { get; set; }
    }
}
