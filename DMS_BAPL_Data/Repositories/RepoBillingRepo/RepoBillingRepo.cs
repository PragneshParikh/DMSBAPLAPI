using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
                    CurrentDealer = di != null ? new DealerInfoViewMode
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
                return new JsonResult(new { DealerDetails = new List<DealerInfoViewMode>(), PartyDetails = new List<PartyDetailsViewModel>(), VehicleDetails = (VehicleDetailsViewModel)null });

            // Initialize collections for Dealers and Parties, adding the current records first
            var dealerList = new List<DealerInfoViewMode>();
            if (baseVehicle.CurrentDealer != null) dealerList.Add(baseVehicle.CurrentDealer);

            var partyList = new List<PartyDetailsViewModel>();
            if (baseVehicle.CurrentParty != null) partyList.Add(baseVehicle.CurrentParty);

            // 2. Fetch ALL history records matching this ChassisNo
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
                    DealerDetails = di != null ? new DealerInfoViewMode
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
                        Email = ld.EMail,
                    } : null
                })
                .AsNoTracking()
                .ToListAsync();

            // 3. Push history records into their respective lists
            foreach (var history in historyRecords)
            {
                if (history.DealerDetails != null) dealerList.Add(history.DealerDetails);
                if (history.PartyDetails != null) partyList.Add(history.PartyDetails);
            }

            // 4. Query component parts only once
            var batteryDetails = await _context.ChassisBatteryDetails
                .AsNoTracking()
                .Where(x => x.ChassisNo == baseVehicle.ChassisNo)
                .ToListAsync();

            // Extract components safely
            var batteries = batteryDetails.Where(x => !string.IsNullOrWhiteSpace(x.BatteryNo)).GroupBy(x => x.BatteryOrderNo).Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First()).Select(x => new BatteryViewModel { SerialNo = x.BatteryOrderNo ?? 0, BatteryNo = x.BatteryNo, Capacity = x.BatteryCapacity, BatteryMake = x.BatteryMake, ChemicalType = x.BatteryChemical }).OrderBy(x => x.SerialNo).ToList();
            var chargers = batteryDetails.Where(x => !string.IsNullOrWhiteSpace(x.ChargerNo)).GroupBy(x => x.ChargerOrderNo).Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First()).Select(x => new ComponentViewModel { SerialNo = x.ChargerOrderNo ?? 0, ComponentNo = x.ChargerNo }).OrderBy(x => x.SerialNo).ToList();
            var controllers = batteryDetails.Where(x => !string.IsNullOrWhiteSpace(x.ControllerNo)).GroupBy(x => x.ControllerOrderNo).Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First()).Select(x => new ComponentViewModel { SerialNo = x.ControllerOrderNo ?? 0, ComponentNo = x.ControllerNo }).OrderBy(x => x.SerialNo).ToList();
            var motors = batteryDetails.Where(x => !string.IsNullOrWhiteSpace(x.MotorNo)).GroupBy(x => x.MotorOrderNo).Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First()).Select(x => new ComponentViewModel { SerialNo = x.MotorOrderNo ?? 0, ComponentNo = x.MotorNo }).OrderBy(x => x.SerialNo).ToList();
            var converters = batteryDetails.Where(x => !string.IsNullOrWhiteSpace(x.ConverterNo)).GroupBy(x => x.ConverterOrderNo).Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).First()).Select(x => new ComponentViewModel { SerialNo = x.ConverterOrderNo ?? 0, ComponentNo = x.ConverterNo }).OrderBy(x => x.SerialNo).ToList();

            // Attach component arrays to the single vehicle details record
            var vehicleDetails = baseVehicle.VehicleDetailsRaw;
            vehicleDetails.Batteries = batteries;
            vehicleDetails.Chargers = chargers;
            vehicleDetails.Controllers = controllers;
            vehicleDetails.Motors = motors;
            vehicleDetails.Converters = converters;

            // 5. Return an anonymous object matching your client's expected shape without changing the target class
            return new JsonResult(new
            {
                DealerDetails = dealerList,
                PartyDetails = partyList,
                VehicleDetails = vehicleDetails
            });
        }
    }
}
