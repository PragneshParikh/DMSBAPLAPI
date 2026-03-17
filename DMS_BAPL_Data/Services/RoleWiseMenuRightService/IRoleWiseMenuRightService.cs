using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Data.DBModels;

namespace DMS_BAPL_Data.Services.RoleWiseMenuRightService
{
    public interface IRoleWiseMenuRightService
    {
        Task<IEnumerable<RoleWiseMenuRight>> Get();
        Task<IEnumerable<RoleWiseMenuRight>> GetMenuRightByRoleId(string? roleId);
    }
}
