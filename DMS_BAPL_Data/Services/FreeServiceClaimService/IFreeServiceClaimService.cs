using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.FreeServiceClaimService
{
    public interface IFreeServiceClaimService
    {
        Task<IEnumerable<FreeServiceClaimHeader>> Get();
        Task<bool> Insert(FreeServiceClaimViewModel freeServiceClaimViewModel);
        Task<IEnumerable<PendingApprovalJobCardViewModel>> GetPendingApprovalJobCard(string? dealerCode);
        Task<PagedResponse<FreeServiceClaimHeaderViewModel>> GetWarrantyClaimByDealerCode(string dealerCode, int pageSize, int pageIndex, DateTime? fromDate, DateTime? toDate);
        Task<object?> GetClaimById(int Id);
        Task<bool> Update(FreeServiceClaimViewModel freeServiceClaimViewModel);

        // Task<IEnumerable<object>> GetPendingApprovalClaims(string? dealerCode);
        Task<IEnumerable<object>> GetPendingApprovalClaims(string? dealerCode, DateTime? fromDate, DateTime? toDate, bool includeProcessed);
        Task<bool> ApproveRejectClaimDetails(List<ApproveRejectDetailViewModel> items, string userId);
        Task<string> GenerateInvoice(int id, string userId);
    }
}
