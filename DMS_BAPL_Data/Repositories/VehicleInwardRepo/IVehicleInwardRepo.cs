using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleDispatchRepo
{
    public interface IVehicleInwardRepo
    {
        Task<IEnumerable<VehicleInward>> Get();
        Task<IEnumerable<VehicleInward>> GetVehicleByStatus(string dealerCode, Boolean status);
        Task<bool> UpdateInvoiceStatus(string invoiceNo, string userId);
        Task<bool> InsertVehicleDispatchDetail(VehicleInwardViewModel vehicleInwardViewModel);
        Task<List<VehicleInward>> GetByChassisNosAsync(List<string> chassisNos);
    }
}
