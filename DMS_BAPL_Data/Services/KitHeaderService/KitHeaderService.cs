using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.KitHeaderRepo;
using DMS_BAPL_Data.Services.ColorMasterService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.KitHeaderService
{
    public partial class KitHeaderService : IKitHeaderService
    {
        private readonly IKitHeaderRepo _kitHeaderRepo;

        public KitHeaderService(IKitHeaderRepo kitHeaderRepo)
        {
            _kitHeaderRepo = kitHeaderRepo;
        }
        Task<PagedResponse<KitHeader>> IKitHeaderService.GetKitByPagedAsync(string? searchTerms, int pageIndex, int pageSize) => _kitHeaderRepo.GetKitByPagedAsync(searchTerms, pageIndex, pageSize);
        Task<int> IKitHeaderService.InsertKitHeader(KitHeader kitHeader) => _kitHeaderRepo.InsertKitHeader(kitHeader);
        Task<KitHeader?> IKitHeaderService.GetKitById(int id) => _kitHeaderRepo.GetKitById(id);
        Task<int> IKitHeaderService.UpdateKitHeader(KitHeader kitHeader) => _kitHeaderRepo.UpdateKitHeader(kitHeader);

    }
}
