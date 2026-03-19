using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.HSNWiseTaxCodeRepo
{
    public class HSNWiseTaxcodeRepo : IHSNWiseTaxcodeRepo
    {
        private readonly BapldmsvadContext _context;

        public HSNWiseTaxcodeRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<List<HSNCodeList>> IHSNWiseTaxcodeRepo.GetHsncodeList()
        {
            return await _context.HsncodeMasters
                .Where(x => !string.IsNullOrEmpty(x.Hsncode))
                .Select(x => new HSNCodeList
                {
                    HsnCodeDD = x.Hsncode
                })
                .Distinct()
                .ToListAsync();
        }

        async Task<List<AggregateTaxCode>> IHSNWiseTaxcodeRepo.GetAggregateTaxCodeList()
        {
            return await _context.AggregateTaxCodes
                .Where(x => !string.IsNullOrEmpty(x.AtaxCode))
                .Select(x => new AggregateTaxCode
                {
                    Id = x.Id,
                    AtaxCode = x.AtaxCode,
                    TaxCode = x.TaxCode,
                    TaxRate = x.TaxRate

                })
                .ToListAsync();
        }

        async Task<HsnwiseTaxCodeViewModel> IHSNWiseTaxcodeRepo.InsertHsnwiseTaxcodedetails(HsnwiseTaxCodeViewModel hsnwiseTaxCodeViewModel)
        {
            var hsnwiseTaxCodeList = new List<HsnwiseTaxCode>();
            try
            {
                var hsnwiseTaxCode = new HsnwiseTaxCode
                {
                    Hsncode = hsnwiseTaxCodeViewModel.Hsncode,
                    AtaxCode = hsnwiseTaxCodeViewModel.AtaxCode,
                    StateFlag = hsnwiseTaxCodeViewModel.StateFlag,
                    EffectiveDate = hsnwiseTaxCodeViewModel.EffectiveDate,
                    CreatedBy = hsnwiseTaxCodeViewModel.CreatedBy,
                    CreatedDate = DateTime.UtcNow
                };

                _context.HsnwiseTaxCodes.AddAsync(hsnwiseTaxCode);
                await _context.SaveChangesAsync();
                return hsnwiseTaxCodeViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while inserting HSN-wise tax code details: {ex.Message}");
                throw;
            }
        }

        async Task<List<HsnwiseTaxCode>> IHSNWiseTaxcodeRepo.GetHsnwiseTaxcodedetails(string? search)
        {
            IQueryable<HsnwiseTaxCode> query = _context.HsnwiseTaxCodes;

            if (!string.IsNullOrWhiteSpace(search))
            {
                DateTime parsedDate;

                if (DateTime.TryParse(search, out parsedDate))
                {
                    // Date search
                    query = query.Where(h => h.EffectiveDate.Date == parsedDate.Date);
                }
                else
                {
                    // Text search
                    query = query.Where(h =>
                        EF.Functions.Like(h.Hsncode, $"%{search}%") ||
                        EF.Functions.Like(h.AtaxCode, $"%{search}%") ||
                        EF.Functions.Like(h.StateFlag, $"%{search}%")
                    );
                }
            }

            return await query.ToListAsync();
        }

    }
}
