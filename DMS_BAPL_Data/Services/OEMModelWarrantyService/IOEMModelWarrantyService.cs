using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.OEMModelWarrantyService
{
    public interface IOEMModelWarrantyService
    {
        Task<OemmodelWarranty> CreateAsync(OemModelWarrantyViewModel entity);
        Task<OemModelWarrantyResponseViewModel?> GetDetailsByIdAsync(int id);
        Task<List<OemModelWarrantyResponseViewModel>> GetAllAsync(string? searchTerm, DateOnly? effectiveDateFrom, DateOnly? effectiveDateTo);
        Task<OemmodelWarranty> UpdateAsync(int id, OemModelWarrantyViewModel entity);
        Task<bool> DeleteAsync(int id);
        Task<string> LastEffectiveDate(int oemmodelId);
        Task<byte[]> downloadExcel();
    }
}

