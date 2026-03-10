using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.APITracking
{
    public interface IAPITrackingRepo
    {
        Task<List<Apitracking>> GetAPITracking();

        Task<List<Apitracking>> GetFilterRecords(DateTime fromDate, DateTime toDate, string endPoint, string searchCriteria, string status);
    }
}
