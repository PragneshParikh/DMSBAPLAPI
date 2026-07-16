using DMS_BAPL_Data.Repositories.RepoBillingRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.RepoBillingService
{
    public partial class RepoBillingService : IRepoBillingService
    {
        private readonly IRepoBillingRepo _repoBillingRepo;

        public RepoBillingService(IRepoBillingRepo repoBillingRepo)
        {
            _repoBillingRepo = repoBillingRepo;
        }

        Task<VehicleInfoViewModel> IRepoBillingService.GetRepoBillingByChassis(string chassis, string regNo) => _repoBillingRepo.GetRepoBillingByChassis(chassis, regNo);
    }
}
