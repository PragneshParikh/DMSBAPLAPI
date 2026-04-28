using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PerformaInvoiceService
{
    public interface IPerformaInvoiceService
    {
        Task<bool> GeneratePerformaInvoice(string vehicleSaleBillNo);
    }
}
