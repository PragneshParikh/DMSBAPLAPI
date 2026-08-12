using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PartInwardRepo
{
    public interface IPartInwardRepo
    {
        Task<IEnumerable<PartsInward>> Get();
        Task<IEnumerable<PartsInward>> GetPartInwardByDealerAsync(string dealerCode);
        Task<IEnumerable<PartsInward>> GetEbwPartInwardByDealerAsync(string dealerCode);
        Task<PartsInward?> GetLatestByPartNoAsync(string partNo);
        Task<bool> UpdateByInvoice(PartsInwardDetailsViewModel partsInwardDetailsViewModel);
        Task<object> PartsInward(PartsInwardViewModel partsInwardViewModel);
        Task<IEnumerable<PartsInward>> GetPendingPartInwardDetailByLocation(string locationCode);
        Task<object> GetInwardPartDetailsByInvoiceNo(string invoiceNo);
        //Task<PartsInward> CreateFromDispatchAsync(DmsPartDispatch dispatch, string userId);
        Task<PartsInward> CreateFromDispatchAsync(DmsPartDispatch dispatch, string userId, bool isAccepted);
        Task<Object> GetPartsInwardDetailsByDealer(int pageIndex, int pageSize, DateTime fromDate, DateTime toDate, string? dealerCode);
        Task<IEnumerable<object>> GetPartInwardExcelByDealer(DateTime fromDate, DateTime toDate, string? dealerCode);
    }
}
