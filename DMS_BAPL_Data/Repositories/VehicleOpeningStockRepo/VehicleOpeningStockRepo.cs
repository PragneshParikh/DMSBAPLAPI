using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleOpeningStockRepo
{
    public class VehicleOpeningStockRepo : IVehicleOpeningStock
    {
        private readonly BapldmsvadContext _context;

        public VehicleOpeningStockRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleOpeningDetailsVM>> GetVehicleSaleDetailsByModelAsync(string modelName, string dealerCode)
        {
            var saleDetail = await(from d in  _context.VehicleSaleBillDetails
                join h in _context.VehicleSaleBillHeaders
                on d.VehicleSaleBillId equals h.Id
                                   where d.ModelName == modelName && h.DealerCode == dealerCode
                orderby d.VehicleSaleBillId descending
                select d).FirstOrDefaultAsync();

            if (saleDetail == null)
                return new List<VehicleOpeningDetailsVM>();

            var saleHeader = await _context.VehicleSaleBillHeaders
                .FirstOrDefaultAsync(x => x.Id == saleDetail.VehicleSaleBillId);

            var battery = await _context.ChassisBatteryDetails
                .Where(x => x.ChassisNo == saleDetail.ChassisNo)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();
            var chassisRate = await _context.VehicleInwards
                .Where(x => x.ChasisNo == saleDetail.ChassisNo)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();
            


            return new List<VehicleOpeningDetailsVM>
    {
        new VehicleOpeningDetailsVM
        {
            VehicleSaleBillDetailId = saleDetail.Id,
            VehicleSaleBillHeaderId = saleDetail.VehicleSaleBillId,
            ModelName = saleDetail.ModelName,
            Rate = chassisRate?.Custprice ?? 0,
            MfgYear = saleDetail.MfgYear,
            ColorName = saleDetail.Colour,

            SaleDate = saleHeader?.SaleDate,
            SaleBillCreatedDate = saleHeader?.CreatedDate,
            IsDeleted = saleHeader?.IsDeleted,
            DeleteDate = saleHeader?.DeletedDate,

            ChassisNo = battery?.ChassisNo,
            ServiceBookNo = battery?.ChassisNo?.Substring(4),
            MotorNo = battery?.MotorNo,
            BatteryMake = battery?.BatteryMake,
            BatteryNo = battery?.BatteryNo,
            ConverterNo = battery?.ConverterNo,
            ChargerNo = battery?.ChargerNo,
            ControllerNo = battery?.ControllerNo,
            BatteryChemical = battery?.BatteryChemical,
            BatteryCapacity = battery?.BatteryCapacity
        }
     };
        }
    }
}
