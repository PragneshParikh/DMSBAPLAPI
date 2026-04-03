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
        Task<int> InsertKitDetails(List<KitDetailsViewModel> kitDetailSViewModels);
        Task<bool> UpdateKitDetails(List<KitDetailsViewModel> kitDetailsViewModels);
    }
}
