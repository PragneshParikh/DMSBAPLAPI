using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ChassisDetailRepo;
using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ChassisDetailsRepo
{
    public partial class ChassisDetailRepo : IChassisDetailRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly ILocationMasterService _locationMasterService;

        public ChassisDetailRepo(BapldmsvadContext context, ILocationMasterService locationMasterService)
        {
            _context = context;
            _locationMasterService = locationMasterService;
        }

        async Task<bool> IChassisDetailRepo.InsertChassis(ChassisDetail chassisDetail)
        {
            try
            {
                var item = await _context.ItemMasters
                        .AsNoTracking()
                        .Where(x => x.Itemcode == chassisDetail.ItemCode)
                        .FirstOrDefaultAsync();

                chassisDetail.ItemName = item.Itemname;

                _context.ChassisDetails
                    .AddAsync(chassisDetail);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async Task<List<VehicleStockTransferChassisListViewModel>> IChassisDetailRepo.GetChassisList(string? locationCode)
        {
            try
            {
                var result = await (from cd in _context.ChassisDetails
                                    join im in _context.ItemMasters
                                    on cd.ItemCode equals im.Itemcode into itemInfo
                                    from im in itemInfo.DefaultIfEmpty()

                                    join cbd in _context.ChassisBatteryDetails
                                    on cd.ChassisNo equals cbd.ChassisNo into ChassisBatteryInfo
                                    from cbd in ChassisBatteryInfo.DefaultIfEmpty()

                                    join vi in _context.VehicleInwards
                                    on cd.ChassisNo equals vi.ChasisNo into VehicleInwardInfo
                                    from vi in VehicleInwardInfo.DefaultIfEmpty()

                                    join clr in _context.ColorMasters
                                    on im.Colorcode equals clr.Colorcode into colourInfo
                                    from clr in colourInfo.DefaultIfEmpty()

                                    where cd.LocationCode == locationCode
                                    select new VehicleStockTransferChassisListViewModel
                                    {
                                        ChassisNo = cd.ChassisNo,
                                        ItemCode = cd.ItemCode,
                                        ModelName = im.Itemname,
                                        Colour = clr.Colorname,
                                        MfgYear = vi.MfgYear,
                                        KeyNo = vi.KeyNo,
                                        BatteryMake = cbd.BatteryMake,
                                        BatteryCapacity = cbd.BatteryCapacity,
                                        BatteryNo = cbd.BatteryNo,
                                        Charger = cbd.ChargerNo,
                                        Convertor = vi.Converter,
                                        Controller = cbd.ControllerNo,
                                        FameII = vi.Fame2Discount,
                                        Rate = vi.Dlrprice
                                    }
                                    ).ToListAsync();
                return result.DistinctBy(i => i.ChassisNo).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ChassisWithRegisterNoViewModel>> GetSoldChassisDetailsList()
        {
            try
            {
                var result = await (
                    from chassis in _context.ChassisDetails
                    join ledger in _context.LedgerMasters
                        on chassis.LedgerId equals ledger.Id into ledgerGroup
                    from ledger in ledgerGroup.DefaultIfEmpty()
                    where chassis.SaleDate.HasValue
                    select new ChassisWithRegisterNoViewModel
                    {
                        CustId = chassis.LedgerId,
                        RegNo = chassis.RegNo,
                        ChassisNo = chassis.ChassisNo,

                        MobileNo = ledger != null ? ledger.MobileNumber : null,
                        PartyName = ledger != null ? ledger.LedgerName : null,
                        PartyState = ledger != null ? ledger.State : null
                    }
                ).ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }

        async Task<bool> IChassisDetailRepo.UpdateNewLedgerForChassis(int ledgerId, string dealerCode, string chassisNo, string userId)
        {
            // 1. Added transaction handling inside a try-catch block
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Fetch the single chassis record
                var chassisDetail = await _context.ChassisDetails
                    .Where(i => i.ChassisNo == chassisNo)
                    .FirstOrDefaultAsync();

                var locationDetails = await _locationMasterService.GetDealerPrimaryLocationByAreaId(1, "S1", dealerCode);
                var primaryLocation = locationDetails.FirstOrDefault();

                string locationCode = string.Empty;
                if (primaryLocation != null)
                {
                    locationCode = ((dynamic)primaryLocation).Loccode;
                }

                // 2. Handle null protection to prevent NullReferenceException
                if (chassisDetail == null)
                {
                    return false;
                }

                // 3. Map directly from the single object (No .Select() needed)
                var historyRecord = new ChassisDetailsD2dhistory
                {
                    LedgerId = chassisDetail.LedgerId, // Using the parameter passed into the method
                    ChassisNo = chassisDetail.ChassisNo,
                    ItemCode = chassisDetail.ItemCode,
                    ItemName = chassisDetail.ItemName,
                    ItemColor = chassisDetail.ItemColor,
                    DealerCode = chassisDetail.DealerId, // Fixed undefined variable
                    LocationCode = chassisDetail.LocationCode,
                    IssueingDealerCode = null,
                    IssueingDealerLocation = null,
                    SaleDate = Convert.ToDateTime(chassisDetail.SaleDate),
                    TransDate = DateTime.Now,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = chassisDetail.UpdatedBy,
                    UpdatedDate = chassisDetail.UpdatedDate
                };

                // Update the existing chassis record's ledger here if needed!
                chassisDetail.LedgerId = ledgerId;
                chassisDetail.DealerId = dealerCode;
                chassisDetail.LocationCode = locationCode;
                chassisDetail.SaleDate = DateTime.Now;

                // Add history and save changes
                await _context.ChassisDetailsD2dhistories.AddAsync(historyRecord);
                await _context.SaveChangesAsync();

                // 4. Commit the transaction to finalize changes in the DB
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // 5. Rollback if anything goes wrong
                await transaction.RollbackAsync();
                // Log your exception here (e.g., _logger.LogError(ex, "Error updating ledger"))
                return false;
            }
        }

    }
}

