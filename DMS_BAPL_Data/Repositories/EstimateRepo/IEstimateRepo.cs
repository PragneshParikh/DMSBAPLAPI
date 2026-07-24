using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.EstimateRepo
{
    public interface IEstimateRepo
    {
        Task<int> CreateAsync(EstimateHeader entity);
        Task<EstimateHeader?> GetByIdAsync(int id);
        Task<EstimatePagedResponse> GetAllAsync(EstimateFilterModel filter);
        Task UpdateAsync(EstimateHeader entity);
        Task<bool> DeleteAsync(int id);
        Task<string?> GetLastEstimationNoAsync();
        Task<List<JobTypeDropdownItem>> GetJobTypesAsync();
        Task<string?> GetJobTypeNameAsync(int? jobTypeId);
        Task<List<string>> GetEstimationNumbersAsync(string? dealerCode);
        Task<List<PartSearchResultViewModel>> SearchPartsAsync(string query, int maxResults = 20);
        Task<List<LabourSearchResultViewModel>> SearchLabourAsync(string query, int maxResults = 20);

        Task<EstimatePrintViewModel?> GetEstimatePrintDataAsync(int id);
    }
}