using DMS_BAPL_Utils.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.WarrantyPackingRepo
{
    public interface IWarrantyPackingRepo
    {
        Task<List<PackingSlipLineViewModel>> GetPackableLines(int warrantyInvoiceHeaderId);
        Task<int> InsertWarrantyPackingSlip(WarrantyPackingSlipViewModel model, string userId);
        Task<WarrantyPackingSlipSearchResultViewModel> SearchWarrantyPackingSlips(WarrantyPackingSlipSearchViewModel filter);
        Task<WarrantyPackingSlipDetailsViewModel?> GetWarrantyPackingSlipById(int id);
        Task<bool> DeleteWarrantyPackingSlip(int id, string userId);
        Task<WarrantyPackingSlipLineSearchResultViewModel> SearchWarrantyPackingSlipLines(WarrantyPackingSlipLineSearchViewModel filter);
        Task<List<string>> SearchPackingSlipNos(string? dealerCode, string searchText);
        Task<List<string>> SearchPackingInvoiceNos(string? dealerCode, string searchText);
        Task<byte[]> GenerateWarrantyPackingSlipPdf(int packingSlipId);
    }
}