using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.RepoBillingService
{
    public interface IRepoBillingService
    {
        Task<JsonResult> GetRepoBillingByChassis(string chassis, string regNo);
    }
}
