using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ServiceTypeRepo
{
    public interface IServiceTypeMasterRepo
    {
        Task<int> InsertserviceType(ServiceTypeMasterViewModel serviceTypeMasterViewModel, string userId);
        Task<int> UpdateserviceTypeName(ServiceTypeMasterViewModel serviceTypeMasterViewModel, string userId);
        Task<int> DeleteserviceType(int serviceTypeId);
        Task<List<ServiceTypeMasterViewModel>> GetAllServiceType();
        Task<byte[]> DownloadserviceTypeMasterExcel();
    }
}
