using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
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

        async Task<IEnumerable<NumberSequence>> IPrefixRepo.Get()
        {
            try
            {
                return await _context.NumberSequences
                                    .AsNoTracking()
                                    .ToListAsync();
            }
            catch { throw; }
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
        public async Task<NumberSequence?> GetPrefixByDealerCodeModuleName(string dealerCode, string moduleName)
        {
            return await _context.NumberSequences
                .AsNoTracking()
                .Where(x => x.DealerCode == dealerCode && x.SequenceName == moduleName)
                .FirstOrDefaultAsync();
        }
        public async Task<int> AddPrefixForDealers(NumberSequenceViewModel numberSequenceViewModel)
        {
            try
            {
                var dealers = await _context.DealerMasters
                    .Select(d => new { d.Id, d.Dealercode })
                    .ToListAsync();

                var newNumberSequences = new List<NumberSequence>();

                foreach (var dealer in dealers)
                {
                    newNumberSequences.Add(new NumberSequence
                    {
                        SequenceCode = numberSequenceViewModel.SequenceCode.Replace("DealerCode", dealer.Dealercode.Length >= 3 ? dealer.Dealercode[^3..] : dealer.Dealercode),
                        SequenceName = numberSequenceViewModel.SequenceName,
                        Format = numberSequenceViewModel.Format,
                        NextNo = numberSequenceViewModel.NextNo,
                        Increment = numberSequenceViewModel.Increment,
                        DealerCode = dealer.Dealercode,
                        Year = numberSequenceViewModel.Year,
                        IsActive = numberSequenceViewModel.IsActive,
                        CreatedBy = numberSequenceViewModel.CreatedBy,
                        CreatedDate = numberSequenceViewModel.CreatedDate
                    });
                }

                await _context.NumberSequences.AddRangeAsync(newNumberSequences);
                int rowsInserted = await _context.SaveChangesAsync();
                return rowsInserted;
            }
            catch { throw; }
        }
        public async Task<int> InsertPrefix(NumberSequenceViewModel numberSequenceViewModel)
        {
            try
            {
                var newNumberSequences = (new NumberSequence
                {
                    SequenceCode = numberSequenceViewModel.SequenceCode,
                    SequenceName = numberSequenceViewModel.SequenceName,
                    Format = numberSequenceViewModel.Format,
                    Year = numberSequenceViewModel.Year,
                    IsActive = numberSequenceViewModel.IsActive,
                    CreatedBy = numberSequenceViewModel.CreatedBy,
                    CreatedDate = numberSequenceViewModel.CreatedDate
                });

                await _context.NumberSequences.AddAsync(newNumberSequences);
                int rowsInserted = await _context.SaveChangesAsync();
                return rowsInserted;
            }
            catch { throw; }
        }
        public async Task<int> UpdateNextNumberByDealerByModule(string dealerCode, string moduleName)
        {
            var existing = await _context.NumberSequences
                .FirstOrDefaultAsync(x => x.DealerCode == dealerCode && x.SequenceName == moduleName);

            if (existing == null)
            {
                return 0;
            }

            existing.NextNo++;

            await _context.SaveChangesAsync();

            return existing.Id;
        }
    }
}
