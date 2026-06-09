using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ChassisDetailRepo;
using DMS_BAPL_Utils.ViewModels;
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

        public ChassisDetailRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<bool> IChassisDetailRepo.InsertChassis(ChassisDetail chassisDetail)
        {
            var item = await _context.ItemMasters
                .AsNoTracking()
                .Where(x => x.Itemcode == chassisDetail.ItemCode)
                .FirstOrDefaultAsync();

            chassisDetail.ItemName = item.Itemname;

            _context.ChassisDetails
                .Add(chassisDetail);

            await _context.SaveChangesAsync();

            return true;
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
                return result.DistinctBy(i=>i.ChassisNo).ToList();
            }
            catch
            {
                throw;
            }
        }
    }
}
