using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PerformaInvoiceRepo
{
    public class PerformaInvoiceRepo : IPerformaInvoiceRepo
    {
        private readonly BapldmsvadContext _context;
        public PerformaInvoiceRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task GetPerformaInvoiceCreationDetails(string saleBillNo)
        {
            try
            {
                var saleBill = await _context.VehicleSaleBillHeaders.Include(i => i.VehicleSaleBillDetails)
                    .Where(i => i.SaleBillNo == saleBillNo).FirstOrDefaultAsync();
                foreach (var item in saleBill.VehicleSaleBillDetails)
                {

                }




            }
            catch { }
        }

        public async Task<bool> GeneratePerformaInvoice(string vehicleSaleBillNo)
        {
            try
            {
                var saleBill= await _context.VehicleSaleBillHeaders.Where(i=>i.SaleBillNo == vehicleSaleBillNo).FirstOrDefaultAsync();
                saleBill.Erpstatus = "Alloted";
                var result= _context.SaveChangesAsync();
                return result.IsCompletedSuccessfully;


            }
            catch
            {
                throw;
            }
        }

        public async Task<List<InvoiceViewModel>> GetAllAsync()
        {
            return await _context.InvoiceHeaders.
                Include(x=>x.InvoiceDetails)
                .Select(x=>new InvoiceViewModel
                {
                    Id = x.Id,
                    InvoiceType = x.InvoiceType,
                    ServiceType = x.ServiceType,
                    DocumentNo = x.DocumentNo,
                    TotalAmount = x.TotalAmount,
                    TaxAmount = x.TaxAmount,
                    DiscountAmount = x.DiscountAmount,
                    NetAmount = x.NetAmount,
                    Status = x.Status,

                    InvoiceDetails =x.InvoiceDetails.Select(d=>new InvoiceDetailVM
                    {
                        Id = d.Id,
                        ItemId = d.ItemId,
                        Description = d.Description,
                        Quantity = d.Quantity,
                        Rate = d.Rate,
                        TaxPercent = d.TaxPercent,
                        Amount = d.Amount
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<InvoiceHeader?> GetByIdAsync(int id)
        {
            return await _context.InvoiceHeaders
                .Include(x => x.InvoiceDetails)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> AddAsync(InvoiceHeader invoice)
        {
            _context.InvoiceHeaders.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice.Id;
        }

        public async Task UpdateAsync(InvoiceHeader invoice)
        {
            _context.InvoiceHeaders.Update(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var invoice = await _context.InvoiceHeaders.FindAsync(id);
            if (invoice != null)
            {
                _context.InvoiceHeaders.Remove(invoice);
                await _context.SaveChangesAsync();
            }
        }

    }
}
