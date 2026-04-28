using DMS_BAPL_Data.Repositories.PerformaInvoiceRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PerformaInvoiceService
{
    public class PerformaInvoiceService: IPerformaInvoiceService
    {
        private readonly IPerformaInvoiceRepo _performaRepo;
        public PerformaInvoiceService(IPerformaInvoiceRepo performaInvoiceRepo)
        {
            _performaRepo = performaInvoiceRepo;
        }

        public async Task<bool> GeneratePerformaInvoice(string vehicleSaleBillNo)
        {
            try
            {

                var result = await _performaRepo.GeneratePerformaInvoice(vehicleSaleBillNo);
                return result;
            }

            catch
            {
                throw;
            }

        }
    }
}
