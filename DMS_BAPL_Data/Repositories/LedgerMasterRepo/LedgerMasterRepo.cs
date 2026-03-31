using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LedgerMasterRepo
{
    public partial class LedgerMasterRepo : ILedgerMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public LedgerMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<LedgerMaster>> ILedgerMasterRepo.GetAll()
        {
            try
            {

                return await _context.LedgerMasters
                                     .AsNoTracking()
                                     .OrderBy(c => c.LedgerName)
                                     .ToListAsync();

            }
            catch { throw; }
        }

        async Task<PagedResponse<LedgerMaster>> ILedgerMasterRepo.GetLedgerByPagedAsync(string? searchTerms, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.LedgerMasters.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerms))
                {
                    query = query.Where(c => c.LedgerType.Contains(searchTerms)
                                        || c.LedgerName.Contains(searchTerms)
                                        || c.MobileNumber.Contains(searchTerms)
                                        || c.EMail.Contains(searchTerms)
                                        || c.City.Contains(searchTerms)
                                        || c.State.Contains(searchTerms));
                }

                int totalRecords = await query.CountAsync();

                var items = await query
                    .OrderBy(c => c.LedgerName)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                int startSrNo = (pageIndex * pageSize) + 1;

                return new PagedResponse<LedgerMaster>
                {
                    Data = items,
                    TotalRecords = totalRecords
                };
            }
            catch { throw; }

        }

        async Task<LedgerMaster?> ILedgerMasterRepo.GetLedgerById(int id)
        {
            try
            {
                return await _context.LedgerMasters
                               .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        async Task<int> ILedgerMasterRepo.InsertLedgerDetail(LedgerMaster ledgerMaster)
        {
            try
            {
                await _context.LedgerMasters.AddAsync(ledgerMaster);
                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        async Task<bool> ILedgerMasterRepo.UpdateLedgerDetail(LedgerMaster ledgerMaster)
        {
            try
            {
                _context.LedgerMasters.Update(ledgerMaster);
                return await _context.SaveChangesAsync() > 0;
            }
            catch { throw; }
        }
    }
}
