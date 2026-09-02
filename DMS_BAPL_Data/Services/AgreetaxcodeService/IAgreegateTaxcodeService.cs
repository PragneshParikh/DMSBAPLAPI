using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.AgreetaxcodeService
{
    public interface IAgreegateTaxcodeService
    {
        Task<AgreeTaxCodeViewModel> InsertAgreeTaxcodeAsync(AgreeTaxCodeViewModel agreeTaxCodeViewModel);
        Task<List<AggregateTaxCode>> GetAggregateTaxcodesAsync(string? search);
        Task<List<AggregateTaxCode>> GetAggregateTaxDetailsAsync(string ataxCode);
        Task<AggregateTaxCode> GetAggregateTaxcodeByIdAsync(int id);
        Task<AggregateTaxCode> UpdateAgreeTaxcodeAsync(int id, AgreeTaxCodeViewModel agreeTaxCodeViewModel);
        Task<List<TaxCodeWithRateViewModel>> GetTaxCodeWithRate();
        Task<AggregateTaxImportResultViewModel> ImportAggregateTaxCodeExcelAsync(IFormFile file);

        // Used only by Excel import — skip TaxCodeMasters validation.
        Task<AgreeTaxCodeViewModel> InsertAgreeTaxcodeNoValidationAsync(AgreeTaxCodeViewModel agreeTaxCodeViewModel);
        Task<AggregateTaxCode> UpdateAgreeTaxcodeNoValidationAsync(int id, AgreeTaxCodeViewModel agreeTaxCodeViewModel);
    }
}