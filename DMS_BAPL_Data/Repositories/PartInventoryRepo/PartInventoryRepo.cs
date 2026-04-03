using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PartInventoryRepo
{
    public partial class PartInventoryRepo : IPartInventoryRepo
    {
        private readonly BapldmsvadContext _context;
        public PartInventoryRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<int> IPartInventoryRepo.GetCurrentStockByItem(string itemCode)
        {
            var item = await _context.PartsInventories
                .Where(x => x.ItemCode == itemCode)
                .Select(x => x.BatchClosingQty)
                .FirstOrDefaultAsync();

            return item;
        }

        public async Task UpdateStock(string itemCode, int transQty, string transType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var lastBatch = await _context.PartsInventories
                    .Where(x => x.ItemCode == itemCode)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                int openingQty = lastBatch != null ? lastBatch.BatchClosingQty : 0;

                int closingQty = transType switch
                {
                    "P" => openingQty + transQty,
                    "S" => openingQty - transQty,
                    _ => throw new Exception("Invalid transaction type")
                };

                if (closingQty < 0)
                    throw new Exception("Negative stock not allowed");

                var newRecord = new PartsInventory
                {
                    ItemCode = itemCode,
                    BatchNo = lastBatch?.BatchNo ?? "",
                    BatchOpeningQty = openingQty,
                    BatchTransQty = transQty,
                    BatchClosingQty = closingQty,
                    TotalRate = lastBatch?.TotalRate ?? 0,
                    VoucherNo = lastBatch?.VoucherNo ?? string.Empty,
                    DealerLocation = lastBatch?.DealerLocation ?? string.Empty,
                    VendorCode = lastBatch?.VendorCode ?? string.Empty,
                    TransType = transType,
                    TransDate = DateOnly.FromDateTime(DateTime.Now),
                    FinalStockFlag = "Y",
                    CreatedBy = lastBatch?.CreatedBy ?? "",
                    CreatedDate = DateTime.Now
                };

                if (lastBatch != null)
                    lastBatch.FinalStockFlag = "N";

                _context.PartsInventories.Add(newRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
