using DMS_BAPL_Data.DBModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.BgEmployeeMasterRepo
{
    public interface IBgEmployeeMasterRepo
    {
        Task<IEnumerable<BgEmployeeMaster>> Get();
        Task<BgEmployeeMaster?> GetById(int id);
        Task<BgEmployeeMaster> Create(BgEmployeeMaster bgEmployee);  // returns entity with generated Id
        Task<int> Update(BgEmployeeMaster bgEmployee);
        Task<int> Delete(int id);
        Task<BgEmployeeMaster?> GetByEmail(string email);
    }
}