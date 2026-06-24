using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.RoleRepo
{
    public interface IRoleRepo
    {
        Task<IEnumerable<AspNetRole>> GetRoles();
        Task<AspNetRole> GetRoleById(string id);

        Task AddRoleCategoryMapping(RoleCategoryMapping mapping);
        Task<List<RoleCategoryMapping>> GetMappingsByCategory(string category);

        Task<List<RoleCategoryMapping>> GetAllMappings();
        Task<bool> DeleteMapping(int id);


    }
}
