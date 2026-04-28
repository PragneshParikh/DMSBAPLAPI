using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ExtendedBatteryWarrantyService
{
    public interface IExtendedBatteryWarrantyService
    {
        Task<IEnumerable<ExtendedBatteryWarranty>> Get();
        Task<PagedResponse<object>> GetExtendedBatteryWarrantyByPaged(string? searchTerm, int pageIndex, int pageSize);
        Task<ExtendedBatteryWarranty?> GetSchemeDetailById(int id);
        int Insert(ExtendedBatteryWarrantyViewModel extendedBatteryWarrntyViewModel);
        Task<int> Update(ExtendedBatteryWarrantyViewModel extendedBatteryWarrntyViewModel);
    }
}
