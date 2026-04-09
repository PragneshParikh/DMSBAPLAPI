using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.KitDetailsRepo;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Vml.Office;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.KitDetailsService
{
    public partial class KitDetailsService : IKitDetailsService
    {
        private readonly IKitDetailsRepo _kitDetailsRepo;
        public KitDetailsService(IKitDetailsRepo kitDetailsRepo)
        {
            _kitDetailsRepo = kitDetailsRepo;
        }

        Task<IEnumerable<object>> IKitDetailsService.GetKitDetailsByHeaderId(int headerId) => _kitDetailsRepo.GetKitDetailsByHeaderId(headerId);
        Task<PagedResponse<object>> IKitDetailsService.GetKitDetailsByPaged(int pageIndex, int pageSize, int headerId) => _kitDetailsRepo.GetKitDetailsByPaged(pageIndex, pageSize, headerId);
        Task<int> IKitDetailsService.InsertKitDetails(List<KitDetailsViewModel> kitDetailsViewModels) => _kitDetailsRepo.InsertKitDetails(kitDetailsViewModels);
        public async Task<bool> UpdateKitDetails(List<KitDetailsViewModel> kitDetailsViewModels)
        {
            bool anyUpdated = false;

            foreach (var vm in kitDetailsViewModels)
            {
                var entity = new KitDetail
                {
                    Id = vm.Id,
                    KitHeaderId = vm.KitHeaderId,
                    ItemId = vm.ItemId,
                    Quantity = vm.Quantity,
                    CreatedBy = vm.CreatedBy,
                    CreatedDate = vm.CreatedDate,
                    UpdatedBy = vm.UpdatedBy,
                    UpdatedDate = vm.UpdatedDate
                };

                var updated = await _kitDetailsRepo.UpdateKitDetails(entity);
                if (updated)
                    anyUpdated = true;
            }

            return anyUpdated;
        }

    }
}
