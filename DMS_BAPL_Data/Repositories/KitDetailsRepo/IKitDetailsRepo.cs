using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.KitDetailsRepo
{
    public interface IKitDetailsRepo
    {
        Task<IEnumerable<object>> GetKitDetailsByHeaderId(int headerId);
        Task<int> InsertKitDetails(List<KitDetailsViewModel> kitDetailsViewModels);
        Task<bool> UpdateKitDetails(KitDetail kitDetail);
    }
}
