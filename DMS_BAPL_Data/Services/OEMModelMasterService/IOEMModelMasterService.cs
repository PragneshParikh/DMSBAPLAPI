using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.OEMModelMasterService
{
    public interface IOEMModelMasterService
    {
        Task<List<OEMModelMasterViewModel>> GetAllOEMModels();
        Task<OEMModelMasterViewModel> GetOEMModelById(int id);
        Task<bool> AddOEMModel(OEMModelMasterViewModel oemViewModel);
        Task<bool> DeleteOEMModel(int id);
        Task<bool> UpdateOEMModel(OEMModelMasterViewModel oemViewModel);
        Task<byte[]> DownloadOEMModelExcel();
        Task<IEnumerable<OemmodelMaster>> GetOEMModelByStatus(bool isActive);
    }
}
