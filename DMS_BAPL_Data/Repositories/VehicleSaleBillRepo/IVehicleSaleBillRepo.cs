using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleSaleBillRepo
{
    public interface IVehicleSaleBillRepo
    {
        Task<int> CreateAsync(VehicleSaleBillHeader entity);
        Task<VehicleSaleBillHeader?> GetByIdAsync(int id);
        Task<List<VehicleSaleBillHeader>> GetAllAsync();
        Task UpdateAsync(VehicleSaleBillHeader entity);
        Task DeleteAsync(int id);

    }
}
