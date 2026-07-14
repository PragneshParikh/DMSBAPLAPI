using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PartInwardRepo
{
    public partial class PartInwardRepo : IPartInwardRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IPartInventoryService _partInventoryService;
        public PartInwardRepo(BapldmsvadContext context, IPartInventoryService partInventoryService)
        {
            _context = context;
            _partInventoryService = partInventoryService;
        }

        async Task<IEnumerable<PartsInward>> IPartInwardRepo.Get()
        {
            return await Task.FromResult(_context.PartsInwards.ToList());
        }

        async Task<IEnumerable<PartsInward>> IPartInwardRepo.GetPartInwardByDealerAsync(string dealerCode)
        {
            return await Task.FromResult(_context.PartsInwards.Where(p => p.DealerCode == dealerCode && p.IsAccepted == false).ToList());
        }

        async Task<bool> IPartInwardRepo.UpdateByInvoice(string invoiceNo, string dealerCode)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var partsListByInvoice = await _context.PartsInwards
                    .Where(x => x.InvoiceNo == invoiceNo)
                    .AsNoTracking()
                    .ToListAsync();

                if (!partsListByInvoice.Any())
                    return false;

                var inventoryList = partsListByInvoice
                    .GroupBy(x => x.PartNo)
                    .Select(g => new PartsInventory
                    {
                        TransId = Guid.NewGuid().ToString(),
                        ItemCode = g.Key,

                        VoucherNo = null!,
                        TransType = "P",
                        BatchNo = "Batch 1",

                        // total qty by part
                        BatchTransQty = g.Sum(x => x.ItemQty),

                        BatchOpeningQty = 0,
                        BatchClosingQty = 0,

                        TransDate = DateOnly.FromDateTime(DateTime.Now),

                        DealerLocation = g.First().LocCode,
                        VendorCode = g.First().DealerCode,

                        // optional calculations
                        TotalRate = g.Sum(x => x.ItemRate * x.ItemQty),
                        PurchaseRate = g.First().ItemRate,

                        Potype = "B2C",
                        PostTransaction = 0,

                        CreatedBy = dealerCode,
                        CreatedDate = DateTime.Now
                    })
                    .ToList();

                foreach (var vehicle in inventoryList)
                {
                    await _partInventoryService.UpdateIncoming(vehicle);
                }

                var affectedRows = await _context.PartsInwards
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.IsAccepted, true)
                        );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return affectedRows > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        async Task<object> IPartInwardRepo.PartsInward(PartsInwardViewModel partsInwardViewModel)
        {
            var exist = await _context.PartsInwards
                .FirstOrDefaultAsync(x =>
                    x.InvoiceNo == partsInwardViewModel.invoice_no &&
                    x.PartNo == partsInwardViewModel.part_no);

            if (exist != null)
            {
                return new
                {
                    Success = false,
                    Message = "Part No and Invoice No already exist. Duplicate entry."
                };
            }

            var entity = new PartsInward
            {
                DealerCode = partsInwardViewModel.dealer_code,
                LocCode = partsInwardViewModel.loc_code,
                InvoiceNo = partsInwardViewModel.invoice_no,
                PartNo = partsInwardViewModel.part_no,
                ItemQty = partsInwardViewModel.item_qty,
                ItemRate = partsInwardViewModel.item_rate,
                IsAccepted = false,
                InvoiceDate = DateTime.ParseExact(partsInwardViewModel.invoice_date, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                ItemIdno = partsInwardViewModel.item_idno,
                ItemHsncode = partsInwardViewModel.item_hsncode,
                ItemMrp = partsInwardViewModel.item_mrp,
                Sgst = partsInwardViewModel.sgst,
                Cgst = partsInwardViewModel.cgst,
                Igst = partsInwardViewModel.igst,
                ItemDisc = partsInwardViewModel.item_disc,
                DiscountType = partsInwardViewModel.discount_type
            };

            await _context.PartsInwards.AddAsync(entity);

            var result = await _context.SaveChangesAsync();

            return new
            {
                Success = result > 0,
                Message = result > 0 ? "Part inward saved successfully." : "Failed to save part inward."
            };
        }
    }
}
