using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.APITracking;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Services.ColorMasterService;
using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.APITrackingService
{
    public class APITrackingService : IAPITrackingService
    {
        private readonly IAPITrackingRepo _apiTrackingRepo;

        public APITrackingService(IAPITrackingRepo apiTrackingRepo)
        {
            _apiTrackingRepo = apiTrackingRepo;
        }

        Task<List<Apitracking>> IAPITrackingService.GetAPITracking()
        {
            return _apiTrackingRepo.GetAPITracking();
        }

        Task<List<Apitracking>> IAPITrackingService.GetFilterRecords(DateTime fromDate, DateTime toDate, string endPoint, string status)
        {
            return _apiTrackingRepo.GetFilterRecords(fromDate, toDate, endPoint, status);
        }

    }
}
