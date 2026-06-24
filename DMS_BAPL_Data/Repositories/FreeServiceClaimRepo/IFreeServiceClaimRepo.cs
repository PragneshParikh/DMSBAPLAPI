using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.FreeServiceClaimRepo
{
    public interface IFreeServiceClaimRepo
    {
        Task<IEnumerable<FreeServiceClaimHeader>> Get();
        Task<int> Insert(FreeServiceClaimHeader freeServiceClaimHeader);
        Task<bool> InsertServiceClaimDetails(List<FreeServiceClaimDetail> freeServiceClaimDetail);
        Task<IEnumerable<PendingApprovalJobCardViewModel>> GetPendingApprovalJobCard(string? dealerCode);
        Task<PagedResponse<FreeServiceClaimHeaderViewModel>> GetWarrantyClaimByDealerCode(string dealerCode, int pageSize, int pageIndex);
        Task<object?> GetClaimById(int Id);
    }
}
