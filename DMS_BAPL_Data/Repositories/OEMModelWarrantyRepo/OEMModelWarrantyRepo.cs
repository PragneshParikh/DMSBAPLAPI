using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.OEMModelWarrantyRepo
{
    public class OEMModelWarrantyRepo : IOEMModelWarrantyRepo
    {
        private readonly BapldmsvadContext _context;
        public OEMModelWarrantyRepo(BapldmsvadContext context)
        {
            _context = context;

        }
        public async Task<OemmodelWarranty> CreateAsync(OemmodelWarranty entity)
        {
            try
            {
                _context.OemmodelWarranties.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<OemModelWarrantyResponseViewModel?> GetDetailsByIdAsync(int id)
        {
            try
            {
                return await _context.OemmodelWarranties
                 .Include(x => x.Oemmodel)
                 .Where(x => x.Id == id) // ✅ ADD THIS
                 .Select(x => new OemModelWarrantyResponseViewModel
                 {
                     Id = x.Id,
                     OemmodelId = x.OemmodelId,
                     Oemmodelname = x.Oemmodel.ModelName,
                     EffectiveDate = x.EffectiveDate,
                     Odoreading = x.Odoreading,
                     DurationType = x.DurationType,
                     Duration = x.Duration,
                     IsB2b = x.IsB2b
                 })
                 .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<OemmodelWarranty?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.OemmodelWarranties.Where(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<OemModelWarrantyResponseViewModel>> GetAllAsync(string? searchTerm,DateOnly? effectiveDateFrom,DateOnly? effectiveDateTo)
        {
            try
            {
                var query = _context.OemmodelWarranties
                    .Include(x => x.Oemmodel)
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();

                    query = query.Where(x =>
                        x.Oemmodel.ModelName.ToLower().Contains(searchTerm) ||
                        x.DurationType.ToLower().Contains(searchTerm) ||
                        x.Odoreading.ToString().Contains(searchTerm) ||
                        x.Duration.ToString().Contains(searchTerm)
                    );
                }

                //  Date From
                if (effectiveDateFrom.HasValue)
                {
                    query = query.Where(x => x.EffectiveDate >= effectiveDateFrom.Value);
                }

                //  Date To
                if (effectiveDateTo.HasValue)
                {
                    query = query.Where(x => x.EffectiveDate <= effectiveDateTo.Value);
                }

                return await query
                    .Select(x => new OemModelWarrantyResponseViewModel
                    {
                        Id = x.Id,
                        OemmodelId = x.OemmodelId,
                        Oemmodelname = x.Oemmodel.ModelName,
                        EffectiveDate = x.EffectiveDate,
                        Odoreading = x.Odoreading,
                        DurationType = x.DurationType,
                        Duration = x.Duration,
                        IsB2b = x.IsB2b
                    })
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<OemmodelWarranty> UpdateAsync(OemmodelWarranty model)
        {
            try
            {
                _context.OemmodelWarranties.Update(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var data = await _context.OemmodelWarranties.FindAsync(id);

                if (data == null)
                    return false;

                _context.OemmodelWarranties.Remove(data);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {

                throw;
            }
        }
        public async Task<string> LastEffectiveDate(int oemmodelId)
        {
            try
            {
                var result = await _context.OemmodelWarranties
                    .Where(x => x.OemmodelId == oemmodelId)
                    .OrderByDescending(x => x.EffectiveDate)
                    .Select(x => x.EffectiveDate)
                    .FirstOrDefaultAsync();

                return result != null
                    ? result.Value.ToString("yyyy-MM-dd")
                    : null;
            }
            catch
            {
                throw;
            }
        }


    }
}
