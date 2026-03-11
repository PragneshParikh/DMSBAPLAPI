using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.ViewModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DealerMasterService
{
    public interface IDealerMasterService
    {
        Task<DealerMaster> AddDealerAsync(DealerMasterDto dealer);
        Task<List<DealerMaster>> GetAllDealersAsync(string? search);
        Task<DealerMaster> GetDealerById(int id);
        Task<DealerMaster?> UpdateDealerAsync(int id, DealerMasterDto dealer);
        Task<byte[]> DownloadDealerExcel();
        Task<List<DealerDropdownViewModel>> GetDealerDropdown();

    }
}
