using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.BgEmployeeMasterService
{
    public interface IBgEmployeeMasterService
    {
        Task<IEnumerable<BgEmployeeMaster>> Get();
        Task<BgEmployeeMaster?> GetById(int id);
        Task<BgEmployeeMaster> Create(BgEmployeeViewModel model);
        Task<int> Update(BgEmployeeViewModel model);
        Task<int> Delete(int id);
        Task<BgEmployeeMaster?> GetByEmail(string email);
    }
}
