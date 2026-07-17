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
        Task<IEnumerable<VehicleInwardD2DViewModel>> GetVehicleByStatus(string dealerCode, bool status);
        Task<bool> UpdateInvoiceStatus(string invoiceNo, string userId);
        Task<object> InsertVehicleDispatchDetail(VehicleInwardViewModel vehicleInwardViewModel);
        Task<List<VehicleInward>> GetByChassisNosAsync(List<string> chassisNos);
    }
}
