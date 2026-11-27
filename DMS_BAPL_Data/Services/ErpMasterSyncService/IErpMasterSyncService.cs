using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DMS_BAPL_Data.Services.ErpMasterSyncService
{
    /// <summary>
    /// 26.11.2026 - Dealer Master / Location Master: UAT ERP report import + ERP sync (BaplConnection, baplfinal).
    /// apply = false: preview only (nothing is written); apply = true: the same run is saved.
    /// </summary>
    public interface IErpMasterSyncService
    {
        /// <summary>
        /// UAT_ERP_ManageDealerReport (exact headers). The report is the master list: its dealers are added / updated
        /// (Current Status decides Active / Inactive) and every other DMS dealer is made Inactive.
        /// </summary>
        Task<ErpSyncResult> ImportUatDealersAsync(IFormFile file, bool apply, string userId, CancellationToken ct);

        /// <summary>
        /// ERP C_CustomerMaster (CustomerCode like '%CUS%' and Active = 'Y') -> details of the ACTIVE DMS dealers
        /// (address, pin, contact person; name / mobile / email only when blank in DMS). Never changes Active.
        /// </summary>
        Task<ErpSyncResult> SyncDealersFromErpAsync(string? dealerCode, bool apply, string userId, CancellationToken ct);

        /// <summary>
        /// UAT_ERP_LocatonList (exact headers). Showroom -> Showroom (1), Store -> Workshop (2), Yard -> Yard (3);
        /// Status decides Active (Y) / Inactive (N). inactivateMissing: DMS locations not in the file become Inactive.
        /// </summary>
        Task<ErpSyncResult> ImportUatLocationsAsync(IFormFile file, bool apply, bool inactivateMissing, string userId, CancellationToken ct);

        /// <summary>
        /// ERP C_DMS_DealerConsigneeDetail -> details of the DMS locations with the same Location Code
        /// (address, city, state, pin, GSTIN, contacts, area, ERP id). Never changes Active. dealerCode = one dealer only.
        /// </summary>
        Task<ErpSyncResult> SyncLocationsFromErpAsync(string? dealerCode, bool apply, string userId, CancellationToken ct);
    }
}
