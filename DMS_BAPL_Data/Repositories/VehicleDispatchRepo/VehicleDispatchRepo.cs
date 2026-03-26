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

        async Task<IEnumerable<VehicleDispatch>> IVehicleDispatchRepo.GetVehicleByStatus(string dealerCode, bool status)
        {
            try
            {
                return await _context.VehicleDispatches
                    .Where(x => x.IsAccepted == status && x.DealerCode == dealerCode)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        async Task<bool> IVehicleDispatchRepo.UpdateInvoiceStatus(string invoiceNo, string userId)
        {
            try
            {
                var affectedRows = await _context.VehicleDispatches
                        .Where(x => x.InvoiceNo == invoiceNo)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.IsAccepted, true)
                        );

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async Task<bool> IVehicleDispatchRepo.InsertVehicleDispatchDetail(List<VehicleDispatch> vehicleDispatches)
        {
            try
            {
                _context.VehicleDispatches.AddRange(vehicleDispatches);
                _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
