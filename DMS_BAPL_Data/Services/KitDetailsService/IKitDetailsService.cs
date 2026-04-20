using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.KitDetailsService
{
    public interface IKitDetailsService
    {
        Task<IEnumerable<object>> GetKitDetailsByHeaderId(int headerId);
        Task<PagedResponse<object>> GetKitDetailsByPaged(int pageIndex, int pageSize, int headerId);
        Task<int> InsertKitDetails(List<KitDetailsViewModel> kitDetailSViewModels);
        Task<bool> UpdateKitDetails(List<KitDetailsViewModel> kitDetailsViewModels);
    }
}
