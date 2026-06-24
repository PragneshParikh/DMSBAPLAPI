using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.FreeServiceClaimRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.FreeServiceClaimService
{
    public partial class FreeServiceClaimService : IFreeServiceClaimService
    {
        private readonly IFreeServiceClaimRepo _freeServiceClaimRepo;
        private readonly BapldmsvadContext _context;

        public FreeServiceClaimService(IFreeServiceClaimRepo freeServiceClaimRepo, BapldmsvadContext context)
        {
            _freeServiceClaimRepo = freeServiceClaimRepo;
            _context = context;
        }

        Task<IEnumerable<FreeServiceClaimHeader>> IFreeServiceClaimService.Get() => _freeServiceClaimRepo.Get();
        public async Task<bool> Insert(FreeServiceClaimViewModel freeServiceClaimViewModel)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var freeServiceClaim = new FreeServiceClaimHeader
                {
                    ClaimPrefix = freeServiceClaimViewModel.ClaimPrefix,
                    ClaimNo = freeServiceClaimViewModel.ClaimNo,
                    ClaimDate = freeServiceClaimViewModel.ClaimDate,
                    LocationCode = freeServiceClaimViewModel.LocationCode,
                    DealerCode = freeServiceClaimViewModel.DealerCode,
                    CreatedBy = freeServiceClaimViewModel.CreatedBy,
                    CreatedDate = freeServiceClaimViewModel.CreatedDate
                };

                _context.FreeServiceClaimHeaders.Add(freeServiceClaim);
                await _context.SaveChangesAsync();

                var details = freeServiceClaimViewModel.ItemDetails
                    .Select(x => new FreeServiceClaimDetail
                    {
                        HeaderClaimId = freeServiceClaim.Id,
                        JobId = x.jobCardId,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate
                    })
                    .ToList();

                _context.FreeServiceClaimDetails.AddRange(details);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        Task<IEnumerable<PendingApprovalJobCardViewModel>> IFreeServiceClaimService.GetPendingApprovalJobCard(string? dealerCode) => _freeServiceClaimRepo.GetPendingApprovalJobCard(dealerCode);
        Task<PagedResponse<FreeServiceClaimHeaderViewModel>> IFreeServiceClaimService.GetWarrantyClaimByDealerCode(string dealerCode, int pageSize, int pageIndex) => _freeServiceClaimRepo.GetWarrantyClaimByDealerCode(dealerCode, pageSize, pageIndex);
        Task<object?> IFreeServiceClaimService.GetClaimById(int Id) => _freeServiceClaimRepo.GetClaimById(Id);
        public async Task<bool> Update(FreeServiceClaimViewModel freeServiceClaimViewModel)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var details = freeServiceClaimViewModel.ItemDetails
                    .Select(x => new FreeServiceClaimDetail
                    {
                        Id = x.Id,
                        JobId = x.jobCardId,
                        HeaderClaimId = x.HeaderClaimId,
                        IsApproved = x.IsApproved,
                        ApprovedRejectBy = x.ApprovedRejectBy,
                        ApprovedRejectDate = x.ApprovedRejectDate,
                        RejectReason = x.RejectReason,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedDate = x.UpdatedDate,
                    })
                    .ToList();

                _context.FreeServiceClaimDetails.UpdateRange(details);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
