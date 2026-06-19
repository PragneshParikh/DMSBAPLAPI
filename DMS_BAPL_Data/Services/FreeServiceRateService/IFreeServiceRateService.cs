using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.FreeServiceRateService
{
    public interface IFreeServiceRateService
    {
        Task<IEnumerable<FreeServiceRate>> Get();
        Task<bool> Insert(List<FreeServiceRateViewModel> freeServiceRateViewModel);
        Task<int> Update(FreeServiceRateViewModel freeServiceRateViewModel);
        Task<IEnumerable<FreeServiceRateGroupViewModel>> GetByOEMModelId(int? Id);
    }
}
