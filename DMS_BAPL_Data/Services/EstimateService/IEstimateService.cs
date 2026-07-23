using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.EstimateService
{
    public interface IEstimateService
    {
        Task<int> CreateAsync(EstimateCreateViewModel model, string userId);
        Task<EstimateResponseViewModel?> GetByIdAsync(int id);
        Task<EstimatePagedResponse> GetAllAsync(EstimateFilterModel filter);
        Task UpdateAsync(int id, EstimateCreateViewModel model, string userId);
        Task<bool> DeleteAsync(int id);
        Task<string> GenerateNextEstimationNoAsync();
        Task<List<JobTypeDropdownItem>> GetJobTypesAsync();
        Task<List<PartSearchResultViewModel>> SearchPartsAsync(string query);
        Task<List<LabourSearchResultViewModel>> SearchLabourAsync(string query);
        Task<List<string>> GetEstimationNumbersAsync(string? dealerCode);

        Task<List<PartSearchResultViewModel>> SearchPartsAsync(string query, int maxResults = 20);
        Task<List<LabourSearchResultViewModel>> SearchLabourAsync(string query, int maxResults = 20);

        Task<byte[]?> DownloadEstimatePdfAsync(int id);
    }
}