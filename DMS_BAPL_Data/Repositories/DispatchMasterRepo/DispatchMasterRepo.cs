using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.DispatchMasterRepo
{
    public class DispatchMasterRepo : IDispatchMasterRepo
    {
        private readonly BapldmsvadContext _context;
        public DispatchMasterRepo(BapldmsvadContext bapldmsvadContext)
        {
            _context = bapldmsvadContext;

        }
        public async Task<List<DispatchMaster>> GetAllAsync(string masterType, string name, int pageNumber, int perPageRecords)
        {
            var query = _context.DispatchMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(masterType))
                query = query.Where(x => x.MasterType == masterType);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.MasterName.Contains(name));

            query = query.OrderBy(x => x.MasterName);

            if (perPageRecords > 0)
            {
                query = query
                    .Skip((pageNumber - 1) * perPageRecords)
                    .Take(perPageRecords);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string masterType, string name)
        {
            var query = _context.DispatchMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(masterType))
                query = query.Where(x => x.MasterType == masterType);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.MasterName.Contains(name));

            return await query.CountAsync();
        }

        public async Task<DispatchMaster> GetByIdAsync(int id)
        {
            return await _context.DispatchMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string masterType, string masterName, int excludeId = 0)
        {
            return await _context.DispatchMasters
                .AnyAsync(x => x.MasterType == masterType
                            && x.MasterName.ToLower() == masterName.ToLower()
                            && x.Id != excludeId);
        }

        public async Task<int> AddAsync(DispatchMaster model)
        {
            _context.DispatchMasters.Add(model);
            await _context.SaveChangesAsync();
            return model.Id;
        }

        public async Task<bool> UpdateAsync(DispatchMaster model)
        {
            var existing = await _context.DispatchMasters.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (existing == null) return false;

            existing.MasterType = model.MasterType;
            existing.MasterName = model.MasterName;
            existing.IsActive = model.IsActive;
            existing.UpdatedBy = model.UpdatedBy;
            existing.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id, bool isActive)
        {
            var existing = await _context.DispatchMasters.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return false;

            existing.IsActive = isActive;
            existing.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.DispatchMasters.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return false;

            _context.DispatchMasters.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
