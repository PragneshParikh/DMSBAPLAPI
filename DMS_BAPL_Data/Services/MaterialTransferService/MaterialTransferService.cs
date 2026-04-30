using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.MaterialTransferRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.MaterialTransferService
{
    public partial class MaterialTransferService : IMaterialTransferService
    {
        private readonly IMaterialTransferRepo _materialTransferRepo;

        public MaterialTransferService(IMaterialTransferRepo materialTransferRepo)
        {
            _materialTransferRepo = materialTransferRepo;
        }

        async Task<string> IMaterialTransferService.GetIssueIdAsync() => await _materialTransferRepo.GetIssueIdAsync();
        async Task<IEnumerable<object>> IMaterialTransferService.GetMeterialByJobId(int jobId) => await _materialTransferRepo.GetMeterialByJobId(jobId);
        async Task<PagedResponse<object>> IMaterialTransferService.GetMaterialTransferDetailsByDealer(string dealerCode, int pageIndex, int pageSize) => await _materialTransferRepo.GetMaterialTransferDetailsByDealer(dealerCode, pageIndex, pageSize);
        async Task<int> IMaterialTransferService.InsertMaterials(List<MaterialTransferViewModel> materialTransferViewModels
            ) => await _materialTransferRepo.InsertMaterials(materialTransferViewModels);
        async Task<int> IMaterialTransferService.DeleteMaterials(List<int> ids) => await _materialTransferRepo.DeleteMaterials(ids);
        async Task<int> IMaterialTransferService.UpdateMaterialDetails(List<MaterialTransferViewModel> materialTransferViewModels) => await _materialTransferRepo.UpdateMaterialDetails(materialTransferViewModels);


    }
}
