using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.AgreetaxcodeService
{
    public class AgreegateTaxcodeService : IAgreegateTaxcodeService
    {
        private readonly IAgreetaxcodeRepo _agreeTaxcodeRepo;

        public AgreegateTaxcodeService(IAgreetaxcodeRepo agreeTaxcodeRepo)
        {
            _agreeTaxcodeRepo = agreeTaxcodeRepo;
        }

        public async Task<AgreeTaxCodeViewModel> InsertAgreeTaxcodeAsync(AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        {
            return await _agreeTaxcodeRepo.InsertAgreeTaxcodeAsync(agreeTaxCodeViewModel);
        }

        public async Task<List<AggregateTaxCode>> GetAggregateTaxcodesAsync(string? search)
        {
            return await _agreeTaxcodeRepo.GetAggregateTaxcodesAsync(search);
        }

        public  async Task<List<AggregateTaxCode>> GetAggregateTaxDetailsAsync(string ataxCode)
        {
            return await _agreeTaxcodeRepo.GetAggregateTaxDetailsAsync(ataxCode);
        }

        public  async Task<AggregateTaxCode> GetAggregateTaxcodeByIdAsync(int id)
        {
            return await _agreeTaxcodeRepo.GetAggregateTaxcodeByIdAsync(id);
        }

        //public  async Task<AggregateTaxCode> UpdateAgreeTaxcodeAsync(int id, AgreeTaxCodeViewModel agreeTaxCodeViewModel)
        //{
        //    return await _agreeTaxcodeRepo.UpdateAgreeTaxcodeAsync(id, agreeTaxCodeViewModel);
        //}

        public  async Task <List<TaxCodeWithRateViewModel>> GetTaxCodeWithRate()
        {
            return await _agreeTaxcodeRepo.GetTaxCodeWithRate();
        }
    }
}
