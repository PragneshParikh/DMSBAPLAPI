using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DepartmentRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.FreeServiceRateRepo
{
    public partial class FreeServiceRateRepo : IFreeServiceRateRepo
    {
        private readonly BapldmsvadContext _context;

        public FreeServiceRateRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<FreeServiceRate>> IFreeServiceRateRepo.Get()
        {
            return await _context.FreeServiceRates
                .AsNoTracking()
                .ToListAsync();
        }
        async Task<bool> IFreeServiceRateRepo.Insert(List<FreeServiceRate> freeServiceRate)
        {
            await _context.FreeServiceRates.AddRangeAsync(freeServiceRate);
            await _context.SaveChangesAsync();

            return true;
        }
        async Task<int> IFreeServiceRateRepo.Update(FreeServiceRate freeServiceRate)
        {
            _context.FreeServiceRates.Update(freeServiceRate);
            return await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<FreeServiceRateViewModel>> GetByOEMModelId(int? id)
        {
            return await (
                from fs in _context.FreeServiceRates
                join om in _context.OemmodelMasters
                    on fs.OemmodelId equals om.Id into modelGroup
                from om in modelGroup.DefaultIfEmpty()
                where !id.HasValue || fs.OemmodelId == id

                select new FreeServiceRateViewModel
                {
                    Id = fs.Id,
                    OemmodelId = fs.OemmodelId,
                    OEMModelName = om != null ? om.ModelName : null,
                    EffectiveDate = fs.EffectiveDate,
                    ServiceId = fs.ServiceId,
                    MetroRate = fs.MetroRate,
                    MetroGst = fs.MetroGst,
                    NonMetroRate = fs.NonMetroRate,
                    NonMetroGst = fs.NonMetroGst,
                    CreatedDate = fs.CreatedDate
                }
            )
            .AsNoTracking()
            .ToListAsync();
        }
    }
}
