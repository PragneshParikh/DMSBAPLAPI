using DMS_BAPL_Data.DBModels;

namespace DMS_BAPL_Data.Repositories.PartDispatchRepo
{
    public interface IPartDispatchRepo
    {
        Task<List<DmsPartDispatch>> GetAllAsync();
        Task<DmsPartDispatch?> GetByIdAsync(int id);
        Task<DmsPartDispatch> CreateAsync(DmsPartDispatch item, string userId);
        Task<DmsPartDispatch?> UpdateAsync(DmsPartDispatch item, string userId);
        Task<bool> DeleteAsync(int id);
        Task<int> ImportAsync(List<DmsPartDispatch> items, string userId);
    }
}