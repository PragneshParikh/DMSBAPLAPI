using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ExtendedBatteryWarrantyRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ExtendedBatteryWarrantyService
{
    public partial class ExtendedBatteryWarrantyService : IExtendedBatteryWarrantyService
    {
        private readonly IExtendedBatteryWarrantyRepo _extendedBatteryWarrantyRepo;

        public ExtendedBatteryWarrantyService(IExtendedBatteryWarrantyRepo extendedBatteryWarrantyRepo)
        {
            _extendedBatteryWarrantyRepo = extendedBatteryWarrantyRepo;
        }

        Task<IEnumerable<ExtendedBatteryWarranty>> IExtendedBatteryWarrantyService.Get() => _extendedBatteryWarrantyRepo.Get();
        Task<PagedResponse<object>> IExtendedBatteryWarrantyService.GetExtendedBatteryWarrantyByPaged(string? searchTerm, int pageIndex, int pageSize) => _extendedBatteryWarrantyRepo.GetExtendedBatteryWarrantyByPaged(searchTerm, pageIndex, pageSize);
        Task<ExtendedBatteryWarranty?> IExtendedBatteryWarrantyService.GetSchemeDetailById(int id) => _extendedBatteryWarrantyRepo.GetSchemeDetailById(id);
        int IExtendedBatteryWarrantyService.Insert(ExtendedBatteryWarrantyViewModel extendedBatteryWarrantyViewModel) => _extendedBatteryWarrantyRepo.Insert(extendedBatteryWarrantyViewModel);
        Task<int> IExtendedBatteryWarrantyService.Update(ExtendedBatteryWarrantyViewModel extendedBatteryWarrntyViewModel) => _extendedBatteryWarrantyRepo.Update(extendedBatteryWarrntyViewModel);
    }
}
