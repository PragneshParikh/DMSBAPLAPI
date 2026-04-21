using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
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
        public VehicleSaleBillRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(VehicleSaleBillHeader entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.VehicleSaleBillHeaders.Add(entity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return entity.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

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
             join vd in _context.VehicleDispatches
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

             where jc.DealerCode == dealerCode
                   && jc.IsPdiSuccess == true

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
                 DealerPrice =im.Dlrprice,
                 CustomerPrice = im.Custprice,
                 PreGstDisc =im.Fame2amount,

                 DealerCode = jc.DealerCode
             }
         ).ToListAsync();

                return data;



            }
            catch
            {
                throw;
            }
        }

    }
}
