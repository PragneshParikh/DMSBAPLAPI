using DMS_BAPL_Data.DBModels;
using Org.BouncyCastle.Bcpg.OpenPgp;
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
        Task<bool> UpdateInvoiceStatus(string invoiceNo, string userId);
        Task<bool> InsertVehicleDispatchDetail(List<VehicleDispatch> vehicleDispatches);
    }
}
