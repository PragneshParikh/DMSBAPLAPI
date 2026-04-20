using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.OEMModelWarrantyRepo
{
    public interface IOEMModelWarrantyRepo
    {
        Task<OemmodelWarranty> CreateAsync(OemmodelWarranty entity);
        Task<OemmodelWarranty?> GetByIdAsync(int id);
        //Task<List<OemModelWarrantyResponseViewModel>> GetAllAsync();
        Task<List<OemModelWarrantyResponseViewModel>> GetAllAsync(string? searchTerm, DateOnly? effectiveDateFrom, DateOnly? effectiveDateTo);
        Task<OemmodelWarranty> UpdateAsync(OemmodelWarranty entity);
        Task<bool> DeleteAsync(int id);
        Task<OemModelWarrantyResponseViewModel?> GetDetailsByIdAsync(int id);
        Task<string> LastEffectiveDate(int oemmodelId);
    }
}
