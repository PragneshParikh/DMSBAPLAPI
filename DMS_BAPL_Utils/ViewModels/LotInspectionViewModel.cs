using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{


    /// <summary>
    /// this DTO used for updated lot inspected details in lot inspection details table and also used for get lot inspected details based on invoice no and lot no in lot inspection details table
    /// </summary>
    public class LotInspectionViewModel
    {
        public LotInspectedHeaderDetails lotInspectedHeaderDetails { get; set; }
        public List<LotInspectedDetails> lotInspectedDetails { get; set; }
    }

    public class LotInspectionHeaderList
    {
        public string? InvoiceNo { get; set; }

        public DateOnly? InvoiceDate { get; set; }

        public int? Lotno { get; set; }

        public DateOnly? ArrivalDate { get; set; }

        public string? ArrivalTime { get; set; }

        public string? Lrno { get; set; }

        public DateOnly? Lrdate { get; set; }

        public string? TruckNo { get; set; }

        public string? TransporterName { get; set; }

        public string? DriverName { get; set; }

        public string? DriverContact { get; set; }

        public string? CommonReMarks { get; set; }

        public string? VehicleFasteningBracket { get; set; }

        public string? PlasticCover { get; set; }

        public string? NameSupervisor { get; set; }
    }
    public class InvoiceAcceptHeaderViewModel
    {
        public string InvoiceNo { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public int LotNo { get; set; }
        public DateOnly? ArrivalDate { get; set; }
        public TimeOnly ArrivalTime { get; set; }
        public string LrNo { get; set; }
        public DateOnly? LrDate { get; set; }
        public string TruckNo { get; set; }
        public string TransporterName { get; set; }
        public string DriverName { get; set; }
        public string DriverContact { get; set; }
        public string CommonRemarks { get; set; }
        public string VehicleFasteningBracket { get; set; }
        public string PlasticCover { get; set; }
        public string SupervisorName { get; set; }
        public string LocCode { get; set; }
        public string DealerCode { get; set; }
        public string CreatedBy { get; set; }
        public DateOnly CreatedDate { get; set; }
    }

    public class TempNumber
    {
        public int? Value { get; set; }
    }
    public class InsertDetailsByInvoiceViewModel
    {
        public string InvoiceNo { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public string DealerCode { get; set; }
        public string LocCode { get; set; }

        public List<LotinspectionDetailViewModel> Details { get; set; }
    }
    public class LotinspectionDetailViewModel
    {
        public string? ModelName { get; set; }
        public int? NoofVehicle { get; set; }
        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? BatteryNo { get; set; }
        public string? ChargerNo { get; set; }
        public int? KeyFobSetQty { get; set; }
        public int? ChargerQty { get; set; }
        public int? MirrorsetQty { get; set; }
        public int? FirstaidkitQty { get; set; }
        public int? ToolKitQty { get; set; }
        public int? OwnersManual { get; set; }
        public int? IgnitionKeyset { get; set; }
        public int? AttributeCard { get; set; }
        public int? ChargingKit { get; set; }
        public DateOnly? InspectionDate { get; set; }
        public string? VehicleStatus { get; set; }
        public string? DamageDetails { get; set; }
        public string? ChassisWiseRemarks { get; set; }
        public string? LocationName { get; set; }
        public string? LotVehicleDamageImage { get; set; }
    }

    public class LotInspectedHeaderDetails
    {
        public string? invoiceNo { get; set; }
        public string? invoiceDate { get; set; }
        public int lotNo { get; set; }
        public string? dateOfArrival { get; set; }
        public string? timeOfArrival { get; set; }
        public string? lrNo { get; set; }
        public string? lrDate { get; set; }
        public string? truckNo { get; set; }
        public string? transporterName { get; set; }
        public string? driverName { get; set; }
        public string? driverContact { get; set; }
        public string? commonRemarks { get; set; }
        public string? vehicleFasteningBracket { get; set; }
        public string? plasticCover { get; set; }
        public string? nameSupervisor { get; set; }
        public string dealerCode { get; set; }
        public string? updatedBy { get; set; }
        public string? updatedDate { get; set; }
    }

    public class LotInspectedDetails
    {
        public int Id { get; set; }
        public int lotHeaderID { get; set; }
        public string? modelName { get; set; }
        public string? chassisNo { get; set; }
        public string? motorNo { get; set; }
        public string? batteryNo { get; set; }
        public string? chargerNo { get; set; }
        public int? keyFobSetQty { get; set; }
        public int? chargerQty { get; set; }
        public int? mirrorsetQty { get; set; }
        public int? firstaidkitQty { get; set; }
        public int? toolKitQty { get; set; }
        public int? ownersManual { get; set; }
        public int? ignitionKeyset { get; set; }
        public int? attributeCard { get; set; }
        public int? chargingKit { get; set; }
        public string? inspectionDate { get; set; }
        public string? vehicleStatus { get; set; }
        public string? damageDetails { get; set; }
        public string? chassisWiseRemarks { get; set; }
        public string? locationName { get; set; }
        // public IFormFile? lotVehicleDamageImage { get; set; }
        public string updatedBy { get; set; }
        public string updatedDate { get; set; }
    }

    ///<summary>
    /// this DTO used for display on Add LTO information page.
    ///</summary>
    public class LotInspectionHeaderDetailsViewModel
    {
        public string? invoiceNo { get; set; }
        public DateOnly? invoiceDate { get; set; }
        public int? lotNo { get; set; }
        public DateOnly? arrivalDate { get; set; }
        public TimeOnly? arrivalTime { get; set; }
        public string? lrNo { get; set; }
        public DateOnly? lrDate { get; set; }
        public string? truckNo { get; set; }
        public string? transporterName { get; set; }
        public string? driverName { get; set; }
        public string? driverContact { get; set; }
        public string? commonRemarks { get; set; }
        public string? vehicleFasteningBracket { get; set; }
        public string? plasticCover { get; set; }
        public string? nameSupervisor { get; set; }

        public int id { get; set; }
        public int lotHeaderID { get; set; }
        public string? modelName { get; set; }
        public int? noofVehicle { get; set; }
        public string? chassisNo { get; set; }
        public string? motorNo { get; set; }
        public string? batteryNo { get; set; }
        public string? chargerNo { get; set; }
        public int? keyFobSetQty { get; set; }
        public int? chargerQty { get; set; }
        public int? mirrorsetQty { get; set; }
        public int? firstaidkitQty { get; set; }
        public int? toolKitQty { get; set; }
        public int? ownersManual { get; set; }
        public int? ignitionKeyset { get; set; }
        public int? attributeCard { get; set; }
        public int? chargingKit { get; set; }
        public DateOnly? inspectionDate { get; set; }
        public string? vehicleStatus { get; set; }
        public string? damageDetails { get; set; }
        public string? chassisWiseRemarks { get; set; }
        public string? locationName { get; set; }
        public string? lotVehicleDamageImage { get; set; }
    }
}
