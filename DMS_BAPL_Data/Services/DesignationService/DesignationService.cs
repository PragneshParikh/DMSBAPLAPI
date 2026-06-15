using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DesignationRepo;
using DMS_BAPL_Data.Services.DepartmentService;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DesignationService
{
    public partial class DesignationService : IDesignationService
    {
        private readonly IDesignationRepo _designationRepo;
        public DesignationService(IDesignationRepo designationRepo)
        {
            _designationRepo = designationRepo;
        }

        Task<IEnumerable<DesignationMaster>> IDesignationService.Get() => _designationRepo.Get();
        Task<bool> IDesignationService.Insert(DesignationViewModel designationViewModel)
        {
            var entity = new DesignationMaster
            {
                DesignationCode = designationViewModel.DesignationCode,
                DesignationName = designationViewModel.DesignationName,
                DepartmentId = designationViewModel.DepartmentId,
                IsActive = designationViewModel.IsActive,
                CreatedBy = designationViewModel.CreatedBy,
                CreatedDate = designationViewModel.CreatedDate
            };
            return _designationRepo.Insert(entity);
        }
        Task<int> IDesignationService.Update(DesignationViewModel designationViewModel)
        {
            var entity = new DesignationMaster
            {
                DesignationId = designationViewModel.DesignationId,
                DesignationCode = designationViewModel.DesignationCode,
                DesignationName = designationViewModel.DesignationName,
                DepartmentId = designationViewModel.DepartmentId,
                IsActive = designationViewModel.IsActive,
                CreatedBy = designationViewModel.CreatedBy,
                CreatedDate = designationViewModel.CreatedDate,
                ModifiedBy = designationViewModel.ModifiedBy,
                ModifiedDate = designationViewModel.ModifiedDate
            };
            return _designationRepo.Update(entity);
        }

    }
}
