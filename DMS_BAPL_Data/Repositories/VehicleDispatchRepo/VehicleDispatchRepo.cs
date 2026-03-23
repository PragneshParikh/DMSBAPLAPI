using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleDispatchRepo
{
    public class VehicleDispatchRepo : IVehicleDispatchRepo
    {
        private readonly BapldmsvadContext _context;
        public VehicleDispatchRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        async Task<IEnumerable<VehicleDispatch>> IVehicleDispatchRepo.Get()
        {
            try
            {
                return _context.VehicleDispatches.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        async Task<IEnumerable<VehicleDispatch>> IVehicleDispatchRepo.GetVehicleByStatus(bool status)
        {
            try
            {
                return _context.VehicleDispatches.Where(x => x.IsAccepted == status).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
