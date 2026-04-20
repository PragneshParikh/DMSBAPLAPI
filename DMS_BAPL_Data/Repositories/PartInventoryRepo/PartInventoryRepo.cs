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

        public async Task UpdateStock(PartsInventory partsInventory)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var lastBatch = await _context.PartsInventories
                    .Where(x => x.ItemCode == partsInventory.ItemCode)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                int openingQty = lastBatch?.BatchClosingQty ?? 0;

                int closingQty = partsInventory.TransType switch
                {
                    "P" => openingQty + partsInventory.BatchTransQty,
                    "S" => openingQty - partsInventory.BatchTransQty,
                    _ => throw new Exception("Invalid transaction type")
                };

                if (closingQty < 0)
                    throw new Exception("Negative stock not allowed");

                var newRecord = new PartsInventory
                {
                    TransId = Guid.NewGuid().ToString(),
                    ItemCode = partsInventory.ItemCode,
                    VoucherNo = lastBatch?.VoucherNo ?? string.Empty,
                    TransType = partsInventory.TransType,
                    BatchNo = lastBatch?.BatchNo ?? "",
                    BatchOpeningQty = openingQty,
                    BatchTransQty = partsInventory.BatchTransQty,
                    BatchClosingQty = closingQty,
                    TransDate = DateOnly.FromDateTime(DateTime.Now),
                    DealerLocation = lastBatch?.DealerLocation ?? string.Empty,
                    VendorCode = partsInventory.VendorCode ?? string.Empty,
                    FinalStockFlag = "Y",
                    TotalRate = lastBatch?.TotalRate ?? 0,
                    PurchaseRate = lastBatch?.PurchaseRate ?? 0,
                    Potype = lastBatch?.Potype ?? "B2C",
                    PostTransaction = lastBatch?.PostTransaction,
                    CreatedBy = partsInventory.CreatedBy,
                    CreatedDate = partsInventory.CreatedDate
                };

                if (lastBatch != null)
                    lastBatch.FinalStockFlag = "N";

                _context.PartsInventories.Add(newRecord);
                //await _context.SaveChangesAsync();
                //await transaction.CommitAsync();
            }
            catch
            {
                //await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
