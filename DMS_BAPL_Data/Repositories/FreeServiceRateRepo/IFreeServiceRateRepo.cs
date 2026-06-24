using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.FreeServiceRateRepo
{
    public interface IFreeServiceRateRepo
    {
        Task<IEnumerable<FreeServiceRate>> Get();
        Task<bool> Insert(List<FreeServiceRate> freeServiceRate);
        Task<int> Update(FreeServiceRate freeServiceRate);
        Task<IEnumerable<FreeServiceRateViewModel>> GetByOEMModelId(int? Id);
    }
}
