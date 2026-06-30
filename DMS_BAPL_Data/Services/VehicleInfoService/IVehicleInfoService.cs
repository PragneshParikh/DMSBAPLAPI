using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleInfoService
{
    public interface IVehicleInfoService
    {
        Task<VehicleInfoViewModel?> GetVehicleInfoByRegNoChassis(string? regNo, string? chassisNo, string? dealerCode);
        Task UpdateVehicleInfo(UpdateVehicleInfoViewModel model);
    }
}
