using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo
{
    public class AgreetaxcodeRepo : IAgreetaxcodeRepo
    {
        private readonly BapldmsvadContext _context;

        public AgreetaxcodeRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<AgreeTaxCodeViewModel> IAgreetaxcodeRepo.InsertAgreeTaxcodeAsync(AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        {
            try
            {
                var aggregateTaxList = new List<AggregateTaxCode>();

                foreach (var item in agreeTaxCodeViewModel.TaxDetails)
                {
                    // DUPLICATE CHECK
                    var isDuplicate = await _context.AggregateTaxCodes
                        .AnyAsync(x => x.AtaxCode == agreeTaxCodeViewModel.AtaxCode
                                    && x.TaxCode == item.TaxCode);

                    if (isDuplicate)
                    {
                        throw new BusinessException($"TaxCode '{item.TaxCode}' already exists for ATaxCode '{agreeTaxCodeViewModel.AtaxCode}'");
                    }

                    //  TAXCODE VALIDATION
                    var taxCodeData = await _context.TaxCodeMasters
                        .Where(t => t.TaxCode == item.TaxCode && t.TaxRate == item.TaxRate)
                        .OrderByDescending(t => t.EffectiveDate)
                        .FirstOrDefaultAsync();

                    if (taxCodeData == null)
                    {
                        throw new Exception("Invalid Tax code");
                    }

                    var aggregateTax = new AggregateTaxCode
                    {
                        AtaxCode = agreeTaxCodeViewModel.AtaxCode,
                        Description = agreeTaxCodeViewModel.Description,
                        SrNo = item.SrNo,
                        TaxCode = taxCodeData.TaxCode,
                        TaxRate = taxCodeData.TaxRate,
                        CreatedBy = agreeTaxCodeViewModel.CreatedBy,
                        CreatedDate = DateTime.UtcNow
                    };

                    aggregateTaxList.Add(aggregateTax);
                }

                _context.AggregateTaxCodes.AddRange(aggregateTaxList);
                await _context.SaveChangesAsync();

                return agreeTaxCodeViewModel;
            }
            catch (Exception ex)
            {
                
                throw new Exception(ex.Message);
            }
        }
        //show the aggregate tax code list based on search
        async Task<List<AggregateTaxCode>> IAgreetaxcodeRepo.GetAggregateTaxcodesAsync(string? search)
        {
            var query = _context.AggregateTaxCodes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    EF.Functions.Like(a.AtaxCode, $"%{search}%") ||
                    EF.Functions.Like(a.Description, $"%{search}%"));
            }

            var result = await query
                .GroupBy(a => new { a.AtaxCode, a.Description })
                .Select(g => new
                {
                    AtaxCode = g.Key.AtaxCode,
                    Description = g.Key.Description,
                    LatestCreatedDate = g.Max(x => x.CreatedDate) // important
                })
                .OrderByDescending(x => x.LatestCreatedDate) // sorting here
                .Select(x => new AggregateTaxCode
                {
                    AtaxCode = x.AtaxCode,
                    Description = x.Description
                })
                .ToListAsync();

            return result;
        }
        //agreecode tax based show the details
        async Task<List<AggregateTaxCode>> IAgreetaxcodeRepo.GetAggregateTaxDetailsAsync(string ataxCode)

        {
            return await _context.AggregateTaxCodes
                .Where(a => a.AtaxCode == ataxCode)
                .OrderBy(a => a.SrNo)
                .ToListAsync();
        }

        async Task<AggregateTaxCode> IAgreetaxcodeRepo.GetAggregateTaxcodeByIdAsync(int id)
        {
            return await _context.AggregateTaxCodes.FindAsync(id);
        }

        async Task<List<TaxCodeWithRateViewModel>> IAgreetaxcodeRepo.GetTaxCodeWithRate()
        {
            return await _context.TaxCodeMasters
         .Where(x => !string.IsNullOrEmpty(x.TaxCode))
         .GroupBy(x => x.TaxCode)
         .Select(g => g
             .OrderByDescending(x => x.EffectiveDate)
             .Select(x => new TaxCodeWithRateViewModel
             {
                 TaxCode = x.TaxCode,
                 TaxRate = x.TaxRate
             })
             .FirstOrDefault())
         .ToListAsync();
        }
        //async Task<AggregateTaxCode> IAgreetaxcodeRepo.UpdateAgreeTaxcodeAsync(int id, AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        //{
        //    // Find existing record
        //    var existingAgreeCodemaster = await _context.AggregateTaxCodes
        //                                                .FirstOrDefaultAsync(a => a.Id == id);

        //    if (existingAgreeCodemaster == null)
        //    {
        //        throw new Exception("Aggregate Tax Code not found");
        //    }

        //    // Validate TaxCode from master table
        //    var taxCodeData = await _context.TaxCodeMasters
        //        .Where(t => t.TaxCode == agreeTaxCodeViewModel.TaxDetails.FirstOrDefault().TaxCode
        //                 && t.TaxRate == agreeTaxCodeViewModel.TaxDetails.FirstOrDefault().TaxRate)
        //        .OrderByDescending(t => t.EffectiveDate)
        //        .FirstOrDefaultAsync();

        //    if (taxCodeData == null)
        //    {
        //        throw new Exception("Invalid Tax code");
        //    }

        //    // Update fields
        //    existingAgreeCodemaster.AtaxCode = agreeTaxCodeViewModel.AtaxCode;
        //    existingAgreeCodemaster.Description = agreeTaxCodeViewModel.Description;
        //    existingAgreeCodemaster.TaxCode = taxCodeData.TaxCode;
        //    existingAgreeCodemaster.TaxRate = taxCodeData.TaxRate;

        //    existingAgreeCodemaster.UpdatedBy = agreeTaxCodeViewModel.UpdatedBy;
        //    existingAgreeCodemaster.UpdatedDate = DateTime.UtcNow;

        //    await _context.SaveChangesAsync();

        //    return existingAgreeCodemaster;
        //}
    }

}