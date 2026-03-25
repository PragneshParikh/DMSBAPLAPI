using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
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
                        LocationName = item.LocationName,
                        LotVehicleDamageImage = item.LotVehicleDamageImage,
                        CreatedBy = userId,
                        CreatedDate = DateOnly.FromDateTime(DateTime.Now),
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
    }
}
