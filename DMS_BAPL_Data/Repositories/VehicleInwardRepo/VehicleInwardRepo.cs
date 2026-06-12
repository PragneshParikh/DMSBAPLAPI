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

        async Task<IEnumerable<VehicleInward>> IVehicleInwardRepo.GetVehicleByStatus(string dealerCode, bool status)
        {
            try
            {
                return await _context.VehicleInwards
                    .Where(x => x.IsAccepted == status && x.DealerCode == dealerCode)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
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
                        Success = false,
                        Message = "Chassis number already exist. Duplicate entry."
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
