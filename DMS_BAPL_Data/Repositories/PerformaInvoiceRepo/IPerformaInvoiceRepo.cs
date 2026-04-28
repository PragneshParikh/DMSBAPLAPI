using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PerformaInvoiceRepo
{
    public interface IPerformaInvoiceRepo
    {
        Task<bool> GeneratePerformaInvoice(string vehicleSaleBillNo);


    }
}
