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
            try
            {
                var lastBatch = await _context.PartsInventories
                    .Where(x => x.ItemCode == partsInventory.ItemCode && x.VendorCode == partsInventory.VendorCode && x.DealerLocation == partsInventory.DealerLocation)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                int openingQty = lastBatch?.BatchClosingQty ?? 0;

                int closingQty = partsInventory.TransType switch
                {
                    "P" => openingQty + partsInventory.BatchTransQty, // Purchase
                    "S" => openingQty - partsInventory.BatchTransQty, // Sell
                    "TI" => openingQty + partsInventory.BatchTransQty, // Touch point inward/CounterBill delete
                    "TO" => openingQty - partsInventory.BatchTransQty, // Touch point out
                    "PI" => openingQty - partsInventory.BatchTransQty, // Return the Parts,
                    "SD" => openingQty + partsInventory.BatchTransQty, // Sale Bill is deleted
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
                    DealerLocation = partsInventory.DealerLocation ?? string.Empty,
                    VendorCode = partsInventory.VendorCode ?? string.Empty,
                    FinalStockFlag = "Y",
                    TotalRate = lastBatch?.TotalRate ?? 0,
                    PurchaseRate = lastBatch?.PurchaseRate ?? 0,
                    Potype = partsInventory.Potype ?? string.Empty,
                    PostTransaction = lastBatch?.PostTransaction,
                    CreatedBy = partsInventory.CreatedBy,
                    CreatedDate = partsInventory.CreatedDate
                };

                if (lastBatch != null)
                    lastBatch.FinalStockFlag = "N";

                _context.PartsInventories.Add(newRecord);
            }
            catch
            {
                throw;
            }
        }
    }
}
