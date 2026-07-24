using DMS_BAPL_Data.DBModels;

namespace DMS_BAPL_Data.Repositories.BgRoleRepo
{
    public interface IBgRoleRepo
    {
        Task<IEnumerable<AspNetRole>> GetRoles();
        Task<AspNetRole> GetRoleById(string id);
        Task AddRoleCategoryMapping(BgRoleCategoryMapping mapping);
        Task<List<BgRoleCategoryMapping>> GetMappingsByCategory(string category);
        Task<List<BgRoleCategoryMapping>> GetAllMappings();
        Task<bool> DeleteMapping(int id);
        Task<BgRoleCategoryMapping?> GetMappingById(int id);
        Task<bool> UpdateMappingNameAndCategory(int id, string roleName, string category);
    }
}