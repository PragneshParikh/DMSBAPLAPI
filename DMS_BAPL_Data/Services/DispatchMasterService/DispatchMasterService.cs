using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DispatchMasterRepo;
using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DispatchMasterService
{
    public class DispatchMasterService : IDispatchMasterService
    {
        private readonly IDispatchMasterRepo _repository;

        public DispatchMasterService(IDispatchMasterRepo repository)
        {
            _repository = repository;
        }

        public async Task<(List<DispatchMasterListViewModel> Data, int TotalRecords)> GetAllAsync(DispatchMasterSearchViewModel searchModel)
        {
            var records = await _repository.GetAllAsync(
                searchModel.MasterType,
                searchModel.Name,
                searchModel.PageNumber,
                searchModel.PerPageRecords);

            var totalCount = await _repository.GetTotalCountAsync(searchModel.MasterType, searchModel.Name);

            var startSrNo = (searchModel.PageNumber - 1) * searchModel.PerPageRecords;

            var result = records.Select((x, index) => new DispatchMasterListViewModel
            {
                SrNo = startSrNo + index + 1,
                Id = x.Id,
                MasterType = x.MasterType,
                MasterName = x.MasterName,
                IsActive = x.IsActive
            }).ToList();

            return (result, totalCount);
        }

        public async Task<DispatchMasterViewModel> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;

            return new DispatchMasterViewModel
            {
                Id = entity.Id,
                MasterType = entity.MasterType,
                MasterName = entity.MasterName,
                IsActive = entity.IsActive
            };
        }

        public async Task<(bool Success, string Message)> SaveAsync(DispatchMasterViewModel model)
        {
            var exists = await _repository.ExistsByNameAsync(model.MasterType, model.MasterName, model.Id);
            if (exists)
                return (false, $"'{model.MasterName}' already exists under '{model.MasterType}'.");

            if (model.Id == 0)
            {
                var entity = new DispatchMaster
                {
                    MasterType = model.MasterType,
                    MasterName = model.MasterName,
                    IsActive = model.IsActive,
                    CreatedBy = model.UpdatedBy,
                    CreatedDate = DateTime.Now
                };

                var newId = await _repository.AddAsync(entity);
                return (true, $"Record created successfully with Id {newId}.");
            }
            else
            {
                var entity = new DispatchMaster
                {
                    Id = model.Id,
                    MasterType = model.MasterType,
                    MasterName = model.MasterName,
                    IsActive = model.IsActive,
                    UpdatedBy = model.UpdatedBy
                };

                var updated = await _repository.UpdateAsync(entity);
                return updated
                    ? (true, "Record updated successfully.")
                    : (false, "Record not found.");
            }
        }

        public async Task<(bool Success, string Message)> ToggleActiveAsync(int id, bool isActive)
        {
            var updated = await _repository.ToggleActiveAsync(id, isActive);
            return updated
                ? (true, $"Record {(isActive ? "activated" : "deactivated")} successfully.")
                : (false, "Record not found.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            return deleted
                ? (true, "Record deleted successfully.")
                : (false, "Record not found.");
        }
    }
}
