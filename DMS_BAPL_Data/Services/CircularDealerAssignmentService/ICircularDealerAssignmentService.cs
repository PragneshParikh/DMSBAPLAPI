using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.CircularDealerAssignmentService
{
    public interface ICircularDealerAssignmentService
    {
        Task<IEnumerable<object>> GetAssignmentByCircularId(int circularId);
        Task<bool> AssignDealersToCircular(int circularId, List<CircularDealerAssignmentViewModel> circularDealerAssignmentViewModel);
        Task<bool> DeleteDealersCircularPermission(int circularId, List<CircularDealerAssignmentViewModel> circularDealerAssignmentViewModel);
    }

}
