using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2010.Excel;
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
        private readonly IHttpContextAccessor _contextAccessor;

        public VehicleSaleBillRepo(BapldmsvadContext context, IPartInventoryService partInventoryService,
            IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _partInventoryService = partInventoryService;
            _contextAccessor = contextAccessor;
        }

        //public async Task<int> CreateAsync(VehicleSaleBillHeader entity)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        _context.VehicleSaleBillHeaders.Add(entity);
        //        await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();
        //        return entity.Id;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

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
                        BillType = h.BillType,
                        BillFrom = h.BillFrom,
                        CustomerName = h.CustomerName,
                        BillingName = h.BillingName,
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
                        ErpStatus = h.Erpstatus,
                        DealerCode = h.DealerCode,

                        Details = (
                from d in _context.VehicleSaleBillDetails
                where d.VehicleSaleBillId == h.Id

                join v in _context.VehicleInwards
                    on d.ChassisNo equals v.ChasisNo into vinJoin
                from v in vinJoin.DefaultIfEmpty()

                join i in _context.ItemMasters
                    on d.ItemCode equals i.Itemcode into itemJoin
                from i in itemJoin.DefaultIfEmpty()

                join inv in _context.InvoiceHeaders
                on d.VehicleSaleBillId equals inv.ReferenceId into InvDetails
                from inv in InvDetails.DefaultIfEmpty()

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
                    HsnCode = i.Hsncode,
                    MotorNo = v.MotorNo,
                    InvoiceNo = inv.InvoiceNo,
                    FameIIDisc = d.FameIi,
                    PostGstDiscount =d.PostGstDisc
                }
            ).ToList()


                    }).FirstOrDefaultAsync();
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

                // 2. Create Proforma Invoice Header
                var invoice = new InvoiceHeader
                {
                    InvoiceType = "Proforma Invoice", // 👈 important
                    ServiceType = "Vehicle Sale Bill",
                    DocumentNo = header.SaleBillNo,
                    ReferenceId = header.Id,
                    CustomerId = header.LedgerId,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    Status = "Proforma"
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
                saleBill.Erpstatus = "PushedToERP";
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
                // 1. Update Header
                _context.VehicleSaleBillHeaders.Update(header);

                // 2. Update JobCards 
                if (jobUpdates == null && header.Erpstatus.ToLower() == "invalid")
                {
                    header.Erpstatus = "Pending";
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
                    header.Erpstatus = "Reserved";

                    // Mark other sale bills with same chassis as Invalid
                    var invalidSalesBills = await _context.VehicleSaleBillHeaders
                    .Include(x => x.VehicleSaleBillDetails)
                    .Where(x => x.SaleBillNo != header.SaleBillNo &&
                                x.VehicleSaleBillDetails.Any(c => allchassis.Contains(c.ChassisNo)))
                    .ToListAsync();

                    foreach (var bill in invalidSalesBills)
                    {
                        bill.Erpstatus = "Invalid";

                        var detailsToRemove = await _context.VehicleSaleBillDetails
                             .Where(d => d.VehicleSaleBillId == bill.Id)
                                .ToListAsync();

                        _context.VehicleSaleBillDetails.RemoveRange(detailsToRemove);

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
                    }
                }


                //updating JobCardCustomer if any of the Chassis no was deleted
                //foreach (var item in deletedChassisList)
                //{
                //    var deletedJC = jobs.Where(c => c.ChassisNo == item).FirstOrDefault();

                //    if (deletedJC == null) continue;
                //    deletedJC.InsuranceExpDate = null;
                //    deletedJC.RegisterNo = null;
                //    deletedJC.SaleDate = null;
                //}

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
                    bill.Erpstatus = "Invalid";

                    var detailsToRemove = await _context.VehicleSaleBillDetails
                         .Where(d => d.VehicleSaleBillId == bill.Id && chassisNos.Contains(d.ChassisNo))
                            .ToListAsync();

                    _context.VehicleSaleBillDetails.RemoveRange(detailsToRemove);

                }

                // CREATE INVOICE HEADER
                // CHECK IF INVOICE ALREADY EXISTS
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

                    // REMOVE OLD DETAILS (important to avoid duplicates)
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
                        ReferenceId = saleBill.Id,
                        CustomerId = saleBill.LedgerId,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        Status = "Invoiced"
                    };

                    _context.InvoiceHeaders.Add(invoice);
                }

                // ADD DETAILS (COMMON FOR BOTH CASES)
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
                saleBill.Erpstatus = "Invoiced";

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

                return saleBill.Id;
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

        public async Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo)
        {
            try
            {
                var data = await (
                     from vi in _context.VehicleInwards
                     join im in _context.ItemMasters
                     on vi.ItemCode equals im.Itemcode

                     join f2 in _context.Form22Masters
                     on (im.Oemmodelname ?? "").Trim().ToLower()
                     equals (f2.OemModelName ?? "").Trim().ToLower()
                     into formGroup

                     from f2 in formGroup.DefaultIfEmpty()

                     where vi.ChasisNo == chassisNo

                     select new Form22SlipViewModel
                     {
                         ChassisNo = chassisNo,
                         TypeApprovalCertNo = f2.ApprovalCertificateNo,
                         BrandName = im.Itemname,
                         MotorNo = vi.MotorNo,
                         Emission = "",
                         SoundLevelHorn = f2.SoundLevelHorn,
                         NoiseLevel = f2.PassbyNoiseLevel

                     }
                     ).FirstOrDefaultAsync();
                return data;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ChassisListWithPDIStatus>> GetAllChassissListWithPDISatatus(string? dealerCode)
        {
            try
            {
                var result = await (
                from vi in _context.VehicleInwards
                join ch in _context.ChassisDetails on vi.ChasisNo equals ch.ChassisNo
                join im in _context.ItemMasters on vi.ItemCode equals im.Itemcode
                join clr in _context.ColorMasters on vi.ColrCode equals clr.Colorcode

                join jc in _context.JobCardHeaders
                    on ch.ChassisNo equals jc.Chassisno into jcGroup
                from jc in jcGroup.DefaultIfEmpty()

                join vd in _context.VehicleSaleBillDetails
                    on ch.ChassisNo equals vd.ChassisNo into vdGroup
                from vd in vdGroup.DefaultIfEmpty()

                join vh in _context.VehicleSaleBillHeaders
                    on vd.VehicleSaleBillId equals vh.Id into vhGroup
                from vh in vhGroup.DefaultIfEmpty()

                where ch.SaleDate == null

                select new
                {
                    ch.ChassisNo,
                    Data = new ChassisListWithPDIStatus
                    {
                        ChassisNo = ch.ChassisNo,
                        ItemCode = vi.ItemCode,
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
                        ProformaCreated = vd == null ? null : vh.SaleBillNo,
                        PDIStatus = (jc != null && jc.IsPdiSuccess == true) ? "OK" : "Not Done"
                    }
                }
                            ).GroupBy(x => x.ChassisNo)
                            .Select(g => g.First().Data)
                            .ToListAsync();

                if (!string.IsNullOrEmpty(dealerCode))
                {
                    result = result.Where(i => i.DealerCode == dealerCode).ToList();
                }

                return result;
            }
            catch
            {
                throw;
            }
        }


    }
}
