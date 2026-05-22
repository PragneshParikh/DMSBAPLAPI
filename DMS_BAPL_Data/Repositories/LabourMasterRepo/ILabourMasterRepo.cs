using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LabourMasterRepo
{
    public interface ILabourMasterRepo
    {
        Task<object> ImportLabourExcel(LabourMasterViewModel labourMasterViewModel, string? createdBy);
        Task<object> ImportPartWiseLabourExcel(LabourMasterViewModel labourMasterViewModel, string? createdBy);
        Task<object> UpdateLabourMaster(LabourMasteUpdateViewModel labourMasterUpdateViewModel, string? updatedBy);
        Task<object> UpdatePartWiseLabourMaster(PartWiseLabourMasterRateViewModel partWiseLabourMasterRateViewModel, string? updatedBy);
        Task<List<LabourMasteUpdateViewModel>> GetLabourMasterModelwiseList();
        Task<List<PartWiseLabourMasterRateViewModel>> GetLabourMasterPartwiseList();

    }
}
