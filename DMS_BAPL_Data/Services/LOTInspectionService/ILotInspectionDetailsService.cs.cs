using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LOTInspectionService
{
    public interface ILotInspectionDetailsService
    {
        Task<bool> InsertDetailsByInvoiceAsync(InsertDetailsByInvoiceViewModel model, string userId);
    }
}
