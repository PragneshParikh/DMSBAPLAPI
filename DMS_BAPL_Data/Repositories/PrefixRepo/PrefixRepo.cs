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

        // ADDED
        public async Task<NumberSequence?> GetById(int id)
        {
            return await _context.NumberSequences
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // ADDED — used by the frontend to disable Save when a duplicate would be created.
        // excludeId lets edit mode ignore the record being edited itself.
        public async Task<bool> CheckDuplicate(string dealerCode, string moduleName, string year, string prefix, int? excludeId)
        {
            return await _context.NumberSequences.AnyAsync(x =>
                x.DealerCode == dealerCode &&
                x.SequenceName == moduleName &&
                x.Year == year &&
                x.SequenceCode.Contains(prefix) &&
                x.IsActive == true &&
                (excludeId == null || x.Id != excludeId.Value)
            );
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
                // ADDED — deactivate any existing active sequence for this
                // dealer + module before inserting the new one, so only the
                // newest configuration is ever "live" at a time.
                var previousActive = await _context.NumberSequences
                    .Where(x => x.DealerCode == numberSequenceViewModel.DealerCode
                             && x.SequenceName == numberSequenceViewModel.SequenceName
                             && x.IsActive == true)
                    .ToListAsync();

                foreach (var old in previousActive)
                {
                    old.IsActive = false;
                    old.UpdatedBy = numberSequenceViewModel.CreatedBy;
                    old.UpdatedDate = DateTime.UtcNow;
                }

                var newNumberSequence = new NumberSequence
                {
                    SequenceCode = numberSequenceViewModel.SequenceCode.Replace("DealerCode", numberSequenceViewModel.DealerCode.Length >= 3 ? numberSequenceViewModel.DealerCode[^3..] : numberSequenceViewModel.DealerCode),
                    SequenceName = numberSequenceViewModel.SequenceName,
                    Format = numberSequenceViewModel.Format,
                    Year = numberSequenceViewModel.Year,
                    DealerCode = numberSequenceViewModel.DealerCode,
                    NextNo = numberSequenceViewModel.NextNo,
                    Increment = numberSequenceViewModel.Increment,
                    IsActive = numberSequenceViewModel.IsActive,
                    CreatedBy = numberSequenceViewModel.CreatedBy,
                    CreatedDate = numberSequenceViewModel.CreatedDate
                };

                await _context.NumberSequences.AddAsync(newNumberSequence);
                await _context.SaveChangesAsync();

                return newNumberSequence.Id;
            }
            catch { throw; }
        }

        // ADDED — edit an existing sequence in place (no deactivation logic needed,
        // since we're modifying the same row, not creating a competing one)
        public async Task<int> UpdatePrefix(int id, NumberSequenceViewModel numberSequenceViewModel)
        {
            try
            {
                var existing = await _context.NumberSequences.FirstOrDefaultAsync(x => x.Id == id);
                if (existing == null) return 0;

                existing.SequenceCode = numberSequenceViewModel.SequenceCode.Replace(
                    "DealerCode",
                    numberSequenceViewModel.DealerCode.Length >= 3 ? numberSequenceViewModel.DealerCode[^3..] : numberSequenceViewModel.DealerCode
                );
                existing.SequenceName = numberSequenceViewModel.SequenceName;
                existing.Format = numberSequenceViewModel.Format;
                existing.Year = numberSequenceViewModel.Year;
                existing.DealerCode = numberSequenceViewModel.DealerCode;
                existing.NextNo = numberSequenceViewModel.NextNo;
                existing.Increment = numberSequenceViewModel.Increment;
                existing.IsActive = numberSequenceViewModel.IsActive;
                existing.UpdatedBy = numberSequenceViewModel.CreatedBy;
                existing.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existing.Id;
            }
            catch { throw; }
        }

        // ADDED
        public async Task<bool> DeletePrefix(int id)
        {
            var existing = await _context.NumberSequences.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null) return false;

            _context.NumberSequences.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> UpdateNextNumberByDealerByModule(string dealerCode, string moduleName)
        {
            var existing = await _context.NumberSequences
                .FirstOrDefaultAsync(x => x.DealerCode == dealerCode && x.SequenceName == moduleName);

            if (existing == null)
            {
                return 0;
            }

            existing.NextNo += existing.Increment;

            await _context.SaveChangesAsync();

            return existing.Id;
        }
        async Task<PagedResponse<NumberSequence>> IPrefixRepo.GetPrefixByPagedByDealer(
            int pageIndex,
            int pageSize,
            string? searchTerms,
            string? dealerCode)
        {
            try
            {
                var query = _context.NumberSequences
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    query = query.Where(x => x.DealerCode == dealerCode);
                }

                if (!string.IsNullOrWhiteSpace(searchTerms))
                {
                    query = query.Where(c =>
                        c.SequenceCode.Contains(searchTerms) ||
                        c.SequenceName.Contains(searchTerms) ||
                        c.Format.Contains(searchTerms) ||
                        c.DealerCode.Contains(searchTerms));
                }

                int totalRecords = await query.CountAsync();

                var prefixes = await query
                    .OrderByDescending(c => c.CreatedDate)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResponse<NumberSequence>
                {
                    Data = prefixes,
                    TotalRecords = totalRecords
                };
            }
            catch
            {
                throw;
            }
        }

    }
}
