using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleInfoRepo
{
    public class VehicleInfoRepo : IVehicleInfoRepo
    {
        private readonly BapldmsvadContext _context;
        public VehicleInfoRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<VehicleInfoViewModel?> GetVehicleInfoByRegNoChassis(string? regNo, string? chassisNo, string? dealerCode)
        {
            var vehicleInfo = await (
                from cd in _context.ChassisDetails

                join vd in _context.VehicleSaleBillDetails
                    on cd.ChassisNo equals vd.ChassisNo into vdJoin
                from vd in vdJoin.DefaultIfEmpty()

                join di in _context.DealerMasters
                    on cd.DealerId equals di.Dealercode into DealerInfo
                from di in DealerInfo.DefaultIfEmpty()

                join locdealer in _context.LocationMasters
                    on cd.LocationCode equals locdealer.Loccode into DlrLocInfo
                from locdealer in DlrLocInfo.DefaultIfEmpty()

                join ld in _context.LedgerMasters
                    on cd.LedgerId equals ld.Id into ldJoin
                from ld in ldJoin.DefaultIfEmpty()

                join ic in _context.LedgerMasters
                    on vd.InsuranceLedgerId equals ic.Id into icJoin
                from ic in icJoin.DefaultIfEmpty()

                join im in _context.ItemMasters
                    on vd.ItemCode equals im.Itemcode into ItemInfo
                from im in ItemInfo.DefaultIfEmpty()

                where ((!string.IsNullOrWhiteSpace(chassisNo) && cd.ChassisNo == chassisNo) ||
                (!string.IsNullOrWhiteSpace(regNo) && cd.RegNo != null && cd.RegNo == regNo)) &&
                (string.IsNullOrWhiteSpace(dealerCode) ? true : cd.SaleDate.HasValue || cd.DealerId == dealerCode
  )

                select new VehicleInfoViewModel
                {
                    DealerDetails = new DealerInfoViewModel
                    {
                        DealerCode = di.Dealercode,
                        DealerName = di.Compname,
                        DealerEmail = di.Email,
                        DealerLocation = locdealer.Locname,
                        MobileNo = di.Mobile,
                        Address = di.Adress1,
                        Email = di.Email,
                        DealerState = di.State,
                        DealerCity = di.City
                    },
                    PartyDetails = new PartyDetailsViewModel
                    {
                        PartyName = ld != null ? ld.LedgerName : string.Empty,
                        PartyMobile = ld != null ? ld.MobileNumber : string.Empty,
                        PartyAltMobile = ld != null ? ld.AlternateMobileNo : string.Empty,
                        Address1 = ld != null ? ld.Address : string.Empty,
                        Address2 = ld != null ? ld.Address2 : string.Empty,
                        City = ld != null && ld.CityNavigation != null ? ld.CityNavigation.CityName : null,
                        State = ld != null && ld.StateNavigation != null ? ld.StateNavigation.StateName : null,
                        Pin = ld != null ? ld.Pin : null,
                        Email = ld != null ? ld.EMail : null,
                    },

                    VehicleDetails = new VehicleDetailsViewModel
                    {
                        ChassisNo = cd.ChassisNo,
                        RegNo = vd != null ? vd.RegNo : string.Empty,
                        SaleDate = cd.SaleDate,
                        ItemCode = cd.ItemCode,
                        ModelName = vd != null ? vd.ModelName : string.Empty,
                        ColorName = vd != null ? vd.Colour : string.Empty,
                        InsuranceDate = vd != null ? vd.InsStartDate : null,
                        PolicyNo = vd != null ? vd.InsNo : string.Empty,
                        PolicyExpiryDate = vd != null ? vd.InsExpDate : null,
                        InsuranceCompanyId = vd != null ? vd.InsuranceLedgerId : null,
                        InsuranceCompany = ic != null ? ic.LedgerName : string.Empty
                    }
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (vehicleInfo == null)
                return null;

            var batteryDetails = await _context.ChassisBatteryDetails
                .AsNoTracking()
                .Where(x => x.ChassisNo == vehicleInfo.VehicleDetails.ChassisNo)
                .ToListAsync();

            // Batteries
            vehicleInfo.VehicleDetails.Batteries = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.BatteryNo))
                .GroupBy(x => x.BatteryOrderNo)
                .Select(g => g
                    .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                    .First())
                .Select(x => new BatteryViewModel
                {
                    SerialNo = x.BatteryOrderNo ?? 0,
                    BatteryNo = x.BatteryNo,
                    Capacity = x.BatteryCapacity,
                    BatteryMake = x.BatteryMake,
                    ChemicalType = x.BatteryChemical
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            // Chargers
            vehicleInfo.VehicleDetails.Chargers = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.ChargerNo))
                .GroupBy(x => x.ChargerOrderNo)
                .Select(g => g
                    .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                    .First())
                .Select(x => new ComponentViewModel
                {
                    SerialNo = x.ChargerOrderNo ?? 0,
                    ComponentNo = x.ChargerNo
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            // Controllers
            vehicleInfo.VehicleDetails.Controllers = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.ControllerNo))
                .GroupBy(x => x.ControllerOrderNo)
                .Select(g => g
                    .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                    .First())
                .Select(x => new ComponentViewModel
                {
                    SerialNo = x.ControllerOrderNo ?? 0,
                    ComponentNo = x.ControllerNo
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            // Motors
            vehicleInfo.VehicleDetails.Motors = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.MotorNo))
                .GroupBy(x => x.MotorOrderNo)
                .Select(g => g
                    .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                    .First())
                .Select(x => new ComponentViewModel
                {
                    SerialNo = x.MotorOrderNo ?? 0,
                    ComponentNo = x.MotorNo
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            // Converters
            vehicleInfo.VehicleDetails.Converters = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.ConverterNo))
                .GroupBy(x => x.ConverterOrderNo)
                .Select(g => g
                    .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                    .First())
                .Select(x => new ComponentViewModel
                {
                    SerialNo = x.ConverterOrderNo ?? 0,
                    ComponentNo = x.ConverterNo
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            return vehicleInfo;
        }

        public async Task UpdateVehicleInfo(UpdateVehicleInfoViewModel model)
        {
            var chassis = await _context.ChassisDetails.FirstOrDefaultAsync(x => x.ChassisNo == model.ChassisNo);
            var saleBill = await _context.VehicleSaleBillDetails.FirstOrDefaultAsync(x => x.ChassisNo == model.ChassisNo);
            var orderRows = new Dictionary<int, ChassisBatteryDetail>();
            var chassisRows = await _context.ChassisBatteryDetails.Where(x => x.ChassisNo == model.ChassisNo).ToListAsync();
            bool saleBillUpdated = false;

            // Batteries
            foreach (var battery in model.Batteries)
            {
                var row = await GetOrCreateOrderRow(model.ChassisNo, battery.OrderNo, chassisRows, orderRows); 


                if (row != null)
                {
                    row.BatteryNo = battery.BatteryNo;
                    row.BatteryCapacity = battery.BatteryCapacity;
                    row.BatteryChemical = battery.BatteryChemical;
                    row.BatteryMake = battery.BatteryMake;
                    row.UpdatedDate = DateTime.Now;
                    row.UpdatedBy = "Admin";
                    if (battery.OrderNo == 1 && saleBill != null)
                    {
                        saleBill.Battery = battery.BatteryNo;
                        saleBillUpdated = true;
                        saleBill.BatteryCapacity = battery.BatteryCapacity;
                        saleBill.BatteryChemical = battery.BatteryChemical;
                        saleBill.BatteryMake = battery.BatteryMake;
                    }
                }
                else
                {
                    _context.ChassisBatteryDetails.Add(new ChassisBatteryDetail
                    {
                        ChassisNo = model.ChassisNo,
                        BatteryOrderNo = battery.OrderNo,
                        BatteryNo = battery.BatteryNo,
                        BatteryCapacity = battery.BatteryCapacity,
                        BatteryChemical = battery.BatteryChemical,
                        BatteryMake = battery.BatteryMake,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin"
                    });
                }
            }

            // Motors
            foreach (var motor in model.Motors)
            {
                var row = await GetOrCreateOrderRow(model.ChassisNo, motor.OrderNo, chassisRows, orderRows);

                if (row != null)
                {
                    row.MotorNo = motor.ComponentNo;
                    row.UpdatedDate = DateTime.Now;
                    row.UpdatedBy = "Admin";
                    //if (motor.OrderNo == 1 && saleBill != null)
                    //{
                    //    saleBill.M = battery.BatteryNo;
                    //    saleBillUpdated = true;
                    //}
                }
                else
                {
                    _context.ChassisBatteryDetails.Add(new ChassisBatteryDetail
                    {
                        ChassisNo = model.ChassisNo,
                        MotorOrderNo = motor.OrderNo,
                        MotorNo = motor.ComponentNo,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin"
                    });
                }
            }

            // Chargers
            foreach (var charger in model.Chargers)
            {
                var row = await GetOrCreateOrderRow(model.ChassisNo, charger.OrderNo, chassisRows, orderRows);

                if (row != null)
                {
                    row.ChargerNo = charger.ComponentNo;
                    row.UpdatedDate = DateTime.Now;
                    row.UpdatedBy = "Admin";
                    if (charger.OrderNo == 1 && saleBill != null)
                    {
                        saleBill.ChargerNo = charger.ComponentNo;
                        saleBillUpdated = true;
                    }
                }
                else
                {
                    _context.ChassisBatteryDetails.Add(new ChassisBatteryDetail
                    {
                        ChassisNo = model.ChassisNo,
                        ChargerOrderNo = charger.OrderNo,
                        ChargerNo = charger.ComponentNo,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin"
                    });
                }
            }

            // Controllers
            foreach (var controller in model.Controllers)
            {
                var row = await GetOrCreateOrderRow(model.ChassisNo, controller.OrderNo, chassisRows, orderRows);

                if (row != null)
                {
                    row.ControllerNo = controller.ComponentNo;
                    row.UpdatedDate = DateTime.Now;
                    row.UpdatedBy = "Admin";
                    if (controller.OrderNo == 1 && saleBill != null)
                    {
                        saleBill.ControllerNo = controller.ComponentNo;
                        saleBillUpdated = true;
                    }
                }
                else
                {
                    _context.ChassisBatteryDetails.Add(new ChassisBatteryDetail
                    {
                        ChassisNo = model.ChassisNo,
                        ControllerOrderNo = controller.OrderNo,
                        ControllerNo = controller.ComponentNo,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin"
                    });
                }
            }

            // Converters
            foreach (var converter in model.Converters)
            {
                var row = await GetOrCreateOrderRow(model.ChassisNo, converter.OrderNo, chassisRows, orderRows);

                if (row != null)
                {
                    row.ConverterNo = converter.ComponentNo;
                    row.UpdatedDate = DateTime.Now;
                    row.UpdatedBy = "Admin";
                    if (converter.OrderNo == 1 && saleBill != null)
                    {
                        saleBill.ConvertorNo = converter.ComponentNo;
                        saleBillUpdated = true;
                    }
                }
                else
                {
                    _context.ChassisBatteryDetails.Add(new ChassisBatteryDetail
                    {
                        ChassisNo = model.ChassisNo,
                        ConverterOrderNo = converter.OrderNo,
                        ConverterNo = converter.ComponentNo,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "Admin"
                    });
                }
            }


            if (chassis != null && chassis.RegNo != model.RegNo)
            {
                chassis.RegNo = model.RegNo;
                chassis.UpdatedDate = DateTime.Now;
                chassis.UpdatedBy = "Admin";
            }
            if (saleBill != null)
            {
                bool isUpdated = false;

                if (saleBill.RegNo != model.RegNo)
                {
                    saleBill.RegNo = model.RegNo;
                    isUpdated = true;
                }

                if (saleBill.InsNo != model.InsuranceNo)
                {
                    saleBill.InsNo = model.InsuranceNo;
                    isUpdated = true;
                }

                if (saleBill.InsStartDate != model.InsuranceStartDate)
                {
                    saleBill.InsStartDate = model.InsuranceStartDate;
                    isUpdated = true;
                }

                if (saleBill.InsExpDate != model.InsuranceExpiryDate)
                {
                    saleBill.InsExpDate = model.InsuranceExpiryDate;
                    isUpdated = true;
                }

                if (saleBill.InsuranceLedgerId != model.InsuranceCompany)
                {
                    saleBill.InsuranceLedgerId = model.InsuranceCompany;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    saleBill.UpdatedDate = DateTime.Now;
                    saleBill.UpdatedBy = "Admin";
                }
            }

            if (!string.IsNullOrWhiteSpace(model.CustomerAltMobile))
            {
                int? ledgerId = chassis?.LedgerId;

                if (ledgerId.HasValue)
                {
                    var ledger = await _context.LedgerMasters
                        .FirstOrDefaultAsync(x => x.Id == ledgerId.Value);

                    if (ledger != null)
                    {
                        ledger.AlternateMobileNo = model.CustomerAltMobile;

                        ledger.UpdatedDate = DateTime.Now;
                        ledger.UpdatedBy = "Admin";
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task<ChassisBatteryDetail> GetOrCreateOrderRow(
    string chassisNo,
    int orderNo,
    List<ChassisBatteryDetail> chassisRows,
    Dictionary<int, ChassisBatteryDetail> orderRows)
        {
            if (orderRows.TryGetValue(orderNo, out var existingRow))
                return existingRow;

            existingRow = chassisRows
                .Where(x =>
                    x.BatteryOrderNo == orderNo ||
                    x.MotorOrderNo == orderNo ||
                    x.ChargerOrderNo == orderNo ||
                    x.ControllerOrderNo == orderNo ||
                    x.ConverterOrderNo == orderNo)
                .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .FirstOrDefault();

            if (existingRow != null)
            {
                orderRows[orderNo] = existingRow;
                return existingRow;
            }

            var latest = chassisRows
                .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .FirstOrDefault();

            var newRow = new ChassisBatteryDetail
            {
                ChassisNo = chassisNo,

                BatteryOrderNo = latest?.BatteryOrderNo,
                BatteryNo = latest?.BatteryNo,
                BatteryCapacity = latest?.BatteryCapacity,
                BatteryChemical = latest?.BatteryChemical,
                BatteryMake = latest?.BatteryMake,

                MotorOrderNo = latest?.MotorOrderNo,
                MotorNo = latest?.MotorNo,

                ChargerOrderNo = latest?.ChargerOrderNo,
                ChargerNo = latest?.ChargerNo,

                ControllerOrderNo = latest?.ControllerOrderNo,
                ControllerNo = latest?.ControllerNo,

                ConverterOrderNo = latest?.ConverterOrderNo,
                ConverterNo = latest?.ConverterNo,

                CreatedDate = DateTime.Now,
                CreatedBy = "Admin"
            };

            _context.ChassisBatteryDetails.Add(newRow);

            chassisRows.Add(newRow);
            orderRows[orderNo] = newRow;

            return newRow;
        }
    }
}
