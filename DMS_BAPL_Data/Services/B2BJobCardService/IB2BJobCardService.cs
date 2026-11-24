using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS_BAPL_Data.JobCardScanner;

namespace DMS_BAPL_Data.Services.B2BJobCardService
{
    /// <summary>Who is calling - taken from the DMS JWT by the controller, never from the request body.</summary>
    public sealed class B2BCaller
    {
        public string UserId { get; init; } = "";
        /// <summary>DMS dealer code of a dealer user; empty for Admin / SuperAdmin</summary>
        public string? DealerCode { get; init; }
        /// <summary>Admin / SuperAdmin (GetUserInfoFromToken.GetUserGroup)</summary>
        public bool IsAdmin { get; init; }
    }

    /// <summary>A business-rule failure - returned to the UI as 400 with the message.</summary>
    public class B2BValidationException : Exception
    {
        public B2BValidationException(string message) : base(message) { }
    }

    public class B2BNotFoundException : Exception
    {
        public B2BNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// B2B Job Card (24.11.2026) on the JobCardScanner database. Business rules follow JobCardScanner
    /// JobCardsController.Create / Update and the Job Card Wizard (web + mobile).
    /// </summary>
    public interface IB2BJobCardService
    {
        Task<List<B2BDealerDto>> GetDealersAsync(CancellationToken ct);
        Task<B2BMastersDto> GetMastersAsync(B2BCaller caller, string? dealerCode, CancellationToken ct);
        Task<List<B2BIdNameDto>> GetJobTypesAsync(CancellationToken ct);
        Task<List<B2BIdNameDto>> GetServiceHeadsAsync(int jobTypeId, CancellationToken ct);
        Task<List<B2BPriorityDto>> GetPrioritiesAsync(int jobTypeId, int serviceHeadId, CancellationToken ct);
        Task<List<B2BComplaintDto>> GetComplaintsAsync(CancellationToken ct);
        Task<List<B2BWorkshopDto>> GetWorkshopsAsync(B2BCaller caller, string? dealerCode, CancellationToken ct);
        Task<B2BStaffListsDto> GetStaffAsync(B2BCaller caller, string? dealerCode, string? locationCode, CancellationToken ct);
        Task<B2BVehicleSuggestResultDto> SuggestVehiclesAsync(B2BCaller caller, string? term, string? dealerCode, CancellationToken ct);
        Task<List<B2BPhotoDto>> AddPhotosAsync(B2BCaller caller, Guid id, B2BSavePhotosRequest req, CancellationToken ct);
        Task<B2BVehicleCardDto> GetVehicleAsync(B2BCaller caller, string? value, string? dealerCode, CancellationToken ct);
        Task<B2BPagedDto<B2BJobCardListRowDto>> ListAsync(B2BCaller caller, B2BJobCardFilter filter, CancellationToken ct);
        Task<B2BJobCardDetailDto> GetAsync(B2BCaller caller, Guid id, CancellationToken ct);
        Task<B2BSaveResultDto> CreateAsync(B2BCaller caller, B2BJobCardCreateRequest req, CancellationToken ct);
        Task<B2BSaveResultDto> UpdateAsync(B2BCaller caller, Guid id, B2BJobCardUpdateRequest req, CancellationToken ct);
        /// <summary>Opens the JobCardScanner connection and counts a few tables - for the connectivity check endpoint.</summary>
        Task<object> PingAsync(CancellationToken ct);
    }
}
