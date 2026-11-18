using DMS_BAPL_Data.CustomModel;
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

        //public async Task<IEnumerable<VehicleInwardD2DViewModel>> GetVehicleByStatus(string dealerCode, bool status)
        //{
        //    try
        //    {
        //        var vehicles = await _context.VehicleInwards
        //            .Where(x => x.IsAccepted == status && x.DealerCode == dealerCode)
        //            .ToListAsync();

        //        var chassisNos = vehicles
        //            .Where(x => !string.IsNullOrEmpty(x.ChasisNo))
        //            .Select(x => x.ChasisNo!)
        //            .Distinct()
        //            .ToList();

        //        var latestHistories = await _context.ChassisDetailsD2dhistories
        //            .Where(x => chassisNos.Contains(x.ChassisNo) && x.DealerCode == dealerCode)
        //            .GroupBy(x => x.ChassisNo)
        //            .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
        //            .ToListAsync();

        //        var dealerCodes = latestHistories.SelectMany(x => new[] { x.DealerCode, x.IssueingDealerCode }).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

        //        var dealers = await _context.DealerMasters
        //            .Where(x => dealerCodes.Contains(x.Dealercode))
        //            .ToDictionaryAsync(x => x.Dealercode, x => x.Compname);

        //        return vehicles.Select(vehicle =>
        //        {
        //            var history = latestHistories
        //                .FirstOrDefault(x => x.ChassisNo == vehicle.ChasisNo);

        //            string? issuedDealerName = null;

        //            if (history != null &&
        //                !string.IsNullOrEmpty(history.IssueingDealerCode) &&
        //                dealers.TryGetValue(history.IssueingDealerCode, out var dealerName))
        //            {
        //                issuedDealerName = dealerName;
        //            }

        //            return new VehicleInwardD2DViewModel
        //            {
        //                Id = vehicle.Id,
        //                InvoiceDate = vehicle.InvoiceDate,
        //                InvoiceNo = vehicle.InvoiceNo,
        //                MfgYear = vehicle.MfgYear,
        //                ItemCode = vehicle.ItemCode,
        //                ColrCode = vehicle.ColrCode,
        //                ChasisNo = vehicle.ChasisNo,
        //                MotorNo = vehicle.MotorNo,
        //                KeyNo = vehicle.KeyNo,
        //                ServBkno = vehicle.ServBkno,
        //                BatteryId = vehicle.BatteryId,
        //                BatteryNo = vehicle.BatteryNo,
        //                BatteryNo2 = vehicle.BatteryNo2,
        //                BatteryNo3 = vehicle.BatteryNo3,
        //                BatteryNo4 = vehicle.BatteryNo4,
        //                BatteryNo5 = vehicle.BatteryNo5,
        //                BatteryNo6 = vehicle.BatteryNo6,
        //                EcuSerno = vehicle.EcuSerno,
        //                EcuImEi = vehicle.EcuImEi,
        //                EcuBalMac = vehicle.EcuBalMac,
        //                ImmoblizerStatus = vehicle.ImmoblizerStatus,
        //                ImmoblizerNo = vehicle.ImmoblizerNo,
        //                BikeSimid = vehicle.BikeSimid,
        //                BikeMobileno = vehicle.BikeMobileno,
        //                ChargerNo = vehicle.ChargerNo,
        //                ControllerNo = vehicle.ControllerNo,
        //                SoundbarSerno = vehicle.SoundbarSerno,
        //                SoundbarBalMac = vehicle.SoundbarBalMac,
        //                Voltage = vehicle.Voltage,
        //                Regnumber = vehicle.Regnumber,
        //                Validity = vehicle.Validity,
        //                Startdate = vehicle.Startdate,
        //                TyreNo1 = vehicle.TyreNo1,
        //                TyreNo2 = vehicle.TyreNo2,
        //                GstIdno = vehicle.GstIdno,
        //                LocCode = vehicle.LocCode,
        //                DealerCode = vehicle.DealerCode,
        //                BatteryChemistry = vehicle.BatteryChemistry,
        //                BatteryCapacity = vehicle.BatteryCapacity,
        //                BatteryMake = vehicle.BatteryMake,
        //                BatteryIdno = vehicle.BatteryIdno,
        //                Fame2Discount = vehicle.Fame2Discount,
        //                Converter = vehicle.Converter,
        //                Vcu = vehicle.Vcu,
        //                Ordertype = vehicle.Ordertype,
        //                MfgMonth = vehicle.MfgMonth,
        //                IsAccepted = vehicle.IsAccepted,
        //                Dlrprice = vehicle.Dlrprice,
        //                Custprice = vehicle.Custprice,
        //                PoType = vehicle.PoType,
        //                Ponumber = vehicle.Ponumber,
        //                CreatedBy = vehicle.CreatedBy,
        //                CreatedDate = vehicle.CreatedDate,
        //                UpdatedBy = vehicle.UpdatedBy,
        //                UpdatedDate = vehicle.UpdatedDate,
        //                IsD2d = vehicle.IsD2d,
        //                InwardType = vehicle.InwardType,
        //                IssuedDealerName = issuedDealerName,
        //                IssuedDealerCode = history == null ? null : history.IssueingDealerCode
        //            };
        //        }).ToList();
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
                    .Where(x => chassisNos.Contains(x.ChassisNo) && x.DealerCode == dealerCode && !x.IsDeleted)
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
                    return new ApiResponse
                    {
                        Valid = false,
                        Description = "Chassis No Already exists.",
                        Value = new List<ApiResponseValue>()
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

                return new ApiResponse
                {
                    Valid = result > 0,
                    Description = result > 0 ? "Data Saved Successfully." : "Failed to save invoice.",
                    Value = new List<ApiResponseValue>()
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

        private static ApiResponse Fail(string msg) => new ApiResponse
        {
            Valid = false,
            Description = msg,
            Value = new List<ApiResponseValue>()
        };

        public async Task<ApiResponse> DeleteInvoiceAsync(string invoiceNo, string userId)
        {
            invoiceNo = (invoiceNo ?? "").Trim();
            if (invoiceNo == "")
                return Fail("Invoice No is required.");

            var chassisList = await _context.VehicleInwards
                .Where(x => x.InvoiceNo == invoiceNo && x.ChasisNo != null)
                .Select(x => x.ChasisNo)
                .Distinct()
                .ToListAsync();

            if (chassisList.Count == 0)
                return Fail("Invoice not found.");

           
            var soldByChassis = await _context.ChassisDetails
                .Where(c => chassisList.Contains(c.ChassisNo) && c.SaleDate != null)
                .Select(c => c.ChassisNo)
                .ToListAsync();

            
            var bills = await (
                from d in _context.VehicleSaleBillDetails
                join h in _context.VehicleSaleBillHeaders on d.VehicleSaleBillId equals h.Id
                where chassisList.Contains(d.ChassisNo)
                      && !h.IsDeleted
                      && h.Status != "Deleted"
                select new { d.ChassisNo, h.Status }
            ).ToListAsync();

            var soldByBill = bills
                .Where(b => b.Status == "Invoiced")
                .Select(b => b.ChassisNo)
                .Distinct()
                .ToList();

            var proformaOnly = bills
                .Where(b => b.Status != "Invoiced")
                .Select(b => b.ChassisNo)
                .Distinct()
                .ToList();

            var sold = soldByChassis.Union(soldByBill).ToList();
            if (sold.Count > 0)
                return Fail($"Vehicle already sold ({sold.Count} of {chassisList.Count} vehicles). Invoice cannot be deleted.");

            if (proformaOnly.Count > 0)
                return Fail($"Proforma bill exists for {proformaOnly.Count} vehicle(s) of this invoice. Delete the proforma first.");

            var hasJobCard = await _context.JobCardCustomers
                .AnyAsync(j => chassisList.Contains(j.ChassisNo));
            if (hasJobCard)
                return Fail("Job card already created for a vehicle of this invoice. Invoice cannot be deleted.");

            var hasHsrp = await _context.Hsrporders
                .AnyAsync(h => chassisList.Contains(h.ChassisNo));
            if (hasHsrp)
                return Fail("HSRP order already created for a vehicle of this invoice. Invoice cannot be deleted.");

            var hasTransfer = await _context.VehicleStockTransferDetails
                .AnyAsync(t => chassisList.Contains(t.ChassisNo));
            if (hasTransfer)
                return Fail("Vehicle stock transfer already done for a vehicle of this invoice. Invoice cannot be deleted.");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var lotHeaderIds = await _context.LotinspectionHeaders
                    .Where(h => h.InvoiceNo == invoiceNo)
                    .Select(h => h.Id)
                    .ToListAsync();

                await _context.LotinspectionDetails
                    .Where(d => lotHeaderIds.Contains(d.LotHeaderId))
                    .ExecuteDeleteAsync();

                await _context.LotinspectionHeaders
                    .Where(h => lotHeaderIds.Contains(h.Id))
                    .ExecuteDeleteAsync();

                await _context.ChassisBatteryDetails
                    .Where(x => chassisList.Contains(x.ChassisNo))
                    .ExecuteDeleteAsync();

                await _context.ChassisDetails
                    .Where(x => chassisList.Contains(x.ChassisNo))
                    .ExecuteDeleteAsync();

                var removed = await _context.VehicleInwards
                    .Where(x => x.InvoiceNo == invoiceNo)
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return new ApiResponse
                {
                    Valid = true,
                    Description = $"Invoice {invoiceNo} deleted ({removed} vehicle record(s) removed).",
                    Value = new List<ApiResponseValue>()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ApiResponse> DeleteVehiclesAsync(List<string> chassisNos, string userId)
        {
            var selected = (chassisNos ?? new List<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct()
                .ToList();

            if (selected.Count == 0)
                return Fail("Select at least one vehicle.");

            var inwardRows = await _context.VehicleInwards
                .Where(x => selected.Contains(x.ChasisNo))
                .Select(x => new { x.ChasisNo, x.InvoiceNo })
                .ToListAsync();

            if (inwardRows.Count == 0)
                return Fail("Selected vehicle(s) not found in Vehicle Inward.");

            var invoiceNos = inwardRows
                .Select(r => r.InvoiceNo)
                .Where(i => i != null)
                .Distinct()
                .ToList();

            var invoiceChassis = await _context.VehicleInwards
                .Where(x => invoiceNos.Contains(x.InvoiceNo) && x.ChasisNo != null)
                .Select(x => new { x.ChasisNo, x.InvoiceNo })
                .ToListAsync();

            var allChassis = invoiceChassis.Select(x => x.ChasisNo).Distinct().ToList();

            var soldByChassis = await _context.ChassisDetails
                .Where(c => allChassis.Contains(c.ChassisNo) && c.SaleDate != null)
                .Select(c => c.ChassisNo)
                .ToListAsync();

            var bills = await (
                from d in _context.VehicleSaleBillDetails
                join h in _context.VehicleSaleBillHeaders on d.VehicleSaleBillId equals h.Id
                where allChassis.Contains(d.ChassisNo)
                      && !h.IsDeleted
                      && h.Status != "Deleted"
                select new { d.ChassisNo, h.Status }
            ).ToListAsync();

            var soldByBill = bills
                .Where(b => b.Status == "Invoiced")
                .Select(b => b.ChassisNo)
                .Distinct()
                .ToList();

            var sold = soldByChassis.Union(soldByBill).ToList();
            if (sold.Count > 0)
            {
                var soldInvoices = invoiceChassis
                    .Where(x => sold.Contains(x.ChasisNo))
                    .Select(x => x.InvoiceNo)
                    .Distinct();
                return Fail($"Vehicle already sold in invoice {string.Join(", ", soldInvoices)} ({sold.Count} vehicle(s)). Cannot delete.");
            }

            var proformaOnly = bills
                .Where(b => selected.Contains(b.ChassisNo) && b.Status != "Invoiced")
                .Select(b => b.ChassisNo)
                .Distinct()
                .ToList();
            if (proformaOnly.Count > 0)
                return Fail($"Proforma bill exists for {proformaOnly.Count} selected vehicle(s). Delete the proforma first.");

            if (await _context.JobCardCustomers.AnyAsync(j => selected.Contains(j.ChassisNo)))
                return Fail("Job card already created for a selected vehicle. Cannot delete.");

            if (await _context.Hsrporders.AnyAsync(h => selected.Contains(h.ChassisNo)))
                return Fail("HSRP order already created for a selected vehicle. Cannot delete.");

            if (await _context.VehicleStockTransferDetails.AnyAsync(t => selected.Contains(t.ChassisNo)))
                return Fail("Vehicle stock transfer already done for a selected vehicle. Cannot delete.");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.LotinspectionDetails
                    .Where(d => selected.Contains(d.ChassisNo))
                    .ExecuteDeleteAsync();

                await _context.ChassisBatteryDetails
                    .Where(x => selected.Contains(x.ChassisNo))
                    .ExecuteDeleteAsync();

                await _context.ChassisDetails
                    .Where(x => selected.Contains(x.ChassisNo))
                    .ExecuteDeleteAsync();

                var removed = await _context.VehicleInwards
                    .Where(x => selected.Contains(x.ChasisNo))
                    .ExecuteDeleteAsync();

                var remainingInvoices = await _context.VehicleInwards
                    .Where(x => invoiceNos.Contains(x.InvoiceNo))
                    .Select(x => x.InvoiceNo)
                    .Distinct()
                    .ToListAsync();

                var emptyInvoices = invoiceNos.Except(remainingInvoices).ToList();

                if (emptyInvoices.Count > 0)
                {
                    var lotHeaderIds = await _context.LotinspectionHeaders
                        .Where(h => emptyInvoices.Contains(h.InvoiceNo))
                        .Select(h => h.Id)
                        .ToListAsync();

                    await _context.LotinspectionDetails
                        .Where(d => lotHeaderIds.Contains(d.LotHeaderId))
                        .ExecuteDeleteAsync();

                    await _context.LotinspectionHeaders
                        .Where(h => lotHeaderIds.Contains(h.Id))
                        .ExecuteDeleteAsync();
                }

                await tx.CommitAsync();

                return new ApiResponse
                {
                    Valid = true,
                    Description = $"{removed} vehicle(s) deleted from {invoiceNos.Count} invoice(s).",
                    Value = new List<ApiResponseValue>()
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

    }
}
