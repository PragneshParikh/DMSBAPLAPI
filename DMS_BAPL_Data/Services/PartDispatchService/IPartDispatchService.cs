using DMS_BAPL_Data.DBModels;

namespace DMS_BAPL_Data.Services.PartDispatchService
{
    public interface IPartDispatchService
    {
        Task<List<DmsPartDispatch>> GetAllAsync();
        Task<DmsPartDispatch?> GetByIdAsync(int id);
        Task<DmsPartDispatch> CreateAsync(DmsPartDispatch item, string userId);
        Task<DmsPartDispatch?> UpdateAsync(DmsPartDispatch item, string userId);
        Task<bool> DeleteAsync(int id);
        Task<List<DmsPartDispatch>> CreateBulkAsync(List<DmsPartDispatch> items, string userId);
        Task<int> ImportFromExcelAsync(Stream fileStream, string userId);
    }
}