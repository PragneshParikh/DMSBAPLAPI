using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.TaxCodeMasterService
{
    public interface ITaxCodeMasterService
    {
        Task<IEnumerable<TaxCodeMaster>> GetAllTaxCodes();
        Task<TaxCodeMaster?> GetTaxCodeById(int id);
        Task<int> AddTaxCode(TaxCodeMasterViewModel taxCodeMasterViewModel);
        Task<int> UpdateTaxCode(TaxCodeMasterViewModel taxCodeMasterViewModel);
        Task<byte[]> DownloadTaxCodeExcel();
    }
}
