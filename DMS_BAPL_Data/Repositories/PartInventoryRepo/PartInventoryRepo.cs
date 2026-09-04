using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock. " +
                        $"ItemCode: {partsInventory.ItemCode}, " +
                        $"DealerCode: {partsInventory.VendorCode}, " +
                        $"Location: {partsInventory.DealerLocation}, " +
                        $"Available: {openingQty}, " +
                        $"Required: {partsInventory.BatchTransQty}");
                }

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
        async Task<IEnumerable<object>> IPartInventoryRepo.Get()
        {
            return await _context.PartsInventories
                .AsNoTracking()
                .ToListAsync();

        }
        async Task<IEnumerable<object>> IPartInventoryRepo.GetByItemCode(List<string> itemCodes)
        {
            var items = await _context.PartsInventories
                .Where(x => itemCodes.Contains(x.ItemCode))
                .AsNoTracking()
                .ToListAsync();

            return items;
        }
        async Task<IEnumerable<object>> IPartInventoryRepo.GetPartsByDealerAndDateRange(InventoryFilterViewModel inventoryFilterViewModel)
        {
            var query =
                from pi in _context.PartsInventories
                join im in _context.ItemMasters
                    on pi.ItemCode equals im.Itemcode
                where (string.IsNullOrWhiteSpace(inventoryFilterViewModel.DealerCode)
                        || pi.VendorCode == inventoryFilterViewModel.DealerCode)
                    && (!inventoryFilterViewModel.FromDate.HasValue
                        || pi.TransDate >= DateOnly.FromDateTime(inventoryFilterViewModel.FromDate.Value))
                    && (!inventoryFilterViewModel.ToDate.HasValue
                        || pi.TransDate <= DateOnly.FromDateTime(inventoryFilterViewModel.ToDate.Value))
                select new
                {
                    pi.ItemCode,
                    im.Itemname,
                    im.Itemdesc,
                    im.Hsncode,
                    ItemType = "Parts",
                    GroupName = "Spares",
                    im.Dlrprice,
                    pi.BatchOpeningQty,
                    pi.CreatedDate
                };

            var pageIndex = inventoryFilterViewModel.PageIndex;
            var pageSize = inventoryFilterViewModel.PageSize;

            var items = await query
                .AsNoTracking()
                .OrderBy(x => x.ItemCode)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return items.Select((x, index) => new
            {
                SrNo = ((pageIndex - 1) * pageSize) + index + 1,
                x.ItemCode,
                x.Itemname,
                x.Itemdesc,
                x.Hsncode,
                x.ItemType,
                x.GroupName,
                x.Dlrprice,
                x.BatchOpeningQty,
                x.CreatedDate
            });
        }
    }
}