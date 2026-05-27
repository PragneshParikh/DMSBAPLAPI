using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.EmployeeMasterService
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeMaster>> Get();
        Task<EmployeeMaster?> GetEmployeeById(int id);
        Task<int> CreateNewUser(EmployeeMaster employeeMaster);
    }
}
