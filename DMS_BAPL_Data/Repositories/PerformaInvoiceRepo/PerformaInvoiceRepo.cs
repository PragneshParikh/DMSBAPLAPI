using DMS_BAPL_Data.DBModels;
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
    }
}
