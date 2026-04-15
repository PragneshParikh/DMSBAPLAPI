using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.MaterialTransferRepo
{
    public interface IMaterialTransferRepo
    {
        Task<string> GetIssueIdAsync();
        Task<IEnumerable<object>> GetMeterialByJobId(int jobId);
        Task<int> InsertMaterials(List<MaterialTransferViewModel> materialTransferViewModels);
        Task<int> DeleteMaterials(List<int> ids);
        Task<int> UpdateMaterialDetails(List<MaterialTransferViewModel> materialTransferViewModels);
    }
}
