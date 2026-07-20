using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
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

        private static string ToRoman(int number)
        {
            return number switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                8 => "VIII",
                9 => "IX",
                10 => "X",
                _ => string.Empty
            };
        }

        public RepoBillingRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<JsonResult> GetRepoBillingByChassis(string chassis, string regNo)
        {
            // 1. Fetch the base vehicle registration and current dealer/party info
            var baseVehicle = await (
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

                where ((!string.IsNullOrWhiteSpace(chassis) && cd.ChassisNo == chassis) ||
                       (!string.IsNullOrWhiteSpace(regNo) && cd.RegNo != null && cd.RegNo == regNo))

                select new
                {
                    ChassisNo = cd.ChassisNo,
                    CurrentDealer = di != null ? new DealerInfoViewModel
                    {
                        DealerCode = di.Dealercode,
                        DealerName = di.Compname,
                        DealerEmail = di.Email,
                        DealerLocation = locdealer != null ? locdealer.Locname : string.Empty,
                        MobileNo = di.Mobile,
                        Address = di.Adress1,
                        Email = di.Email,
                        DealerState = di.State,
                        DealerCity = di.City
                    } : null,
                    CurrentParty = ld != null ? new PartyDetailsViewModel
                    {
                        PartyName = ld.LedgerName ?? string.Empty,
                        PartyMobile = ld.MobileNumber ?? string.Empty,
                        PartyAltMobile = ld.AlternateMobileNo ?? string.Empty,
                        Address1 = ld.Address ?? string.Empty,
                        Address2 = ld.Address2 ?? string.Empty,
                        City = ld.CityNavigation != null ? ld.CityNavigation.CityName : null,
                        State = ld.StateNavigation != null ? ld.StateNavigation.StateName : null,
                        Pin = ld.Pin,
                        Email = ld.EMail,
                    } : null,
                    VehicleDetailsRaw = new VehicleDetailsViewModel
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

            if (baseVehicle == null)
            {
                return new JsonResult(new
                {
                    DealerDetails = new List<DealerInfoViewModel>(),
                    PartyDetails = new List<PartyDetailsViewModel>(),
                    VehicleDetails = (VehicleDetailsViewModel)null
                });
            }

            // History
            var historyRecords = await (
                from h in _context.ChassisDetailsD2dhistories

                join di in _context.DealerMasters
                    on h.DealerCode equals di.Dealercode into DealerInfo
                from di in DealerInfo.DefaultIfEmpty()

                join locdealer in _context.LocationMasters
                    on h.LocationCode equals locdealer.Loccode into DlrLocInfo
                from locdealer in DlrLocInfo.DefaultIfEmpty()

                join ld in _context.LedgerMasters
                    on h.LedgerId equals ld.Id into ldJoin
                from ld in ldJoin.DefaultIfEmpty()

                where h.ChassisNo == baseVehicle.ChassisNo

                select new
                {
                    DealerDetails = di != null ? new DealerInfoViewModel
                    {
                        //DealerCode = di.Dealercode,
                        DealerCode = locdealer != null && !string.IsNullOrWhiteSpace(locdealer.Dealercode)
                        ? locdealer.Dealercode : di.Dealercode,
                        DealerName = di.Compname,
                        DealerEmail = di.Email,
                        DealerLocation = locdealer != null ? locdealer.Locname : string.Empty,
                        MobileNo = di.Mobile,
                        Address = di.Adress1,
                        Email = di.Email,
                        DealerState = di.State,
                        DealerCity = di.City
                    } : null,

                    PartyDetails = ld != null ? new PartyDetailsViewModel
                    {
                        PartyName = ld.LedgerName ?? string.Empty,
                        PartyMobile = ld.MobileNumber ?? string.Empty,
                        PartyAltMobile = ld.AlternateMobileNo ?? string.Empty,
                        Address1 = ld.Address ?? string.Empty,
                        Address2 = ld.Address2 ?? string.Empty,
                        City = ld.CityNavigation != null ? ld.CityNavigation.CityName : null,
                        State = ld.StateNavigation != null ? ld.StateNavigation.StateName : null,
                        Pin = ld.Pin,
                        Email = ld.EMail
                    } : null
                })
                .AsNoTracking()
                .ToListAsync();

            var dealerList = historyRecords
                .Where(x => x.DealerDetails != null)
                .Select(x => x.DealerDetails)
                .ToList();

            var partyList = historyRecords
                .Where(x => x.PartyDetails != null)
                .Select(x => x.PartyDetails)
                .ToList();

            // Append current owner/dealer at the end
            if (baseVehicle.CurrentDealer != null)
            {
                dealerList.Add(baseVehicle.CurrentDealer);
            }

            if (baseVehicle.CurrentParty != null)
            {
                partyList.Add(baseVehicle.CurrentParty);
            }

            // Assign Owner/Dealer order
            for (int i = 0; i < dealerList.Count; i++)
            {
                dealerList[i].DealerOrder = $"{ToRoman(i + 1)}";
            }

            for (int i = 0; i < partyList.Count; i++)
            {
                partyList[i].OwnerOrder = $"{ToRoman(i + 1)}";
            }

            // Components
            var batteryDetails = await _context.ChassisBatteryDetails
                .AsNoTracking()
                .Where(x => x.ChassisNo == baseVehicle.ChassisNo)
                .ToListAsync();

            var vehicleDetails = baseVehicle.VehicleDetailsRaw;

            vehicleDetails.Batteries = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.BatteryNo))
                .GroupBy(x => x.BatteryOrderNo)
                .Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First())
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

            vehicleDetails.Chargers = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.ChargerNo))
                .GroupBy(x => x.ChargerOrderNo)
                .Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First())
                .Select(x => new ComponentViewModel
                {
                    SerialNo = x.ChargerOrderNo ?? 0,
                    ComponentNo = x.ChargerNo
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            vehicleDetails.Controllers = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.ControllerNo))
                .GroupBy(x => x.ControllerOrderNo)
                .Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First())
                .Select(x => new ComponentViewModel
                {
                    SerialNo = x.ControllerOrderNo ?? 0,
                    ComponentNo = x.ControllerNo
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            vehicleDetails.Motors = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.MotorNo))
                .GroupBy(x => x.MotorOrderNo)
                .Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First())
                .Select(x => new ComponentViewModel
                {
                    SerialNo = x.MotorOrderNo ?? 0,
                    ComponentNo = x.MotorNo
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            vehicleDetails.Converters = batteryDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.ConverterNo))
                .GroupBy(x => x.ConverterOrderNo)
                .Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First())
                .Select(x => new ComponentViewModel
                {
                    SerialNo = x.ConverterOrderNo ?? 0,
                    ComponentNo = x.ConverterNo
                })
                .OrderBy(x => x.SerialNo)
                .ToList();

            return new JsonResult(new
            {
                DealerDetails = dealerList,
                PartyDetails = partyList,
                VehicleDetails = vehicleDetails
            });
        }
    }

}

