using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.WarrantyInvoiceRepo
{
    public interface IWarrantyInvoiceRepo
    {
        Task<int> InsertWarrantyInvoice(WarrantyInvoiceViewModel model, string userId);
        Task<bool> UpdateWarrantyInvoice(WarrantyInvoiceViewModel model, string userId);
        Task<bool> DeleteWarrantyInvoice(int id, string userId);
        Task<WarrantyInvoiceViewModel?> GetWarrantyInvoiceById(int id);
        Task<WarrantyInvoiceSearchResultViewModel> SearchWarrantyInvoices(WarrantyInvoiceSearchViewModel filter);
        Task<(string BatchNo, string InvoicePrefix, string InvoiceNo)> GetNextInvoiceNumbers(string dealerCode);

        Task<List<string>> SearchInvoiceBatchNos(string dealerCode, string searchText);
        Task<List<string>> SearchInvoiceNos(string dealerCode, string searchText);
        Task<List<LocationDropdownItemViewModel>> GetDistinctInvoiceLocations(string dealerCode);

        Task<List<string>> SearchClaimInvoiceNos(string dealerCode, string searchText);

        Task<byte[]> GenerateWarrantyInvoicePartPdf(int invoiceId);
        Task<byte[]> GenerateWarrantyInvoiceLabourPdf(int invoiceId);
        Task<byte[]> GenerateWarrantyClaimTagPdf(int invoiceId);

        Task<string?> GetErpUniqueId(int invoiceId);
        Task SetErpUniqueId(int invoiceId, string uniqueId);

        Task<List<ErpWarrantyClaimLineViewModel>> BuildErpWarrantyClaimPayload(int invoiceId);

    }
}