using DMS_BAPL_Data.DBModels;
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
        private readonly IPerformaInvoiceRepo _repository;
        public PerformaInvoiceService(IPerformaInvoiceRepo repository)
        {
            _repository = repository;
        }

        public async Task<bool> GeneratePerformaInvoice(string vehicleSaleBillNo)
        {
            try
            {

                var result = await _repository.GeneratePerformaInvoice(vehicleSaleBillNo);
                return result;
            }

            catch
            {
                throw;
            }

        }

        public async Task<InvoiceHeader?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(InvoiceHeader invoice)
        {
            //   Business Logic Example

            decimal total = 0;
            decimal tax = 0;

            foreach (var item in invoice.InvoiceDetails)
            {
                item.Amount = (item.Quantity ?? 0) * (item.Rate ?? 0);

                var itemTax = item.Amount * (item.TaxPercent ?? 0) / 100;

                total += item.Amount ?? 0;
                tax += itemTax ?? 0;
            }

            invoice.TotalAmount = total;
            invoice.TaxAmount = tax;
            invoice.NetAmount = total + tax - (invoice.DiscountAmount ?? 0);
            invoice.CreatedDate = DateTime.Now;

            return await _repository.AddAsync(invoice);
        }

        public async Task UpdateAsync(InvoiceHeader invoice)
        {
            await _repository.UpdateAsync(invoice);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
        public async Task<List<InvoiceHeader>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
