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
                var data = await _context.VehicleSaleBillHeaders.FindAsync(id);
                if (data != null)
                {
                    _context.VehicleSaleBillHeaders.Remove(data);
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
                var vehicleItemCodes = await _context.VehicleInwards
                    .Where(v => v.ChasisNo != null && chassisNos.Contains(v.ChasisNo))
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

                foreach (var bill in invalidSalesBills)
                {
                    //bill.Erpstatus = "Invalid";

                    var detailsToRemove = await _context.VehicleSaleBillDetails
                         .Where(d => d.VehicleSaleBillId == bill.Id && chassisNos.Contains(d.ChassisNo))
                            .ToListAsync();

                    _context.VehicleSaleBillDetails.RemoveRange(detailsToRemove);

                }
                var existingInvoice = _context.InvoiceHeaders
                    .FirstOrDefault(x => x.DocumentNo == saleBill.SaleBillNo);

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
                    var vehicleInwards = await _context.VehicleInwards.Where(x => chassisNos.Contains(x.ChasisNo) && x.DealerCode == saleBill.DealerCode).ToListAsync(); var receivingDealerCode = await _context.LedgerMasters.Where(x => x.Id == saleBill.LedgerId).Select(x => x.DealerCode).FirstOrDefaultAsync();
                    var gstNo = await _context.DealerMasters.Where(x => x.Dealercode == saleBill.DealerCode).Select(x => x.CompgstinNo).FirstOrDefaultAsync();
                    var vehicleLookup = vehicleInwards.ToDictionary(x => x.ChasisNo);
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
                            ColrCode = vehicle.ColrCode,
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
                            BatteryId = vehicle.BatteryId,
                            ChargerNo = item.ChargerNo,
                            ControllerNo = item.ControllerNo,
                            Regnumber = item.RegNo,
                            MfgMonth = vehicle.MfgMonth,
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

        private async Task updateChassisForD2d(List<string> chassises, int? ledgerId, string dealerCode, string locationCode)
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
                var receivingDealerCode = await _context.LedgerMasters.Where(i => i.Id == ledgerId).FirstOrDefaultAsync();

                if (chassises == null || !chassises.Any())
                    return;

                var chassisToMove = await _context.ChassisDetails
                    .Where(x => chassises.Contains(x.ChassisNo))
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
                    DealerCode = receivingDealerCode.DealerCode,
                    LocationCode = receivingDealerCode.DealerCode + "S1",
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

                var newChassisRecords = chassisToMove.Select(x => new ChassisDetail
                {
                    LedgerId = null,
                    ChassisNo = x.ChassisNo,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    ItemColor = x.ItemColor,
                    DealerId = receivingDealerCode.DealerCode,
                    LocationCode = receivingDealerCode.DealerCode + "S1",
                    SaleDate = null,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = null,
                    UpdatedDate = null,
                    RegNo = null
                }).ToList();

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

        //public 
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
                    join im in _context.ItemMasters
                        on vi.ItemCode equals im.Itemcode
                    join clr in _context.ColorMasters
                        on vi.ColrCode equals clr.Colorcode

                    join jc in _context.JobCardHeaders
                        on ch.ChassisNo equals jc.Chassisno into jcGroup
                    from jc in jcGroup.DefaultIfEmpty()

                    join vd in _context.VehicleSaleBillDetails
                        on ch.ChassisNo equals vd.ChassisNo into vdGroup
                    from vd in vdGroup.DefaultIfEmpty()

                    join vh in _context.VehicleSaleBillHeaders
                        on vd.VehicleSaleBillId equals vh.Id into vhGroup
                    from vh in vhGroup.DefaultIfEmpty()

                    where latestVehicleInwardIds.Contains(vi.Id)
                          && ch.SaleDate == null
                          && (vh == null || vh.IsD2d == true || vh.Status == "PerformaCreated")
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
                            BatteryNo = vi.BatteryNo,
                            BatteryChemical = vi.BatteryChemistry,
                            BatteryCapacity = vi.BatteryCapacity,
                            BatteryMake = vi.BatteryMake,
                            ChargerNo = vi.ChargerNo,
                            ControllerNo = vi.ControllerNo,
                            FameIIAmnt = vi.Fame2Discount,
                            DealerPrice = vi.Dlrprice,
                            CustomerPrice = vi.Custprice,
                            DealerCode = ch.DealerId,
                            CustomerSaleDate = ch.SaleDate,
                            ProformaCreated = vh != null ? vh.SaleBillNo : null,
                            PDIStatus = jc != null && jc.IsPdiSuccess == true
                                ? "OK"
                                : "Not Done",
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
    }
}
