using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.RoleWiseMenuRightRepo
{
    public interface IRoleWiseMenuRightRepo
    {
        Task<IEnumerable<RoleWiseMenuRight>> Get();
        Task<IEnumerable<RoleWiseMenuRight>> GetMenuRightByRoleId(string roleId);
    }
}
