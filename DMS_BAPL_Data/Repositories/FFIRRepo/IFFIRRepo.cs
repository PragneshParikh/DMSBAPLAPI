using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.FFIRRepo
{
    public interface IFFIRRepo
    {
        Task<List<PartDropdownviewmodel>> GetPartDropdownlist();
        Task<List<FFirCompalintCodeListViewModel>> GetComplaintCodeList();

        Task<List<JobCardHistoryViewModel>> GetJobCardHistory(string chassisNo);
        Task<int> InsertFFIRAsync(FFIRViewModel model);

        Task<List<FFIRViewModelList>> GetFFIRDetailListing(string dealerCode, string? search);

        Task<FFIRViewModel> GetFFIRById(int id);

        Task<bool> UpdateFFIRAsync(int id, FFIRViewModel model);
    }
}
