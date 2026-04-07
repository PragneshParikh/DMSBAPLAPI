using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PrefixRepo
{
    public class PrefixRepo : IPrefixRepo
    {
        private readonly BapldmsvadContext _context;

        public PrefixRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<NumberSequence>> GetPrefixByPagedAsync(string? searchTerms, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.NumberSequences.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerms))
                {
                    query = query.Where(c => c.SequenceCode.Contains(searchTerms) ||
                                             c.SequenceName.Contains(searchTerms) ||
                                             c.Format.Contains(searchTerms) ||
                                             c.DealerCode.Contains(searchTerms));
                }

                int totalRecords = await query.CountAsync();

                var prefixes = await query
                    .AsNoTracking()
                    .OrderByDescending(c => c.CreatedDate)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                int startSrNo = (pageIndex * pageSize) + 1;

                return new PagedResponse<NumberSequence>
                {
                    Data = prefixes,
                    TotalRecords = totalRecords
                };
            }
            catch { throw; }
        }
        public async Task<IEnumerable<NumberSequence>> GetPrefixByDealerCode(string dealerCode)
        {
            return await _context.NumberSequences
                .Where(x => x.DealerCode == dealerCode)
                .ToListAsync();
        }
        public async Task<int> InsertPrefix(NumberSequence numberSequence)
        {
            await _context.AddAsync(numberSequence);
            return await _context.SaveChangesAsync();
        }
    }
}
