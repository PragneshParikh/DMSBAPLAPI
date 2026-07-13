using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.BgEmployeeMasterService
{
    public interface IBgEmployeeMasterService
    {
        Task<IEnumerable<BgEmployeeMaster>> Get();
        Task<BgEmployeeMaster?> GetById(int id);
        Task<BgEmployeeMaster> Create(BgEmployeeViewModel model);
        Task<int> Update(BgEmployeeViewModel model);
        Task<int> UpdateStatus(int id, bool isActive);
        Task<byte[]> DownloadBgEmployeeExcel();
        Task<int> Delete(int id);
        Task<BgEmployeeMaster?> GetByEmail(string email);
        Task<IEnumerable<AssignedDealerInfo>> GetAssignedDealerCodes(int excludeEmployeeId);
        Task<IEnumerable<BgEmployeeRoleMapping>> GetRoleMappings(int employeeId);
        Task<IEnumerable<BgEmployeeListItemViewModel>> GetEmployeeListView();

        // ---- TSM ERP integration ----------------------------------
        Task<TsmEntryViewModel?> FetchTsmDetailsAsync(string tsmCode);
        Task<List<(string Attempt, int StatusCode, string Body)>> FetchTsmRawAsync(string tsmCode);
        Task<BgEmployeeMaster> ConsumeTsmEntryAsync(TsmEntryPayload payload);
    }
}