using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.BatteryCapacityMasterRepo
{
    public class BatteryCapacityMasterRepo : IBatteryCapacityMasterRepo
    {
        private readonly BapldmsvadContext _dbContext;

        public BatteryCapacityMasterRepo(BapldmsvadContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Add new battery capacity record
        public async Task<BatteryCapacityMaster> AddBatteryCapacityMasterAsync(
            BatteryCapacityMasterViewModel batteryCapacityMaster,
            string userId)
        {
            try
            {
                var newBatteryCapacity = new BatteryCapacityMaster
                {
                    BatteryCapacity = batteryCapacityMaster.BatteryCapacity,
                    IsActive = batteryCapacityMaster.IsActive ?? true,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                await _dbContext.BatteryCapacityMasters.AddAsync(newBatteryCapacity);
                await _dbContext.SaveChangesAsync();

                return newBatteryCapacity;
            }
            catch
            {
                throw;
            }
        }

        // Get all battery capacity records
        public async Task<List<BatteryCapacityMaster>> GetBatteryCapacityMastersAsync()
        {
            try
            {
                return await _dbContext.BatteryCapacityMasters.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        // Update battery capacity record
        public async Task<BatteryCapacityMaster?> UpdateBatteryCapacityMasterAsync(
            int id,
            BatteryCapacityMasterViewModel batteryCapacityMasterViewModel,
            string userId)
        {
            try
            {
                var existingBatterCapacityMaster = await _dbContext
                    .BatteryCapacityMasters.FindAsync(id);

                if (existingBatterCapacityMaster == null)
                    return null;

                existingBatterCapacityMaster.BatteryCapacity =
                    batteryCapacityMasterViewModel.BatteryCapacity;

                existingBatterCapacityMaster.IsActive =
                    batteryCapacityMasterViewModel.IsActive ?? true;

                existingBatterCapacityMaster.UpdatedBy = userId;
                existingBatterCapacityMaster.UpdatedDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return existingBatterCapacityMaster;
            }
            catch
            {
                throw;
            }
        }

        // Get paginated battery capacity records
        public async Task<PagedResponseBattery<BatteryCapacityMaster>> GetPaginatedBatteryCapacityMastersAsync(
            string? batteryCapacity,
            int? page,
            int? pageSize)
        {
            try
            {
                int currentPage = page ?? 1;
                int currentPageSize = pageSize ?? 10;

                var query = _dbContext.BatteryCapacityMasters.AsQueryable();

                if (!string.IsNullOrWhiteSpace(batteryCapacity))
                {
                    query = query.Where(x =>
                        x.BatteryCapacity.Contains(batteryCapacity));
                }

                var totalRecords = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((currentPage - 1) * currentPageSize)
                    .Take(currentPageSize)
                    .ToListAsync();

                return new PagedResponseBattery<BatteryCapacityMaster>
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
    }
}