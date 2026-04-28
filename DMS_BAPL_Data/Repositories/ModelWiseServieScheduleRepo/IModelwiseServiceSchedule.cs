using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ModelWiseServieScheduleRepo
{
    public interface IModelwiseServiceSchedule
    {
        Task<List<ServiceHeadViewModel>> GetServiceHeadViews();
        Task<int> SavemodelwiseserviceScheduleAsync(List<ServiceScheduleVM> serviceScheduleVM);
        Task<List<ServiceSchedulelistVM>> GetModelwiseservicescheduleListAsync(int? oemModelId, DateTime? effectiveDate);
        Task<List<ServiceScheduleVM>> GetByModelwiseservicescheduleAsync(int oemModelId);

        Task<List<ModellistVMbasedOnOemMode>> GetOemModelbasedModelVarientListAsync(int oemModelId);
    }
}
