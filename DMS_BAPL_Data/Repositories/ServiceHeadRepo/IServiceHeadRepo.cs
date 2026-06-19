using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ServiceHeadRepo
{
    public interface IServiceHeadRepo
    {

        Task<int> InsertServiceHead(ServiceHeadMasterViewModel serviceHeadMasterView, string userId);
        Task<int> UpdateServiceHeadName(ServiceHeadMasterViewModel serviceHeadMasterView, string userId);
        Task<int> DeleteServiceHead(int serviceHeadId);
        Task<List<ServiceHeadMasterViewModel>> GetAllServiceHead();
        Task<byte[]> DownloadServiceHeadMasterExcel();
    }
}
