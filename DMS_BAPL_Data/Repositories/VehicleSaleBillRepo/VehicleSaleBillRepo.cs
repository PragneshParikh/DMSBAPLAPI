using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleSaleBillRepo
{
    public class VehicleSaleBillRepo :IVehicleSaleBillRepo
    {
        private readonly BapldmsvadContext _context;
        public VehicleSaleBillRepo(BapldmsvadContext context )
        {
            _context = context;
        }

        public async Task<int> CreateAsync(VehicleSaleBillHeader entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.VehicleSaleBillHeaders.Add(entity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return entity.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<VehicleSaleBillHeader?> GetByIdAsync(int id)
        {
            return await _context.VehicleSaleBillHeaders
                .Include(x => x.VehicleSaleBillDetails)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<VehicleSaleBillHeader>> GetAllAsync()
        {
            return await _context.VehicleSaleBillHeaders
                .Include(x => x.VehicleSaleBillDetails)
                .ToListAsync();
        }

        public async Task UpdateAsync(VehicleSaleBillHeader entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.VehicleSaleBillHeaders.Update(entity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var data = await _context.VehicleSaleBillHeaders.FindAsync(id);
            if (data != null)
            {
                _context.VehicleSaleBillHeaders.Remove(data);
                await _context.SaveChangesAsync();
            }
        }
    }
}
