using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ComplaintMasterRepo
{
    public interface IComplaintMaster
    {
        Task<List<ComplaintMasterViewModel>> GetComplaintMasterList();
        Task<ComplaintMasterViewModel?> GetComplaintMasterById(int id);
        Task<int> InsertComplaintMaster(ComplaintMasterViewModel model, string userId);
        Task<bool> UpdateComplaintMaster(ComplaintMasterViewModel model, string userId);
        Task<bool> DeleteComplaintMaster(int id, string userId);
        Task<bool> DeleteComplaintMaster(int id);
        Task<byte[]> DownloadComplaintMasterExcel();
    }
}
