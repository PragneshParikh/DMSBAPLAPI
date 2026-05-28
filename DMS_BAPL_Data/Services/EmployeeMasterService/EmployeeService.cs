using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.EmployeeMasterRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.EmployeeMasterService
{
    public partial class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeMasterRepo _employeeMasterRepo;

        public EmployeeService(IEmployeeMasterRepo employeeMasterRepo)
        {
            _employeeMasterRepo = employeeMasterRepo;
        }

        Task<IEnumerable<EmployeeMaster>> IEmployeeService.Get() => _employeeMasterRepo.Get();
        Task<EmployeeMaster?> IEmployeeService.GetEmployeeById(int id) => _employeeMasterRepo.GetEmployeeById(id);
        Task<int> IEmployeeService.CreateNewUser(EmployeeMaster employeeMaster) => _employeeMasterRepo.CreateNewUser(employeeMaster);

        Task<int> IEmployeeService.UpdateEmployee(EmployeeMaster employeeMaster)=> _employeeMasterRepo.UpdateEmployee(employeeMaster);

    }
}
