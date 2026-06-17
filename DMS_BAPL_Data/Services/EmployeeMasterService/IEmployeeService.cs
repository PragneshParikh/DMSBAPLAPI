using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.EmployeeMasterRepo;
using DMS_BAPL_Utils.ViewModels;
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

        Task<int> UpdateEmployee(EmployeeMaster employeeMaster);

        Task<object?> GetDealerByCode(string dealerCode);
        Task<List<object>> GetLocationsByDealer(string dealerCode);

        Task<List<EmployeeDesignationWiseViewModel>> GetEmployeesByDesignation(string? dealerCode, string designation);
    }
}
