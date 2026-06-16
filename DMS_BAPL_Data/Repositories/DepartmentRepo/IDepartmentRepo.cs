using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.DepartmentRepo
{
    public interface IDepartmentRepo
    {
        Task<IEnumerable<DepartmentMaster>> Get();
        Task<bool> Insert(DepartmentMaster departmentMaster);
        Task<int> Update(DepartmentMaster departmentMaster);
    }
}
