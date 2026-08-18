using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.DispatchMasterRepo
{
    public interface IDispatchMasterRepo
    {
        Task<List<DispatchMaster>> GetAllAsync(string masterType, string name, int pageNumber, int perPageRecords);
        Task<int> GetTotalCountAsync(string masterType, string name);
        Task<DispatchMaster> GetByIdAsync(int id);
        Task<bool> ExistsByNameAsync(string masterType, string masterName, int excludeId = 0);
        Task<int> AddAsync(DispatchMaster model);
        Task<bool> UpdateAsync(DispatchMaster model);
        Task<bool> ToggleActiveAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
    }
}
