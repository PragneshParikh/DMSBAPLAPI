using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LOTInspectionRepo
{
    public interface ILotInspectionDetails
    {
        Task<bool> InsertDetailsByInvoiceAsync(InsertDetailsByInvoiceViewModel model, string userId);
        Task<List<LotInspectionHeaderDetailsViewModel>> GetAllDetailsByInvoiceAsync(string? invoiceNo);
        Task<int> InsertLotDetailsByInvoiceNo(string invoiceNo, int id, string userId);
    }
}
