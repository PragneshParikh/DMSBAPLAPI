using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo
{
    public interface IAgreetaxcodeRepo
    {
        Task<AgreeTaxCodeViewModel> InsertAgreeTaxcodeAsync(AgreeTaxCodeViewModel agreeTaxCodeViewModel);
       
        Task<List<AggregateTaxCode>> GetAggregateTaxcodesAsync(string? search);

        Task<List<AggregateTaxCode>> GetAggregateTaxDetailsAsync(string ataxCode);
        Task<AggregateTaxCode> GetAggregateTaxcodeByIdAsync(int id);
       // Task<AggregateTaxCode> UpdateAgreeTaxcodeAsync(int id, AgreeTaxCodeViewModel aggregateTaxCode);
        Task <List<TaxCodeWithRateViewModel>> GetTaxCodeWithRate();
    }
}
