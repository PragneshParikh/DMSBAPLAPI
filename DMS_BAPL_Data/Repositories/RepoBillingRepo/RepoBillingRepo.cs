using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.RepoBillingRepo
{
    public partial class RepoBillingRepo : IRepoBillingRepo
    {

        private readonly BapldmsvadContext _context;

        public RepoBillingRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<VehicleInfoViewModel> GetRepoBillingByChassis(string chassis, string regNo)
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

                where ((!string.IsNullOrWhiteSpace(chassis) && cd.ChassisNo == chassis) ||
                (!string.IsNullOrWhiteSpace(regNo) && cd.RegNo != null && cd.RegNo == regNo))

                select new VehicleInfoViewModel
                {
                    DealerDetails = new DealerInfoViewMode
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
    }
}
