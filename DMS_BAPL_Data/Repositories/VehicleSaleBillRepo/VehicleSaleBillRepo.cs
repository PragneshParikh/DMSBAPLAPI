using DMS_BAPL_Data.Helpers;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PrefixRepo;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2013.Drawing.Chart;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using XL = ClosedXML.Excel;

namespace DMS_BAPL_Data.Repositories.VehicleSaleBillRepo
{
    public class VehicleSaleBillRepo : IVehicleSaleBillRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IPartInventoryService _partInventoryService;
        private readonly IPrefixRepo _prefixRepo;
        private readonly IHttpContextAccessor _contextAccessor;

        public VehicleSaleBillRepo(BapldmsvadContext context, IPartInventoryService partInventoryService,
            IPrefixRepo prefixRepo,
            IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _partInventoryService = partInventoryService;
            _contextAccessor = contextAccessor;
            _prefixRepo = prefixRepo;
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
        public async Task<VehicleSaleBillResponseViewModel?> GetVehicleWithMotorDetailsByIdAsync(int id)
        {
            try
            {

                var result = await (
                    from h in _context.VehicleSaleBillHeaders
                    join cled in _context.LedgerMasters on h.LedgerId equals cled.Id

                    join finLed in _context.LedgerMasters on h.Financier equals finLed.Id into finLedJoin
                    from finLed in finLedJoin.DefaultIfEmpty()

                    where h.Id == id

                    select new VehicleSaleBillResponseViewModel
                    {
                        SaleDate = h.SaleDate,
                        SaleBillNo = h.SaleBillNo,
                        isD2d = h.IsD2d,
                        CustomerType = h.CustomerType,
                        Location = h.Location,
                        SaleType = h.SaleType,
                        CashAccount = h.CashAccount,
                        Financier = h.Financier,
                        FinancierName = finLed != null ? finLed.LedgerName : null,
                        BillType = h.BillType,
                        BillFrom = h.BillFrom,
                        CustomerName = cled.LedgerName,
                        BillingName = cled.LedgerName,
                        SalesExecutive = h.SalesExecutive,
                        LedgerId = h.LedgerId,
                        TempRegNo = h.TempRegNo,
                        BookingId = h.BookingId,
                        PrintType = h.PrintType,
                        RefName = h.RefName,
                        RefAddress = h.RefAddress,
                        RefEmail = h.RefEmail,
                        RefPoint = h.RefPoint,
                        RefRemarks = h.RefRemarks,
                        TotalAmount = h.TotalAmount,
                        Status = h.Status,
                        RefMobile = h.RefMobile,
                        isTempRegNo = h.TempRegNo,
                        AccessoryAmount = h.AccessoryAmount,
                        AccessoryBillNo = h.AccessoryBillNo,
                        HandlingCharges = h.HandlingCharges,
                        Hpamount = h.Hpamount,
                        NoPlateAmount = h.NoPlateAmount,
                        StateSubsidyAmount = h.StateSubsidyAmount,

                        DealerCode = h.DealerCode,

                        Details = (
                        from d in _context.VehicleSaleBillDetails

                        join i in _context.ItemMasters
                            on d.ItemCode equals i.Itemcode into itemJoin
                        from i in itemJoin.DefaultIfEmpty()

                        join insLed in _context.LedgerMasters
                            on d.InsuranceLedgerId equals insLed.Id into insLedJoin
                        from insLed in insLedJoin.DefaultIfEmpty()

                        where d.VehicleSaleBillId == h.Id

                        select new VehicleSaleBillDetailVM
                        {
                            Id = d.Id,
                            ChassisNo = d.ChassisNo,
                            ItemRate = d.ItemRate,
                            ItemCode = d.ItemCode,
                            PreGstDiscount = d.PreGstDiscount,
                            RegAmount = d.RegAmount,
                            InsuranceAmount = d.InsuranceAmount,
                            HasDevice = d.HasDevice,
                            HasKit = d.HasKit,
                            IsDelivered = d.IsDelivered,
                            Segment = d.Segment,
                            InstitutionalType = d.InstitutionalType,
                            SchemeName = d.SchemeName,
                            Narration = d.Narration,
                            Sgstper = d.Sgstper,
                            Sgstamnt = d.Sgstamnt,
                            Cgstper = d.Cgstper,
                            Cgstamnt = d.Cgstamnt,
                            Igstper = d.Igstper,
                            Igstamnt = d.Igstamnt,
                            MfgYear = d.MfgYear,
                            InsNo = d.InsNo,
                            InsStartDate = d.InsStartDate,
                            RegNo = d.RegNo,
                            InsExpDate = d.InsExpDate,
                            FinalAmount = d.FinalAmount,
                            IsAgainstExchange = d.IsAgainstExchange,
                            ModelName = d.ModelName,
                            Colour = d.Colour,
                            Battery = d.Battery,
                            ConvertorNo = d.ConvertorNo,
                            ChargerNo = d.ChargerNo,
                            ControllerNo = d.ControllerNo,
                            Key = d.Key,
                            BookNo = d.BookNo,
                            ExtWarranty = d.ExtWarranty,
                            BatteryChemical = d.BatteryChemical,
                            BatteryCapacity = d.BatteryCapacity,
                            BatteryMake = d.BatteryMake,
                            StockDetailsNo = d.StockDetailsNo,
                            Vcu = d.Vcu,
                            HsnCode = i != null ? i.Hsncode : null,

                            MotorNo = _context.VehicleInwards
                                .Where(x => x.ChasisNo == d.ChassisNo)
                                .OrderByDescending(x => x.CreatedDate)
                                .Select(x => x.MotorNo)
                                .FirstOrDefault(),

                            InvoiceNo = _context.InvoiceHeaders
                                .Where(x => x.ReferenceId == d.VehicleSaleBillId)
                                .OrderByDescending(x => x.CreatedDate)
                                .Select(x => x.InvoiceNo)
                                .FirstOrDefault(),

                            FameIIDisc = d.FameIi,
                            PostGstDiscount = d.PostGstDisc,
                            InsuranceId = d.InsuranceLedgerId,
                            InsuranceName = insLed != null ? insLed.LedgerName : null
                        }
                            ).ToList()


                    }).FirstOrDefaultAsync();
                if (result == null)
                {
                    return null;
                }
                result.TermsAndConditions = await _context.TermandConditionMasters
                    .Where(i => i.ConditionModule == 6 && i.ConditionEffectiveDate <= result.SaleDate)

                    .Select(i => new SalesConditionViewModel
                    {
                        Id = i.Id,
                        SrNo = null,
                        ConditionText = i.TermCondition,
                    }).ToListAsync();
                return result;
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
                    .Where(x => x.IsDeleted == false)
                    .Include(x => x.VehicleSaleBillDetails)
                    .OrderByDescending(x => x.CreatedDate)
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
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
                var data = await _context.VehicleSaleBillHeaders.Include(i => i.VehicleSaleBillDetails).Where(i => i.Id == id).FirstOrDefaultAsync();

                if (data != null)
                {

                    // Cogxim-imported bills never moved stock in this DMS, so deleting one must not add stock back.
                    if (data.Status == "Invoiced" && data.BillFrom != CogximBillFromTag)
                    {
                        var chassisNos = data.VehicleSaleBillDetails.Select(d => d.ChassisNo).ToList();
                        var vehicleItemCodes = await _context.ChassisDetails.Where(v => v.ChassisNo != null && chassisNos.Contains(v.ChassisNo)).Select(v => v.ItemCode).ToListAsync();
                        var groupedItems = vehicleItemCodes.GroupBy(x => x).Select(g => new
                        {
                            ItemCode = g.Key,
                            Qty = g.Count()
                        }).ToList();
                        var invoice = await _context.InvoiceHeaders.Where(i => i.DocumentNo == data.SaleBillNo && i.ReferenceId == data.Id).FirstOrDefaultAsync();
                        var ChassisList = await _context.VehicleSaleBillDetails.Where(i => i.VehicleSaleBillId == id).Select(i => i.ChassisNo).ToListAsync();
                        var ChassisDetails = await _context.ChassisDetails.Where(i => (ChassisList).Contains(i.ChassisNo)).ToListAsync();
                        foreach (var chassis in ChassisDetails)
                        {
                            chassis.SaleDate = null;
                            chassis.LedgerId = null;
                            chassis.RegNo = null;
                            chassis.UpdatedDate = DateTime.Now;
                            chassis.UpdatedBy = userId;
                            if (data.IsD2d == true)
                            {
                                var receivingDealerCode = await _context.LedgerMasters.Where(i => i.Id == data.LedgerId).Select(i => i.DealerCode).FirstOrDefaultAsync();
                                var ChassisD2DHistory = await _context.ChassisDetailsD2dhistories.Where(i => i.ChassisNo == chassis.ChassisNo && i.IssueingDealerCode == data.DealerCode && i.DealerCode == receivingDealerCode).OrderByDescending(i => i.CreatedDate).FirstOrDefaultAsync();
                                var ChassisInward = await _context.VehicleInwards.Where(i => i.ChasisNo == chassis.ChassisNo && i.InvoiceNo == invoice.InvoiceNo).ToListAsync();
                                foreach (var inward in ChassisInward)
                                {
                                    if (inward.IsAccepted == false)
                                    {
                                        foreach (var chassisDetailsToUpdate in ChassisDetails)
                                        {
                                            chassisDetailsToUpdate.DealerId = data.DealerCode;
                                            chassisDetailsToUpdate.LocationCode = data.Location;
                                        }
                                        _context.VehicleInwards.Remove(inward);
                                    }
                                    else
                                    {
                                        throw new Exception("Already Accepted by Dealer");
                                    }
                                }
                                ChassisD2DHistory.IsDeleted = true;

                            }

                        }
                        foreach (var item in groupedItems)
                        {
                            await _partInventoryService.UpdateIncoming(new PartsInventory
                            {
                                ItemCode = item.ItemCode,
                                TransType = "SD",
                                BatchTransQty = item.Qty,
                                VendorCode = data.DealerCode,
                                DealerLocation = data.Location,
                                CreatedBy = userId,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }
                    data.Status = "Deleted";
                    data.IsDeleted = true;
                   
                    data.DeletedDate = DateTime.Now;
                    data.UpdatedDate = DateTime.Now;
                    data.UpdatedBy = userId;
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
             join vd in _context.VehicleInwards
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
             join jcu in _context.JobCardCustomers
             on jc.Chassisno equals jcu.ChassisNo into jobCardGroup
             from jcu in jobCardGroup.DefaultIfEmpty()


             where jc.DealerCode == dealerCode
                && jc.IsPdiSuccess == true
             && jcu.SaleDate == null

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
                 DealerPrice = vd.Dlrprice,
                 CustomerPrice = vd.Custprice,
                 //PreGstDisc = im.Fame2amount,

                 DealerCode = jc.DealerCode,
                 CustomerSaleDate = jcu.SaleDate
             }
         ).ToListAsync();

                return data;



            }
            catch
            {
                throw;
            }
        }

        public async Task<int> CreateWithJobUpdateAsync(VehicleSaleBillHeader header)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Save Sale Bill
                _context.VehicleSaleBillHeaders.Add(header);
                await _context.SaveChangesAsync();


                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
                if (header.IsD2d == true)
                {
                    await ConfirmInvoiceAndReserveChassis(header.SaleBillNo);
                }
                else
                {
                    var invoice = new InvoiceHeader
                    {
                        InvoiceType = "Proforma Invoice",
                        ServiceType = "Vehicle Sale Bill",
                        DocumentNo = header.SaleBillNo,
                        ReferenceId = header.Id,
                        CustomerId = header.LedgerId,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        Status = "Proforma",
                        InvoiceNo = "IN-" + header.SaleBillNo,
                        DealerCode = header.DealerCode,
                    };

                    // 3. Add Invoice Details (optional but recommended)
                    foreach (var detail in header.VehicleSaleBillDetails)
                    {
                        var item = new InvoiceDetail
                        {
                            ItemId = detail.Id,
                            Description = detail.ModelName,
                            Quantity = 1,
                            Rate = detail.FinalAmount,
                            TaxPercent = detail.Igstper ?? (detail.Cgstper + detail.Sgstper),
                        };

                        item.Amount = (item.Quantity ?? 0) * (item.Rate ?? 0);

                        invoice.InvoiceDetails.Add(item);
                    }

                    // 4. Calculate totals
                    decimal total = invoice.InvoiceDetails.Sum(x => x.Amount ?? 0);
                    decimal tax = invoice.InvoiceDetails.Sum(x => (x.Amount ?? 0) * (x.TaxPercent ?? 0) / 100);

                    invoice.TotalAmount = total;
                    invoice.TaxAmount = tax;
                    invoice.NetAmount = total + tax;

                    // 5. Save Invoice
                    _context.InvoiceHeaders.Add(invoice);
                    await _context.SaveChangesAsync();
                }

                //await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return header.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> UpdateERPStatus(int id)
        {
            try
            {
                var saleBill = await _context.VehicleSaleBillHeaders.Where(i => i.Id == id).FirstOrDefaultAsync();
                //saleBill.Erpstatus = "PushedToERP";
                return await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateWithJobUpdateAsync(VehicleSaleBillHeader header, List<UpdateSaleDetailsVM>? jobUpdates, List<string> deletedChassisList)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);

                var chassisNos = jobUpdates
                    .Select(x => x.ChassisNo)
                    .Concat(deletedChassisList)
                    .Distinct()
                    .ToList();

                var chassisDetailsToUpdate = await _context.ChassisDetails
                    .Where(i => chassisNos.Contains(i.ChassisNo))
                    .ToListAsync();

                if (chassisDetailsToUpdate.Any())
                {
                    foreach (var chassis in chassisDetailsToUpdate)
                    {
                        chassis.SaleDate = header.SaleDate;
                        chassis.RegNo = jobUpdates.Where(i => i.ChassisNo == chassis.ChassisNo).Select(i => i.RegisterNo).FirstOrDefault();
                        chassis.LedgerId = header.LedgerId;
                        chassis.UpdatedBy = userId;
                        chassis.UpdatedDate = DateTime.Now;
                    }

                    _context.ChassisDetails.UpdateRange(chassisDetailsToUpdate);
                    //  await _context.SaveChangesAsync();
                }
                // 1. Update Header
                _context.VehicleSaleBillHeaders.Update(header);

                // 2. Update JobCards 
                if (jobUpdates == null && header.Status.ToLower() == "invalid")
                {
                    header.Status = "Pending";
                }

                if (jobUpdates != null)
                {

                    var allchassis = jobUpdates.Select(x => x.ChassisNo).Concat(deletedChassisList).Distinct().ToList();
                    var jobs = await _context.JobCardCustomers
                        .Where(x => allchassis.Contains(x.ChassisNo))
                        .ToListAsync();




                    foreach (var item in jobUpdates)
                    {
                        var job = jobs.Where(c => c.ChassisNo == item.ChassisNo).FirstOrDefault();

                        if (job == null) continue;

                        job.SaleDate = item.SaleDate;
                        job.InsuranceExpDate = item.InsuranceExpDate;
                        job.RegisterNo = item.RegisterNo;
                    }
                    //  header.Erpstatus = "Reserved";

                    // Mark other sale bills with same chassis as Invalid
                    var invalidSalesBills = await _context.VehicleSaleBillHeaders
                    .Include(x => x.VehicleSaleBillDetails)
                    .Where(x => x.SaleBillNo != header.SaleBillNo &&
                                x.VehicleSaleBillDetails.Any(c => allchassis.Contains(c.ChassisNo)))
                    .ToListAsync();

                    foreach (var bill in invalidSalesBills)
                    {
                        // bill.Erpstatus = "Invalid";

                        var detailsToRemove = await _context.VehicleSaleBillDetails
                             .Where(d => d.VehicleSaleBillId == bill.Id)
                                .ToListAsync();

                        _context.VehicleSaleBillDetails.RemoveRange(detailsToRemove);
                        await _context.SaveChangesAsync();

                    }
                }
                // 3. Remove from VehicleDetails
                if (deletedChassisList != null && deletedChassisList.Any())
                {
                    var vehiclesToDelete = await _context.VehicleSaleBillDetails
                        .Where(v => deletedChassisList.Contains(v.ChassisNo) && v.VehicleSaleBillId == header.Id)
                        .ToListAsync();

                    if (vehiclesToDelete.Any())
                    {
                        _context.VehicleSaleBillDetails.RemoveRange(vehiclesToDelete);
                        await _context.SaveChangesAsync();
                    }
                }


                // 3. Save
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //public async Task<int> ConfirmInvoiceAndReserveChassis(string saleBillNo)
        //{
        //    try
        //    {
        //        //  Get Sale Bill with details
        //        var saleBill = await _context.VehicleSaleBillHeaders
        //            .Include(i => i.VehicleSaleBillDetails)
        //            .FirstOrDefaultAsync(i => i.SaleBillNo == saleBillNo);
        //        var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
        //        if (saleBill == null)
        //            return 0;

        //        saleBill.SaleDate = DateTime.Now;

        //        // Get all chassis numbers
        //        var chassisNos = saleBill.VehicleSaleBillDetails
        //            .Select(d => d.ChassisNo)
        //            .ToList();

        //        // Get ItemCodes from VehicleInward using chassis
        //        var vehicleItemCodes = await _context.ChassisDetails
        //            .Where(v => v.ChassisNo != null && chassisNos.Contains(v.ChassisNo))
        //            .Select(v => v.ItemCode)
        //            .ToListAsync();
        //        var groupedItems = vehicleItemCodes
        //            .GroupBy(x => x)
        //            .Select(g => new
        //            {
        //                ItemCode = g.Key,
        //                Qty = g.Count()
        //            }).ToList();


        //        //  REDUCE STOCK
        //        foreach (var item in groupedItems)
        //        {
        //            var stockTransaction = new PartsInventory
        //            {
        //                TransId = Guid.NewGuid().ToString(),
        //                ItemCode = item.ItemCode,
        //                VoucherNo = null!,
        //                TransType = "S",
        //                BatchNo = "Batch 1",
        //                BatchTransQty = item.Qty,
        //                BatchOpeningQty = 0,
        //                BatchClosingQty = 0,
        //                TransDate = DateOnly.FromDateTime(DateTime.Now),
        //                DealerLocation = saleBill?.Location,
        //                VendorCode = saleBill.DealerCode,
        //                TotalRate = 100.00M,
        //                PurchaseRate = 110.00M,
        //                Potype = saleBill?.CustomerType,
        //                PostTransaction = 0,
        //                CreatedBy = userId,
        //                CreatedDate = DateTime.Now
        //            };

        //            await _partInventoryService.UpdateOutgoing(stockTransaction);
        //        }



        //        var jobCardList = await _context.JobCardCustomers
        //            .Where(x => chassisNos.Contains(x.ChassisNo))
        //            .ToListAsync();


        //        foreach (var job in jobCardList)
        //        {
        //            var saleDetail = saleBill.VehicleSaleBillDetails
        //                .FirstOrDefault(d => d.ChassisNo == job.ChassisNo);

        //            if (saleDetail == null) continue;

        //            job.CustomerLedgerId = saleBill.LedgerId;
        //            job.VehicleSaleBillid = saleBill.Id;
        //            job.SaleDate = saleBill.SaleDate;
        //            job.InsuranceExpDate = saleDetail.InsExpDate;
        //            job.RegisterNo = saleDetail.RegNo;
        //        }

        //        var invalidSalesBills = await _context.VehicleSaleBillHeaders
        //        .Include(x => x.VehicleSaleBillDetails)
        //        .Where(x => x.SaleBillNo != saleBillNo &&
        //                    x.VehicleSaleBillDetails.Any(c => chassisNos.Contains(c.ChassisNo)))
        //        .ToListAsync();

        //        //foreach (var bill in invalidSalesBills)
        //        //{
        //        //    //bill.Erpstatus = "Invalid";

        //        //    var detailsToRemove = await _context.VehicleSaleBillDetails
        //        //         .Where(d => d.VehicleSaleBillId == bill.Id && chassisNos.Contains(d.ChassisNo))
        //        //            .ToListAsync();

        //        //    _context.VehicleSaleBillDetails.RemoveRange(detailsToRemove);

        //        //}
        //        var existingInvoice = _context.InvoiceHeaders
        //            .FirstOrDefault(x => x.DocumentNo == saleBill.SaleBillNo);

        //        InvoiceHeader invoice;

        //        if (existingInvoice != null)
        //        {
        //            // UPDATE EXISTING INVOICE
        //            invoice = existingInvoice;

        //            invoice.InvoiceType = "Invoice";
        //            invoice.ServiceType = "Vehicle Sale Bill";
        //            invoice.CustomerId = saleBill.LedgerId;
        //            invoice.Status = "Invoiced";
        //            invoice.UpdatedBy = userId;
        //            invoice.UpdatedDate = DateTime.Now;
        //            invoice.InvoiceNo = "IN" + saleBill.SaleBillNo;
        //            _context.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
        //            invoice.InvoiceDetails.Clear();
        //        }
        //        else
        //        {
        //            // CREATE NEW INVOICE
        //            invoice = new InvoiceHeader
        //            {
        //                InvoiceType = "Invoice",
        //                ServiceType = "Vehicle Sale Bill",
        //                DocumentNo = saleBill.SaleBillNo,
        //                InvoiceNo = "IN" + saleBill.SaleBillNo,
        //                ReferenceId = saleBill.Id,
        //                CustomerId = saleBill.LedgerId,
        //                CreatedBy = userId,
        //                CreatedDate = DateTime.Now,
        //                Status = "Invoiced"
        //            };

        //            _context.InvoiceHeaders.Add(invoice);
        //        }
        //        foreach (var detail in saleBill.VehicleSaleBillDetails)
        //        {
        //            var item = new InvoiceDetail
        //            {
        //                ItemId = detail.Id,
        //                Description = detail.ModelName,
        //                Quantity = 1,
        //                Rate = detail.FinalAmount,
        //                TaxPercent = detail.Igstper ?? (detail.Cgstper + detail.Sgstper),
        //            };

        //            item.Amount = (item.Quantity ?? 0) * (item.Rate ?? 0);

        //            invoice.InvoiceDetails.Add(item);
        //        }

        //        // RECALCULATE TOTALS
        //        decimal total = invoice.InvoiceDetails.Sum(x => x.Amount ?? 0);
        //        decimal tax = invoice.InvoiceDetails.Sum(x => (x.Amount ?? 0) * (x.TaxPercent ?? 0) / 100);

        //        invoice.TotalAmount = total;
        //        invoice.TaxAmount = tax;
        //        invoice.NetAmount = total + tax;

        //        // UPDATE SALE BILL STATUS
        //        saleBill.Status = "Invoiced";

        //        var chassisDetailsToUpdate = await _context.ChassisDetails
        //            .Where(i => chassisNos.Contains(i.ChassisNo)).ToListAsync();

        //        foreach (var chassis in chassisDetailsToUpdate)
        //        {
        //            chassis.SaleDate = saleBill.SaleDate;
        //            chassis.LedgerId = saleBill.LedgerId;
        //            chassis.UpdatedBy = userId;
        //            chassis.UpdatedDate = DateTime.Now;
        //        }


        //        await _context.SaveChangesAsync();

        //        if (saleBill.IsD2d == true)
        //        {
        //            var chassises = saleBill.VehicleSaleBillDetails.Select(x => x.ChassisNo).ToList();
        //            await updateChassisForD2d(chassises, saleBill?.LedgerId, saleBill.DealerCode, saleBill.Location);

        //            var batteryDetails = await _context.ChassisBatteryDetails.Where(x => chassisNos.Contains(x.ChassisNo)).ToListAsync();

        //            var vehicleInwards = await _context.VehicleInwards.Where(x => chassisNos.Contains(x.ChasisNo) && x.DealerCode == saleBill.DealerCode).ToListAsync();

        //            var receivingDealerCode = await _context.LedgerMasters.Where(x => x.Id == saleBill.LedgerId).Select(x => x.DealerCode).FirstOrDefaultAsync();

        //            var gstNo = await _context.DealerMasters.Where(x => x.Dealercode == saleBill.DealerCode).Select(x => x.CompgstinNo).FirstOrDefaultAsync();

        //            var vehicleLookup = vehicleInwards.GroupBy(x => x.ChasisNo).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

        //            var batteryLookup = batteryDetails.GroupBy(x => x.ChassisNo).ToDictionary(x => x.Key, x => x.ToList());

        //            foreach (var item in saleBill.VehicleSaleBillDetails)
        //            {
        //                vehicleLookup.TryGetValue(item.ChassisNo, out var vehicle);

        //                batteryLookup.TryGetValue(item.ChassisNo, out var batteries);

        //                var vehicleInward = new VehicleInward
        //                {
        //                    InvoiceDate = invoice.CreatedDate.HasValue ? DateOnly.FromDateTime(invoice.CreatedDate.Value) : null,
        //                    InvoiceNo = invoice.InvoiceNo,
        //                    MfgYear = item.MfgYear,
        //                    ItemCode = item.ItemCode,
        //                    ColrCode = vehicle.ColrCode,
        //                    ChasisNo = item.ChassisNo,
        //                    MotorNo = batteries?.FirstOrDefault(x => x.MotorOrderNo == 1)?.MotorNo,
        //                    BatteryNo = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 1)?.BatteryNo,
        //                    BatteryNo2 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 2)?.BatteryNo,
        //                    BatteryNo3 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 3)?.BatteryNo,
        //                    BatteryNo4 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 4)?.BatteryNo,
        //                    BatteryNo5 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 5)?.BatteryNo,
        //                    BatteryNo6 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 6)?.BatteryNo,
        //                    EcuSerno = vehicle?.EcuSerno,
        //                    EcuImEi = vehicle?.EcuImEi,
        //                    EcuBalMac = vehicle?.EcuBalMac,
        //                    ImmoblizerStatus = vehicle?.ImmoblizerStatus,
        //                    ImmoblizerNo = vehicle?.ImmoblizerNo,
        //                    BikeSimid = vehicle?.BikeSimid,
        //                    BikeMobileno = vehicle?.BikeMobileno,
        //                    SoundbarSerno = vehicle?.SoundbarSerno,
        //                    SoundbarBalMac = vehicle?.SoundbarBalMac,
        //                    Voltage = vehicle?.Voltage,
        //                    Validity = vehicle?.Validity,
        //                    Startdate = vehicle?.Startdate,
        //                    TyreNo1 = vehicle?.TyreNo1,
        //                    TyreNo2 = vehicle?.TyreNo2,
        //                    BatteryIdno = vehicle?.BatteryIdno,
        //                    Dlrprice = vehicle?.Dlrprice,
        //                    Custprice = vehicle?.Custprice,
        //                    GstIdno = int.TryParse(gstNo, out var gst) ? gst : 0,
        //                    DealerCode = receivingDealerCode,
        //                    LocCode = $"{receivingDealerCode}S1",
        //                    BatteryChemistry = item.BatteryChemical,
        //                    BatteryCapacity = item.BatteryCapacity,
        //                    BatteryMake = item.BatteryMake,
        //                    Fame2Discount = item.FameIi,
        //                    Converter = item.ConvertorNo,
        //                    Vcu = item.Vcu,
        //                    IsAccepted = false,
        //                    PoType = saleBill.CustomerType,
        //                    IsD2d = true,
        //                    InwardType = saleBill.CustomerType,
        //                    KeyNo = item.Key,
        //                    ServBkno = item.BookNo,
        //                    BatteryId = vehicle.BatteryId,
        //                    ChargerNo = item.ChargerNo,
        //                    ControllerNo = item.ControllerNo,
        //                    Regnumber = item.RegNo,
        //                    MfgMonth = vehicle.MfgMonth,
        //                    CreatedBy = userId,
        //                    CreatedDate = DateTime.Now

        //                };

        //                _context.VehicleInwards.Add(vehicleInward);
        //            }
        //        }
        //        return saleBill.Id;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public async Task<int> ConfirmInvoiceAndReserveChassis(string saleBillNo)
        {
            try
            {
                //  Get Sale Bill with details
                var saleBill = await _context.VehicleSaleBillHeaders
                    .Include(i => i.VehicleSaleBillDetails)
                    .FirstOrDefaultAsync(i => i.SaleBillNo == saleBillNo);
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
                if (saleBill == null)
                    return 0;

                saleBill.SaleDate = DateTime.Now;

                // Get all chassis numbers
                var chassisNos = saleBill.VehicleSaleBillDetails
                    .Select(d => d.ChassisNo)
                    .ToList();

                // Get ItemCodes from VehicleInward using chassis
                var vehicleItemCodes = await _context.ChassisDetails
                    .Where(v => v.ChassisNo != null && chassisNos.Contains(v.ChassisNo))
                    .Select(v => v.ItemCode)
                    .ToListAsync();
                var groupedItems = vehicleItemCodes
                    .GroupBy(x => x)
                    .Select(g => new
                    {
                        ItemCode = g.Key,
                        Qty = g.Count()
                    }).ToList();


                //  REDUCE STOCK
                foreach (var item in groupedItems)
                {
                    var stockTransaction = new PartsInventory
                    {
                        TransId = Guid.NewGuid().ToString(),
                        ItemCode = item.ItemCode,
                        VoucherNo = null!,
                        TransType = "S",
                        BatchNo = "Batch 1",
                        BatchTransQty = item.Qty,
                        BatchOpeningQty = 0,
                        BatchClosingQty = 0,
                        TransDate = DateOnly.FromDateTime(DateTime.Now),
                        DealerLocation = saleBill?.Location,
                        VendorCode = saleBill.DealerCode,
                        TotalRate = 100.00M,
                        PurchaseRate = 110.00M,
                        Potype = saleBill?.CustomerType,
                        PostTransaction = 0,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };

                    await _partInventoryService.UpdateOutgoing(stockTransaction);
                }



                var jobCardList = await _context.JobCardCustomers
                    .Where(x => chassisNos.Contains(x.ChassisNo))
                    .ToListAsync();


                foreach (var job in jobCardList)
                {
                    var saleDetail = saleBill.VehicleSaleBillDetails
                        .FirstOrDefault(d => d.ChassisNo == job.ChassisNo);

                    if (saleDetail == null) continue;

                    job.CustomerLedgerId = saleBill.LedgerId;
                    job.VehicleSaleBillid = saleBill.Id;
                    job.SaleDate = saleBill.SaleDate;
                    job.InsuranceExpDate = saleDetail.InsExpDate;
                    job.RegisterNo = saleDetail.RegNo;
                }

                var invalidSalesBills = await _context.VehicleSaleBillHeaders
                .Include(x => x.VehicleSaleBillDetails)
                .Where(x => x.SaleBillNo != saleBillNo &&
                            x.VehicleSaleBillDetails.Any(c => chassisNos.Contains(c.ChassisNo)))
                .ToListAsync();

                var existingInvoice = await _context.InvoiceHeaders
                    .FirstOrDefaultAsync(x => x.DocumentNo == saleBill.SaleBillNo);

                InvoiceHeader invoice;

                if (existingInvoice != null)
                {
                    // UPDATE EXISTING INVOICE
                    invoice = existingInvoice;

                    invoice.InvoiceType = "Invoice";
                    invoice.ServiceType = "Vehicle Sale Bill";
                    invoice.CustomerId = saleBill.LedgerId;
                    invoice.Status = "Invoiced";
                    invoice.UpdatedBy = userId;
                    invoice.UpdatedDate = DateTime.Now;
                    invoice.InvoiceNo = "IN" + saleBill.SaleBillNo;
                    _context.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
                    invoice.InvoiceDetails.Clear();
                }
                else
                {
                    // CREATE NEW INVOICE
                    invoice = new InvoiceHeader
                    {
                        InvoiceType = "Invoice",
                        ServiceType = "Vehicle Sale Bill",
                        DocumentNo = saleBill.SaleBillNo,
                        InvoiceNo = "IN" + saleBill.SaleBillNo,
                        ReferenceId = saleBill.Id,
                        CustomerId = saleBill.LedgerId,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        Status = "Invoiced"
                    };

                    _context.InvoiceHeaders.Add(invoice);
                }
                foreach (var detail in saleBill.VehicleSaleBillDetails)
                {
                    var item = new InvoiceDetail
                    {
                        ItemId = detail.Id,
                        Description = detail.ModelName,
                        Quantity = 1,
                        Rate = detail.FinalAmount,
                        TaxPercent = detail.Igstper ?? (detail.Cgstper + detail.Sgstper),
                    };

                    item.Amount = (item.Quantity ?? 0) * (item.Rate ?? 0);

                    invoice.InvoiceDetails.Add(item);
                }

                // RECALCULATE TOTALS
                decimal total = invoice.InvoiceDetails.Sum(x => x.Amount ?? 0);
                decimal tax = invoice.InvoiceDetails.Sum(x => (x.Amount ?? 0) * (x.TaxPercent ?? 0) / 100);

                invoice.TotalAmount = total;
                invoice.TaxAmount = tax;
                invoice.NetAmount = total + tax;

                // UPDATE SALE BILL STATUS
                saleBill.Status = "Invoiced";

                var chassisDetailsToUpdate = await _context.ChassisDetails
                    .Where(i => chassisNos.Contains(i.ChassisNo)).ToListAsync();

                foreach (var chassis in chassisDetailsToUpdate)
                {
                    chassis.SaleDate = saleBill.SaleDate;
                    chassis.LedgerId = saleBill.LedgerId;
                    chassis.UpdatedBy = userId;
                    chassis.UpdatedDate = DateTime.Now;
                }


                await _context.SaveChangesAsync();

                if (saleBill.IsD2d == true)
                {
                    var chassises = saleBill.VehicleSaleBillDetails.Select(x => x.ChassisNo).ToList();
                    await updateChassisForD2d(chassises, saleBill?.LedgerId, saleBill.DealerCode, saleBill.Location);

                    var batteryDetails = await _context.ChassisBatteryDetails.Where(x => chassisNos.Contains(x.ChassisNo)).ToListAsync();

                    var vehicleInwards = await _context.VehicleInwards.Where(x => chassisNos.Contains(x.ChasisNo) && x.DealerCode == saleBill.DealerCode).ToListAsync();

                    var receivingDealerCode = await _context.LedgerMasters.Where(x => x.Id == saleBill.LedgerId).Select(x => x.DealerCode).FirstOrDefaultAsync();

                    var gstNo = await _context.DealerMasters.Where(x => x.Dealercode == saleBill.DealerCode).Select(x => x.CompgstinNo).FirstOrDefaultAsync();

                    var vehicleLookup = vehicleInwards.GroupBy(x => x.ChasisNo).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

                    var batteryLookup = batteryDetails.GroupBy(x => x.ChassisNo).ToDictionary(x => x.Key, x => x.ToList());

                    foreach (var item in saleBill.VehicleSaleBillDetails)
                    {
                        vehicleLookup.TryGetValue(item.ChassisNo, out var vehicle);

                        batteryLookup.TryGetValue(item.ChassisNo, out var batteries);

                        var vehicleInward = new VehicleInward
                        {
                            InvoiceDate = invoice.CreatedDate.HasValue ? DateOnly.FromDateTime(invoice.CreatedDate.Value) : null,
                            InvoiceNo = invoice.InvoiceNo,
                            MfgYear = item.MfgYear,
                            ItemCode = item.ItemCode,
                            // FIXED: was `vehicle.ColrCode` — threw NullReferenceException
                            // whenever this chassis has no VehicleInward row yet for
                            // saleBill.DealerCode (vehicleLookup miss -> vehicle == null).
                            ColrCode = vehicle?.ColrCode,
                            ChasisNo = item.ChassisNo,
                            MotorNo = batteries?.FirstOrDefault(x => x.MotorOrderNo == 1)?.MotorNo,
                            BatteryNo = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 1)?.BatteryNo,
                            BatteryNo2 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 2)?.BatteryNo,
                            BatteryNo3 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 3)?.BatteryNo,
                            BatteryNo4 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 4)?.BatteryNo,
                            BatteryNo5 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 5)?.BatteryNo,
                            BatteryNo6 = batteries?.FirstOrDefault(x => x.BatteryOrderNo == 6)?.BatteryNo,
                            EcuSerno = vehicle?.EcuSerno,
                            EcuImEi = vehicle?.EcuImEi,
                            EcuBalMac = vehicle?.EcuBalMac,
                            ImmoblizerStatus = vehicle?.ImmoblizerStatus,
                            ImmoblizerNo = vehicle?.ImmoblizerNo,
                            BikeSimid = vehicle?.BikeSimid,
                            BikeMobileno = vehicle?.BikeMobileno,
                            SoundbarSerno = vehicle?.SoundbarSerno,
                            SoundbarBalMac = vehicle?.SoundbarBalMac,
                            Voltage = vehicle?.Voltage,
                            Validity = vehicle?.Validity,
                            Startdate = vehicle?.Startdate,
                            TyreNo1 = vehicle?.TyreNo1,
                            TyreNo2 = vehicle?.TyreNo2,
                            BatteryIdno = vehicle?.BatteryIdno,
                            Dlrprice = vehicle?.Dlrprice,
                            Custprice = vehicle?.Custprice,
                            GstIdno = int.TryParse(gstNo, out var gst) ? gst : 0,
                            DealerCode = receivingDealerCode,
                            LocCode = $"{receivingDealerCode}S1",
                            BatteryChemistry = item.BatteryChemical,
                            BatteryCapacity = item.BatteryCapacity,
                            BatteryMake = item.BatteryMake,
                            Fame2Discount = item.FameIi,
                            Converter = item.ConvertorNo,
                            Vcu = item.Vcu,
                            IsAccepted = false,
                            PoType = saleBill.CustomerType,
                            IsD2d = true,
                            InwardType = saleBill.CustomerType,
                            KeyNo = item.Key,
                            ServBkno = item.BookNo,
                            // FIXED: was `vehicle.BatteryId` — same null-reference risk.
                            BatteryId = vehicle?.BatteryId,
                            ChargerNo = item.ChargerNo,
                            ControllerNo = item.ControllerNo,
                            Regnumber = item.RegNo,
                            // FIXED: was `vehicle.MfgMonth` — same null-reference risk.
                            MfgMonth = vehicle?.MfgMonth,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now

                        };

                        _context.VehicleInwards.Add(vehicleInward);
                    }
                }
                return saleBill.Id;
            }
            catch
            {
                throw;
            }
        }

        //private async Task updateChassisForD2d(List<string> chassises, int? ledgerId, string dealerCode, string locationCode)
        //{
        //    try
        //    {
        //        var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
        //        var receivingDealerCode = await _context.LedgerMasters.Where(i => i.Id == ledgerId).FirstOrDefaultAsync();

        //        if (chassises == null || !chassises.Any())
        //            return;

        //        var chassisToMove = await _context.ChassisDetails
        //            .Where(x => chassises.Contains(x.ChassisNo))
        //            .ToListAsync();

        //        if (!chassisToMove.Any())
        //            return;

        //        var historyRecords = chassisToMove.Select(x => new ChassisDetailsD2dhistory
        //        {
        //            LedgerId = x.LedgerId,
        //            ChassisNo = x.ChassisNo,
        //            ItemCode = x.ItemCode,
        //            ItemName = x.ItemName,
        //            ItemColor = x.ItemColor,
        //            DealerCode = receivingDealerCode.DealerCode,
        //            LocationCode = receivingDealerCode.DealerCode + "S1",
        //            IssueingDealerCode = dealerCode,
        //            IssueingDealerLocation = locationCode,
        //            SaleDate = DateTime.Now,
        //            TransDate = DateTime.Now,
        //            CreatedBy = userId,
        //            CreatedDate = DateTime.Now,
        //            UpdatedBy = x.UpdatedBy,
        //            UpdatedDate = x.UpdatedDate
        //        }).ToList();

        //        await _context.ChassisDetailsD2dhistories.AddRangeAsync(historyRecords);

        //        _context.ChassisDetails.RemoveRange(chassisToMove);

        //        var newChassisRecords = chassisToMove.Select(x => new ChassisDetail
        //        {
        //            LedgerId = null,
        //            ChassisNo = x.ChassisNo,
        //            ItemCode = x.ItemCode,
        //            ItemName = x.ItemName,
        //            ItemColor = x.ItemColor,
        //            DealerId = receivingDealerCode.DealerCode,
        //            LocationCode = receivingDealerCode.DealerCode + "S1",
        //            SaleDate = null,
        //            CreatedBy = userId,
        //            CreatedDate = DateTime.Now,
        //            UpdatedBy = null,
        //            UpdatedDate = null,
        //            RegNo = null
        //        }).ToList();

        //        await _context.ChassisDetails.AddRangeAsync(newChassisRecords);

        //        await _context.SaveChangesAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        private async Task updateChassisForD2d(List<string> chassises, int? ledgerId, string dealerCode, string locationCode)
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);

                var receivingDealer = await _context.LedgerMasters
                    .FirstOrDefaultAsync(i => i.Id == ledgerId);

                if (receivingDealer == null || chassises == null || !chassises.Any())
                    return;

                var uniqueChassisNos = chassises
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                var chassisToMove = await _context.ChassisDetails
                    .Where(x => uniqueChassisNos.Contains(x.ChassisNo))
                    .GroupBy(x => x.ChassisNo)
                    .Select(g => g.First())
                    .ToListAsync();

                if (!chassisToMove.Any())
                    return;

                var historyRecords = chassisToMove.Select(x => new ChassisDetailsD2dhistory
                {
                    LedgerId = x.LedgerId,
                    ChassisNo = x.ChassisNo,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    ItemColor = x.ItemColor,
                    DealerCode = receivingDealer.DealerCode,
                    LocationCode = receivingDealer.DealerCode + "S1",
                    IssueingDealerCode = dealerCode,
                    IssueingDealerLocation = locationCode,
                    SaleDate = DateTime.Now,
                    TransDate = DateTime.Now,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedDate = x.UpdatedDate
                }).ToList();

                await _context.ChassisDetailsD2dhistories.AddRangeAsync(historyRecords);

                _context.ChassisDetails.RemoveRange(chassisToMove);

                var newChassisRecords = chassisToMove
                    .GroupBy(x => x.ChassisNo)
                    .Select(g => g.First())
                    .Select(x => new ChassisDetail
                    {
                        LedgerId = null,
                        ChassisNo = x.ChassisNo,
                        ItemCode = x.ItemCode,
                        ItemName = x.ItemName,
                        ItemColor = x.ItemColor,
                        DealerId = receivingDealer.DealerCode,
                        LocationCode = receivingDealer.DealerCode + "S1",
                        SaleDate = null,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = null,
                        UpdatedDate = null,
                        RegNo = null
                    })
                    .ToList();

                await _context.ChassisDetails.AddRangeAsync(newChassisRecords);

                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> UpdateVehicleInward(int saleBillId)
        {
            try
            {
                var data = await (from d in _context.VehicleSaleBillDetails
                                  join h in _context.VehicleSaleBillHeaders on d.VehicleSaleBillId equals h.Id
                                  join i in _context.InvoiceHeaders on d.Id equals i.ReferenceId
                                  join vi in _context.VehicleInwards on d.ChassisNo equals vi.ChasisNo
                                  join cbd in _context.ChassisBatteryDetails.Where(x => x.MotorOrderNo == 1 && x.BatteryOrderNo == 1 && x.ChargerOrderNo == 1 && x.ControllerOrderNo == 1 && x.ConverterOrderNo == 1) on d.ChassisNo equals cbd.ChassisNo
                                  join dlr in _context.DealerMasters on h.DealerCode equals dlr.Dealercode into dealInfo
                                  join inDelaer in _context.LedgerMasters on h.LedgerId equals inDelaer.Id
                                  join inDlrCode in _context.DealerMasters on inDelaer.DealerCode equals inDlrCode.Dealercode
                                  from dlr in dealInfo.DefaultIfEmpty()
                                  where d.VehicleSaleBillId == saleBillId && vi.DealerCode == h.DealerCode
                                  select new
                                  {
                                      SaleHeader = h,
                                      Detail = d,
                                      Invoice = i,
                                      VehicleInward = vi,
                                      ChassisBatteryDetail = cbd,
                                      IssuingDealer = dlr,
                                      ReceivingDealer = inDlrCode
                                  }).ToListAsync();

                foreach (var item in data)
                {
                    var invoice = item.Invoice;
                    var vehicleInward = new VehicleInward
                    {
                        //Id = -1,
                        InvoiceDate = invoice.CreatedDate.HasValue ? DateOnly.FromDateTime(invoice.CreatedDate.Value) : null,
                        InvoiceNo = invoice.InvoiceNo,
                        MfgYear = item.VehicleInward.MfgYear,
                        ItemCode = item.VehicleInward.ItemCode,
                        ColrCode = item.VehicleInward.ColrCode,
                        ChasisNo = item.Detail.ChassisNo,
                        MotorNo = item.ChassisBatteryDetail.MotorNo,
                        KeyNo = item?.Detail.Key,
                        ServBkno = item?.Detail.BookNo,
                        BatteryId = item.ChassisBatteryDetail.BatteryNo,
                        BatteryNo = item.ChassisBatteryDetail.BatteryNo,
                        BatteryNo2 = "",
                        BatteryNo3 = "",
                        BatteryNo4 = "",
                        BatteryNo5 = item.VehicleInward.BatteryNo5,
                        BatteryNo6 = item.VehicleInward.BatteryNo6,
                        EcuSerno = item.VehicleInward.EcuSerno,
                        EcuImEi = item.VehicleInward.EcuImEi,
                        EcuBalMac = item.VehicleInward.EcuBalMac,
                        ImmoblizerStatus = item.VehicleInward.ImmoblizerStatus,
                        ImmoblizerNo = item.VehicleInward.ImmoblizerNo,
                        BikeSimid = item.VehicleInward.BikeSimid,
                        BikeMobileno = item.VehicleInward.BikeMobileno,
                        ChargerNo = item.Detail.ChargerNo,
                        ControllerNo = item.Detail.ControllerNo,
                        SoundbarSerno = item.VehicleInward.SoundbarSerno,
                        SoundbarBalMac = item.VehicleInward.SoundbarBalMac,
                        Voltage = item.VehicleInward.Voltage,
                        Regnumber = item.Detail.RegNo,
                        Validity = item.VehicleInward.Validity,
                        Startdate = item.VehicleInward.Startdate,
                        TyreNo1 = item.VehicleInward.TyreNo1,
                        TyreNo2 = item.VehicleInward.TyreNo2,
                        GstIdno = int.Parse(item.IssuingDealer.CompgstinNo),
                        LocCode = item.ReceivingDealer.Dealercode + "S1",
                        DealerCode = item.ReceivingDealer.Dealercode,
                        BatteryChemistry = item.Detail.BatteryChemical,
                        BatteryCapacity = item.Detail.BatteryCapacity,
                        BatteryMake = item.Detail.BatteryMake,
                        BatteryIdno = item.VehicleInward.BatteryIdno,
                        Fame2Discount = item.Detail.FameIi,
                        Converter = item.Detail.ConvertorNo,
                        Vcu = item.Detail.Vcu,
                        Ordertype = "",
                        MfgMonth = item.VehicleInward.MfgMonth,
                        IsAccepted = false,
                        Dlrprice = item.VehicleInward.Dlrprice,
                        Custprice = item.VehicleInward.Custprice,
                        PoType = "D2D",
                        IsD2d = true,
                        InwardType = item.SaleHeader.CustomerType
                    };
                    _context.VehicleInwards.Add(vehicleInward);

                }
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<VehicleSaleBillHeader> UpdateRegistrationAndReserveChassis(string? saleBillNo, List<UpdateSaleDetailsVM> updateSaleDetails)
        {
            try
            {

                var saleBill = await _context.VehicleSaleBillHeaders
                    .Include(i => i.VehicleSaleBillDetails)
                    .Where(i => i.SaleBillNo == saleBillNo).FirstOrDefaultAsync();

                var allocatedChassis = updateSaleDetails.Select(i => i.ChassisNo).ToList();

                var jobCardsToUpdate = await _context.JobCardCustomers
                    .Where(x => allocatedChassis.Contains(x.ChassisNo)).ToListAsync();
                foreach (var item in updateSaleDetails)
                {
                    var job = jobCardsToUpdate.Where(c => c.ChassisNo == item.ChassisNo).FirstOrDefault();

                    if (job == null) continue;

                    job.SaleDate = item.SaleDate;
                    job.InsuranceExpDate = item.InsuranceExpDate;
                    job.RegisterNo = item.RegisterNo;

                }
                await _context.SaveChangesAsync();
                return saleBill;
            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<ChassisListWithPDIStatus>> GetAllChassissListWithPDISatatus(string? dealerCode)
        //{
        //    try
        //    {
        //        var result = await (
        //        from vi in _context.VehicleInwards
        //        join ch in _context.ChassisDetails on vi.ChasisNo equals ch.ChassisNo
        //        join im in _context.ItemMasters on vi.ItemCode equals im.Itemcode
        //        join clr in _context.ColorMasters on vi.ColrCode equals clr.Colorcode

        //        join jc in _context.JobCardHeaders
        //            on ch.ChassisNo equals jc.Chassisno into jcGroup
        //        from jc in jcGroup.DefaultIfEmpty()

        //        join vd in _context.VehicleSaleBillDetails
        //            on ch.ChassisNo equals vd.ChassisNo into vdGroup
        //        from vd in vdGroup.DefaultIfEmpty()

        //        join vh in _context.VehicleSaleBillHeaders
        //            on vd.VehicleSaleBillId equals vh.Id into vhGroup
        //        from vh in vhGroup.DefaultIfEmpty()

        //        where ch.SaleDate == null && (vh == null || vh.IsD2d == false)

        //        select new
        //        {
        //            ch.ChassisNo,
        //            Data = new ChassisListWithPDIStatus
        //            {
        //                ChassisNo = ch.ChassisNo,
        //                ItemCode = ch.ItemCode,
        //                ItemColor = clr.Colorname,
        //                MfgYear = vi.MfgYear,
        //                ItemName = im.Itemname,
        //                KeyNo = vi.KeyNo,
        //                BookNo = vi.ServBkno,
        //                BatteryNo = vi.BatteryNo,
        //                BatteryChemical = vi.BatteryChemistry,
        //                BatteryCapacity = vi.BatteryCapacity,
        //                BatteryMake = vi.BatteryMake,
        //                ChargerNo = vi.ChargerNo,
        //                ControllerNo = vi.ControllerNo,
        //                FameIIAmnt = vi.Fame2Discount,
        //                DealerPrice = vi.Dlrprice,
        //                CustomerPrice = vi.Custprice,
        //                DealerCode = ch.DealerId,
        //                CustomerSaleDate = ch.SaleDate,
        //                ProformaCreated = vd == null ? null : vh.SaleBillNo,
        //                PDIStatus = jc == null ? "Not Done" : (jc.IsPdiSuccess == true ? "OK" : "Not Done"),
        //                LocationCode = ch.LocationCode
        //            }
        //        })
        //        .GroupBy(x => x.ChassisNo)
        //        .Select(g => g.First().Data)
        //        .ToListAsync();

        //        if (!string.IsNullOrEmpty(dealerCode))
        //        {
        //            result = result.Where(i => i.DealerCode == dealerCode).ToList();
        //        }
        //        return result;
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //}

        public async Task<List<ChassisListWithPDIStatus>> GetAllChassissListWithPDISatatus(string? dealerCode)
        {
            try
            {
                // Get latest VehicleInward per ChassisNo + DealerCode
                var latestVehicleInwardIds = await _context.VehicleInwards
                    .GroupBy(x => new { x.ChasisNo, x.DealerCode })
                    .Select(g => g
                        .OrderByDescending(x => x.CreatedDate)
                        .Select(x => x.Id)
                        .First())
                    .ToListAsync();

                var result = await (
                    from vi in _context.VehicleInwards
                    join ch in _context.ChassisDetails
                        on vi.ChasisNo equals ch.ChassisNo
                     join li in _context.LotinspectionHeaders
                        on vi.InvoiceNo equals li.InvoiceNo 
                    join im in _context.ItemMasters
                        on vi.ItemCode equals im.Itemcode
                    join cbd in _context.ChassisBatteryDetails.Where(x => x.MotorOrderNo == 1 &&
                                x.BatteryOrderNo == 1 && x.ChargerOrderNo == 1 && x.ControllerOrderNo == 1 &&
                                x.ConverterOrderNo == 1) on ch.ChassisNo equals cbd.ChassisNo into ChassisBatteryInfo
                    from cbd in ChassisBatteryInfo.DefaultIfEmpty()

                    join clr in _context.ColorMasters
                        on vi.ColrCode equals clr.Colorcode

                    join jc in _context.JobCardHeaders
                        on ch.ChassisNo equals jc.Chassisno into jcGroup
                    from jc in jcGroup.DefaultIfEmpty()

                    join rp in _context.RepairBillHeaders
                       on jc.Id equals rp.JobId into repairGroup
                    from rp in repairGroup.DefaultIfEmpty()

                    join vd in _context.VehicleSaleBillDetails
                        on ch.ChassisNo equals vd.ChassisNo into vdGroup
                    from vd in vdGroup.DefaultIfEmpty()

                    join vh in _context.VehicleSaleBillHeaders
                        on vd.VehicleSaleBillId equals vh.Id into vhGroup
                    from vh in vhGroup.DefaultIfEmpty()

                    where latestVehicleInwardIds.Contains(vi.Id)
                          && ch.SaleDate == null && li.IsLotInspected == true
                          && (vh == null || vh.IsD2d == true || vh.Status == "PerformaCreated" || vh.IsDeleted == true)
                          && (string.IsNullOrEmpty(dealerCode) || vi.DealerCode == dealerCode)

                    select new
                    {
                        ch.ChassisNo,
                        Data = new ChassisListWithPDIStatus
                        {
                            ChassisNo = ch.ChassisNo,
                            ItemCode = ch.ItemCode,
                            ItemColor = clr.Colorname,
                            MfgYear = vi.MfgYear,
                            ItemName = im.Itemname,
                            KeyNo = vi.KeyNo,
                            BookNo = vi.ServBkno,
                            BatteryNo = cbd.BatteryNo,
                            BatteryChemical = cbd.BatteryChemical,
                            BatteryCapacity = cbd.BatteryCapacity,
                            BatteryMake = cbd.BatteryMake,
                            ChargerNo = cbd.ChargerNo,
                            ControllerNo = cbd.ControllerNo,
                            MotorNo =cbd.MotorNo,
                            FameIIAmnt = vi.Fame2Discount,
                            DealerPrice = vi.Dlrprice,
                            CustomerPrice = vi.Custprice,
                            DealerCode = ch.DealerId,
                            CustomerSaleDate = ch.SaleDate,
                            ProformaCreated = vh != null && !vh.IsDeleted ? vh.SaleBillNo : null,
                            PDIStatus = jc != null && jc.IsPdiSuccess == true ? "OK" : "Not Done",
                            RepairBillStatus = rp != null && rp.RepairbillStatus == "Billed"? "OK" : "Not Done",
                            LocationCode = ch.LocationCode,
                            IsD2D = vh.IsD2d

                        }
                    })
                    .GroupBy(x => x.ChassisNo)
                    .Select(g => g.First().Data)
                    .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<VehicleSaleExportViewModel?> GetExportDetails(string dealerCode, int id)
        {
            try
            {
                var data = await (
                    from vd in _context.VehicleSaleBillDetails

                    join vh in _context.VehicleSaleBillHeaders
                        on vd.VehicleSaleBillId equals vh.Id

                    join vi in _context.VehicleInwards
                        on vd.ChassisNo equals vi.ChasisNo

                    join c in _context.LedgerMasters
                        on vh.LedgerId equals c.Id into customerInfo
                    from c in customerInfo.DefaultIfEmpty()

                    join invoice in _context.InvoiceHeaders
                        on vd.Id equals invoice.ReferenceId into invoiceInfo
                    from invoice in invoiceInfo.DefaultIfEmpty()

                    join city in _context.Cities
                        on c.City equals city.CityId into cityInfo
                    from city in cityInfo.DefaultIfEmpty()

                    join state in _context.States
                        on c.State equals state.StateId into stateInfo
                    from state in stateInfo.DefaultIfEmpty()

                    join location in _context.LocationMasters
                        on vh.Location equals location.Loccode into locationInfo
                    from location in locationInfo.DefaultIfEmpty()

                    join im in _context.ItemMasters
                    on vd.ItemCode equals im.Itemcode into ItemInfo
                    from im in ItemInfo.DefaultIfEmpty()

                    join f in _context.LedgerMasters
                    on vh.Financier equals f.Id into financierInfo
                    from f in financierInfo.DefaultIfEmpty()

                    where vh.Id == id


                    select new
                    {
                        vd,
                        vh,
                        vi,
                        c,
                        invoice,
                        city,
                        state,
                        location,
                        f,
                        im
                    }
                ).ToListAsync();

                if (!data.Any())
                    return null;

                var first = data.First();

                return new VehicleSaleExportViewModel
                {
                    User = new UserViewModel
                    {
                        Mobile = first.c?.MobileNumber,
                        FirstName = first.c?.LedgerName,
                        EmailId = first.c?.EMail,
                        DateOfBirth = first.c?.DateOfBirth?.ToString("yyyy-MM-dd"),
                        DateOfAnniversary = "",
                        Id = first.c?.Id.ToString(),
                        Address1 = first.c?.Address,
                        Address2 = "",
                        City = first.city?.CityName,
                        State = first.state?.StateName
                    },

                    Vehicle = data.DistinctBy(x => x.vi.ChasisNo).Select(x => new VehicleViewModel
                    {
                        ChassisNo = x.vi?.ChasisNo,
                        ModelId = x.vd?.ItemCode,
                        MotorId = x.vi?.MotorNo,
                        MotorControllerNo = x.vi?.ControllerNo,
                        EcuSerialNo = x.vi?.EcuSerno,
                        EcuImeiNo = x.vi?.EcuImEi,
                        EcuBlMac = x.vi?.EcuBalMac,
                        BikeSimId = x.vi?.BikeSimid,
                        BikeMobileNo = x.vi?.BikeMobileno,
                        SoundSerialNo = x.vi?.SoundbarSerno,
                        SoundBleMac = x.vi?.SoundbarBalMac,
                        BatteryId = x.vi?.BatteryId,
                        BatterySerialNo = x.vi?.BatteryNo,

                        RegNumber = x.vd?.RegNo,
                        Validity = "",
                        StartDate = x.vh?.SaleDate.ToShortDateString(),

                        DealerCode = dealerCode,
                        VendorIdNo = "",

                        SaleBillNo = x.vh?.SaleBillNo,
                        SaleBillDate = x.vh?.SaleDate.ToString("yyyy-MM-dd"),

                        ReceiptGuid = "",
                        ItemModel = x.vd?.ItemCode,

                        LocationName = x.location?.Locname,

                        Id = x.vd.Id.ToString(),
                        CustId = x.c?.Id.ToString(),

                        DeletedOn = "",
                        InvoiceDate = x.invoice?.CreatedDate.ToString(),
                        InvoiceNo = x.invoice?.InvoiceNo,

                        LocCode = x.location?.Loccode,
                        LocationCity = x.location?.City,
                        Pin = x.location?.Pincode,


                        AccountType = x.c?.LedgerType,
                        OEMModel = x.im?.Oemmodelname,
                        Group1 = x.im?.Grpidno.ToString(),
                        HSNSACCode = x.im?.Hsncode,

                        SaleType = x.vh?.SaleType,
                        FinancedBy = x.f?.LedgerName,


                        ItemRate = x.vd?.ItemRate,
                        InsuAmnt = x.vd?.InsuranceAmount,
                        RegnAmnt = x.vd?.RegAmount,
                        RegDiscAmnt = x.vd?.PostGstDisc,
                        DiscountType = 0,

                        FameII = x.vd?.FameIi,
                        StateFameII = 0,

                        SGSTPer = x.vd?.Sgstper,
                        SGSTAmnt = x.vd?.Sgstamnt,
                        CGSTPer = x.vd?.Cgstper,
                        CGSTAmnt = x.vd?.Cgstamnt,
                        IGSTPer = x.vd?.Igstper,
                        IGSTAmnt = x.vd?.Igstamnt,

                        NetAmnt = x.vd?.FinalAmount,

                        BatteryChemical = x.vi?.BatteryChemistry,
                        BatteryCapacity = x.vi?.BatteryCapacity,
                        BatteryMake = x.vi?.BatteryMake,

                        ChargerNo = x.vi?.ChargerNo,
                        Converter = x.vi?.Converter,
                        VCU = x.vi?.Vcu,
                        FameIIRequired = x.vi?.Fame2Discount > 0 ? "Yes" : "No",

                        SegmentName = x.vd?.Segment,
                        InstitutionalName = x.vd?.InstitutionalType,
                        SchemeName = x.vd?.SchemeName,
                    }).ToList()
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<VehicleSaleBillDeletionChecksViewModel>> GetVehicleDeletionPreRequisiteCheck(int saleBillId)
        {
            try
            {
                var saleBill = await _context.VehicleSaleBillHeaders
                    .Include(x => x.VehicleSaleBillDetails)
                    .FirstOrDefaultAsync(x => x.Id == saleBillId);
                InvoiceHeader? invoice = null;
                VehicleInward? inward = null;

                if (saleBill == null)
                    return new List<VehicleSaleBillDeletionChecksViewModel>();
                if (saleBill.IsD2d == true)
                {
                    invoice = await _context.InvoiceHeaders.Where(i => i.DocumentNo == saleBill.SaleBillNo && i.ReferenceId == saleBill.Id).FirstOrDefaultAsync();
                    inward = await _context.VehicleInwards.Where(i => i.InvoiceNo == invoice.InvoiceNo).FirstOrDefaultAsync();
                }

                var chassisList = saleBill.VehicleSaleBillDetails
                    .Select(x => x.ChassisNo)
                    .Distinct()
                    .ToList();

                var jobCards = await _context.JobCardHeaders
                    .Where(x => chassisList.Contains(x.Chassisno) && x.Jobtype != 1)
                    .ToListAsync();

                var chassisDetails = await _context.ChassisDetails
                    .Where(x => chassisList.Contains(x.ChassisNo))
                    .ToListAsync();

                var result = chassisList.Select(chassis =>
                {
                    var jobCard = jobCards.FirstOrDefault(x => x.Chassisno == chassis);
                    var chassisDetail = chassisDetails.FirstOrDefault(x => x.ChassisNo == chassis);


                    return new VehicleSaleBillDeletionChecksViewModel
                    {
                        ChassisNo = chassis,
                        JobCardNumber = jobCard != null ? $"{jobCard.Jobprefix}{jobCard.JobNo}" : null,
                        IsJobCardOpened = jobCard != null,
                        IsVDNDone = !string.IsNullOrEmpty(chassisDetail?.RegNo),
                        IsAccepted = inward?.IsAccepted ?? false,
                    };
                }).ToList();

                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<string>> GetPolicyNo(string chassisNo)
        {
            try
            {
                var policyNos = await _context.VehicleSaleBillDetails
                    .Where(d => d.ChassisNo == chassisNo && d.InsNo != null)
                    .Select(d => d.InsNo!)
                    .ToListAsync();
                return policyNos;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Form22PdfModel?> GetForm22DataAsync(int id)
        {
            try
            {
                var rows = await (
                    from vd in _context.VehicleSaleBillDetails

                    join vh in _context.VehicleSaleBillHeaders
                        on vd.VehicleSaleBillId equals vh.Id

                    join vi in _context.VehicleInwards
                        on vd.ChassisNo equals vi.ChasisNo into viJoin
                    from vi in viJoin.DefaultIfEmpty()

                    join im in _context.ItemMasters
                        on vd.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join clr in _context.ColorMasters
                        on vi.ColrCode equals clr.Colorcode into clrJoin
                    from clr in clrJoin.DefaultIfEmpty()

                    join dm in _context.DealerMasters
                        on vh.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    join cust in _context.LedgerMasters
                        on vh.LedgerId equals cust.Id into custJoin
                    from cust in custJoin.DefaultIfEmpty()

                    where vh.Id == id

                    select new
                    {
                        vd,
                        vh,
                        vi,
                        ItemName = im != null ? im.Itemname : null,
                        OemModel = im != null ? im.Oemmodelname : null,
                        ColorName = clr != null ? clr.Colorname : null,
                        Dealer = dm,
                        Customer = cust
                    }
                ).ToListAsync();

                if (rows.Count == 0)
                    return null;

                // Resolve Form22Master per OEM model (normalized) in memory.
                var formMasters = await _context.Form22Masters.ToListAsync();
                var formLookup = formMasters
                    .GroupBy(f => (f.OemModelName ?? "").Trim().ToLower())
                    .ToDictionary(g => g.Key, g => g.First());

                var first = rows.First();
                var h = first.vh;
                var dealer = first.Dealer;
                var customer = first.Customer;

                var model = new Form22PdfModel
                {
                    DealerName = dealer?.Compname,
                    DealerAddress = dealer != null
                        ? $"{dealer.Adress1} {dealer.Adress2}, {dealer.City}, {dealer.State} - {dealer.Pin}".Trim()
                        : null,
                    DealerPhone = string.IsNullOrWhiteSpace(dealer?.PhoneOff) ? dealer?.Mobile : dealer?.PhoneOff,
                    DealerGstin = dealer?.CompgstinNo,
                    DealerTradeCertNo = dealer?.TradCert,

                    SaleBillNo = h.SaleBillNo,
                    SaleDate = h.SaleDate,

                    OwnerName = h.CustomerName ?? customer?.LedgerName,
                    OwnerAddress = customer?.Address,
                    OwnerMobile = customer?.MobileNumber
                };

                int sr = 1;
                foreach (var r in rows)
                {
                    var key = (r.OemModel ?? "").Trim().ToLower();
                    formLookup.TryGetValue(key, out var f2);

                    model.Vehicles.Add(new Form22VehicleLine
                    {
                        SrNo = sr++,
                        MakeModel = r.ItemName ?? r.vd.ModelName,
                        OemModelName = r.OemModel,
                        ChassisNo = r.vd.ChassisNo,
                        MotorNo = r.vi?.MotorNo,
                        MfgYear = r.vd.MfgYear ?? r.vi?.MfgYear,
                        Colour = r.ColorName ?? r.vd.Colour,

                        // Form22Master fields (ToString keeps it safe whether the
                        // master columns are string or numeric)
                        TypeApprovalCertNo = f2?.ApprovalCertificateNo,
                        Emission = "",
                        SoundLevelHorn = f2?.SoundLevelHorn?.ToString(),
                        NoiseLevel = f2?.PassbyNoiseLevel?.ToString(),

                        BatteryNo = r.vi?.BatteryNo ?? r.vd.Battery,
                        BatteryMake = r.vi?.BatteryMake ?? r.vd.BatteryMake,
                        BatteryCapacity = r.vi?.BatteryCapacity ?? r.vd.BatteryCapacity,
                        ChargerNo = r.vi?.ChargerNo ?? r.vd.ChargerNo,
                        ControllerNo = r.vi?.ControllerNo ?? r.vd.ControllerNo
                    });
                }

                return model;
            }
            catch
            {
                throw;
            }
        }

        public async Task<ProformaInvoicePdfModel?> GetProformaInvoiceDataAsync(int id)
        {
            try
            {
                var rows = await (
                    from vd in _context.VehicleSaleBillDetails

                    join vh in _context.VehicleSaleBillHeaders
                        on vd.VehicleSaleBillId equals vh.Id

                    join vi in _context.VehicleInwards
                        on vd.ChassisNo equals vi.ChasisNo into viJoin
                    from vi in viJoin.DefaultIfEmpty()

                    join im in _context.ItemMasters
                        on vd.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join dm in _context.DealerMasters
                        on vh.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    join cust in _context.LedgerMasters
                        on vh.LedgerId equals cust.Id into custJoin
                    from cust in custJoin.DefaultIfEmpty()

                    join fin in _context.LedgerMasters
                        on vh.Financier equals fin.Id into finJoin
                    from fin in finJoin.DefaultIfEmpty()

                    join city in _context.Cities
                        on cust.City equals city.CityId into cityJoin
                    from city in cityJoin.DefaultIfEmpty()

                    join state in _context.States
                        on cust.State equals state.StateId into stateJoin
                    from state in stateJoin.DefaultIfEmpty()

                    where vh.Id == id

                    select new
                    {
                        vd,
                        vh,
                        vi,
                        ItemName = im != null ? im.Itemname : null,
                        Hsn = im != null ? im.Hsncode : null,
                        Dealer = dm,
                        Customer = cust,
                        Financier = fin,
                        CityName = city != null ? city.CityName : null,
                        StateName = state != null ? state.StateName : null
                    }
                ).ToListAsync();

                if (rows.Count == 0)
                    return null;

                var inv = await _context.InvoiceHeaders
                    .Where(i => i.ReferenceId == id)
                    .OrderBy(i => i.Id)
                    .FirstOrDefaultAsync();

                var first = rows.First();
                var h = first.vh;
                var dealer = first.Dealer;
                var customer = first.Customer;
                var financier = first.Financier;

                var model = new ProformaInvoicePdfModel
                {
                    DealerName = dealer?.Compname,
                    DealerAddress = dealer != null
                        ? $"{dealer.Adress1} {dealer.Adress2}, {dealer.City}, {dealer.State} - {dealer.Pin}".Trim()
                        : null,
                    DealerPhone = string.IsNullOrWhiteSpace(dealer?.PhoneOff) ? dealer?.Mobile : dealer?.PhoneOff,
                    DealerEmail = dealer?.Email,
                    DealerGstin = dealer?.CompgstinNo,
                    DealerPan = dealer?.Pan,
                    DealerTradeCertNo = dealer?.TradCert,

                    ProformaNo = inv?.InvoiceNo ?? h.SaleBillNo,
                    SaleBillNo = h.SaleBillNo,
                    InvoiceDate = inv?.CreatedDate ?? h.SaleDate,
                    SaleType = h.SaleType,
                    BillTypeText = BuildBillTypeText(h.BillType),
                    CustomerType = h.CustomerType,

                    CustomerName = h.CustomerName ?? customer?.LedgerName,
                    BillingName = h.BillingName,
                    CustomerAddress = customer?.Address,
                    CustomerCity = first.CityName,
                    CustomerState = first.StateName,
                    CustomerCountry = "India",
                    CustomerMobile = customer?.MobileNumber,
                    CustomerEmail = customer?.EMail,
                    CustomerGstin = customer?.Gstno,
                    FinancedBy = financier?.LedgerName
                };

                int sr = 1;
                foreach (var r in rows)
                {
                    var vd = r.vd;

                    var preGst = vd.PreGstDiscount ?? 0;
                    var taxable = vd.ItemRate - preGst;

                    var line = new ProformaInvoiceLine
                    {
                        SrNo = sr++,
                        Description = r.ItemName ?? vd.ModelName,
                        ProductCode = vd.ItemCode,
                        Hsn = r.Hsn,
                        ChassisNo = vd.ChassisNo,
                        MotorNo = r.vi?.MotorNo,
                        Colour = vd.Colour,
                        Qty = 1,

                        ItemRate = vd.ItemRate,
                        PreGstDiscount = preGst,
                        TaxableValue = taxable,

                        IgstPer = vd.Igstper ?? 0,
                        IgstAmt = vd.Igstamnt ?? 0,
                        CgstPer = vd.Cgstper ?? 0,
                        CgstAmt = vd.Cgstamnt ?? 0,
                        SgstPer = vd.Sgstper ?? 0,
                        SgstAmt = vd.Sgstamnt ?? 0,

                        FameII = vd.FameIi ?? 0,
                        RegAmount = vd.RegAmount ?? 0,
                        InsuranceAmount = vd.InsuranceAmount ?? 0,
                        FinalAmount = vd.FinalAmount,

                        BatteryNo = r.vi?.BatteryNo ?? vd.Battery,
                        Chemistry = r.vi?.BatteryChemistry ?? vd.BatteryChemical,
                        BatteryMake = r.vi?.BatteryMake ?? vd.BatteryMake,
                        BatteryCapacity = r.vi?.BatteryCapacity ?? vd.BatteryCapacity,
                        CouponNo = null,
                        ChargerNo = r.vi?.ChargerNo ?? vd.ChargerNo,
                        ControllerNo = r.vi?.ControllerNo ?? vd.ControllerNo,
                        Vcu = r.vi?.Vcu ?? vd.Vcu,
                        MfgYear = vd.MfgYear ?? r.vi?.MfgYear
                    };

                    model.Lines.Add(line);

                    model.TaxableTotal += line.TaxableValue;
                    model.IgstTotal += line.IgstAmt;
                    model.CgstTotal += line.CgstAmt;
                    model.SgstTotal += line.SgstAmt;
                    model.SubsidyTotal += line.FameII;
                    model.RegTotal += line.RegAmount;
                    model.InsuranceTotal += line.InsuranceAmount;
                }


                var firstLine = model.Lines[0];
                model.IgstPer = firstLine.IgstPer;
                model.CgstPer = firstLine.CgstPer;
                model.SgstPer = firstLine.SgstPer;

                model.IsIgst = model.IgstTotal > 0;

                var taxTotal = model.IsIgst ? model.IgstTotal : (model.CgstTotal + model.SgstTotal);
                model.GrandTotal = model.TaxableTotal + taxTotal - model.SubsidyTotal
                                   + model.RegTotal + model.InsuranceTotal;
                model.AmountInWords = ProformaInvoicePdfModel.IndianCurrencyWords(model.GrandTotal);

                return model;
            }
            catch
            {
                throw;
            }
        }

        // Maps VehicleSaleBillHeader.BillType (int?) to a label.
        // NOTE: confirm this id->text mapping against your BillType master.
        private static string BuildBillTypeText(int? billType) => billType switch
        {
            1 => "Tax Invoice",
            2 => "Counter Sale",
            _ => "Proforma Invoice"
        };

        // ═════════════════════════════════════════════════════════════════════
        // COGXIM SALE REGISTER IMPORT  (old DMS tblSaleRegister2021-2027-Cogxim
        // exported to Excel  ->  VehicleSaleBillHeader + VehicleSaleBillDetails)
        //  * 1 Excel row = 1 vehicle; rows with the same Bill No (per dealer) = 1 bill.
        //  * Bills are saved as Status "Invoiced" + BillFrom "Cogxim Import" so every
        //    report counts them and they can always be identified.
        //  * No stock / ChassisDetails / InvoiceHeader / JobCardCustomer / ERP change.
        //  * Re-running is safe: existing Sale Bill Nos and billed chassis are skipped.
        //  * A bill is imported only if ALL its rows are valid.
        // ═════════════════════════════════════════════════════════════════════
        #region Cogxim import
        public const string CogximBillFromTag = "Cogxim Import";
        private const string CogximImportStatus = "Invoiced";
        private const int CogximMaxIssues = 1000;
        private const int CogximSaveChunk = 200;

        // ═════════════════════════════════════════════════════════════════════
        // FIELD CATALOGUE  (key, label, group, required, DB target, aliases)
        // Aliases are compared after lower-casing and removing everything that
        // is not a letter/digit, so "Chasis No." == "chasisno" == "CHASIS_NO".
        // The same catalogue reads BOTH the DMS import template AND the original
        // Cogxim tblSaleRegister20xx sheets (no conversion needed).
        // ═════════════════════════════════════════════════════════════════════
        private static readonly List<ImportFieldViewModel> CogximFields = new()
        {
            // ---------- Bill (header) ----------
            CxF("dealerCode", "Dealer Code", "Header", false, "VehicleSaleBillHeader.DealerCode", "dealercode", "dlrcode", "dealer", "dealerid", "compcode"),
            CxF("saleBillNo", "Bill No", "Header", true, "VehicleSaleBillHeader.SaleBillNo", "billno", "salebillno", "invoiceno", "billnumber", "invno", "voucherno"),
            CxF("saleDate", "Bill Date", "Header", true, "VehicleSaleBillHeader.SaleDate", "billdate", "saledate", "invoicedate", "invdate", "date"),
            CxF("bookingId", "Booking Id", "Header", false, "VehicleSaleBillHeader.BookingId", "bookingid", "bookingno", "referenceno"),
            CxF("customerName", "Party Name (Cogxim: Name of the Dealer)", "Header", false, "VehicleSaleBillHeader.CustomerName + LedgerMaster.LedgerName", "partyname", "nameofthedealer", "customername", "customer", "party"),
            CxF("billingName", "Contact Person (Cogxim: Sold To)", "Header", false, "VehicleSaleBillHeader.BillingName", "contactperson", "billingname", "soldto"),
            CxF("customerMobile", "Party Mobile", "Header", false, "LedgerMaster.MobileNumber (customer match)", "partymobile", "custmobile", "cusmob", "customermobile", "mobileno", "mobile", "phone"),
            CxF("customerEmail", "Party Email", "Header", false, "LedgerMaster.eMail", "partyemail", "email", "emailid"),
            CxF("customerAddress", "Party Address (Address1)", "Header", false, "LedgerMaster.Address", "partyaddress", "address1", "address"),
            CxF("customerAddress2", "Address2 (joined to Party Address)", "Header", false, "LedgerMaster.Address", "partyaddress2", "address2"),
            CxF("customerCity", "City", "Header", false, "LedgerMaster.City (City master)", "city", "customercity", "partycity", "custcity"),
            CxF("customerState", "State", "Header", false, "LedgerMaster.State (State master)", "state", "customerstate", "partystate", "custstate"),
            CxF("customerPin", "Pin", "Header", false, "LedgerMaster.Pin", "pin", "pincode", "pinno", "customerpin", "partypin"),
            CxF("gender", "Gender", "Header", false, "LedgerMaster.Gender", "gender", "sex", "custgender"),
            CxF("dob", "Customer DOB", "Header", false, "LedgerMaster.DateOfBirth", "dob", "custdob", "customerdob", "dateofbirth", "birthdate"),
            CxF("occupation", "Occupation", "Header", false, "LedgerMaster.OccupationId (Occupation master)", "occupation", "occupationname"),
            CxF("customerGstin", "GSTN No", "Header", false, "LedgerMaster.GSTNo (new customer)", "gstnno", "gstno", "gstin", "partygstin"),
            CxF("location", "Location (Loc. Code)", "Header", false, "VehicleSaleBillHeader.Location (Loccode)", "loccode", "location", "locationcode", "locationname", "locname", "branch"),
            CxF("salesExecutive", "Executive Name", "Header", false, "VehicleSaleBillHeader.SalesExecutive", "executivename", "salesexecutive", "executive"),
            CxF("saleType", "Sales Type (Cash/Credit)", "Header", false, "VehicleSaleBillHeader.SaleType", "salestype", "saletype", "paymenttype", "paymentmode"),
            CxF("financier", "Financer Name (Financed By)", "Header", false, "VehicleSaleBillHeader.Financier (LedgerMaster.Id)", "financername", "financiername", "financedby", "financer", "financier"),
            CxF("customerType", "Customer Type (B2C/B2B)", "Header", false, "VehicleSaleBillHeader.CustomerType + BillType", "customertype"),
            CxF("tempRegNo", "Temp Reg No", "Header", false, "VehicleSaleBillHeader.TempRegNo", "tempregno", "temporaryregno"),
            CxF("accessoryBillNo", "Accessory Bill No", "Header", false, "VehicleSaleBillHeader.AccessoryBillNo", "accessorybillno", "acsrybillno"),
            CxF("accessoryAmount", "Accessory Amount (summed per bill)", "Header", false, "VehicleSaleBillHeader.AccessoryAmount", "acsryamnt", "acsryamount", "accessoryamount", "accessoryamnt"),
            CxF("hpAmount", "Hyp Amount", "Header", false, "VehicleSaleBillHeader.HPAmount", "hypamnt", "hypamount", "hpamount"),
            CxF("noPlateAmount", "Number Plate Amount", "Header", false, "VehicleSaleBillHeader.NoPlateAmount", "numplateamnt", "numplateamount", "numberplateamount", "noplateamount", "hsrpamount"),
            CxF("handlingCharges", "Handling Charges", "Header", false, "VehicleSaleBillHeader.HandlingCharges", "handlingcharges", "handlingcharge"),
            CxF("stateSubsidyAmount", "State Subsidy (summed per bill)", "Header", false, "VehicleSaleBillHeader.StateSubsidyAmount", "statesubsidyamnt", "statesubsidyamount", "statefameii"),
            CxF("totalAmount", "Bill Total (optional)", "Header", false, "VehicleSaleBillHeader.TotalAmount", "billamount", "billtotal", "grandtotal", "totalamount", "totalbillamount"),

            // ---------- Vehicle (detail) ----------
            CxF("chassisNo", "Chassis No", "Detail", true, "VehicleSaleBillDetails.ChassisNo", "chasisno", "chassisno", "chassis", "vinno", "vin", "frameno"),
            CxF("modelName", "Item Model", "Detail", false, "VehicleSaleBillDetails.ModelName", "itemmodel", "modelname", "model", "itemname", "vehiclemodel"),
            CxF("oemModel", "OEM Model Name", "Detail", false, "used to find ItemCode", "oemmodelname", "oemmodel"),
            CxF("itemCode", "Item Code", "Detail", false, "VehicleSaleBillDetails.ItemCode", "itemcode", "productcode"),
            CxF("hsn", "HSN / SAC Code", "Detail", false, "VehicleSaleBillDetails.Narration (HSN: ...) - report fallback", "hsncode", "hsnsaccode", "hsn", "hsnsac", "hsnno"),
            CxF("colour", "Colour", "Detail", false, "VehicleSaleBillDetails.Colour", "color", "colour", "colorname", "colourname", "colorcode"),
            CxF("mfgYear", "Mfg Year", "Detail", false, "VehicleSaleBillDetails.MfgYear", "mfgyear", "manufacturingyear", "yearofmfg"),
            CxF("itemRate", "Item Rate", "Detail", false, "VehicleSaleBillDetails.ItemRate", "itemrate", "rate", "basicrate", "exshowroomrate"),
            CxF("preGstDiscount", "Pre-GST Discount (Disc Amnt)", "Detail", false, "VehicleSaleBillDetails.PreGstDiscount", "pregstdiscamnt", "pregstdiscamount", "pregstdisc", "pregstdiscount", "discamnt", "discamount"),
            CxF("postGstDiscount", "Post-GST Discount", "Detail", false, "VehicleSaleBillDetails.PostGstDisc", "postgstdiscamnt", "postgstdiscamount", "postgstdisc", "postgstdiscount"),
            CxF("fameII", "Subsidy (FAME-II / PM E-DRIVE)", "Detail", false, "VehicleSaleBillDetails.FameII", "subsidyamnt", "subsidyamount", "fameii", "fame2", "fame2amount", "pmedrive"),
            CxF("sgstPer", "SGST %", "Detail", false, "VehicleSaleBillDetails.SGSTPer", "sgstper", "sgstpercent", "sgstrate", "sgst"),
            CxF("sgstAmount", "SGST Amount", "Detail", false, "VehicleSaleBillDetails.SGSTAmnt", "sgstamnt", "sgstamount"),
            CxF("cgstPer", "CGST %", "Detail", false, "VehicleSaleBillDetails.CGSTPer", "cgstper", "cgstpercent", "cgstrate", "cgst"),
            CxF("cgstAmount", "CGST Amount", "Detail", false, "VehicleSaleBillDetails.CGSTAmnt", "cgstamnt", "cgstamount"),
            CxF("igstPer", "IGST %", "Detail", false, "VehicleSaleBillDetails.IGSTPer", "igstper", "igstpercent", "igstrate", "igst"),
            CxF("igstAmount", "IGST Amount", "Detail", false, "VehicleSaleBillDetails.IGSTAmnt", "igstamnt", "igstamount"),
            CxF("regAmount", "Registration Amount", "Detail", false, "VehicleSaleBillDetails.RegAmount", "regnamnt", "regnamount", "regamount", "registrationamount"),
            CxF("insuranceAmount", "Insurance Amount", "Detail", false, "VehicleSaleBillDetails.InsuranceAmount", "insuamnt", "insuamount", "insuranceamount", "insamount", "insamnt"),
            CxF("finalAmount", "Net Amount (check only - DMS amount is recalculated)", "Detail", false, "compared with the calculated FinalAmount", "netamnt", "netamount", "finalamount", "nettotal"),
            CxF("regNo", "Reg No", "Detail", false, "VehicleSaleBillDetails.RegNo", "regno", "registrationno", "vehicleregno", "regnumber"),
            CxF("insNo", "Insurance Policy No", "Detail", false, "VehicleSaleBillDetails.InsNo", "insno", "policyno", "insurancepolicyno", "insurancono"),
            CxF("insStartDate", "Insurance Start Date", "Detail", false, "VehicleSaleBillDetails.InsStartDate", "insstartdate", "policystartdate", "insurancestartdate"),
            CxF("insExpDate", "Insurance Expiry Date", "Detail", false, "VehicleSaleBillDetails.InsExpDate", "insexpdate", "policyexpirydate", "insuranceexpdate", "insuranceexpirydate"),
            CxF("motorNo", "Motor No", "Detail", false, "VehicleSaleBillDetails.Narration (Motor: ...) - report fallback", "motorno", "motornumber", "engineno"),
            CxF("battery", "Battery No", "Detail", false, "VehicleSaleBillDetails.Battery", "battery", "batteryno", "batteryserialno"),
            CxF("batteryMake", "Battery Make", "Detail", false, "VehicleSaleBillDetails.BatteryMake", "batterymake"),
            CxF("batteryChemical", "Battery Chemistry / Type", "Detail", false, "VehicleSaleBillDetails.BatteryChemical", "batterychemical", "batterychemistry", "batterytype"),
            CxF("batteryCapacity", "Battery Capacity", "Detail", false, "VehicleSaleBillDetails.BatteryCapacity", "batterycapacity"),
            CxF("chargerNo", "Charger No", "Detail", false, "VehicleSaleBillDetails.ChargerNo", "chargerno", "charger"),
            CxF("controllerNo", "Controller No", "Detail", false, "VehicleSaleBillDetails.ControllerNo", "controllerno", "controller"),
            CxF("convertorNo", "Converter No", "Detail", false, "VehicleSaleBillDetails.ConvertorNo", "converter", "convertor", "converterno", "convertorno"),
            CxF("vcu", "VCU", "Detail", false, "VehicleSaleBillDetails.VCU", "vcu", "vcuno"),
            CxF("keyNo", "Key No", "Detail", false, "VehicleSaleBillDetails.Key", "keyno", "key"),
            CxF("bookNo", "Service Book No", "Detail", false, "VehicleSaleBillDetails.BookNo", "bookno", "servicebookno", "servbkno"),
            CxF("segment", "Segment", "Detail", false, "VehicleSaleBillDetails.Segment", "segment", "segmentname"),
            CxF("institutionalType", "Institutional Name", "Detail", false, "VehicleSaleBillDetails.InstitutionalType", "institutionalname", "institutionaltype"),
            CxF("schemeName", "Scheme Name", "Detail", false, "VehicleSaleBillDetails.SchemeName", "schemename", "scheme"),
            CxF("sourceRowId", "Cogxim ImportRowId", "Detail", false, "VehicleSaleBillDetails.Narration", "importrowid", "cogximrowid", "cogximimportrowid"),
            CxF("narration", "Narration / Remarks", "Detail", false, "VehicleSaleBillDetails.Narration", "narration", "remarks", "remark"),
        };

        private static ImportFieldViewModel CxF(string key, string label, string group, bool required, string target, params string[] aliases)
            => new() { Key = key, Label = label, Group = group, Required = required, Target = target, Aliases = aliases.ToList() };

        public List<ImportFieldViewModel> GetCogximImportFields() => CogximFields;

        // ═════════════════════════════════════════════════════════════════════
        // PREVIEW
        // ═════════════════════════════════════════════════════════════════════
        public VehicleSaleImportPreviewResponse PreviewCogximImport(System.IO.Stream excel, string fileName, string? sheetName, int headerRow)
        {
            using var wb = CxOpen(excel);
            var ws = CxPickSheet(wb, sheetName);
            headerRow = headerRow < 1 ? 1 : headerRow;
            var headers = CxReadHeaders(ws, headerRow);
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;

            var response = new VehicleSaleImportPreviewResponse
            {
                FileName = fileName,
                Sheets = wb.Worksheets.Select(s => new ImportSheetInfo { Name = s.Name, Rows = s == ws ? Math.Max(0, lastRow - headerRow) : Math.Max(0, s.CountRowsFast() - 1) }).ToList(),
                SheetName = ws.Name,
                HeaderRow = headerRow,
                Headers = headers.Select(h => h.Title).ToList(),
                SuggestedMapping = CxSuggestMapping(headers.Select(h => h.Title).ToList()),
                Fields = CogximFields
            };

            var rows = 0;
            for (var r = headerRow + 1; r <= lastRow; r++)
            {
                var row = ws.Row(r);
                if (row.IsEmpty()) continue;
                rows++;
                if (response.SampleRows.Count < 10)
                {
                    var dict = new Dictionary<string, string?>();
                    foreach (var h in headers) dict[h.Title] = CxText(row.Cell(h.Column));
                    response.SampleRows.Add(dict);
                }
            }
            response.TotalRows = rows;
            return response;
        }

        // ═════════════════════════════════════════════════════════════════════
        // IMPORT (dry run or real). New bills are inserted; bills already imported
        // from Cogxim are updated when options.UpdateExisting is on.
        // ═════════════════════════════════════════════════════════════════════
        public async Task<VehicleSaleImportResult> ImportCogximSalesAsync(System.IO.Stream excel, string fileName, VehicleSaleImportOptions options, string userId)
        {
            options ??= new VehicleSaleImportOptions();
            var headerRow = options.HeaderRow < 1 ? 1 : options.HeaderRow;

            using var wb = CxOpen(excel);
            var ws = CxPickSheet(wb, options.SheetName);
            var headers = CxReadHeaders(ws, headerRow);
            var colOf = CxResolveMapping(options.Mapping, headers);

            foreach (var f in CogximFields.Where(f => f.Required))
                if (!colOf.ContainsKey(f.Key))
                    throw new ArgumentException($"Map a column to the required field \"{f.Label}\".");
            if (!colOf.ContainsKey("dealerCode") && string.IsNullOrWhiteSpace(options.DefaultDealerCode))
                throw new ArgumentException("Map the Dealer Code column or choose a default dealer.");

            var result = new VehicleSaleImportResult { DryRun = options.DryRun, FileName = fileName, SheetName = ws.Name };
            var issues = new List<ImportIssue>();
            // messages starting with "[Info] " are data-quality notes: shown under Info, not counted as warnings
            void Issue(CxImportRow? r, string level, string msg, int rowNo = 0)
            {
                if (msg.StartsWith(CxInfoTag)) { level = "Info"; msg = msg[CxInfoTag.Length..]; }
                issues.Add(new ImportIssue { RowNo = r?.RowNo ?? rowNo, SaleBillNo = r?.SaleBillNo, ChassisNo = r?.ChassisNo, Level = level, Message = msg });
            }

            // ---------- 1. read + type-convert every row ----------
            var rows = new List<CxImportRow>();
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
            for (var r = headerRow + 1; r <= lastRow; r++)
            {
                var xr = ws.Row(r);
                if (xr.IsEmpty()) continue;
                var row = CxParseRow(xr, r, colOf, options.DefaultDealerCode);
                if (row.SaleBillNo == null && row.ChassisNo == null && row.SaleDate == null) continue;   // blank / filler line
                rows.Add(row);
            }
            result.TotalRows = rows.Count;

            // ---------- 2. reference data (only what the file needs) ----------
            var dealerCodesInFile = rows.Select(x => x.DealerCode).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.ToUpperInvariant()).Distinct().ToList();
            var dealers = (await _context.DealerMasters.AsNoTracking().Select(d => d.Dealercode).ToListAsync())
                .Where(d => d != null).GroupBy(d => d.Trim().ToUpperInvariant()).ToDictionary(g => g.Key, g => g.First().Trim());

            var locations = await _context.LocationMasters.AsNoTracking()
                .Select(l => new { l.Dealercode, l.Loccode, l.Locname, l.Locareaidno }).ToListAsync();

            var items = await _context.ItemMasters.AsNoTracking()
                .Select(i => new { i.Itemcode, i.Itemname, i.Displayname, i.Oemmodelname }).ToListAsync();
            var itemByCode = items.Where(i => i.Itemcode != null).GroupBy(i => CxNorm(i.Itemcode)).ToDictionary(g => g.Key, g => g.First().Itemcode);
            var itemByName = new Dictionary<string, string>();
            foreach (var i in items)
                foreach (var n in new[] { i.Itemname, i.Displayname, i.Oemmodelname })
                    if (!string.IsNullOrWhiteSpace(n) && !itemByName.ContainsKey(CxNorm(n))) itemByName[CxNorm(n)] = i.Itemcode;

            var financierList = (await _context.LedgerMasters.AsNoTracking()
                    .Where(l => l.LedgerType != null && l.LedgerType.ToLower() == "financier")
                    .Select(l => new { l.Id, l.LedgerName, l.DealerCode }).ToListAsync())
                .Where(l => !string.IsNullOrWhiteSpace(l.LedgerName))
                .Select(l => (l.Id, Norm: CxNorm(l.LedgerName), Dealer: (l.DealerCode ?? "").Trim().ToUpperInvariant())).ToList();
            var financierByDealer = financierList.GroupBy(l => l.Dealer + "|" + l.Norm).ToDictionary(g => g.Key, g => g.First().Id);
            var financiers = financierList.GroupBy(l => l.Norm).ToDictionary(g => g.Key, g => g.First().Id);

            var stateByName = (await _context.States.AsNoTracking().Select(s => new { s.StateId, s.StateName }).ToListAsync())
                .GroupBy(s => CxNorm(s.StateName)).ToDictionary(g => g.Key, g => g.First().StateId);
            var cityList = (await _context.Cities.AsNoTracking().Select(c => new { c.CityId, c.CityName, c.StateId }).ToListAsync())
                .Select(c => (c.CityId, Norm: CxNorm(c.CityName), c.StateId)).ToList();
            var occupationByName = (await _context.OccupationMasters.AsNoTracking().Select(o => new { o.Id, o.OccupationName }).ToListAsync())
                .GroupBy(o => CxNorm(o.OccupationName)).ToDictionary(g => g.Key, g => g.First().Id);

            var canonicalDealers = dealerCodesInFile.Where(dealers.ContainsKey).Select(d => dealers[d]).ToList();
            var customers = await _context.LedgerMasters.AsNoTracking()
                .Where(l => l.DealerCode != null && canonicalDealers.Contains(l.DealerCode)
                            && (l.LedgerType == "Party" || l.LedgerType == "Institutional"))
                .Select(l => new { l.Id, l.DealerCode, l.LedgerName, l.MobileNumber, l.LedgerCode }).ToListAsync();
            var custByMobile = new Dictionary<string, int>();
            var custByName = new Dictionary<string, int>();
            foreach (var c in customers)
            {
                var d = c.DealerCode!.ToUpperInvariant();
                var m = CxDigits(c.MobileNumber);
                if (m.Length > 10) m = m[^10..];
                if (m.Length == 10 && !custByMobile.ContainsKey(d + "|" + m)) custByMobile[d + "|" + m] = c.Id;
                if (!string.IsNullOrWhiteSpace(c.LedgerName) && !custByName.ContainsKey(d + "|" + CxNorm(c.LedgerName))) custByName[d + "|" + CxNorm(c.LedgerName)] = c.Id;
            }

            // ---------- 3. row-level validation ----------
            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.SaleBillNo)) r.Errors.Add("Bill No is blank.");
                if (r.SaleDate == null) r.Errors.Add(r.SaleDateRaw == null ? "Bill Date is blank." : $"Bill Date \"{r.SaleDateRaw}\" is not a valid date.");
                if (string.IsNullOrWhiteSpace(r.ChassisNo)) r.SkipReason ??= "Chassis No is blank - row skipped.";
                if (string.IsNullOrWhiteSpace(r.DealerCode)) r.Errors.Add("Dealer Code is blank.");
                else if (!dealers.TryGetValue(r.DealerCode!.ToUpperInvariant(), out var canon)) r.Errors.Add($"Dealer \"{r.DealerCode}\" not found in Dealer Master.");
                else r.DealerCode = canon;
                if (r.ChassisNo?.Length > 50) r.Errors.Add("Chassis No is longer than 50 characters.");
                foreach (var p in new[] { r.SgstPer, r.CgstPer, r.IgstPer })
                    if (p is < 0 or > 100) r.Errors.Add($"GST % {p} is outside 0-100.");

                // Bill grouping key. GenerateSaleBillNo: the Cogxim invoice no is kept as InvoiceNo and the DMS
                // Sale Bill No comes from Prefix Master; Cogxim restarts invoice numbers every year and per
                // dealer, so a bill = dealer + invoice no + date + customer mobile.
                if (!string.IsNullOrWhiteSpace(r.SaleBillNo) && !string.IsNullOrWhiteSpace(r.DealerCode))
                {
                    if (options.GenerateSaleBillNo)
                    {
                        if (r.SaleBillNo!.Length > 50) r.Errors.Add($"Invoice No \"{r.SaleBillNo}\" is longer than 50 characters.");
                        r.FinalBillNo = CxBillKey(r.DealerCode!, r.SaleBillNo!, r.SaleDate) + "|" + CxMobile(r.CustomerMobile);
                    }
                    else
                    {
                        r.FinalBillNo = options.PrefixDealerCodeOnBillNo ? $"{r.DealerCode}/{r.SaleBillNo}" : r.SaleBillNo!;
                        if (r.FinalBillNo.Length > 50) r.Errors.Add($"Bill No \"{r.FinalBillNo}\" is longer than 50 characters.");
                    }
                }
            }

            // same bill number used by two different dealers in the file
            if (!options.PrefixDealerCodeOnBillNo && !options.GenerateSaleBillNo)
            {
                foreach (var g in rows.Where(r => r.FinalBillNo != null).GroupBy(r => r.FinalBillNo!, StringComparer.OrdinalIgnoreCase))
                {
                    var owners = g.Select(r => r.DealerCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    if (owners.Count <= 1) continue;
                    foreach (var r in g.Where(r => !string.Equals(r.DealerCode, owners[0], StringComparison.OrdinalIgnoreCase)))
                        r.Errors.Add($"Bill No also used by dealer {owners[0]}. Tick \"Prefix dealer code on Bill No\" to import both.");
                }
            }

            // duplicate chassis inside the file
            var seenChassis = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in rows.Where(r => !string.IsNullOrWhiteSpace(r.ChassisNo)))
            {
                if (seenChassis.TryGetValue(r.ChassisNo!, out var firstRow)) r.SkipReason ??= $"Duplicate chassis - already on row {firstRow} of this file, this row skipped.";
                else seenChassis[r.ChassisNo!] = r.RowNo;
            }

            // ---------- 4. what already exists in the DB ----------
            var existingBills = new Dictionary<string, CxExistingBill>(StringComparer.OrdinalIgnoreCase);
            var existingByInvoice = new Dictionary<string, Queue<CxExistingBill>>(StringComparer.OrdinalIgnoreCase);   // dealer|invoice|date
            if (options.GenerateSaleBillNo)
            {
                // earlier Cogxim imports: matched on dealer + invoice no + bill date
                // (bills imported before InvoiceNo existed carry the invoice no in SaleBillNo)
                var invNos = rows.Where(r => r.FinalBillNo != null).Select(r => r.SaleBillNo!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var chunk in invNos.Chunk(1000))
                {
                    var found = await _context.VehicleSaleBillHeaders.AsNoTracking()
                        .Where(h => h.BillFrom == CogximBillFromTag && h.DealerCode != null && canonicalDealers.Contains(h.DealerCode)
                                    && ((h.InvoiceNo != null && chunk.Contains(h.InvoiceNo)) || (h.InvoiceNo == null && chunk.Contains(h.SaleBillNo))))
                        .Select(h => new { h.Id, h.SaleBillNo, h.InvoiceNo, h.DealerCode, h.IsDeleted, h.BillFrom, h.LedgerId, h.SaleDate }).ToListAsync();
                    foreach (var f in found.OrderBy(f => f.Id))
                    {
                        var key = CxBillKey(f.DealerCode!, f.InvoiceNo ?? f.SaleBillNo, f.SaleDate);
                        if (!existingByInvoice.TryGetValue(key, out var q)) existingByInvoice[key] = q = new Queue<CxExistingBill>();
                        q.Enqueue(new CxExistingBill(f.Id, f.DealerCode, f.IsDeleted, f.BillFrom, f.LedgerId, f.SaleBillNo, f.InvoiceNo));
                    }
                }
            }
            else
            {
                var billNos = rows.Where(r => r.FinalBillNo != null).Select(r => r.FinalBillNo!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var chunk in billNos.Chunk(1000))
                {
                    var found = await _context.VehicleSaleBillHeaders.AsNoTracking()
                        .Where(h => chunk.Contains(h.SaleBillNo))
                        .Select(h => new { h.Id, h.SaleBillNo, h.InvoiceNo, h.DealerCode, h.IsDeleted, h.BillFrom, h.LedgerId }).ToListAsync();
                    foreach (var f in found) existingBills[f.SaleBillNo] = new CxExistingBill(f.Id, f.DealerCode, f.IsDeleted, f.BillFrom, f.LedgerId, f.SaleBillNo, f.InvoiceNo);
                }
            }

            var chassisList = seenChassis.Keys.ToList();
            var billedChassis = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var chunk in chassisList.Chunk(1000))
            {
                var found = await (from d in _context.VehicleSaleBillDetails.AsNoTracking()
                                   join h in _context.VehicleSaleBillHeaders.AsNoTracking() on d.VehicleSaleBillId equals h.Id
                                   where chunk.Contains(d.ChassisNo) && !h.IsDeleted
                                   select new { d.ChassisNo, h.SaleBillNo }).ToListAsync();
                foreach (var f in found) billedChassis.TryAdd(f.ChassisNo, f.SaleBillNo);
            }

            // financier: dealer's own ledger, else any financier ledger, else one unique partial match
            int? FindFinancier(string dKey, string? name)
            {
                var fn = CxNorm(name);
                if (fn.Length == 0) return null;
                if (financierByDealer.TryGetValue(dKey + "|" + fn, out var a)) return a;
                if (financiers.TryGetValue(fn, out var b)) return b;
                if (fn.Length < 5) return null;
                var partial = financierList.Where(x => x.Norm.Length >= 5 && (x.Norm.StartsWith(fn) || fn.StartsWith(x.Norm))).Select(x => x.Id).Distinct().ToList();
                return partial.Count == 1 ? partial[0] : null;
            }

            // ---------- 4b. create the masters the file needs (OEM model, Item, Financier, City, Occupation) ----------
            if (options.CreateMissingMasters)
            {
                var good = rows.Where(r => r.Errors.Count == 0 && r.SkipReason == null && r.DealerCode != null && r.FinalBillNo != null).ToList();
                await CxCreateMissingMasters(good, options.DryRun, userId, result, (msg) => Issue(null, "Warning", msg),
                    occupationByName, stateByName, cityList, financierList, financierByDealer, financiers, FindFinancier, itemByCode, itemByName);
            }

            // ---------- 5. group into bills ----------
            var bills = rows.Where(r => r.FinalBillNo != null && r.DealerCode != null)
                .GroupBy(r => r.FinalBillNo!, StringComparer.OrdinalIgnoreCase)
                .ToList();
            result.BillsInFile = bills.Count;

            foreach (var r in rows.Where(r => r.FinalBillNo == null || r.DealerCode == null))
            {
                foreach (var e in r.Errors) Issue(r, "Error", e);
                result.RowsWithErrors++;
            }

            // LedgerMaster.Address / Address2 sizes are read from the database itself (the EF model
            // does not match the live column size) - long Cogxim addresses are split to fit.
            var ledgerAddressMax = await CxColumnLengthAsync("LedgerMaster", "Address") ?? 100;
            var ledgerAddress2Max = await CxColumnLengthAsync("LedgerMaster", "Address2") ?? 0;

            var newLedgers = new Dictionary<string, LedgerMaster>();          // key dealer|mobile or dealer|name
            var locFallback = new Dictionary<(string Dealer, string Location, string Showroom), (int Bills, int FirstRow)>();
            var ledgerPatches = new Dictionary<int, CxCustomerInfo>();       // existing ledger -> values to fill where blank
            var ready = new List<CxReadyBill>();

            foreach (var g in bills)
            {
                var billRows = g.OrderBy(r => r.RowNo).ToList();
                var first = billRows[0];

                CxExistingBill? existing = null;
                CxExistingBill? ex = null;
                if (options.GenerateSaleBillNo)
                {
                    if (existingByInvoice.TryGetValue(CxBillKey(first.DealerCode!, first.SaleBillNo!, first.SaleDate), out var q) && q.Count > 0)
                        ex = q.Dequeue();
                }
                else existingBills.TryGetValue(g.Key, out ex);
                if (ex != null)
                {
                    var canUpdate = options.UpdateExisting && !ex.Deleted
                                    && string.Equals(ex.BillFrom, CogximBillFromTag, StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(ex.DealerCode, first.DealerCode, StringComparison.OrdinalIgnoreCase);
                    if (!canUpdate)
                    {
                        var why = ex.Deleted ? "Bill No exists (deleted) in DMS - Sale Bill No must be unique, so it is skipped."
                                : !string.Equals(ex.BillFrom, CogximBillFromTag, StringComparison.OrdinalIgnoreCase) ? $"Bill No already used by a DMS bill (dealer {ex.DealerCode}) - skipped, DMS bills are never overwritten."
                                : !string.Equals(ex.DealerCode, first.DealerCode, StringComparison.OrdinalIgnoreCase) ? $"Bill No already imported for dealer {ex.DealerCode} - skipped."
                                : "Already imported from Cogxim - skipped (tick \"Update bills already imported\" to refresh it).";
                        foreach (var r in billRows) Issue(r, "Skipped", why);
                        result.RowsSkipped += billRows.Count;
                        continue;
                    }
                    existing = ex;
                }

                // chassis already billed in another DMS bill -> skip that vehicle only
                var usable = new List<CxImportRow>();
                foreach (var r in billRows)
                {
                    if (r.SkipReason != null)
                    {
                        Issue(r, "Skipped", r.SkipReason);
                        result.RowsSkipped++;
                    }
                    else if (r.ChassisNo != null && billedChassis.TryGetValue(r.ChassisNo, out var otherBill)
                        && !string.Equals(otherBill, existing?.SaleBillNo ?? g.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        Issue(r, "Skipped", $"Chassis already billed in DMS bill {otherBill} - skipped.");
                        result.RowsSkipped++;
                    }
                    else usable.Add(r);
                }
                if (usable.Count == 0) continue;

                var bad = usable.Where(r => r.Errors.Count > 0).ToList();
                if (bad.Count > 0)
                {
                    foreach (var r in bad) foreach (var e in r.Errors) Issue(r, "Error", e);
                    foreach (var r in usable.Except(bad))
                        Issue(r, "Error", $"Bill not imported because row {bad[0].RowNo} of the same bill has errors.");
                    result.RowsWithErrors += usable.Count;
                    continue;
                }

                // header values that differ between rows of the same bill
                if (usable.Select(r => r.SaleDate!.Value.Date).Distinct().Count() > 1)
                    Issue(first, "Warning", "Rows of this bill have different Bill Dates - the first row's date is used.");
                if (usable.Select(r => CxNorm(r.BillingName ?? r.CustomerName)).Distinct().Count() > 1)
                    Issue(first, "Warning", "Rows of this bill have different customers - the first row's customer is used.");

                var dealer = first.DealerCode!;
                var dKey = dealer.ToUpperInvariant();

                // ---- location (stored as Loccode) ----
                var dealerLocs = locations.Where(l => string.Equals(l.Dealercode, dealer, StringComparison.OrdinalIgnoreCase)).ToList();
                string? locCode = null;
                if (!string.IsNullOrWhiteSpace(first.Location))
                {
                    var n = CxNorm(first.Location);
                    locCode = dealerLocs.FirstOrDefault(l => CxNorm(l.Loccode) == n || CxNorm(l.Locname) == n)?.Loccode
                              ?? locations.FirstOrDefault(l => CxNorm(l.Loccode) == n)?.Loccode;
                }
                if (locCode == null && !string.IsNullOrWhiteSpace(options.DefaultLocation)
                    && dealerLocs.Any(l => string.Equals(l.Loccode, options.DefaultLocation, StringComparison.OrdinalIgnoreCase)))
                    locCode = options.DefaultLocation;
                if (locCode == null)
                {
                    locCode = (dealerLocs.FirstOrDefault(l => l.Locareaidno == 1) ?? dealerLocs.FirstOrDefault())?.Loccode;
                    if (locCode == null)
                        Issue(first, "Warning", $"No location found for dealer {dealer}; Location stored as \"{CxCut(first.Location, 100)}\".");
                    else if (!string.IsNullOrWhiteSpace(first.Location))
                    {
                        // Cogxim sub-dealer / mobile outlets (M1, SD3 ...) are not in Location Master:
                        // bill goes to the showroom, Cogxim location kept in Narration, ONE Info line per location (below)
                        var key = (dealer.ToUpperInvariant(), first.Location!.Trim().ToUpperInvariant(), locCode);
                        locFallback[key] = locFallback.TryGetValue(key, out var lf) ? (lf.Bills + 1, lf.FirstRow) : (1, first.RowNo);
                        foreach (var r in usable) r.UnmatchedLocation = first.Location!.Trim();
                    }
                    locCode ??= first.Location;
                }

                // ---- financier (exact name, else one unique partial match) ----
                int? financierId = null;
                string? unmatchedFinancier = null;
                if (!string.IsNullOrWhiteSpace(first.Financier))
                {
                    financierId = FindFinancier(dKey, first.Financier!);
                    if (financierId == null)
                    {
                        unmatchedFinancier = first.Financier;
                        Issue(first, "Warning", $"Financier \"{first.Financier}\" not in Ledger Master and not created (switch \"Create missing masters\" on) - name kept in Narration.");
                    }
                }

                // ---- customer master values (City / State / Gender / DOB / Occupation / Pin) ----
                var info = CxCustomerInfoFor(first, stateByName, cityList, occupationByName, w => Issue(first, "Warning", w));
                CxSplitAddress(info, ledgerAddressMax, ledgerAddress2Max);

                // ---- customer ledger ----
                int? ledgerId = existing?.LedgerId;
                LedgerMaster? newLedger = null;
                var mobile = CxDigits(first.CustomerMobile);
                if (mobile.Length > 10) mobile = mobile[^10..];
                if (ledgerId == null)
                {
                    if (mobile.Length == 10)
                    {
                        // a mobile number identifies the customer - never fall back to name (Party Name is the dealer's name)
                        if (custByMobile.TryGetValue(dKey + "|" + mobile, out var cid)) ledgerId = cid;
                    }
                    else if (!string.IsNullOrWhiteSpace(first.CustomerName) && custByName.TryGetValue(dKey + "|" + CxNorm(first.CustomerName), out var cid2))
                        ledgerId = cid2;

                    if (ledgerId == null && !string.IsNullOrWhiteSpace(first.CustomerName))
                    {
                        var key = mobile.Length == 10 ? dKey + "|" + mobile : dKey + "|" + CxNorm(first.CustomerName);
                        if (options.CreateMissingCustomers)
                        {
                            if (!newLedgers.TryGetValue(key, out newLedger))
                            {
                                newLedger = new LedgerMaster
                                {
                                    LedgerName = CxCut(first.CustomerName, 100),
                                    LedgerType = "Party",
                                    MobileNumber = mobile.Length > 0 ? CxCut(mobile, 20) : null,
                                    Gstno = CxCut(first.CustomerGstin, 50),
                                    DealerCode = dealer,
                                    CreatedBy = CxCut(userId, 100)!,
                                    CreatedDate = DateTime.Now
                                };
                                CxFillLedger(newLedger, info);
                                newLedgers[key] = newLedger;
                            }
                        }
                        else Issue(first, "Warning", "Customer not found in Ledger Master - bill imported without LedgerId (it will not open in the Sale Bill edit screen).");
                    }
                    else if (ledgerId == null) Issue(first, "Warning", "Party Name is blank - bill imported without customer.");
                }
                if (ledgerId != null && info.HasAny && !ledgerPatches.ContainsKey(ledgerId.Value)) ledgerPatches[ledgerId.Value] = info;

                // ---- sale / customer type ----
                var saleType = CxNormaliseSaleType(first.SaleType, first.Financier);
                var customerType = (first.CustomerType ?? "").ToUpperInvariant().Contains("B2B") ? "B2B"
                                 : (first.CustomerType ?? "").ToUpperInvariant().Contains("B2C") ? "B2C"
                                 : !string.IsNullOrWhiteSpace(first.CustomerGstin) ? "B2B" : "B2C";

                // bill-level charges: Cogxim repeats them per vehicle row -> add up the rows of the bill
                decimal? SumOf(Func<CxImportRow, decimal?> pick) => usable.Any(r => pick(r).HasValue) ? usable.Sum(r => pick(r) ?? 0) : null;
                var accessoryAmount = SumOf(r => r.AccessoryAmount);
                var stateSubsidy = SumOf(r => r.StateSubsidyAmount);

                var header = new VehicleSaleBillHeader
                {
                    // GenerateSaleBillNo: replaced below by the Prefix Master number
                    SaleBillNo = options.GenerateSaleBillNo ? (existing?.SaleBillNo ?? first.SaleBillNo!) : g.Key,
                    InvoiceNo = CxCut(first.SaleBillNo, 50),
                    SaleDate = first.SaleDate!.Value,
                    DealerCode = dealer,
                    Location = CxCut(locCode, 100),
                    LedgerId = ledgerId,
                    CustomerName = CxCut(first.CustomerName, 150),
                    BillingName = CxCut(first.BillingName ?? first.CustomerName, 150),
                    CustomerType = customerType,
                    BillType = customerType == "B2B" ? 1 : 2,
                    SaleType = saleType,
                    Financier = financierId,
                    SalesExecutive = CxCut(first.SalesExecutive, 100),
                    BookingId = CxCut(first.BookingId, 50),
                    TempRegNo = CxCut(first.TempRegNo, 50),
                    AccessoryBillNo = CxCut(first.AccessoryBillNo, 50),
                    AccessoryAmount = accessoryAmount,
                    Hpamount = first.HpAmount,
                    NoPlateAmount = first.NoPlateAmount,
                    HandlingCharges = first.HandlingCharges,
                    StateSubsidyAmount = stateSubsidy,
                    BillFrom = CogximBillFromTag,
                    Status = CogximImportStatus,
                    IsD2d = false,
                    IsDeleted = false,
                    CreatedBy = CxCut(userId, 100)!,
                    CreatedDate = DateTime.Now
                };

                foreach (var r in usable)
                {
                    var d = CxBuildDetail(r, itemByCode, itemByName, userId, ws.Name, unmatchedFinancier, info,
                        w => Issue(r, "Warning", w));
                    header.VehicleSaleBillDetails.Add(d);
                }

                header.TotalAmount = first.TotalAmount ?? CxBillTotal(header, header.VehicleSaleBillDetails);
                ready.Add(new CxReadyBill(header, newLedger, usable, existing?.Id, first.TotalAmount.HasValue));
            }

            // ---------- 5b. DMS Sale Bill No from Prefix Master ("sale_bill") ----------
            var sequenceUpdates = new Dictionary<int, int>();   // NumberSequence.Id -> NextNo after this import
            var renumberedOld = new Dictionary<int, string>();  // already-imported bill Id -> its Sale Bill No before this run
            if (options.GenerateSaleBillNo && ready.Count > 0)
            {
                // every bill is numbered in the series of its OWN financial year (Apr-Mar of the sale date):
                //  * new bills, and earlier imports that still carry the Cogxim invoice no as Sale Bill No
                //  * earlier imports whose Sale Bill No is in another year's series -> renumbered
                //  * earlier imports already in the right year's series keep their number
                var toNumber = ready.OrderBy(x => x.Header.SaleDate).ThenBy(x => x.Rows[0].RowNo).ToList();
                var keepIfFits = new HashSet<VehicleSaleBillHeader>(toNumber
                    .Where(x => x.ExistingId != null && !string.Equals(x.Header.SaleBillNo, x.Header.InvoiceNo, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Header));
                var oldNos = toNumber.ToDictionary(x => x.Header, x => x.Header.SaleBillNo);
                var noSeqWarned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var numbered = await CxAssignSaleBillNos(toNumber.Select(x => x.Header).ToList(), sequenceUpdates,
                    (dealer, pattern) =>
                    {
                        if (noSeqWarned.Add(dealer + "|" + pattern))
                            Issue(null, "Warning", $"Dealer {dealer}: no 'sale_bill' prefix in Prefix Master for this financial year - Sale Bill No uses {pattern} (add the year's prefix in Prefix Master, or run docs/cogxim/Create_Cogxim_Sale_Bill_Prefixes.sql).");
                    }, keepIfFits);
                foreach (var x in toNumber.Where(x => x.ExistingId != null && !string.Equals(oldNos[x.Header], x.Header.SaleBillNo, StringComparison.OrdinalIgnoreCase)))
                    renumberedOld[x.ExistingId!.Value] = oldNos[x.Header];
                foreach (var x in toNumber.Where(x => !numbered.Contains(x.Header) && !keepIfFits.Contains(x.Header)))
                    foreach (var r in x.Rows) Issue(r, "Error", "Sale Bill No could not be generated.");
                // one line per dealer + year for bills moved into their own year's series
                foreach (var g in toNumber.Where(x => keepIfFits.Contains(x.Header)
                                                      && !string.Equals(oldNos[x.Header], x.Header.SaleBillNo, StringComparison.OrdinalIgnoreCase))
                                          .GroupBy(x => (x.Header.DealerCode, Fy: CxFyText(x.Header.SaleDate))))
                {
                    var ex1 = g.First();
                    Issue(null, "Info", $"Dealer {g.Key.DealerCode}: {g.Count()} already-imported bill(s) of FY {g.Key.Fy} renumbered into that year's series " +
                                        $"(e.g. {oldNos[ex1.Header]} -> {ex1.Header.SaleBillNo}).");
                }
            }

            var inserts = ready.Where(x => x.ExistingId == null).ToList();
            var updates = ready.Where(x => x.ExistingId != null).ToList();
            result.BillsReady = inserts.Count;
            result.BillsToUpdate = updates.Count;
            result.VehiclesReady = ready.Sum(x => x.Header.VehicleSaleBillDetails.Count);
            result.CustomersCreated = newLedgers.Count;
            result.TotalAmount = ready.Sum(x => x.Header.TotalAmount ?? 0);
            if (ready.Count > 0)
            {
                result.FromDate = ready.Min(x => x.Header.SaleDate);
                result.ToDate = ready.Max(x => x.Header.SaleDate);
            }

            // ---------- 6. write ----------
            if (!options.DryRun && ready.Count > 0)
            {
                var prevDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
                try
                {
                    // 6a. new customer ledgers (one batch, so the bills can reference their Id)
                    _context.ChangeTracker.AutoDetectChangesEnabled = false;
                    if (newLedgers.Count > 0)
                    {
                        foreach (var grp in newLedgers.Values.GroupBy(l => l.DealerCode!))
                        {
                            var next = await CxNextLedgerNumber(grp.Key);
                            var suffix = grp.Key.Length >= 4 ? grp.Key[^4..] : grp.Key;
                            foreach (var l in grp) l.LedgerCode = $"LED/{suffix}/{next++}";
                        }
                        _context.LedgerMasters.AddRange(newLedgers.Values);
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();
                    }
                    foreach (var x in ready.Where(x => x.NewLedger != null)) x.Header.LedgerId = x.NewLedger!.Id;

                    // 6a-2. renumbered bills give up their old Sale Bill No first (it is unique and may be reused
                    //       by another bill of this run); 6c writes their new number
                    foreach (var chunk in renumberedOld.Keys.Chunk(1000))
                        await _context.VehicleSaleBillHeaders.Where(h => chunk.Contains(h.Id))
                            .ExecuteUpdateAsync(sp => sp.SetProperty(h => h.SaleBillNo, h => "CXRN-" + h.Id.ToString()));

                    // 6b. new bills in chunks; a failing chunk is retried bill-by-bill to isolate the bad one
                    foreach (var chunk in inserts.Chunk(CogximSaveChunk))
                    {
                        try
                        {
                            _context.VehicleSaleBillHeaders.AddRange(chunk.Select(c => c.Header));
                            await _context.SaveChangesAsync();
                            result.BillsImported += chunk.Length;
                            result.VehiclesImported += chunk.Sum(c => c.Header.VehicleSaleBillDetails.Count);
                        }
                        catch (Exception)
                        {
                            _context.ChangeTracker.Clear();
                            foreach (var c in chunk)
                            {
                                try
                                {
                                    CxResetKeys(c.Header);
                                    _context.VehicleSaleBillHeaders.Add(c.Header);
                                    await _context.SaveChangesAsync();
                                    result.BillsImported++;
                                    result.VehiclesImported += c.Header.VehicleSaleBillDetails.Count;
                                }
                                catch (Exception exOne)
                                {
                                    _context.ChangeTracker.Clear();
                                    var msg = exOne.GetBaseException().Message;
                                    foreach (var r in c.Rows) Issue(r, "Error", "Database rejected this bill: " + msg);
                                    result.RowsWithErrors += c.Rows.Count;
                                }
                            }
                        }
                        _context.ChangeTracker.Clear();
                    }

                    // 6c. bills already imported from Cogxim -> update in place (tracked, change detection on)
                    _context.ChangeTracker.AutoDetectChangesEnabled = true;
                    foreach (var u in updates)
                    {
                        try
                        {
                            var db = await _context.VehicleSaleBillHeaders.Include(h => h.VehicleSaleBillDetails)
                                .FirstAsync(h => h.Id == u.ExistingId!.Value);
                            var oldNo = renumberedOld.TryGetValue(db.Id, out var before) ? before : db.SaleBillNo;
                            var vehicles = CxApplyUpdate(db, u.Header, u.ExplicitTotal, userId);
                            if (!string.Equals(oldNo, db.SaleBillNo, StringComparison.Ordinal))
                            {
                                // Sale Bill No renumbered -> invoice linked to this bill follows it
                                var invs = await _context.InvoiceHeaders.Where(i => i.DocumentNo == oldNo && i.ReferenceId == db.Id).ToListAsync();
                                foreach (var inv in invs)
                                {
                                    inv.DocumentNo = db.SaleBillNo;
                                    if (inv.InvoiceNo == "IN-" + oldNo) inv.InvoiceNo = "IN-" + db.SaleBillNo;
                                    else if (inv.InvoiceNo == "IN" + oldNo) inv.InvoiceNo = "IN" + db.SaleBillNo;
                                }
                            }
                            await _context.SaveChangesAsync();
                            result.BillsUpdated++;
                            result.VehiclesUpdated += vehicles;
                        }
                        catch (Exception exU)
                        {
                            var msg = exU.GetBaseException().Message;
                            foreach (var r in u.Rows) Issue(r, "Error", "Update failed: " + msg);
                            _context.ChangeTracker.Clear();
                            // put the old Sale Bill No back if it is still free
                            if (renumberedOld.TryGetValue(u.ExistingId!.Value, out var back))
                            {
                                try
                                {
                                    var id = u.ExistingId!.Value;
                                    if (!await _context.VehicleSaleBillHeaders.AnyAsync(h => h.SaleBillNo == back))
                                        await _context.VehicleSaleBillHeaders.Where(h => h.Id == id)
                                            .ExecuteUpdateAsync(sp => sp.SetProperty(h => h.SaleBillNo, back));
                                    else
                                        Issue(u.Rows[0], "Error", $"Bill Id {id} keeps the temporary Sale Bill No CXRN-{id} - re-run the import to number it.");
                                }
                                catch { /* reported above */ }
                            }
                            result.RowsWithErrors += u.Rows.Count;
                        }
                        finally { _context.ChangeTracker.Clear(); }
                    }

                    // 6c-2. move the Prefix Master counters past the numbers used for the current year
                    if (sequenceUpdates.Count > 0)
                    {
                        var ids = sequenceUpdates.Keys.ToList();
                        var seqs = await _context.NumberSequences.Where(n => ids.Contains(n.Id)).ToListAsync();
                        foreach (var sq in seqs)
                            if (sequenceUpdates[sq.Id] > sq.NextNo)
                            {
                                sq.NextNo = sequenceUpdates[sq.Id];
                                sq.UpdatedBy = CxCut(userId, 100);
                                sq.UpdatedDate = DateTime.Now;
                            }
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();
                    }

                    // 6d. existing customer ledgers: fill City / State / Gender / DOB / Occupation / Pin / Address / Email where blank
                    foreach (var chunk in ledgerPatches.Keys.Chunk(500))
                    {
                        var ledgers = await _context.LedgerMasters.Where(l => chunk.Contains(l.Id)).ToListAsync();
                        foreach (var l in ledgers)
                            if (CxFillLedger(l, ledgerPatches[l.Id], onlyBlank: true))
                            {
                                l.UpdatedBy = CxCut(userId, 100);
                                l.UpdatedDate = DateTime.Now;
                                result.CustomersUpdated++;
                            }
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();
                    }
                }
                finally
                {
                    _context.ChangeTracker.AutoDetectChangesEnabled = prevDetect;
                }
            }
            else if (options.DryRun)
            {
                result.CustomersUpdated = ledgerPatches.Count;   // ledgers that may get blank fields filled
            }

            var done = options.DryRun ? ready.Select(x => x.Header)
                : ready.Where(x => x.Header.Id > 0 || x.ExistingId != null).Select(x => x.Header);
            result.Dealers = done.GroupBy(h => h.DealerCode!)
                .Select(gp => new ImportDealerSummary
                {
                    DealerCode = gp.Key,
                    Bills = gp.Count(),
                    Vehicles = gp.Sum(h => h.VehicleSaleBillDetails.Count),
                    Amount = gp.Sum(h => h.TotalAmount ?? 0)
                })
                .OrderByDescending(x => x.Vehicles).ToList();

            foreach (var ((dlr, loc, showroom), (locBills, firstRow)) in locFallback.OrderBy(k => k.Key.Dealer).ThenBy(k => k.Key.Location))
                Issue(null, "Info", $"Location \"{loc}\" (dealer {dlr}) is not in Location Master - {locBills} bill(s) saved under showroom {showroom}, " +
                                    $"Cogxim location kept in Narration. To move them: add {loc} in Location Master and re-import with \"Update bills already imported\" on.", firstRow);

            result.Warnings = issues.Count(i => i.Level == "Warning");
            result.IssuesTruncated = issues.Count > CogximMaxIssues;
            result.Issues = issues
                .OrderBy(i => i.Level == "Error" ? 0 : i.Level == "Skipped" ? 1 : i.Level == "Warning" ? 2 : 3).ThenBy(i => i.RowNo)
                .Take(CogximMaxIssues).ToList();
            return result;
        }

        // ═════════════════════════════════════════════════════════════════════
        // helpers
        // ═════════════════════════════════════════════════════════════════════
        private sealed class CxImportRow
        {
            public int RowNo;
            public List<string> Errors = new();
            /// <summary>Row is left out (duplicate / blank chassis) without blocking the rest of the bill.</summary>
            public string? SkipReason;
            public string? DealerCode, SaleBillNo, FinalBillNo, SaleDateRaw, BookingId, CustomerName, BillingName, CustomerMobile,
                CustomerEmail, CustomerAddress, CustomerAddress2, CustomerCity, CustomerState, CustomerPin, Gender, Occupation,
                CustomerGstin, Location, SalesExecutive, SaleType, Financier, CustomerType, TempRegNo, AccessoryBillNo;
            public DateTime? SaleDate, Dob, InsStartDate, InsExpDate;
            public decimal? AccessoryAmount, HpAmount, NoPlateAmount, HandlingCharges, StateSubsidyAmount, TotalAmount;
            public string? ChassisNo, ModelName, OemModel, ItemCode, Hsn, Colour, RegNo, InsNo, MotorNo, Battery, BatteryMake,
                BatteryChemical, BatteryCapacity, ChargerNo, ControllerNo, ConvertorNo, Vcu, KeyNo, BookNo, Segment,
                InstitutionalType, SchemeName, SourceRowId, Narration;
            public string? UnmatchedLocation;   // Cogxim location not in Location Master (bill saved under the showroom)
            public int? MfgYear;
            public decimal? ItemRate, PreGst, PostGst, FameII, SgstPer, SgstAmt, CgstPer, CgstAmt, IgstPer, IgstAmt,
                RegAmount, InsAmount, FinalAmount;
        }

        private sealed record CxExistingBill(int Id, string? DealerCode, bool Deleted, string? BillFrom, int? LedgerId, string SaleBillNo, string? InvoiceNo);

        private static string CxBillKey(string dealer, string invoiceNo, DateTime? date)
            => $"{dealer.Trim().ToUpperInvariant()}|{invoiceNo.Trim().ToUpperInvariant()}|{date:yyyyMMdd}";

        private static string CxMobile(string? raw)
        {
            var m = CxDigits(raw);
            return m.Length > 10 ? m[^10..] : m;
        }

        private static readonly System.Text.RegularExpressions.Regex CxFyToken =
            new(@"(?<!\d)(\d{4}|\d{2})-(\d{4}|\d{2})(?!\d)", System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>Puts the bill's own financial year (Apr-Mar) into the sequence code, in the code's own style
        /// ("25-26", "2025-26" or "2025-2026"). Codes without a year are returned unchanged.</summary>
        private static string CxCodeForYear(string code, DateTime saleDate)
        {
            var fy = saleDate.Month >= 4 ? saleDate.Year : saleDate.Year - 1;
            return CxFyToken.Replace(code, m =>
            {
                var a = m.Groups[1].Value.Length == 4 ? fy.ToString() : (fy % 100).ToString("00");
                var b = m.Groups[2].Value.Length == 4 ? (fy + 1).ToString() : ((fy + 1) % 100).ToString("00");
                return $"{a}-{b}";
            }, 1);
        }

        /// <summary>Financial year (Apr-Mar) of a date as "25-26".</summary>
        private static string CxFyText(DateTime d)
        {
            var fy = d.Month >= 4 ? d.Year : d.Year - 1;
            return $"{fy % 100:00}-{(fy + 1) % 100:00}";
        }

        /// <summary>Start year of a financial year written as "25-26", "2025-26", "2025-2026", "FY 25-26" or "2025"; null if none.</summary>
        private static int? CxFyStartOf(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var m = CxFyToken.Match(text);
            string? first = m.Success ? m.Groups[1].Value : null;
            if (first == null)
            {
                var y = System.Text.RegularExpressions.Regex.Match(text, @"(?<!\d)(20\d{2})(?!\d)");
                if (!y.Success) return null;
                first = y.Groups[1].Value;
            }
            var v = int.Parse(first);
            return first.Length == 2 ? 2000 + v : v;
        }

        /// <summary>true when the number is head + digits only + tail.</summary>
        private static bool CxFits(string? no, string head, string tail)
        {
            if (no == null || no.Length <= head.Length + tail.Length) return false;
            if (!no.StartsWith(head, StringComparison.OrdinalIgnoreCase) || !no.EndsWith(tail, StringComparison.OrdinalIgnoreCase)) return false;
            return no.Substring(head.Length, no.Length - head.Length - tail.Length).All(char.IsDigit);
        }

        private sealed record CxSeries(NumberSequence? Seq, string Pattern, string Head, string Tail, int Digits, bool IsActiveYear);

        /// <summary>Gives every header a Sale Bill No in the dealer's "sale_bill" Prefix Master format for the bill's
        /// OWN financial year (Apr-Mar of the sale date):
        ///  1) the dealer's prefix whose Year (25-26 / 2025-26 / 2025-2026) or code is that year, active or not;
        ///  2) else the active prefix with the year in its code changed to the bill's year;
        ///  3) else (no prefix, or an active code without a year that belongs to another year) VSB/&lt;last 3&gt;/&lt;FY&gt;/###.
        /// Numbers continue after the highest one already used in that series. Headers in keepIfFits (bills imported
        /// earlier) keep their number when it is already in the right year's series; the others are renumbered, and a
        /// series that held wrong-year bills is renumbered from its start so it has no gap.
        /// Bills are numbered in the order given (sale date).</summary>
        private async Task<HashSet<VehicleSaleBillHeader>> CxAssignSaleBillNos(List<VehicleSaleBillHeader> headers,
            Dictionary<int, int> sequenceUpdates, Action<string, string> noSequence, HashSet<VehicleSaleBillHeader>? keepIfFits = null)
        {
            keepIfFits ??= new HashSet<VehicleSaleBillHeader>();
            var done = new HashSet<VehicleSaleBillHeader>();
            var dealersInRun = headers.Select(h => h.DealerCode!).Distinct().ToList();
            var sequences = await _context.NumberSequences.AsNoTracking()
                .Where(n => n.SequenceName == "sale_bill" && n.DealerCode != null && dealersInRun.Contains(n.DealerCode))
                .ToListAsync();
            var todayFy = DateTime.Today.Month >= 4 ? DateTime.Today.Year : DateTime.Today.Year - 1;

            CxSeries? SeriesFor(VehicleSaleBillHeader h)
            {
                var dealer = h.DealerCode!;
                var mine = sequences.Where(n => string.Equals(n.DealerCode, dealer, StringComparison.OrdinalIgnoreCase)).ToList();
                NumberSequence? Pick(IEnumerable<NumberSequence> list) =>
                    list.Where(n => n.BillingType == h.BillType).OrderByDescending(n => n.IsActive).ThenByDescending(n => n.CreatedDate).FirstOrDefault()
                    ?? list.Where(n => n.BillingType == null).OrderByDescending(n => n.IsActive).ThenByDescending(n => n.CreatedDate).FirstOrDefault()
                    ?? list.OrderByDescending(n => n.IsActive).ThenByDescending(n => n.CreatedDate).FirstOrDefault();
                var fyStart = h.SaleDate.Month >= 4 ? h.SaleDate.Year : h.SaleDate.Year - 1;
                // year of a prefix row: from its code if the code holds one, else from its Year column, else today's year
                int YearOf(NumberSequence n) => CxFyStartOf(n.SequenceCode) ?? CxFyStartOf(n.Year) ?? todayFy;

                // 1) a prefix created for the bill's own year
                var seq = Pick(mine.Where(n => YearOf(n) == fyStart));
                string? code = seq?.SequenceCode;
                if (seq == null)
                {
                    // 2) the active prefix, if its code carries a year that can be switched to the bill's year
                    var active = Pick(mine.Where(n => n.IsActive));
                    if (active != null && !string.IsNullOrWhiteSpace(active.SequenceCode) && CxFyToken.IsMatch(active.SequenceCode))
                    {
                        seq = active;
                        code = active.SequenceCode;
                    }
                }
                var isActiveYear = false;
                string pattern;
                if (seq == null || string.IsNullOrWhiteSpace(code))
                {
                    // 3) default series for that year
                    seq = null;
                    pattern = CxCodeForYear($"VSB/{(dealer.Length >= 3 ? dealer[^3..] : dealer)}/25-26/###", h.SaleDate);
                    noSequence(dealer, pattern);
                }
                else
                {
                    pattern = CxCodeForYear(code, h.SaleDate);
                    // the prefix row's own year: continue from its NextNo and move NextNo on
                    isActiveYear = YearOf(seq) == fyStart;
                }
                var digits = pattern.Count(c => c == '#');
                if (digits == 0) { pattern += "###"; digits = 3; }
                var hashes = new string('#', digits);
                var at = pattern.IndexOf(hashes, StringComparison.Ordinal);
                if (at < 0) return null;
                return new CxSeries(seq, pattern, pattern[..at], pattern[(at + digits)..], digits, isActiveYear);
            }

            // pass 1: target series of every bill; earlier imports already in the right series keep their number
            var target = new Dictionary<VehicleSaleBillHeader, CxSeries?>();
            foreach (var h in headers) target[h] = SeriesFor(h);
            var allSeries = target.Values.Where(t => t != null).Select(t => t!)
                .GroupBy(t => t.Pattern, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
            var wrong = headers.Where(h => keepIfFits.Contains(h) && target[h] != null && !CxFits(h.SaleBillNo, target[h]!.Head, target[h]!.Tail)).ToList();
            // series that hold wrong-year bills -> renumber the whole series (imported bills of this run) from the start
            var dirty = new HashSet<string>(allSeries.Where(t => wrong.Any(h => CxFits(h.SaleBillNo, t.Head, t.Tail))).Select(t => t.Pattern),
                StringComparer.OrdinalIgnoreCase);
            var vacating = new HashSet<string>(wrong.Select(h => h.SaleBillNo), StringComparer.OrdinalIgnoreCase);
            foreach (var h in headers.Where(h => keepIfFits.Contains(h) && target[h] != null && dirty.Contains(target[h]!.Pattern)))
                vacating.Add(h.SaleBillNo);

            // pass 2: numbers
            var next = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);   // pattern -> next number
            foreach (var h in headers)
            {
                var t = target[h];
                if (t == null) continue;
                if (keepIfFits.Contains(h) && !vacating.Contains(h.SaleBillNo) && CxFits(h.SaleBillNo, t.Head, t.Tail))
                {
                    done.Add(h);   // already right
                    continue;
                }

                if (!next.TryGetValue(t.Pattern, out var n))
                {
                    // highest number already used in this series (any dealer - Sale Bill No is unique),
                    // leaving out the numbers this run moves away
                    var used = await _context.VehicleSaleBillHeaders.AsNoTracking()
                        .Where(x => x.SaleBillNo.StartsWith(t.Head)).Select(x => x.SaleBillNo).ToListAsync();
                    var max = 0;
                    foreach (var u in used)
                    {
                        if (vacating.Contains(u) || !CxFits(u, t.Head, t.Tail)) continue;
                        if (int.TryParse(u.Substring(t.Head.Length, u.Length - t.Head.Length - t.Tail.Length), out var v) && v > max) max = v;
                    }
                    n = max + 1;
                    // NextNo of the prefix is respected unless this run rebuilds the series
                    if (t.IsActiveYear && !dirty.Contains(t.Pattern) && t.Seq!.NextNo > n) n = t.Seq.NextNo;
                }

                var no = t.Head + n.ToString().PadLeft(t.Digits, '0') + t.Tail;
                if (no.Length > 50) continue;
                h.SaleBillNo = no;
                done.Add(h);
                n += Math.Max(1, t.Seq?.Increment ?? 1);
                next[t.Pattern] = n;
                if (t.IsActiveYear)
                    foreach (var s in sequences.Where(x => string.Equals(x.SequenceCode, t.Seq!.SequenceCode, StringComparison.OrdinalIgnoreCase)
                                                           && string.Equals(x.DealerCode, h.DealerCode, StringComparison.OrdinalIgnoreCase)))
                        sequenceUpdates[s.Id] = Math.Max(sequenceUpdates.TryGetValue(s.Id, out var cur) ? cur : 0, n);
            }
            return done;
        }

        private sealed record CxReadyBill(VehicleSaleBillHeader Header, LedgerMaster? NewLedger, List<CxImportRow> Rows, int? ExistingId, bool ExplicitTotal);

        /// <summary>Customer-master values resolved against City / State / Occupation masters.</summary>
        private sealed class CxCustomerInfo
        {
            public string? Address, Address2, Email, Pin, Gender;
            public int? CityId, StateId, OccupationId;
            public DateOnly? Dob;
            public string? UnmatchedCity, UnmatchedState, UnmatchedOccupation;
            public bool HasAny => Address != null || Address2 != null || Email != null || Pin != null || Gender != null || CityId != null
                                  || StateId != null || OccupationId != null || Dob != null;
        }

        private static CxCustomerInfo CxCustomerInfoFor(CxImportRow r, Dictionary<string, int> stateByName,
            List<(int CityId, string Norm, int StateId)> cities, Dictionary<string, int> occupationByName, Action<string> warn)
        {
            var info = new CxCustomerInfo
            {
                Address = CxCut(string.Join(", ", new[] { r.CustomerAddress, r.CustomerAddress2 }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim())), 250),
                Email = CxCut(r.CustomerEmail, 50),
                Pin = CxDigits(r.CustomerPin) is { Length: 6 } pin ? pin : null,
                Gender = CxGender(r.Gender),
                Dob = r.Dob is { } dob && dob.Year >= 1920 && dob.Date < DateTime.Today ? DateOnly.FromDateTime(dob) : null
            };
            // "0" / "000000" = no PIN in Cogxim -> nothing to report. A short PIN (digit missing in Cogxim) is not
            // guessed: Ledger PIN stays blank, the Cogxim value is kept in Narration ("Pin (incomplete): 41108").
            if (!string.IsNullOrWhiteSpace(r.CustomerPin) && info.Pin == null && CxDigits(r.CustomerPin).Trim('0').Length > 0)
                warn($"{CxInfoTag}Pin \"{r.CustomerPin}\" in Cogxim is incomplete (not 6 digits) - customer PIN left blank, Cogxim value kept in Narration. Correct it in Customer / Ledger Master.");

            if (!string.IsNullOrWhiteSpace(r.CustomerState))
            {
                if (CxFindState(stateByName, r.CustomerState!) is { } sid) info.StateId = sid;
                else { info.UnmatchedState = r.CustomerState!.Trim(); warn($"State \"{r.CustomerState}\" not in State master - shown from Narration."); }
            }
            if (!string.IsNullOrWhiteSpace(r.CustomerCity))
            {
                var n = CxNorm(r.CustomerCity);
                var hits = cities.Where(c => c.Norm == n).ToList();
                var pick = hits.FirstOrDefault(c => info.StateId != null && c.StateId == info.StateId);
                if (pick.CityId == 0 && hits.Count == 1) pick = hits[0];
                if (pick.CityId != 0)
                {
                    info.CityId = pick.CityId;
                    info.StateId ??= pick.StateId;
                }
                else
                {
                    info.UnmatchedCity = r.CustomerCity!.Trim();
                    warn(hits.Count > 1 ? $"City \"{r.CustomerCity}\" exists in more than one state - add the State column." : $"City \"{r.CustomerCity}\" not in City master - shown from Narration.");
                }
            }
            if (!string.IsNullOrWhiteSpace(r.Occupation))
            {
                if (occupationByName.TryGetValue(CxNorm(r.Occupation), out var oid)) info.OccupationId = oid;
                else { info.UnmatchedOccupation = r.Occupation!.Trim(); warn($"Occupation \"{r.Occupation}\" not in Occupation master - shown from Narration."); }
            }
            if (!string.IsNullOrWhiteSpace(r.Gender) && info.Gender == null) warn($"Gender \"{r.Gender}\" not recognised - not saved.");
            return info;
        }

        private static readonly Dictionary<string, string> CxStateAlias = new()
        {
            ["lakhswadeep"] = "lakshadweep", ["lakshwadeep"] = "lakshadweep", ["orissa"] = "odisha", ["pondicherry"] = "puducherry",
            ["uttaranchal"] = "uttarakhand", ["chattisgarh"] = "chhattisgarh", ["newdelhi"] = "delhi", ["nctofdelhi"] = "delhi",
            ["jammukashmir"] = "jammuandkashmir", ["andamannicobar"] = "andamanandnicobarislands"
        };

        /// <summary>State by exact name, known spelling alias, else one master name that contains the file name or vice versa
        /// (e.g. "DAMAN AND DIU" -> "Dadra and Nagar Haveli and Daman and Diu").</summary>
        private static int? CxFindState(Dictionary<string, int> stateByName, string raw)
        {
            var n = CxNorm(raw);
            if (n.Length == 0) return null;
            if (stateByName.TryGetValue(n, out var id)) return id;
            if (CxStateAlias.TryGetValue(n, out var alias) && stateByName.TryGetValue(alias, out id)) return id;
            if (n.Length < 4) return null;
            var hits = stateByName.Where(kv => kv.Key.Contains(n) || n.Contains(kv.Key)).Select(kv => kv.Value).Distinct().ToList();
            return hits.Count == 1 ? hits[0] : null;
        }

        /// <summary>Copies customer values to a ledger. onlyBlank = never overwrite what is already in the ledger.</summary>
        private static bool CxFillLedger(LedgerMaster l, CxCustomerInfo i, bool onlyBlank = false)
        {
            var changed = false;
            void Set<T>(Func<T?> get, Action<T?> set, T? value)
            {
                if (value == null) return;
                var cur = get();
                if (onlyBlank && cur != null && !(cur is string s && string.IsNullOrWhiteSpace(s))) return;
                if (Equals(cur, value)) return;
                set(value); changed = true;
            }
            Set(() => l.Address, v => l.Address = v, i.Address);
            if (i.Address2 != null && Equals(l.Address, i.Address))   // overflow only goes with its own first part
                Set(() => l.Address2, v => l.Address2 = v, i.Address2);
            Set(() => l.EMail, v => l.EMail = v, i.Email);
            Set(() => l.Pin, v => l.Pin = v, i.Pin);
            Set(() => l.Gender, v => l.Gender = v, i.Gender);
            Set(() => l.City, v => l.City = v, i.CityId);
            Set(() => l.State, v => l.State = v, i.StateId);
            Set(() => l.OccupationId, v => l.OccupationId = v, i.OccupationId);
            Set(() => l.DateOfBirth, v => l.DateOfBirth = v, i.Dob);
            return changed;
        }

        private static string? CxGender(string? raw)
        {
            var v = CxNorm(raw);
            return v switch
            {
                "" => null,
                "m" or "male" or "mr" => "Male",
                "f" or "female" or "ms" or "mrs" => "Female",
                "o" or "other" or "others" => "Other",
                _ => null
            };
        }

        private CxImportRow CxParseRow(LargeExcelRow xr, int rowNo, Dictionary<string, int> colOf, string? defaultDealer)
        {
            var r = new CxImportRow { RowNo = rowNo };
            string? S(string key) => colOf.TryGetValue(key, out var c) ? CxText(xr.Cell(c)) : null;
            decimal? N(string key)
            {
                if (!colOf.TryGetValue(key, out var c)) return null;
                var v = CxNumber(xr.Cell(c), out var ok);
                if (!ok) r.Errors.Add($"\"{CxText(xr.Cell(c))}\" in {CogximFields.First(f => f.Key == key).Label} is not a number.");
                return v;
            }
            DateTime? D(string key, out string? raw)
            {
                raw = null;
                if (!colOf.TryGetValue(key, out var c)) return null;
                raw = CxText(xr.Cell(c));
                return CxDate(xr.Cell(c));
            }

            r.DealerCode = S("dealerCode") ?? defaultDealer?.Trim();
            if (string.IsNullOrWhiteSpace(r.DealerCode)) r.DealerCode = defaultDealer?.Trim();
            r.SaleBillNo = S("saleBillNo");
            r.SaleDate = D("saleDate", out var raw); r.SaleDateRaw = raw;
            r.BookingId = S("bookingId");
            r.CustomerName = S("customerName");
            r.BillingName = S("billingName");
            r.CustomerMobile = S("customerMobile");
            r.CustomerEmail = S("customerEmail");
            r.CustomerAddress = S("customerAddress");
            r.CustomerAddress2 = S("customerAddress2");
            r.CustomerCity = S("customerCity");
            r.CustomerState = S("customerState");
            r.CustomerPin = S("customerPin");
            r.Gender = S("gender");
            r.Dob = D("dob", out _);
            r.Occupation = S("occupation");
            r.CustomerGstin = S("customerGstin");
            r.Location = S("location");
            r.SalesExecutive = S("salesExecutive");
            r.SaleType = S("saleType");
            r.Financier = S("financier");
            r.CustomerType = S("customerType");
            r.TempRegNo = S("tempRegNo");
            r.AccessoryBillNo = S("accessoryBillNo");
            r.AccessoryAmount = N("accessoryAmount");
            r.HpAmount = N("hpAmount");
            r.NoPlateAmount = N("noPlateAmount");
            r.HandlingCharges = N("handlingCharges");
            r.StateSubsidyAmount = N("stateSubsidyAmount");
            r.TotalAmount = N("totalAmount");

            r.ChassisNo = S("chassisNo")?.Replace(" ", "").ToUpperInvariant();
            r.ModelName = S("modelName");
            r.OemModel = S("oemModel");
            r.ItemCode = S("itemCode");
            r.Hsn = S("hsn");
            r.Colour = S("colour");
            var year = N("mfgYear");
            r.MfgYear = year is >= 1990 and <= 2100 ? (int)year.Value : null;
            r.ItemRate = N("itemRate");
            r.PreGst = N("preGstDiscount");
            r.PostGst = N("postGstDiscount");
            r.FameII = N("fameII");
            r.SgstPer = N("sgstPer"); r.SgstAmt = N("sgstAmount");
            r.CgstPer = N("cgstPer"); r.CgstAmt = N("cgstAmount");
            r.IgstPer = N("igstPer"); r.IgstAmt = N("igstAmount");
            r.RegAmount = N("regAmount");
            r.InsAmount = N("insuranceAmount");
            r.FinalAmount = N("finalAmount");
            r.RegNo = S("regNo");
            r.InsNo = S("insNo");
            r.InsStartDate = D("insStartDate", out _);
            r.InsExpDate = D("insExpDate", out _);
            r.MotorNo = S("motorNo");
            r.Battery = S("battery");
            r.BatteryMake = S("batteryMake");
            r.BatteryChemical = S("batteryChemical");
            r.BatteryCapacity = S("batteryCapacity");
            r.ChargerNo = S("chargerNo");
            r.ControllerNo = S("controllerNo");
            r.ConvertorNo = S("convertorNo");
            r.Vcu = S("vcu");
            r.KeyNo = S("keyNo");
            r.BookNo = S("bookNo");
            r.Segment = S("segment");
            r.InstitutionalType = S("institutionalType");
            r.SchemeName = S("schemeName");
            r.SourceRowId = S("sourceRowId");
            r.Narration = S("narration");
            return r;
        }

        private static VehicleSaleBillDetail CxBuildDetail(CxImportRow r, Dictionary<string, string> itemByCode, Dictionary<string, string> itemByName,
            string userId, string sheet, string? unmatchedFinancier, CxCustomerInfo info, Action<string> warn)
        {
            // ItemCode: explicit code, else match model / OEM model name against Item Master
            string? itemCode = null;
            if (!string.IsNullOrWhiteSpace(r.ItemCode) && itemByCode.TryGetValue(CxNorm(r.ItemCode), out var c1)) itemCode = c1;
            else if (!string.IsNullOrWhiteSpace(r.ModelName) && itemByName.TryGetValue(CxNorm(r.ModelName), out var c2)) itemCode = c2;
            else if (!string.IsNullOrWhiteSpace(r.OemModel) && itemByName.TryGetValue(CxNorm(r.OemModel), out var c3)) itemCode = c3;
            if (itemCode == null)
            {
                itemCode = r.ItemCode;
                warn($"Model \"{r.ModelName ?? r.OemModel ?? r.ItemCode}\" not found in Item Master and not created (switch \"Create missing masters\" on) - shown as Unmapped.");
            }

            // DMS FinalAmount = Rate - PreGST disc + GST + Regn + Insu - PostGST disc - Subsidy (Sale Bill screen formula)
            var rate = r.ItemRate ?? 0;
            var pre = r.PreGst ?? 0;
            var taxable = rate - pre;
            decimal Tax(decimal? amt, decimal? per) => amt ?? Math.Round(taxable * (per ?? 0) / 100m, 2);
            var sgst = Tax(r.SgstAmt, r.SgstPer);
            var cgst = Tax(r.CgstAmt, r.CgstPer);
            var igst = Tax(r.IgstAmt, r.IgstPer);
            var reg = r.RegAmount ?? 0;
            var ins = r.InsAmount ?? 0;
            var final = Math.Round(taxable + sgst + cgst + igst + reg + ins - (r.PostGst ?? 0) - (r.FameII ?? 0), 2);
            if (final <= 0) warn($"{CxInfoTag}Net Amount is 0 in Cogxim (Rate {rate:0.00}, Discount {pre:0.00}) - imported as is; check if this was a free / replacement vehicle.");
            if (r.FinalAmount is { } fileNet && Math.Abs(fileNet - final) > 1m && Math.Abs(fileNet + reg + ins - final) > 1m)
                warn($"Net Amnt in file {fileNet:0.00} differs from calculated {final:0.00} - calculated amount saved.");

            // Narration: values that have no column in VehicleSaleBillDetails; the sale report reads them back ("Tag: value").
            var notes = new List<string>();
            void Tag(string tag, string? v) { if (!string.IsNullOrWhiteSpace(v)) notes.Add($"{tag}: {v!.Trim()}"); }
            Tag("Motor", r.MotorNo);
            if (info.Pin == null && CxDigits(r.CustomerPin).Trim('0').Length > 0) Tag("Pin (incomplete)", r.CustomerPin);
            Tag("HSN", r.Hsn);
            Tag("Financier", unmatchedFinancier);
            Tag("City", info.UnmatchedCity);
            Tag("State", info.UnmatchedState);
            Tag("Occupation", info.UnmatchedOccupation);
            Tag("Location", r.UnmatchedLocation);
            notes.Add(!string.IsNullOrWhiteSpace(r.SourceRowId) ? $"Cogxim ImportRowId {r.SourceRowId}" : $"Cogxim {sheet} row {r.RowNo}");
            if (!string.IsNullOrWhiteSpace(r.Narration) && !r.Narration!.StartsWith("Cogxim ImportRowId", StringComparison.OrdinalIgnoreCase))
                notes.Add(r.Narration!.Trim());
            else if (!string.IsNullOrWhiteSpace(r.Narration) && string.IsNullOrWhiteSpace(r.SourceRowId))
                notes[notes.Count - 1] = r.Narration!.Trim();     // template: narration already carries "Cogxim ImportRowId n | remarks"

            return new VehicleSaleBillDetail
            {
                ChassisNo = r.ChassisNo!,
                ItemCode = CxCut(itemCode, 50),
                ModelName = CxCut(r.ModelName ?? r.OemModel, 100),
                Colour = CxCut(r.Colour, 50),
                MfgYear = r.MfgYear,
                ItemRate = rate,
                PreGstDiscount = pre,
                PostGstDisc = r.PostGst ?? 0,
                FameIi = r.FameII ?? 0,
                Sgstper = r.SgstPer, Sgstamnt = sgst,
                Cgstper = r.CgstPer, Cgstamnt = cgst,
                Igstper = r.IgstPer, Igstamnt = igst,
                RegAmount = reg,
                InsuranceAmount = ins,
                FinalAmount = final,
                RegNo = CxCut(r.RegNo, 50),
                InsNo = CxCut(r.InsNo, 50),
                InsStartDate = r.InsStartDate,
                InsExpDate = r.InsExpDate,
                Battery = CxCut(r.Battery, 50),
                BatteryMake = CxCut(r.BatteryMake, 50),
                BatteryChemical = CxCut(r.BatteryChemical, 50),
                BatteryCapacity = CxCut(r.BatteryCapacity, 50),
                ChargerNo = CxCut(r.ChargerNo, 50),
                ControllerNo = CxCut(r.ControllerNo, 50),
                ConvertorNo = CxCut(r.ConvertorNo, 50),
                Vcu = CxCut(r.Vcu, 50),
                Key = CxCut(r.KeyNo, 50),
                BookNo = CxCut(r.BookNo, 50),
                Segment = CxCut(r.Segment, 50),
                InstitutionalType = CxCut(r.InstitutionalType, 50),
                SchemeName = CxCut(r.SchemeName, 100),
                Narration = CxCut(string.Join(" | ", notes), 255),
                IsDelivered = true,
                HasDevice = false,
                HasKit = false,
                IsAgainstExchange = false,
                CreatedBy = CxCut(userId, 100)!,
                CreatedDate = DateTime.Now
            };
        }

        private static decimal CxBillTotal(VehicleSaleBillHeader h, IEnumerable<VehicleSaleBillDetail> details)
            => details.Sum(x => x.FinalAmount) + (h.AccessoryAmount ?? 0) + (h.NoPlateAmount ?? 0)
               + (h.HandlingCharges ?? 0) + (h.Hpamount ?? 0) - (h.StateSubsidyAmount ?? 0);

        /// <summary>Copies the file's values onto an already-imported bill. Vehicles are matched by chassis;
        /// new chassis are added, vehicles not in the file are left as they are. Returns vehicles touched.</summary>
        private static int CxApplyUpdate(VehicleSaleBillHeader db, VehicleSaleBillHeader src, bool explicitTotal, string userId)
        {
            var now = DateTime.Now;
            db.SaleBillNo = src.SaleBillNo;
            db.InvoiceNo = src.InvoiceNo ?? db.InvoiceNo;
            db.SaleDate = src.SaleDate;
            db.Location = src.Location;
            db.LedgerId ??= src.LedgerId;
            db.CustomerName = src.CustomerName;
            db.BillingName = src.BillingName;
            db.CustomerType = src.CustomerType;
            db.BillType = src.BillType;
            db.SaleType = src.SaleType;
            db.Financier = src.Financier;
            db.SalesExecutive = src.SalesExecutive;
            db.BookingId = src.BookingId;
            db.TempRegNo = src.TempRegNo ?? db.TempRegNo;
            db.AccessoryBillNo = src.AccessoryBillNo ?? db.AccessoryBillNo;
            db.AccessoryAmount = src.AccessoryAmount;
            db.Hpamount = src.Hpamount;
            db.NoPlateAmount = src.NoPlateAmount;
            db.HandlingCharges = src.HandlingCharges;
            db.StateSubsidyAmount = src.StateSubsidyAmount;
            db.UpdatedBy = CxCut(userId, 100);
            db.UpdatedDate = now;

            var touched = 0;
            foreach (var s in src.VehicleSaleBillDetails)
            {
                var d = db.VehicleSaleBillDetails.FirstOrDefault(x => string.Equals(x.ChassisNo, s.ChassisNo, StringComparison.OrdinalIgnoreCase));
                if (d == null)
                {
                    db.VehicleSaleBillDetails.Add(s);
                    touched++;
                    continue;
                }
                d.ItemCode = s.ItemCode; d.ModelName = s.ModelName; d.Colour = s.Colour; d.MfgYear = s.MfgYear ?? d.MfgYear;
                d.ItemRate = s.ItemRate; d.PreGstDiscount = s.PreGstDiscount; d.PostGstDisc = s.PostGstDisc; d.FameIi = s.FameIi;
                d.Sgstper = s.Sgstper; d.Sgstamnt = s.Sgstamnt; d.Cgstper = s.Cgstper; d.Cgstamnt = s.Cgstamnt;
                d.Igstper = s.Igstper; d.Igstamnt = s.Igstamnt;
                d.RegAmount = s.RegAmount; d.InsuranceAmount = s.InsuranceAmount; d.FinalAmount = s.FinalAmount;
                d.RegNo = s.RegNo ?? d.RegNo; d.InsNo = s.InsNo ?? d.InsNo;
                d.InsStartDate = s.InsStartDate ?? d.InsStartDate; d.InsExpDate = s.InsExpDate ?? d.InsExpDate;
                d.Battery = s.Battery; d.BatteryMake = s.BatteryMake; d.BatteryChemical = s.BatteryChemical; d.BatteryCapacity = s.BatteryCapacity;
                d.ChargerNo = s.ChargerNo; d.ControllerNo = s.ControllerNo; d.ConvertorNo = s.ConvertorNo; d.Vcu = s.Vcu;
                d.Key = s.Key ?? d.Key; d.BookNo = s.BookNo ?? d.BookNo;
                d.Segment = s.Segment; d.InstitutionalType = s.InstitutionalType; d.SchemeName = s.SchemeName;
                d.Narration = s.Narration;
                d.UpdatedBy = CxCut(userId, 100);
                d.UpdatedDate = now;
                touched++;
            }
            db.TotalAmount = explicitTotal ? src.TotalAmount : CxBillTotal(db, db.VehicleSaleBillDetails);
            return touched;
        }

        private const string CxMasterCreatedBy = "Cogxim Import";
        private const string CxInfoTag = "[Info] ";

        /// <summary>
        /// Creates the masters the file refers to but DMS does not have, and adds them to the lookups so the
        /// bills pick them up: OEMModelMaster, ItemMaster (inactive legacy model, settings copied from the most-sold
        /// vehicle item of the same family), Financier ledger (per dealer, LED/xxxx/n), City (under its State), Occupation.
        /// Dry run: nothing is written, the lookups get temporary negative ids and the result shows what would be created.
        /// </summary>
        private async Task CxCreateMissingMasters(List<CxImportRow> rows, bool dryRun, string userId, VehicleSaleImportResult result,
            Action<string> warn,
            Dictionary<string, int> occupationByName, Dictionary<string, int> stateByName, List<(int CityId, string Norm, int StateId)> cityList,
            List<(int Id, string Norm, string Dealer)> financierList, Dictionary<string, int> financierByDealer, Dictionary<string, int> financiers,
            Func<string, string?, int?> findFinancier,
            Dictionary<string, string> itemByCode, Dictionary<string, string> itemByName)
        {
            var fakeId = -1;
            var createdBy = CxCut(userId, 100) ?? CxMasterCreatedBy;
            void Note(string s) { if (result.MastersCreated.Count < 1000) result.MastersCreated.Add(s); }
            static string Clean(string s) => System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");

            // ---- Occupation ----
            var newOcc = rows.Where(r => !string.IsNullOrWhiteSpace(r.Occupation) && CxNorm(r.Occupation).Length > 0
                                         && !occupationByName.ContainsKey(CxNorm(r.Occupation)))
                .GroupBy(r => CxNorm(r.Occupation)).Select(g => Clean(g.First().Occupation!)).ToList();
            if (newOcc.Count > 0)
            {
                var ents = newOcc.Select(n => new OccupationMaster { OccupationName = CxCut(n, 100)!, IsActive = true, CreatedBy = createdBy, CreatedDate = DateTime.Now }).ToList();
                if (await CxSaveMasters(ents, dryRun, "Occupation", warn))
                    foreach (var e in ents)
                    {
                        occupationByName[CxNorm(e.OccupationName)] = dryRun ? fakeId-- : e.Id;
                        result.OccupationsCreated++; Note($"Occupation: {e.OccupationName}");
                    }
            }

            // ---- City (only when its State is known) ----
            var newCity = new Dictionary<string, City>();
            var cityState = new Dictionary<City, string>();
            foreach (var r in rows.Where(r => !string.IsNullOrWhiteSpace(r.CustomerCity) && !string.IsNullOrWhiteSpace(r.CustomerState)))
            {
                var n = CxNorm(r.CustomerCity);
                if (n.Length == 0 || CxFindState(stateByName, r.CustomerState!) is not { } sid) continue;
                var hits = cityList.Where(c => c.Norm == n).ToList();
                if (hits.Any(c => c.StateId == sid)) continue;              // already there for this state
                var key = sid + "|" + n;
                if (newCity.ContainsKey(key)) continue;
                var c = new City { CityName = CxCut(Clean(r.CustomerCity!), 100)!, StateId = sid, IsActive = true, CreatedBy = createdBy, CreatedDate = DateTime.Now };
                newCity[key] = c;
                cityState[c] = Clean(r.CustomerState!);
            }
            if (newCity.Count > 0 && await CxSaveMasters(newCity.Values.ToList(), dryRun, "City", warn))
                foreach (var c in newCity.Values)
                {
                    cityList.Add((dryRun ? fakeId-- : c.CityId, CxNorm(c.CityName), c.StateId));
                    result.CitiesCreated++; Note($"City: {c.CityName} ({cityState[c]})");
                }

            // ---- Financier ledger, per dealer ----
            var newFin = new Dictionary<string, LedgerMaster>();
            foreach (var r in rows.Where(r => !string.IsNullOrWhiteSpace(r.Financier)))
            {
                var dKey = r.DealerCode!.ToUpperInvariant();
                var n = CxNorm(r.Financier);
                if (n.Length == 0 || newFin.ContainsKey(dKey + "|" + n) || findFinancier(dKey, r.Financier) != null) continue;
                newFin[dKey + "|" + n] = new LedgerMaster
                {
                    LedgerName = CxCut(Clean(r.Financier!), 100),
                    LedgerType = "Financier",
                    DealerCode = r.DealerCode,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
            }
            if (newFin.Count > 0)
            {
                if (!dryRun)
                    foreach (var grp in newFin.Values.GroupBy(l => l.DealerCode!))
                    {
                        var next = await CxNextLedgerNumber(grp.Key);
                        var suffix = grp.Key.Length >= 4 ? grp.Key[^4..] : grp.Key;
                        foreach (var l in grp) l.LedgerCode = $"LED/{suffix}/{next++}";
                    }
                if (await CxSaveMasters(newFin.Values.ToList(), dryRun, "Financier ledger", warn))
                    foreach (var l in newFin.Values)
                    {
                        var id = dryRun ? fakeId-- : l.Id;
                        var dKey = l.DealerCode!.ToUpperInvariant();
                        var n = CxNorm(l.LedgerName);
                        financierList.Add((id, n, dKey));
                        financierByDealer[dKey + "|" + n] = id;
                        financiers.TryAdd(n, id);
                        result.FinanciersCreated++; Note($"Financier ({l.DealerCode}): {l.LedgerName}");
                    }
            }

            // ---- OEM Model Master ----
            var oemNames = await _context.OemmodelMasters.AsNoTracking().Select(o => new { o.ModelName, o.ModelShortName, o.CreatedBy, o.Id }).ToListAsync();
            var oemKnown = new HashSet<string>(oemNames.SelectMany(o => new[] { CxNorm(o.ModelName), CxNorm(o.ModelShortName) }).Where(x => x.Length > 0));
            var newOem = rows.Where(r => !string.IsNullOrWhiteSpace(r.OemModel) && CxNorm(r.OemModel).Length > 0 && !oemKnown.Contains(CxNorm(r.OemModel)))
                .GroupBy(r => CxNorm(r.OemModel)).Select(g => Clean(g.First().OemModel!)).ToList();
            if (newOem.Count > 0)
            {
                var oemBy = int.TryParse(userId, out var uid) ? uid : oemNames.OrderByDescending(o => o.Id).Select(o => o.CreatedBy).FirstOrDefault();
                var ents = newOem.Select(n => new OemmodelMaster { ModelName = CxCut(n, 150)!, ModelShortName = CxCut(n, 150), IsActive = true, CreatedBy = oemBy, CreatedDate = DateTime.Now }).ToList();
                if (await CxSaveMasters(ents, dryRun, "OEM Model", warn))
                    foreach (var e in ents) { result.OemModelsCreated++; Note($"OEM Model: {e.ModelName}"); }
            }

            // ---- Item Master: legacy model (inactive, prices 0) ----
            string? Resolve(CxImportRow r) =>
                !string.IsNullOrWhiteSpace(r.ItemCode) && itemByCode.TryGetValue(CxNorm(r.ItemCode), out var c1) ? c1
                : !string.IsNullOrWhiteSpace(r.ModelName) && itemByName.TryGetValue(CxNorm(r.ModelName), out var c2) ? c2
                : !string.IsNullOrWhiteSpace(r.OemModel) && itemByName.TryGetValue(CxNorm(r.OemModel), out var c3) ? c3
                : null;
            var missing = rows.Where(r => Resolve(r) == null && CxNorm(r.ModelName ?? r.OemModel).Length > 0)
                .GroupBy(r => CxNorm(r.ModelName ?? r.OemModel)).ToList();
            if (missing.Count == 0) return;

            // templates: vehicle items ranked by how often they were sold
            var sold = await _context.VehicleSaleBillDetails.AsNoTracking().Where(d => d.ItemCode != null)
                .GroupBy(d => d.ItemCode).Select(g => new { Code = g.Key, N = g.Count() })
                .OrderByDescending(x => x.N).Take(300).ToListAsync();
            var soldCodes = sold.Select(s => s.Code).ToList();
            var templates = await _context.ItemMasters.AsNoTracking().Where(i => soldCodes.Contains(i.Itemcode)).ToListAsync();
            var rank = sold.GroupBy(s => s.Code!, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.Sum(x => x.N), StringComparer.OrdinalIgnoreCase);
            templates = templates.OrderByDescending(t => rank.TryGetValue(t.Itemcode, out var k) ? k : 0).ToList();
            if (templates.Count == 0)
                templates = await _context.ItemMasters.AsNoTracking().Where(i => i.Iselectric).OrderByDescending(i => i.Id).Take(50).ToListAsync();
            if (templates.Count == 0)
            {
                warn("No vehicle item in Item Master to copy settings from - missing models were not created.");
                return;
            }

            var usedCodes = new HashSet<string>(itemByCode.Keys);
            var newItems = new List<(ItemMaster Item, string? Model, string? Oem)>();
            foreach (var g in missing)
            {
                var r = g.First();
                var model = Clean(r.ModelName ?? r.OemModel!);
                var oem = string.IsNullOrWhiteSpace(r.OemModel) ? null : Clean(r.OemModel!);

                // family = first token with a digit (C12, B8, D15, A2 ...), else first word; "BGAUSS" ignored
                var tokens = System.Text.RegularExpressions.Regex.Split(model.ToUpperInvariant(), "[^A-Z0-9]+")
                    .Where(t => t.Length > 0 && t != "BGAUSS").ToList();
                var family = CxNorm(tokens.FirstOrDefault(t => t.Any(char.IsDigit)) ?? tokens.FirstOrDefault());
                var tpl = (family.Length > 0
                              ? templates.FirstOrDefault(t => CxNorm(t.Itemname).Contains(family) || CxNorm(t.Displayname).Contains(family) || CxNorm(t.Oemmodelname).Contains(family))
                              : null)
                          ?? templates[0];

                var baseCode = ("CGX-" + CxNorm(model).ToUpperInvariant());
                baseCode = baseCode.Length > 95 ? baseCode[..95] : baseCode;
                var code = baseCode;
                for (var k = 2; usedCodes.Contains(CxNorm(code)); k++) code = $"{baseCode}-{k}";
                usedCodes.Add(CxNorm(code));

                var hsn = CxCut(g.Select(x => x.Hsn).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim(), 100);
                decimal? Pct(Func<CxImportRow, decimal?> f) => g.Select(f).FirstOrDefault(v => v is >= 0 and <= 100);
                var item = new ItemMaster
                {
                    DealerCode = null,
                    Itemtype = tpl.Itemtype,
                    Itemname = CxCut(model, 100)!,
                    Itemcode = code,
                    Itemdesc = CxCut(oem ?? model, 200)!,
                    Status = false,
                    Hsncode = hsn ?? tpl.Hsncode,
                    HsncodeId = hsn == null || string.Equals(hsn, tpl.Hsncode, StringComparison.OrdinalIgnoreCase) ? tpl.HsncodeId : null,
                    Dlrprice = 0, Custprice = 0, Ipurrate = 0, Fame2amount = 0,
                    Moq = tpl.Moq, Boq = tpl.Boq,
                    Sgst = Pct(x => x.SgstPer) ?? tpl.Sgst,
                    Cgst = Pct(x => x.CgstPer) ?? tpl.Cgst,
                    Igst = Pct(x => x.IgstPer) ?? tpl.Igst,
                    Ugst = tpl.Ugst,
                    Grpidno = tpl.Grpidno,
                    Iselectric = tpl.Iselectric,
                    Vehtype = tpl.Vehtype,
                    Noofbatteries = tpl.Noofbatteries,
                    Rrgitemidno = tpl.Rrgitemidno,
                    Itemcc = tpl.Itemcc,
                    Batterytypeidno = tpl.Batterytypeidno,
                    Compcode = tpl.Compcode,
                    Displayname = CxCut(model, 100),
                    Oemmodelname = CxCut(oem ?? model, 100),
                    MinBillQty = tpl.MinBillQty, MinOrderQty = tpl.MinOrderQty,
                    WarrantyPeriod = tpl.WarrantyPeriod, WarrantyDurationType = tpl.WarrantyDurationType, WarrantyKms = tpl.WarrantyKms,
                    IsWarrantyApproval = tpl.IsWarrantyApproval, Vorrate = tpl.Vorrate, IsVor = tpl.IsVor,
                    Remarks = "Cogxim legacy model - created by the sale history import (inactive)",
                    IsExempted = tpl.IsExempted, IsToolkitFirstAid = tpl.IsToolkitFirstAid, IsStockRequired = false,
                    IsHelmet = tpl.IsHelmet, IsInventory = false, IsInEligibleInput = tpl.IsInEligibleInput,
                    Uom = tpl.Uom, SupplierId = tpl.SupplierId,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
                newItems.Add((item, r.ModelName, r.OemModel));
            }
            if (await CxSaveMasters(newItems.Select(x => x.Item).ToList(), dryRun, "Item Master model", warn))
                foreach (var (item, m, o) in newItems)
                {
                    itemByCode[CxNorm(item.Itemcode)] = item.Itemcode;
                    foreach (var n in new[] { m, o, item.Itemname })
                        if (!string.IsNullOrWhiteSpace(n)) itemByName.TryAdd(CxNorm(n), item.Itemcode);
                    result.ItemModelsCreated++; Note($"Item: {item.Itemcode} - {item.Itemname}");
                }
        }

        /// <summary>Saves one kind of master in a single batch. Returns false (with a warning) when the database rejects it.</summary>
        private async Task<bool> CxSaveMasters<T>(List<T> entities, bool dryRun, string what, Action<string> warn) where T : class
        {
            if (dryRun || entities.Count == 0) return true;
            try
            {
                _context.Set<T>().AddRange(entities);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                warn($"{what}: could not create {entities.Count} record(s) - {ex.GetBaseException().Message}");
                return false;
            }
            finally
            {
                _context.ChangeTracker.Clear();
            }
        }

        private async Task<int> CxNextLedgerNumber(string dealerCode)
        {
            // Same LED/{last4}/{n} series as LedgerMasterRepo.GetNextLedCode
            var codes = await _context.LedgerMasters.AsNoTracking()
                .Where(x => x.DealerCode == dealerCode && x.LedgerCode != null && x.LedgerCode.StartsWith("LED/"))
                .Select(x => x.LedgerCode!).ToListAsync();
            var max = 0;
            foreach (var c in codes)
            {
                var p = c.Split('/');
                if (p.Length == 3 && int.TryParse(p[2], out var n) && n > max) max = n;
            }
            return max + 1;
        }

        private static void CxResetKeys(VehicleSaleBillHeader h)
        {
            h.Id = 0;
            foreach (var d in h.VehicleSaleBillDetails) { d.Id = 0; d.VehicleSaleBillId = 0; }
        }

        private static string CxNormaliseSaleType(string? raw, string? financier)
        {
            var v = (raw ?? "").Trim().ToLowerInvariant();
            if (v.Contains("cash")) return "Cash";
            if (v.Contains("credit") || v.Contains("financ") || v.Contains("loan") || v.Contains("emi")) return "Credit";
            return string.IsNullOrWhiteSpace(financier) ? "Cash" : "Credit";
        }

        /// <summary>Opens the workbook with the streaming reader: no 32,767-character cell limit, comments / pictures /
        /// pivot tables ignored, low memory for very large year sheets (only the chosen sheet is read).</summary>
        private static LargeExcelWorkbook CxOpen(System.IO.Stream excel)
        {
            try { return new LargeExcelWorkbook(excel); }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                throw new ArgumentException("This file is not a valid .xlsx workbook (" + ex.GetBaseException().Message +
                    "). Open it in Excel and Save As \"Excel Workbook (.xlsx)\".");
            }
        }

        private static LargeExcelSheet CxPickSheet(LargeExcelWorkbook wb, string? sheetName)
        {
            if (!string.IsNullOrWhiteSpace(sheetName))
                return wb.Worksheets.FirstOrDefault(s => string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                       ?? throw new ArgumentException($"Sheet \"{sheetName}\" not found.");
            return wb.Worksheets.FirstOrDefault(s => s.Name.Contains("cogxim", StringComparison.OrdinalIgnoreCase)
                                                    || s.Name.Contains("saleregister", StringComparison.OrdinalIgnoreCase))
                   ?? wb.Worksheets.First();
        }

        private sealed record CxHeaderCol(string Title, int Column);

        private static List<CxHeaderCol> CxReadHeaders(LargeExcelSheet ws, int headerRow)
        {
            var list = new List<CxHeaderCol>();
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var row = ws.Row(headerRow);
            var last = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
            for (var c = 1; c <= last; c++)
            {
                var title = CxText(row.Cell(c));
                if (string.IsNullOrWhiteSpace(title)) continue;
                if (seen.TryGetValue(title, out var n)) { seen[title] = n + 1; title = $"{title} ({n + 1})"; }
                else seen[title] = 1;
                list.Add(new CxHeaderCol(title, c));
            }
            if (list.Count == 0) throw new ArgumentException($"Row {headerRow} of sheet \"{ws.Name}\" has no column titles.");
            return list;
        }

        private static Dictionary<string, string> CxSuggestMapping(List<string> headers)
        {
            var map = new Dictionary<string, string>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var norm = headers.ToDictionary(h => h, CxNorm);

            // pass 1: exact alias match, in alias priority order
            foreach (var f in CogximFields)
                foreach (var a in f.Aliases)
                {
                    var h = headers.FirstOrDefault(x => !used.Contains(x) && norm[x] == a);
                    if (h == null) continue;
                    map[f.Key] = h; used.Add(h); break;
                }
            // pass 2: header contains a long alias (e.g. "Party Mobile No" -> partymobile)
            foreach (var f in CogximFields.Where(f => !map.ContainsKey(f.Key)))
                foreach (var a in f.Aliases.Where(a => a.Length >= 6))
                {
                    var h = headers.FirstOrDefault(x => !used.Contains(x) && norm[x].Contains(a));
                    if (h == null) continue;
                    map[f.Key] = h; used.Add(h); break;
                }
            return map;
        }

        private static Dictionary<string, int> CxResolveMapping(Dictionary<string, string>? mapping, List<CxHeaderCol> headers)
        {
            mapping ??= new();
            if (mapping.Count == 0) mapping = CxSuggestMapping(headers.Select(h => h.Title).ToList());
            var result = new Dictionary<string, int>();
            foreach (var kv in mapping)
            {
                if (string.IsNullOrWhiteSpace(kv.Value) || CogximFields.All(f => f.Key != kv.Key)) continue;
                var h = headers.FirstOrDefault(x => string.Equals(x.Title, kv.Value.Trim(), StringComparison.OrdinalIgnoreCase));
                if (h == null) throw new ArgumentException($"Column \"{kv.Value}\" is not in the sheet.");
                result[kv.Key] = h.Column;
            }
            return result;
        }

        // ---------- cell conversion ----------
        private static string? CxText(LargeExcelCell cell)
        {
            var v = cell.Value;
            string? s;
            if (v.IsBlank) return null;
            if (v.IsNumber)
            {
                var n = v.GetNumber();
                s = Math.Abs(n % 1) < 1e-9 && Math.Abs(n) < 1e15
                    ? ((long)Math.Round(n)).ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : n.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (v.IsDateTime) s = v.GetDateTime().ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
            else if (v.IsBoolean) s = v.GetBoolean() ? "Y" : "N";
            else if (v.IsError) return null;
            else s = v.ToString(System.Globalization.CultureInfo.InvariantCulture);
            s = s?.Trim();
            return string.IsNullOrEmpty(s) || s == "'" || s == "-" || s.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : s;
        }

        private static decimal? CxNumber(LargeExcelCell cell, out bool ok)
        {
            ok = true;
            var v = cell.Value;
            if (v.IsBlank) return null;
            if (v.IsNumber) return (decimal)v.GetNumber();
            var s = CxText(cell);
            if (s == null) return null;
            var neg = s.StartsWith("(") && s.EndsWith(")");
            s = new string(s.Where(ch => char.IsDigit(ch) || ch == '.' || ch == '-').ToArray());
            if (s.Length == 0 || s == "-") return null;
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d)) return neg ? -d : d;
            ok = false;
            return null;
        }

        private static readonly string[] CxDateFormats =
        {
            "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy", "d-M-yyyy", "d/M/yyyy", "d.M.yyyy",
            "dd-MMM-yyyy", "dd MMM yyyy", "d-MMM-yy", "dd-MM-yy", "dd/MM/yy", "yyyy-MM-dd", "yyyy/MM/dd",
            "dd-MM-yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt",
            "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff", "dd/MM/yyyy HH:mm", "dd-MM-yyyy HH:mm"
        };

        private static DateTime? CxDate(LargeExcelCell cell)
        {
            var v = cell.Value;
            if (v.IsBlank) return null;
            if (v.IsDateTime) return v.GetDateTime();
            if (v.IsNumber)
            {
                var n = v.GetNumber();
                return n is > 20000 and < 80000 ? DateTime.FromOADate(n) : null;   // Excel serial 1954-2119
            }
            var s = CxText(cell);
            if (s == null) return null;
            if (DateTime.TryParseExact(s, CxDateFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var d)) return d;
            if (DateTime.TryParse(s, new System.Globalization.CultureInfo("en-IN"), System.Globalization.DateTimeStyles.AllowWhiteSpaces, out d)) return d;
            return null;
        }

        private static string CxNorm(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s) if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            return sb.ToString();
        }

        private static string CxDigits(string? s) => s == null ? "" : new string(s.Where(char.IsDigit).ToArray());

        /// <summary>Fits a long address into LedgerMaster.Address; the rest goes to Address2 (cut to its size).</summary>
        private static void CxSplitAddress(CxCustomerInfo info, int max1, int max2)
        {
            var full = info.Address;
            if (full == null || max1 <= 0 || full.Length <= max1) return;

            // break after the last comma / space that fits, so words are not split
            var cut = full.LastIndexOfAny(new[] { ',', ' ' }, max1 - 1);
            if (cut < max1 / 2) cut = max1;
            info.Address = full.Substring(0, cut).TrimEnd(' ', ',');
            info.Address2 = max2 > 0 ? CxCut(full.Substring(cut).TrimStart(' ', ','), max2) : null;
        }

        /// <summary>Character size of a column in the live database (int.MaxValue for nvarchar(max)); null if unknown.</summary>
        private async Task<int?> CxColumnLengthAsync(string table, string column)
        {
            try
            {
                var sizes = await _context.Database.SqlQuery<int>(
                    $"SELECT CAST(CHARACTER_MAXIMUM_LENGTH AS int) AS [Value] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = {table} AND COLUMN_NAME = {column} AND CHARACTER_MAXIMUM_LENGTH IS NOT NULL")
                    .ToListAsync();
                if (sizes.Count == 0) return null;
                return sizes[0] < 0 ? int.MaxValue : sizes[0];
            }
            catch
            {
                return null;
            }
        }

        private static string? CxCut(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length <= max ? s : s[..max];
        }
        #endregion
    }
}
