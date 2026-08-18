using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DispatchMasterService
{
    public interface IDispatchMasterService
    {
        Task<(List<DispatchMasterListViewModel> Data, int TotalRecords)> GetAllAsync(DispatchMasterSearchViewModel searchModel);
        Task<DispatchMasterViewModel> GetByIdAsync(int id);
        Task<(bool Success, string Message)> SaveAsync(DispatchMasterViewModel model);
        Task<(bool Success, string Message)> ToggleActiveAsync(int id, bool isActive);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }
}
