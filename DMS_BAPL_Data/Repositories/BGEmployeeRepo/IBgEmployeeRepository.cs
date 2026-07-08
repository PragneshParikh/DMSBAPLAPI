using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.BgEmployeeMasterRepo
{
    public interface IBgEmployeeMasterRepo
    {
        Task<IEnumerable<BgEmployeeMaster>> Get();
        Task<BgEmployeeMaster?> GetById(int id);
        Task<BgEmployeeMaster> Create(BgEmployeeMaster bgEmployee);
        Task<int> Update(BgEmployeeMaster bgEmployee);
        Task<int> Delete(int id);
        Task<BgEmployeeMaster?> GetByEmail(string email);

        // Add to interface:
        Task<IEnumerable<AssignedDealerInfo>> GetAssignedDealerCodes(int excludeEmployeeId);

        Task<int> UpdateEntity(BgEmployeeMaster bgEmployee);

        Task<int> UpdateStatus(int id, bool isActive);


        Task SaveRoleMappings(int employeeId, List<RoleMappingDto> roleMappings);
        Task<IEnumerable<BgEmployeeRoleMapping>> GetRoleMappings(int employeeId);

        Task<IEnumerable<BgEmployeeListItemViewModel>> GetEmployeeListView();
    }
}