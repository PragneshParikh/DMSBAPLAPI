using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<BatteryCapacityMaster> AddBatteryCapacityMasterAsync(BatteryCapacityMasterViewModel batteryCapacityMaster, string userId)
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

        public async Task<List<BatteryCapacityMaster>> GetBatteryCapacityMastersAsync()
        {
            return await _dbContext.BatteryCapacityMasters.ToListAsync();
        }



        public async Task<BatteryCapacityMaster?> UpdateBatteryCapacityMasterAsync(int id, BatteryCapacityMasterViewModel batteryCapacityMasterViewModel,string userId)
        {
            var existingBatterCapacityMaster = await _dbContext.BatteryCapacityMasters.FindAsync(id);

            if (existingBatterCapacityMaster == null)
                return null;


            existingBatterCapacityMaster.BatteryCapacity = batteryCapacityMasterViewModel.BatteryCapacity;
            existingBatterCapacityMaster.IsActive = batteryCapacityMasterViewModel.IsActive ?? true;

            existingBatterCapacityMaster.UpdatedBy = userId;
            existingBatterCapacityMaster.UpdatedDate = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return existingBatterCapacityMaster;
        }

        public async Task<PagedResponseBattery<BatteryCapacityMaster>> GetPaginatedBatteryCapacityMastersAsync(
      string? batteryCapacity,
      int? page,
      int? pageSize)
        {
            int currentPage = page ?? 1;
            int currentPageSize = pageSize ?? 10;

            var query = _dbContext.BatteryCapacityMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(batteryCapacity))
            {
                query = query.Where(x => x.BatteryCapacity.Contains(batteryCapacity));
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

    }
}
