using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleDispatchRepo
{
    public interface IVehicleDispatchRepo
    {
        Task<IEnumerable<VehicleDispatch>> Get();
        Task<IEnumerable<VehicleDispatch>> GetVehicleByStatus(string dealerCode, Boolean status);
    }
}
