using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
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
        Task<List<InvoiceViewModel>> GetAllAsync();
       Task<InvoiceHeader?> GetByIdAsync(int id);
      ///  Task<int> CreateAsync(InvoiceHeader invoice);
        Task UpdateAsync(InvoiceHeader invoice);
        Task DeleteAsync(int id);
        Task<int> AddAsync(InvoiceHeader invoice);


    }
}
