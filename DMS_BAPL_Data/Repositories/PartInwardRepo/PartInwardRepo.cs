using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.InventoryService;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    }
}
