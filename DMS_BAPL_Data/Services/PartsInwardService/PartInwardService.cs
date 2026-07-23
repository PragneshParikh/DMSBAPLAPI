using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PartInventoryRepo;
using DMS_BAPL_Data.Repositories.PartInwardRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PartsInwardService
{
    public partial class PartInwardService : IPartInwardService
    {
        private readonly IPartInwardRepo _partInwardRepo;
        public PartInwardService(IPartInwardRepo partInwardRepo)
        {
            _partInwardRepo = partInwardRepo;
        }

        Task<IEnumerable<PartsInward>> IPartInwardService.Get() => _partInwardRepo.Get();
        Task<IEnumerable<PartsInward>> IPartInwardService.GetPartInwardByDealerAsync(string dealerCode) => _partInwardRepo.GetPartInwardByDealerAsync(dealerCode);
        Task<bool> IPartInwardService.UpdateByInvoice(PartsInwardDetailsViewModel partsInwardDetailsViewModel) => _partInwardRepo.UpdateByInvoice(partsInwardDetailsViewModel);
        Task<object> IPartInwardService.PartsInward(PartsInwardViewModel partsInwardViewModel) => _partInwardRepo.PartsInward(partsInwardViewModel);
        Task<IEnumerable<PartsInward>> IPartInwardService.GetPendingPartInwardDetailByLocation(string locationCode) => _partInwardRepo.GetPendingPartInwardDetailByLocation(locationCode);
        Task<object> IPartInwardService.GetInwardPartDetailsByInvoiceNo(string invoiceNo) => _partInwardRepo.GetInwardPartDetailsByInvoiceNo(invoiceNo);
        Task<Object> IPartInwardService.GetPartsInwardDetailsByDealer(int pageIndex, int pageSize, DateTime fromDate, DateTime toDate, string? dealerCode) => _partInwardRepo.GetPartsInwardDetailsByDealer(pageIndex, pageSize, fromDate, toDate, dealerCode);

    }
}
