using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DepartmentRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DepartmentService
{
    public partial class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepo _departmentRepo;

        public DepartmentService(IDepartmentRepo departmentRepo)
        {
            _departmentRepo = departmentRepo;
        }

        Task<IEnumerable<DepartmentMaster>> IDepartmentService.Get() => _departmentRepo.Get();
        Task<bool> IDepartmentService.Insert(DepartmentViewModel departmentViewModel)
        {
            var entity = new DepartmentMaster
            {
                DepartmentName = departmentViewModel.DepartmentName,
                IsActive = departmentViewModel.IsActive,
                CreatedBy = departmentViewModel.CreatedBy,
                CreatedDate = departmentViewModel.CreatedDate,
            };
            return _departmentRepo.Insert(entity);
        }
        Task<int> IDepartmentService.Update(DepartmentViewModel departmentViewModel)
        {
            var entity = new DepartmentMaster
            {
                DepartmentId = departmentViewModel.DepartmentId,
                DepartmentCode = departmentViewModel.DepartmentCode,
                DepartmentName = departmentViewModel.DepartmentName,
                IsActive = departmentViewModel.IsActive,
                CreatedBy = departmentViewModel.CreatedBy,
                CreatedDate = departmentViewModel.CreatedDate,
                ModifiedBy = departmentViewModel.ModifiedBy,
                ModifiedDate = departmentViewModel.ModifiedDate
            };
            return _departmentRepo.Update(entity);
        }

    }
}
