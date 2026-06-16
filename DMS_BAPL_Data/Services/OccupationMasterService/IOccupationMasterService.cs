using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.OccupationMasterService
{
    public interface IOccupationMasterService
    {
        Task<OccupationMaster> AddOccupationMasterAsync(OccupationViewModel occupationViewModel, string userId);
        Task<List<OccupationMaster>> GetOccupationMastersAsync();
        Task<OccupationMaster?> UpdateOccupationMasterAsync(int id, OccupationViewModel occupationViewModel, string userId);
        Task<PagedResponseBattery<OccupationMaster>> GetPaginatedOccupationMastersAsync(string? occupationName, int? page, int? pageSize);
        Task<List<OccupationMaster>> GetAllActiveOccupation();
    }
}
