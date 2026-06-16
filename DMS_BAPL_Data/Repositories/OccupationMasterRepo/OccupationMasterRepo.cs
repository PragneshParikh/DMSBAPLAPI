using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.OccupationMasterRepo
{
    public class OccupationMasterRepo : IOccupationMasterRepo
    {
        private readonly BapldmsvadContext _context;
        public OccupationMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        // Add Occupation
        public async Task<OccupationMaster> AddOccupationMasterAsync(OccupationViewModel occupationViewModel, string userId)
        {
            try
            {
                var occupation = new OccupationMaster
                {
                    OccupationName = occupationViewModel.OccupationName,
                    IsActive = occupationViewModel.IsActive,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };
                await _context.OccupationMasters.AddAsync(occupation);
                await _context.SaveChangesAsync();
                return occupation;
            }
            catch
            {
                throw;
            }
        }

        // Get All Occupations
        public async Task<List<OccupationMaster>> GetOccupationMastersAsync()
        {
            try
            {
                return await _context.OccupationMasters.OrderByDescending(x => x.CreatedDate).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        // Update Occupation
        public async Task<OccupationMaster?> UpdateOccupationMasterAsync(int id, OccupationViewModel occupationViewModel, string userId)
        {
            try
            {
                var existingOccupation = await _context.OccupationMasters.FindAsync(id);

                if (existingOccupation == null)
                    return null;

                existingOccupation.OccupationName = occupationViewModel.OccupationName;
                existingOccupation.IsActive = occupationViewModel.IsActive;
                existingOccupation.UpdatedBy = userId;
                existingOccupation.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return existingOccupation;
            }
            catch
            {
                throw;
            }
        }

        // Pagination + Search
        public async Task<PagedResponseBattery<OccupationMaster>> GetPaginatedOccupationMastersAsync(string? occupationName, int? page, int? pageSize)
        {
            try
            {
                int currentPage = page ?? 1;
                int currentPageSize = pageSize ?? 10;

                var query = _context.OccupationMasters.AsQueryable();

                if (!string.IsNullOrWhiteSpace(occupationName))
                {
                    query = query.Where(x => x.OccupationName.Contains(occupationName));
                }

                var totalRecords = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((currentPage - 1) * currentPageSize)
                    .Take(currentPageSize)
                    .ToListAsync();

                return new PagedResponseBattery<OccupationMaster>
                {
                    TotalRecords = totalRecords,
                    Data = data
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<OccupationMaster>> GetAllActiveOccupation()
        {
            try 
            { 
                return await _context.OccupationMasters.Where(i=>i.IsActive == true).OrderByDescending(i=>i.CreatedDate).ToListAsync();
            }
            catch
            {
                throw;
            }
        }
    }

}
