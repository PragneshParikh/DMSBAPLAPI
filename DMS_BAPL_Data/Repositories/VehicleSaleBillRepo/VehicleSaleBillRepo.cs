using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleSaleBillRepo
{
    public class VehicleSaleBillRepo : IVehicleSaleBillRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IPartInventoryService _partInventoryService;
        private readonly IHttpContextAccessor _contextAccessor;

        public VehicleSaleBillRepo(BapldmsvadContext context, IPartInventoryService partInventoryService,
            IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _partInventoryService = partInventoryService;
            _contextAccessor = contextAccessor;
        }

        //public async Task<int> CreateAsync(VehicleSaleBillHeader entity)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        _context.VehicleSaleBillHeaders.Add(entity);
        //        await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();
        //        return entity.Id;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        public async Task<VehicleSaleBillHeader?> GetByIdAsync(int id)
        {
            try
            {

                return await _context.VehicleSaleBillHeaders
                    .Include(x => x.VehicleSaleBillDetails)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<VehicleSaleBillHeader>> GetAllAsync()
        {
            try
            {
                return await _context.VehicleSaleBillHeaders
                    .Include(x => x.VehicleSaleBillDetails)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(VehicleSaleBillHeader entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.VehicleSaleBillHeaders.Update(entity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task DeleteAsync(int id)
        {
            try
            {
                var data = await _context.VehicleSaleBillHeaders.FindAsync(id);
                if (data != null)
                {
                    _context.VehicleSaleBillHeaders.Remove(data);
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task<string?> GetLastSaleBillNo()
        {
            try
            {
                return await _context.VehicleSaleBillHeaders.OrderByDescending(x => x.CreatedDate)
                    .Select(x => x.SaleBillNo).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<string?> GetDealerLocation(string dealerCode)
        {
            try
            {
                return await _context.PartsInventories
                    .Where(x => x.VendorCode == dealerCode)
                    .Select(x => x.DealerLocation)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<ItemMaster?> GetItem(string itemCode)
        {
            try
            {
                return await _context.ItemMasters
                    .FirstOrDefaultAsync(x => x.Itemcode == itemCode);
            }
            catch
            {
                throw;
            }
        }

        public async Task<decimal?> GetPurchaseRate(string dealerCode, string itemCode)
        {
            try
            {
                return await _context.PartsInventories
                    .Where(x => x.VendorCode == dealerCode && x.ItemCode == itemCode)
                    .OrderByDescending(x => x.CreatedDate)
                    .Select(x => x.PurchaseRate)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<(string chassisNo, string itemCode)>> GetChassisByDealer(string dealerCode)
        {
            try
            {

                var headers = await _context.LotinspectionHeaders
                    .Where(h => h.DealerCode == dealerCode && h.LocCode.EndsWith("S1"))
                    .Select(h => h.Id)
                    .ToListAsync();

                return await _context.LotinspectionDetails
                    .Where(d => headers.Contains(d.LotHeaderId))
                    .Select(d => new ValueTuple<string, string>(d.ChassisNo, d.Itemcode))
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<PdiOkVehicleChassisViewModel>> GetPdiRawDataAsync(string dealerCode)
        {
            try
            {
                var data = await (
             from jc in _context.JobCardHeaders
             join vd in _context.VehicleInwards
                 on jc.Chassisno equals vd.ChasisNo
             join bd in _context.JobCardBatteryDetails
                 on jc.Id equals bd.JobCardHeaderId into batteryGroup
             from bd in batteryGroup.DefaultIfEmpty()
             join im in _context.ItemMasters
                on vd.ItemCode equals im.Itemcode into itemGroups
             from im in itemGroups.DefaultIfEmpty()
             join cm in _context.ColorMasters
             on vd.ColrCode equals cm.Colorcode into itemColors
             from cm in itemColors.DefaultIfEmpty()
             join jcu in _context.JobCardCustomers
             on jc.Chassisno equals jcu.ChassisNo into jobCardGroup
             from jcu in jobCardGroup.DefaultIfEmpty()


             where jc.DealerCode == dealerCode
                   && jc.IsPdiSuccess == true
             && jcu.SaleDate == null

             select new PdiOkVehicleChassisViewModel
             {
                 ChassisNo = jc.Chassisno,
                 ItemCode = vd.ItemCode,
                 ItemColor = cm.Colorname,
                 MfgYear = vd.MfgYear,
                 ItemName = im.Itemname,

                 KeyNo = vd.KeyNo,
                 BookNo = vd.ServBkno,

                 BatteryNo = vd.BatteryNo,
                 BatteryChemical = vd.BatteryChemistry,
                 BatteryCapacity = vd.BatteryCapacity,
                 BatteryMake = vd.BatteryMake,

                 ChargerNo = bd.ChargerNo ?? vd.ChargerNo,
                 ControllerNo = bd.ControllerNo ?? vd.ControllerNo,
                 ConverterNo = bd.ConverterNo ?? vd.Converter,
                 DealerPrice = im.Dlrprice,
                 CustomerPrice = im.Custprice,
                 PreGstDisc = im.Fame2amount,

                 DealerCode = jc.DealerCode,
                 CustomerSaleDate = jcu.SaleDate
             }
         ).ToListAsync();

                return data;



            }
            catch
            {
                throw;
            }
        }

        public async Task<int> CreateWithJobUpdateAsync(VehicleSaleBillHeader header)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Save Sale Bill
                _context.VehicleSaleBillHeaders.Add(header);
                await _context.SaveChangesAsync();


                //  Single commit
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return header.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> UpdateERPStatus(int id)
        {
            try
            {
                var saleBill = await _context.VehicleSaleBillHeaders.Where(i => i.Id == id).FirstOrDefaultAsync();
                saleBill.Erpstatus = "PushedToERP";
                return await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateWithJobUpdateAsync(VehicleSaleBillHeader header, List<UpdateSaleDetailsVM>? jobUpdates, List<string> deletedChassisList)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Update Header
                _context.VehicleSaleBillHeaders.Update(header);

                // 2. Update JobCards 
                if(jobUpdates == null && header.Erpstatus.ToLower() =="invalid")
                {
                    header.Erpstatus = "Pending";
                }

                if (jobUpdates != null)
                {

                    var allchassis = jobUpdates.Select(x => x.ChassisNo).Concat(deletedChassisList).Distinct().ToList();
                    var jobs = await _context.JobCardCustomers
                        .Where(x => allchassis.Contains(x.ChassisNo))
                        .ToListAsync();




                    foreach (var item in jobUpdates)
                    {
                        var job = jobs.Where(c => c.ChassisNo == item.ChassisNo).FirstOrDefault();

                        if (job == null) continue;

                        job.SaleDate = item.SaleDate;
                        job.InsuranceExpDate = item.InsuranceExpDate;
                        job.RegisterNo = item.RegisterNo;
                    }
                    header.Erpstatus = "Reserved";

                    // Mark other sale bills with same chassis as Invalid
                    var invalidSalesBills = await _context.VehicleSaleBillHeaders
                    .Include(x => x.VehicleSaleBillDetails)
                    .Where(x => x.SaleBillNo != header.SaleBillNo &&
                                x.VehicleSaleBillDetails.Any(c => allchassis.Contains(c.ChassisNo)))
                    .ToListAsync();

                    foreach (var bill in invalidSalesBills)
                    {
                        bill.Erpstatus = "Invalid";

                        var detailsToRemove = await _context.VehicleSaleBillDetails
                             .Where(d => d.VehicleSaleBillId == bill.Id)
                                .ToListAsync();

                        _context.VehicleSaleBillDetails.RemoveRange(detailsToRemove);

                    }
                }
                // 3. Remove from VehicleDetails
                if (deletedChassisList != null && deletedChassisList.Any())
                {
                    var vehiclesToDelete = await _context.VehicleSaleBillDetails
                        .Where(v => deletedChassisList.Contains(v.ChassisNo) && v.VehicleSaleBillId == header.Id)
                        .ToListAsync();

                    if (vehiclesToDelete.Any())
                    {
                        _context.VehicleSaleBillDetails.RemoveRange(vehiclesToDelete);
                    }
                }


                //updating JobCardCustomer if any of the Chassis no was deleted
                //foreach (var item in deletedChassisList)
                //{
                //    var deletedJC = jobs.Where(c => c.ChassisNo == item).FirstOrDefault();

                //    if (deletedJC == null) continue;
                //    deletedJC.InsuranceExpDate = null;
                //    deletedJC.RegisterNo = null;
                //    deletedJC.SaleDate = null;
                //}

                // 3. Save
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ConfirmInvoiceAndReserveChassis(string saleBillNo)
        {
            try
            {
                // 🔹 Get Sale Bill with details
                var saleBill = await _context.VehicleSaleBillHeaders
                    .Include(i => i.VehicleSaleBillDetails)
                    .FirstOrDefaultAsync(i => i.SaleBillNo == saleBillNo);
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
                if (saleBill == null)
                    return false;

                saleBill.SaleDate = DateTime.Now;

                // Get all chassis numbers
                var chassisNos = saleBill.VehicleSaleBillDetails
                    .Select(d => d.ChassisNo)
                    .ToList();

                // Get ItemCodes from VehicleInward using chassis
                var vehicleItemCodes = await _context.VehicleInwards
                    .Where(v => v.ChasisNo != null && chassisNos.Contains(v.ChasisNo))
                    .Select(v => v.ItemCode)
                    .ToListAsync();
                var groupedItems = vehicleItemCodes
                    .GroupBy(x => x)
                    .Select(g => new
                    {
                        ItemCode = g.Key,
                        Qty = g.Count()
                    }).ToList();
                //  REDUCE STOCK
                foreach (var item in groupedItems)
                {
                    var stockTransaction = new PartsInventory
                    {
                        TransId = Guid.NewGuid().ToString(),
                        ItemCode = item.ItemCode,
                        VoucherNo = null!,
                        TransType = "S",
                        BatchNo = "Batch 1",
                        BatchTransQty = item.Qty,
                        BatchOpeningQty = 0,
                        BatchClosingQty = 0,
                        TransDate = DateOnly.FromDateTime(DateTime.Now),
                        DealerLocation = saleBill?.Location,
                        VendorCode = saleBill.DealerCode,
                        TotalRate = 100.00M,
                        PurchaseRate = 110.00M,
                        Potype = saleBill?.CustomerType,
                        PostTransaction = 0,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };

                    await _partInventoryService.UpdateOutgoing(stockTransaction);
                }



                var jobCardList = await _context.JobCardCustomers
                    .Where(x => chassisNos.Contains(x.ChassisNo))
                    .ToListAsync();


                foreach (var item in jobCardList)
                {
                    var job = jobCardList.Where(c => c.ChassisNo == item.ChassisNo).FirstOrDefault();

                    if (job == null) continue;

                    job.SaleDate = item.SaleDate;
                    job.InsuranceExpDate = item.InsuranceExpDate;
                    job.RegisterNo = item.RegisterNo;
                }

                // Mark other sale bills with same chassis as Invalid
                var invalidSalesBills = await _context.VehicleSaleBillHeaders
                .Include(x => x.VehicleSaleBillDetails)
                .Where(x => x.SaleBillNo != saleBillNo &&
                            x.VehicleSaleBillDetails.Any(c => chassisNos.Contains(c.ChassisNo)))
                .ToListAsync();

                foreach (var bill in invalidSalesBills)
                {
                    bill.Erpstatus = "Invalid";

                    var detailsToRemove = await _context.VehicleSaleBillDetails
                         .Where(d => d.VehicleSaleBillId == bill.Id && chassisNos.Contains(d.ChassisNo))
                            .ToListAsync();

                    _context.VehicleSaleBillDetails.RemoveRange(detailsToRemove);

                }

                //Mark current bill as Invoiced
                saleBill.Erpstatus = "Invoiced";


                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<VehicleSaleBillHeader> UpdateRegistrationAndReserveChassis(string? saleBillNo, List<UpdateSaleDetailsVM> updateSaleDetails)
        {
            try
            {

                var saleBill = await _context.VehicleSaleBillHeaders
                    .Include(i => i.VehicleSaleBillDetails)
                    .Where(i => i.SaleBillNo == saleBillNo).FirstOrDefaultAsync();

                var allocatedChassis = updateSaleDetails.Select(i => i.ChassisNo).ToList();

                var jobCardsToUpdate = await _context.JobCardCustomers
                    .Where(x => allocatedChassis.Contains(x.ChassisNo)).ToListAsync();
                foreach (var item in updateSaleDetails)
                {
                    var job = jobCardsToUpdate.Where(c => c.ChassisNo == item.ChassisNo).FirstOrDefault();

                    if (job == null) continue;

                    job.SaleDate = item.SaleDate;
                    job.InsuranceExpDate = item.InsuranceExpDate;
                    job.RegisterNo = item.RegisterNo;
                }
                await _context.SaveChangesAsync();
                return saleBill;
            }
            catch
            {
                throw;
            }
        }
    }



}
