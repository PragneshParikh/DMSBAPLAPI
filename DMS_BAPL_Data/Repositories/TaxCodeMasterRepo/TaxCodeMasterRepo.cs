using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.TaxCodeMasterRepo
{
    public class TaxCodeMasterRepo : ITaxCodeMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public TaxCodeMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaxCodeMaster>> GetAllTaxCodes()
        {
            IEnumerable<TaxCodeMaster> taxCodeMasterList = await _context.TaxCodeMasters
                .OrderByDescending(taxCodeMaster => taxCodeMaster.Id)
                .ToListAsync();

            return taxCodeMasterList;
        }

        public async Task<TaxCodeMaster?> GetTaxCodeById(int id)
        {
            TaxCodeMaster? taxCodeMaster = await _context.TaxCodeMasters
                .FirstOrDefaultAsync(taxCodeMasterRecord => taxCodeMasterRecord.Id == id);

            return taxCodeMaster;
        }

        public async Task<int> AddTaxCode(TaxCodeMasterViewModel taxCodeMasterViewModel)
        {
            TaxCodeMaster taxCodeMaster = new TaxCodeMaster
            {
                TaxCode = taxCodeMasterViewModel.TaxCode,
                Description = taxCodeMasterViewModel.Description,
                TaxRate = taxCodeMasterViewModel.TaxRate,
                EffectiveDate = taxCodeMasterViewModel.EffectiveDate,
                CreatedBy = taxCodeMasterViewModel.CreatedBy,
                CreatedDate = DateTime.Now,
                UpdatedBy = taxCodeMasterViewModel.UpdatedBy,
                UpdatedDate = taxCodeMasterViewModel.UpdatedDate
            };

            await _context.TaxCodeMasters.AddAsync(taxCodeMaster);
            await _context.SaveChangesAsync();

            return taxCodeMaster.Id;
        }

        public async Task<int> UpdateTaxCode(TaxCodeMasterViewModel taxCodeMasterViewModel)
        {
            TaxCodeMaster? existingTaxCodeMaster = await _context.TaxCodeMasters
                .FirstOrDefaultAsync(taxCodeMasterRecord => taxCodeMasterRecord.Id == taxCodeMasterViewModel.Id);

            if (existingTaxCodeMaster == null)
            {
                return 0;
            }

            existingTaxCodeMaster.TaxCode = taxCodeMasterViewModel.TaxCode;
            existingTaxCodeMaster.Description = taxCodeMasterViewModel.Description;
            existingTaxCodeMaster.TaxRate = taxCodeMasterViewModel.TaxRate;
            existingTaxCodeMaster.EffectiveDate = taxCodeMasterViewModel.EffectiveDate;
            existingTaxCodeMaster.UpdatedBy = taxCodeMasterViewModel.UpdatedBy;
            existingTaxCodeMaster.UpdatedDate = DateTime.Now;

            _context.TaxCodeMasters.Update(existingTaxCodeMaster);

            int affectedRows = await _context.SaveChangesAsync();
            return affectedRows;
        }
    }
}
   
