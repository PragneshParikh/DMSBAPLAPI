using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleInfoRepo
{
    public interface IVehicleInfoRepo
    {
        Task<VehicleInfoViewModel?> GetVehicleInfoByRegNoChassis(string? regNo, string? chassisNo);
        Task UpdateVehicleInfo(UpdateVehicleInfoViewModel model);
    }
}
