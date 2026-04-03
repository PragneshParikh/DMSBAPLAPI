using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.KitHeaderRepo
{
    public interface IKitHeaderRepo
    {
        Task<PagedResponse<KitHeader>> GetKitByPagedAsync(string? searchTerms, int pageIndex, int pageSize);
        Task<int> InsertKitHeader(KitHeader kitHeader);
        Task<KitHeader?> GetKitById(int id);
        Task<int> UpdateKitHeader(KitHeader kitHeader);
    }
}
