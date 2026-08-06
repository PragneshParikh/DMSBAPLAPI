using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.PartDispatchRepo
{
    public class PartDispatchRepo : IPartDispatchRepo
    {
        private readonly BapldmsvadContext _context;

        public PartDispatchRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<List<DmsPartDispatch>> GetAllAsync()
        {
            try
            {
                return await _context.DmsPartDispatches
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<DmsPartDispatch?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.DmsPartDispatches
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        public async Task<DmsPartDispatch> CreateAsync(DmsPartDispatch item, string userId)
        {
            try
            {
                item.CreatedBy = userId;
                item.CreatedDate = DateTime.UtcNow;

                _context.DmsPartDispatches.Add(item);
                await _context.SaveChangesAsync();

                return item;
            }
            catch { throw; }
        }

        public async Task<DmsPartDispatch?> UpdateAsync(DmsPartDispatch item, string userId)
        {
            try
            {
                var existing = await _context.DmsPartDispatches
                    .FirstOrDefaultAsync(x => x.Id == item.Id);

                if (existing == null)
                    return null;

                existing.InvoiceDate = item.InvoiceDate;
                existing.InvoiceNo = item.InvoiceNo;
                existing.PartNo = item.PartNo;
                existing.ItemIdno = item.ItemIdno;
                existing.ItemHsncode = item.ItemHsncode;
                existing.ItemRate = item.ItemRate;
                existing.ItemMrp = item.ItemMrp;
                existing.ItemQty = item.ItemQty;
                existing.Sgst = item.Sgst;
                existing.Cgst = item.Cgst;
                existing.Igst = item.Igst;
                existing.Ugst = item.Ugst;
                existing.ItemDisc = item.ItemDisc;
                existing.DiscountType = item.DiscountType;
                existing.LocCode = item.LocCode;
                existing.VendorIdno = item.VendorIdno;
                existing.DealerCode = item.DealerCode;

                existing.UpdatedBy = userId;
                existing.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existing;
            }
            catch { throw; }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var existing = await _context.DmsPartDispatches
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existing == null)
                    return false;

                _context.DmsPartDispatches.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            catch { throw; }
        }

        public async Task<int> ImportAsync(List<DmsPartDispatch> items, string userId)
        {
            try
            {
                foreach (var item in items)
                {
                    item.CreatedBy = userId;
                    item.CreatedDate = DateTime.UtcNow;
                }

                await _context.DmsPartDispatches.AddRangeAsync(items);
                await _context.SaveChangesAsync();

                return items.Count;
            }
            catch { throw; }
        }
    }
}