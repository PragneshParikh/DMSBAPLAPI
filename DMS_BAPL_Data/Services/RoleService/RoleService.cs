using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.RoleRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.RoleService
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepo _roleRepo;
        public RoleService(IRoleRepo roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<IEnumerable<AspNetRole>> GetRoles()
        {
            return await _roleRepo.GetRoles();
        }

        public async Task<AspNetRole> GetRoleById(string roleId)
        {
            return await _roleRepo.GetRoleById(roleId);
        }
    }
}
