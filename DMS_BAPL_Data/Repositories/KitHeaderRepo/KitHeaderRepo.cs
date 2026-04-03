using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.KitHeaderRepo
{
    public partial class KitHeaderRepo : IKitHeaderRepo
    {
        private readonly BapldmsvadContext _context;

        public KitHeaderRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        public async Task<PagedResponse<KitHeader>> GetKitByPagedAsync(string? searchTerms, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.KitHeaders.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerms))
                {
                    query = query.Where(c => c.KitName.Contains(searchTerms));
                }

                int totalRecords = await query.CountAsync();

                var items = await query
                    .AsNoTracking()
                    .OrderBy(c => c.KitName)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                int startSrNo = (pageIndex * pageSize) + 1;

                var ModelItems = items.Select((item, index) => new KitHeader
                {
                    Id = item.Id,
                    KitName = item.KitName,
                    KitDate = item.KitDate,
                    Status = item.Status,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = item.CreatedDate,
                    UpdatedBy = item.UpdatedBy,
                    UpdatedDate = item.UpdatedDate
                }).ToList();

                return new PagedResponse<KitHeader>
                {
                    Data = ModelItems,
                    TotalRecords = totalRecords
                };
            }
            catch { throw; }
        }

        public async Task<int> InsertKitHeader(KitHeader kitHeader)
        {
            try
            {
                await _context.KitHeaders.AddAsync(kitHeader);
                await _context.SaveChangesAsync();

                return kitHeader.Id;
            }
            catch { throw; }
        }
        public async Task<KitHeader?> GetKitById(int id)
        {
            try
            {
                return await _context.KitHeaders
                    .AsNoTracking()
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch { throw; }
        }

        public async Task<int> UpdateKitHeader(KitHeader kitHeader)
        {
            try
            {
                _context.KitHeaders.Update(kitHeader);
                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }
    }
}
