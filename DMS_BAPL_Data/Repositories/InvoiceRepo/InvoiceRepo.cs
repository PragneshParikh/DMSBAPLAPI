using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.InvoiceRepo
{
    public class InvoiceRepo
    {
        private readonly BapldmsvadContext _context;
        public InvoiceRepo(BapldmsvadContext context)
        {
            _context = context;
        }


        public async Task<List<InvoiceHeader>> GetAllInvoices()
        {
            try
            {
                return await _context.InvoiceHeaders
                     .Include(i => i.InvoiceItems)
                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<InvoiceHeader> GetInvoiceById(int Id)
        {
            try
            {
                return await _context.InvoiceHeaders
                    .Include(i => i.InvoiceItems)
                    .Where(i => i.Id == Id)
                    .FirstOrDefaultAsync(); 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<int> CreateAsync(InvoiceHeader header)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.InvoiceHeaders.Add(header);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return header.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
