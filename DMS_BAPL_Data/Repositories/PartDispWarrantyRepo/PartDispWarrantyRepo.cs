using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.PartDispWarrantyRepo
{
    public class PartDispWarrantyRepo : IPartDispWarrantyRepo
    {
        private readonly BapldmsvadContext _context;

        public PartDispWarrantyRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<List<ZDmsPartDispWarranty>> GetAllAsync()
        {
            try
            {
                return await _context.ZDmsPartDispWarranties
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<ZDmsPartDispWarranty?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.ZDmsPartDispWarranties
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        public async Task<ZDmsPartDispWarranty> CreateAsync(ZDmsPartDispWarranty item, string userId)
        {
            try
            {
                item.CreatedBy = userId;
                item.CreatedDate = DateTime.UtcNow;

                _context.ZDmsPartDispWarranties.Add(item);
                await _context.SaveChangesAsync();

                return item;
            }
            catch { throw; }
        }

        public async Task<ZDmsPartDispWarranty?> UpdateAsync(ZDmsPartDispWarranty item, string userId)
        {
            try
            {
                var existing = await _context.ZDmsPartDispWarranties
                    .FirstOrDefaultAsync(x => x.Id == item.Id);

                if (existing == null)
                    return null;

                existing.Invoicedate = item.Invoicedate;
                existing.Invoiceno = item.Invoiceno;
                existing.Invoicetype = item.Invoicetype;
                existing.Chassisnumber = item.Chassisnumber;
                existing.Itemcode = item.Itemcode;
                existing.Serialno = item.Serialno;
                //existing.Vendorid = item.Vendorid;
                existing.Dealercode = item.Dealercode;
                existing.Devicetype = item.Devicetype;
                existing.Itemqty = item.Itemqty;
                existing.Lotno = item.Lotno;
                existing.Mfgdate = item.Mfgdate;
                existing.Invoiceitemcode = item.Invoiceitemcode;
                existing.Lineno = item.Lineno;

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
                var existing = await _context.ZDmsPartDispWarranties
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existing == null)
                    return false;

                _context.ZDmsPartDispWarranties.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            catch { throw; }
        }

        public async Task<int> ImportAsync(List<ZDmsPartDispWarranty> items, string userId)
        {
            try
            {
                foreach (var item in items)
                {
                    item.CreatedBy = userId;
                    item.CreatedDate = DateTime.UtcNow;
                }

                await _context.ZDmsPartDispWarranties.AddRangeAsync(items);
                await _context.SaveChangesAsync();

                return items.Count;
            }
            catch { throw; }
        }

        public async Task<List<string>> GetSerialNosByItemCodeAsync(string itemCode, int? excludeInvoiceId = null)
        {
            try
            {
                var usedSerials = _context.EbwInvoiceHeaders
                    .Where(x => excludeInvoiceId == null || x.Id != excludeInvoiceId)
                    .Select(x => x.SerialNo);

                return await _context.ZDmsPartDispWarranties
                    .Where(x => x.Itemcode == itemCode
                             && x.Serialno != null
                             && !usedSerials.Contains(x.Serialno))
                    .Select(x => x.Serialno!)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<List<ZDmsPartDispWarranty>> GetByItemCodesAsync(List<string> itemCodes)
        {
            try
            {
                return await _context.ZDmsPartDispWarranties
                    .Where(x => x.Itemcode != null && itemCodes.Contains(x.Itemcode))
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<ZDmsPartDispWarranty?> GetBySerialNoAsync(string serialNo)
        {
            try
            {
                return await _context.ZDmsPartDispWarranties
                    .FirstOrDefaultAsync(x => x.Serialno == serialNo);
            }
            catch { throw; }
        }
    }
}