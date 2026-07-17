using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleDispatchRepo
{
    public class VehicleInwardRepo : IVehicleInwardRepo
    {
        private readonly BapldmsvadContext _context;
        public VehicleInwardRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        async Task<IEnumerable<VehicleInward>> IVehicleInwardRepo.Get()
        {
            try
            {
                return _context.VehicleInwards.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //async Task<IEnumerable<VehicleInward>> IVehicleInwardRepo.GetVehicleByStatus(string dealerCode, bool status)
        //{
        //    try
        //    {
        //        return await _context.VehicleInwards
        //            .Where(x => x.IsAccepted == status && x.DealerCode == dealerCode)
        //            .ToListAsync();

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<VehicleInwardD2DViewModel>> GetVehicleByStatus(string dealerCode, bool status)
        {
            try
            {
                var vehicles = await _context.VehicleInwards
                    .Where(x => x.IsAccepted == status && x.DealerCode == dealerCode)
                    .ToListAsync();

                var chassisNos = vehicles
                    .Where(x => !string.IsNullOrEmpty(x.ChasisNo))
                    .Select(x => x.ChasisNo!)
                    .Distinct()
                    .ToList();

                var latestHistories = await _context.ChassisDetailsD2dhistories
                    .Where(x => chassisNos.Contains(x.ChassisNo) && x.DealerCode == dealerCode)
                    .GroupBy(x => x.ChassisNo)
                    .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
                    .ToListAsync();

                var dealerCodes = latestHistories.SelectMany(x => new[] { x.DealerCode, x.IssueingDealerCode }).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

                var dealers = await _context.DealerMasters
                    .Where(x => dealerCodes.Contains(x.Dealercode))
                    .ToDictionaryAsync(x => x.Dealercode, x => x.Compname);

                return vehicles.Select(vehicle =>
                {
                    var history = latestHistories
                        .FirstOrDefault(x => x.ChassisNo == vehicle.ChasisNo);

                    string? issuedDealerName = null;

                    if (history != null &&
                        !string.IsNullOrEmpty(history.IssueingDealerCode) &&
                        dealers.TryGetValue(history.IssueingDealerCode, out var dealerName))
                    {
                        issuedDealerName = dealerName;
                    }

                    return new VehicleInwardD2DViewModel
                    {
                        Id = vehicle.Id,
                        InvoiceDate = vehicle.InvoiceDate,
                        InvoiceNo = vehicle.InvoiceNo,
                        MfgYear = vehicle.MfgYear,
                        ItemCode = vehicle.ItemCode,
                        ColrCode = vehicle.ColrCode,
                        ChasisNo = vehicle.ChasisNo,
                        MotorNo = vehicle.MotorNo,
                        KeyNo = vehicle.KeyNo,
                        ServBkno = vehicle.ServBkno,
                        BatteryId = vehicle.BatteryId,
                        BatteryNo = vehicle.BatteryNo,
                        BatteryNo2 = vehicle.BatteryNo2,
                        BatteryNo3 = vehicle.BatteryNo3,
                        BatteryNo4 = vehicle.BatteryNo4,
                        BatteryNo5 = vehicle.BatteryNo5,
                        BatteryNo6 = vehicle.BatteryNo6,
                        EcuSerno = vehicle.EcuSerno,
                        EcuImEi = vehicle.EcuImEi,
                        EcuBalMac = vehicle.EcuBalMac,
                        ImmoblizerStatus = vehicle.ImmoblizerStatus,
                        ImmoblizerNo = vehicle.ImmoblizerNo,
                        BikeSimid = vehicle.BikeSimid,
                        BikeMobileno = vehicle.BikeMobileno,
                        ChargerNo = vehicle.ChargerNo,
                        ControllerNo = vehicle.ControllerNo,
                        SoundbarSerno = vehicle.SoundbarSerno,
                        SoundbarBalMac = vehicle.SoundbarBalMac,
                        Voltage = vehicle.Voltage,
                        Regnumber = vehicle.Regnumber,
                        Validity = vehicle.Validity,
                        Startdate = vehicle.Startdate,
                        TyreNo1 = vehicle.TyreNo1,
                        TyreNo2 = vehicle.TyreNo2,
                        GstIdno = vehicle.GstIdno,
                        LocCode = vehicle.LocCode,
                        DealerCode = vehicle.DealerCode,
                        BatteryChemistry = vehicle.BatteryChemistry,
                        BatteryCapacity = vehicle.BatteryCapacity,
                        BatteryMake = vehicle.BatteryMake,
                        BatteryIdno = vehicle.BatteryIdno,
                        Fame2Discount = vehicle.Fame2Discount,
                        Converter = vehicle.Converter,
                        Vcu = vehicle.Vcu,
                        Ordertype = vehicle.Ordertype,
                        MfgMonth = vehicle.MfgMonth,
                        IsAccepted = vehicle.IsAccepted,
                        Dlrprice = vehicle.Dlrprice,
                        Custprice = vehicle.Custprice,
                        PoType = vehicle.PoType,
                        Ponumber = vehicle.Ponumber,
                        CreatedBy = vehicle.CreatedBy,
                        CreatedDate = vehicle.CreatedDate,
                        UpdatedBy = vehicle.UpdatedBy,
                        UpdatedDate = vehicle.UpdatedDate,
                        IsD2d = vehicle.IsD2d,
                        InwardType = vehicle.InwardType,
                        IssuedDealerName = issuedDealerName,
                        IssuedDealerCode = history == null ? null : history.IssueingDealerCode
                    };
                }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task<IEnumerable<VehicleInward>> GetVehicleByStatus(string dealerCode, bool status)
        //{
        //    try
        //    {
        //        var inwards = await _context.VehicleInwards
        //            .Where(x => x.IsAccepted == status && x.DealerCode == dealerCode)
        //            .ToListAsync();

        //        var chassisNos = inwards
        //            .Where(x => x.IsD2d == true)
        //            .Select(x => x.ChasisNo)
        //            .ToList();

        //        var historyLookup = await _context.ChassisDetailsD2dhistories
        //            .Where(x => chassisNos.Contains(x.ChassisNo))
        //            .GroupBy(x => x.ChassisNo)
        //            .Select(g => g.OrderByDescending(x => x.TransDate).First())
        //            .ToListAsync();

        //        var dealerCodes = historyLookup
        //            .Select(x => x.DealerCode)
        //            .Distinct()
        //            .ToList();

        //        var dealers = await _context.DealerMasters
        //            .Where(x => dealerCodes.Contains(x.Dealercode))
        //            .ToDictionaryAsync(x => x.Dealercode, x => x.Compname);

        //        var result = inwards.Select(inward =>
        //        {
        //            string? dealerName = null;

        //            if (inward.IsD2d == true)
        //            {
        //                var history = historyLookup
        //                    .FirstOrDefault(x => x.ChassisNo == inward.ChasisNo);

        //                if (history != null &&
        //                    dealers.TryGetValue(history.DealerCode, out var name))
        //                {
        //                    dealerName = name;
        //                }
        //            }

        //            return new
        //            {
        //                Vehicle = inward,
        //                DealerName = dealerName
        //            };
        //        });

        //        return result;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
        async Task<bool> IVehicleInwardRepo.UpdateInvoiceStatus(string invoiceNo, string userId)
        {
            try
            {
                var affectedRows = await _context.VehicleInwards
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.IsAccepted, true)
                        );

                return affectedRows > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        async Task<object> IVehicleInwardRepo.InsertVehicleDispatchDetail(VehicleInwardViewModel vehicleInwardViewModel)
        {
            try
            {

                var exist = await _context.VehicleInwards
               .FirstOrDefaultAsync(x =>
                   x.ChasisNo == vehicleInwardViewModel.chasis_no);

                if (exist != null)
                {
                    return new
                    {
                        Valid = false,
                        Message = "Chassis number already exist. Duplicate entry.",
                        Value = Array.Empty<object>()
                    };
                }

                var vehicleInward = new VehicleInward
                {
                    //Id = -1,
                    InvoiceDate = vehicleInwardViewModel.invoice_date,
                    InvoiceNo = vehicleInwardViewModel.invoice_no,
                    MfgYear = vehicleInwardViewModel.mfg_year,
                    ItemCode = vehicleInwardViewModel.item_code,
                    ColrCode = vehicleInwardViewModel.colr_code,
                    ChasisNo = vehicleInwardViewModel.chasis_no,
                    MotorNo = vehicleInwardViewModel.motor_no,
                    KeyNo = vehicleInwardViewModel?.key_no,
                    ServBkno = vehicleInwardViewModel.serv_bkno,
                    BatteryId = vehicleInwardViewModel.battery_id,
                    BatteryNo = vehicleInwardViewModel.battery_no,
                    BatteryNo2 = vehicleInwardViewModel.battery_no2,
                    BatteryNo3 = vehicleInwardViewModel.battery_no3,
                    BatteryNo4 = vehicleInwardViewModel.battery_no4,
                    BatteryNo5 = vehicleInwardViewModel.battery_no5,
                    BatteryNo6 = vehicleInwardViewModel.battery_no5,
                    EcuSerno = vehicleInwardViewModel.ecu_serno,
                    EcuImEi = vehicleInwardViewModel.ecu_im_ei,
                    EcuBalMac = vehicleInwardViewModel.ecu_bal_mac,
                    ImmoblizerStatus = vehicleInwardViewModel.immoblizer_status,
                    ImmoblizerNo = vehicleInwardViewModel.immoblizer_no,
                    BikeSimid = vehicleInwardViewModel.bike_simid,
                    BikeMobileno = vehicleInwardViewModel.bike_mobileno,
                    ChargerNo = vehicleInwardViewModel.charger_no,
                    ControllerNo = vehicleInwardViewModel.controller_no,
                    SoundbarSerno = vehicleInwardViewModel.soundbar_serno,
                    SoundbarBalMac = vehicleInwardViewModel.soundbar_bal_mac,
                    Voltage = vehicleInwardViewModel.voltage,
                    Regnumber = vehicleInwardViewModel.regnumber,
                    Validity = vehicleInwardViewModel.validity,
                    Startdate = vehicleInwardViewModel.startdate,
                    TyreNo1 = vehicleInwardViewModel.tyre_no1,
                    TyreNo2 = vehicleInwardViewModel?.tyre_no2,
                    GstIdno = vehicleInwardViewModel.gst_idno,
                    LocCode = vehicleInwardViewModel.loc_code,
                    DealerCode = vehicleInwardViewModel.dealer_code,
                    BatteryChemistry = vehicleInwardViewModel.battery_chemistry,
                    BatteryCapacity = vehicleInwardViewModel.battery_capacity,
                    BatteryMake = vehicleInwardViewModel.battery_make,
                    BatteryIdno = vehicleInwardViewModel.battery_idno,
                    Fame2Discount = vehicleInwardViewModel.fame2_discount,
                    Converter = vehicleInwardViewModel.converter,
                    Vcu = vehicleInwardViewModel.vcu,
                    Ordertype = vehicleInwardViewModel.ordertype,
                    MfgMonth = vehicleInwardViewModel.mfg_month,
                    IsAccepted = vehicleInwardViewModel.IsAccepted,
                    Dlrprice = vehicleInwardViewModel.dlrprice,
                    Custprice = vehicleInwardViewModel.custprice,
                    PoType = vehicleInwardViewModel.poType,
                    Ponumber = vehicleInwardViewModel.DMSPoNo,
                    CreatedBy = "Admin",
                    CreatedDate = DateTime.Now,
                };

                _context.VehicleInwards.Add(vehicleInward);
                var result = await _context.SaveChangesAsync();

                return new
                {
                    Success = result > 0,
                    Message = result > 0 ? "Invoice saved successfully." : "Failed to save invoice."
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<VehicleInward>> GetByChassisNosAsync(List<string> chassisNos)
        {
            try
            {
                return await _context.VehicleInwards
               .Where(x => x.ChasisNo != null && chassisNos.Contains(x.ChasisNo))
               .ToListAsync();
            }
            catch
            {
                throw;
            }
        }


    }
}
