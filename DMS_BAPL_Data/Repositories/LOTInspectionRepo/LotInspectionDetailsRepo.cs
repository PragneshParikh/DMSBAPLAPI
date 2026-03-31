using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LOTInspectionRepo
{
    public class LotInspectionDetailsRepo : ILotInspectionDetails
    {
        private readonly BapldmsvadContext _context;

        public LotInspectionDetailsRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        // create insert api for lot inspection details
        public async Task<bool> InsertDetailsByInvoiceAsync(InsertDetailsByInvoiceViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get HeaderId using Invoice
                var InvoiceheaderID = await _context.LotinspectionHeaders.FirstOrDefaultAsync(x =>
                    x.InvoiceNo.Trim().ToLower() == model.InvoiceNo.Trim().ToLower() &&
                    x.InvoiceDate == model.InvoiceDate &&
                    x.DealerCode.Trim().ToLower() == model.DealerCode.Trim().ToLower() &&
                    x.LocCode.Trim().ToLower() == model.LocCode.Trim().ToLower()
                );

                if (InvoiceheaderID == null)
                    throw new Exception("Header not found");

                int headerId = InvoiceheaderID.Id;

                var detailList = new List<LotinspectionDetail>();

                foreach (var item in model.Details)
                {
                    // Duplicate chassis check (optional but recommended)
                    bool exists = await _context.LotinspectionDetails.AnyAsync(x =>
                        x.ChassisNo == item.ChassisNo &&
                        x.LotHeaderId == headerId
                    );

                    if (exists)
                        continue;

                    detailList.Add(new LotinspectionDetail
                    {
                        LotHeaderId = headerId,
                        ModelName = item.ModelName,
                        NoofVehicle = item.NoofVehicle,
                        ChassisNo = item.ChassisNo,
                        MotorNo = item.MotorNo,
                        BatteryNo = item.BatteryNo,
                        ChargerNo = item.ChargerNo,
                        KeyFobSetQty = item.KeyFobSetQty,
                        ChargerQty = item.ChargerQty,
                        MirrorsetQty = item.MirrorsetQty,
                        FirstaidkitQty = item.FirstaidkitQty,
                        ToolKitQty = item.ToolKitQty,
                        OwnersManual = item.OwnersManual,
                        IgnitionKeyset = item.IgnitionKeyset,
                        AttributeCard = item.AttributeCard,
                        ChargingKit = item.ChargingKit,
                        InspectionDate = item.InspectionDate,
                        VehicleStatus = item.VehicleStatus,
                        DamageDetails = item.DamageDetails,
                        ChassisWiseRemarks = item.ChassisWiseRemarks,
                        ModelWiseSupervisor = item.modelWiseSupervisorName,
                        LocationName = item.LocationName,
                        LotVehicleDamageImage = item.LotVehicleDamageImage,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    });
                }

                if (detailList.Any())
                {
                    await _context.LotinspectionDetails.AddRangeAsync(detailList);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Get all Lot inspection details based on invoice no
        public async Task<List<LotInspectionHeaderDetailsViewModel>> GetAllDetailsByInvoiceAsync(string? invoiceNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoiceNo))
                    return new List<LotInspectionHeaderDetailsViewModel>();

                var details = await (
                    from lotHeader in _context.LotinspectionHeaders
                    join lotDetail in _context.LotinspectionDetails
                        on lotHeader.Id equals lotDetail.LotHeaderId
                    where lotHeader.InvoiceNo != null &&
                          lotHeader.InvoiceNo.Trim().ToLower() == invoiceNo.Trim().ToLower()
                    select new LotInspectionHeaderDetailsViewModel
                    {

                        invoiceNo = lotHeader.InvoiceNo,
                        invoiceDate = lotHeader.InvoiceDate,
                        lotNo = lotHeader.LotNo,
                        arrivalDate=lotHeader.ArrivalDate,
                        arrivalTime=lotHeader.ArrivalTime,
                        lrNo = lotHeader.LrNo,
                        lrDate = lotHeader.LrDate,
                        truckNo = lotHeader.TruckNo,
                        transporterName = lotHeader.TransporterName,
                        driverName = lotHeader.DriverName,
                        driverContact = lotHeader.DriverContact,
                        commonRemarks=lotHeader.CommonRemarks,
                        vehicleFasteningBracket=lotHeader.VehicleFasteningBracket,
                        plasticCover=lotHeader.PlasticCover,
                        nameSupervisor=lotHeader.SupervisorName,

                        id = lotDetail.Id,
                        lotHeaderID = lotDetail.LotHeaderId,
                        modelName = lotDetail.ModelName,
                        noofVehicle = lotDetail.NoofVehicle,
                        chassisNo = lotDetail.ChassisNo,
                        motorNo = lotDetail.MotorNo,
                        batteryNo = lotDetail.BatteryNo,
                        chargerNo = lotDetail.ChargerNo,
                        keyFobSetQty = lotDetail.KeyFobSetQty,
                        chargerQty = lotDetail.ChargerQty,
                        mirrorsetQty = lotDetail.MirrorsetQty,
                        firstaidkitQty = lotDetail.FirstaidkitQty,
                        toolKitQty = lotDetail.ToolKitQty,
                        ownersManual = lotDetail.OwnersManual,
                        ignitionKeyset = lotDetail.IgnitionKeyset,
                        attributeCard = lotDetail.AttributeCard,
                        chargingKit = lotDetail.ChargingKit,
                        inspectionDate = lotDetail.InspectionDate,
                        vehicleStatus = lotDetail.VehicleStatus,
                        damageDetails = lotDetail.DamageDetails,
                        chassisWiseRemarks = lotDetail.ChassisWiseRemarks,
                        modelWiseSupervisorName = lotDetail.ModelWiseSupervisor,
                        locationName = lotDetail.LocationName,
                        lotVehicleDamageImage = lotDetail.LotVehicleDamageImage
                    }).ToListAsync();

                return details;
            }
            catch (Exception ex)
            {
                //  log error (recommended)
                Console.WriteLine($"Error in GetAllDetailsByInvoiceAsync: {ex.Message}");

                // Option 1: return empty list (safe for UI)
                return new List<LotInspectionHeaderDetailsViewModel>();

                throw;
            }
        }

        public async Task<int> InsertLotDetailsByInvoiceNo(string invoiceNo, int id, string userId)
        {
            try
            {
                var invoiceLot = await _context.VehicleDispatches.Where(x => x.InvoiceNo == invoiceNo).ToListAsync();

                if (invoiceLot == null || !invoiceLot.Any())
                    return 0;

                var lotDetails = invoiceLot.Select(x => new LotinspectionDetail
                {
                    Id = 0,
                    LotHeaderId = id,
                    ModelName = x.ItemCode,
                    NoofVehicle = 1,
                    ChassisNo = x.ChasisNo,
                    MotorNo = x.MotorNo,
                    BatteryNo = x.BatteryNo,
                    ChargerNo = x.ChargerNo,
                    KeyFobSetQty = null,
                    ChargerQty = null,
                    MirrorsetQty = null,
                    FirstaidkitQty = null,
                    ToolKitQty = null,
                    OwnersManual = null,
                    IgnitionKeyset = null,
                    AttributeCard = null,
                    ChargingKit = null,
                    InspectionDate = null,
                    VehicleStatus = null,
                    DamageDetails = null,
                    ChassisWiseRemarks = null,
                    LocationName = null,
                    LotVehicleDamageImage = null,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                }).ToList();

                await _context.LotinspectionDetails.AddRangeAsync(lotDetails);
                await _context.SaveChangesAsync();

                return lotDetails.Count;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
