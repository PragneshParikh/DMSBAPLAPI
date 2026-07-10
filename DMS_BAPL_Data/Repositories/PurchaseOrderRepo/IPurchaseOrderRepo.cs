using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Data.Repositories.PurchaseOrderRepo
{
    public interface IPurchaseOrderRepo
    {
        Task AddPODetailAsync(PurchaseOrderDetail detail);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task AddPOAsync(PurchaseOrder po);
        Task AddTaxAsync(TaxDetail tax);
        Task UpdatePOHeaderAsync(PurchaseOrder po);
        Task UpdatePOAmountAsync(string poNumber, decimal amount);
        Task<ItemMaster> GetItemAsync(string itemCode);
        Task<HsncodeMaster> GetHSNByCodeAsync(string? hsnCode);
        Task<HsnwiseTaxCode> GetHSNTaxAsync(string hsnCode);
        Task<List<AggregateTaxCode>> GetAggregateTaxesAsync(string aTaxCode);
        Task<TaxCodeMaster> GetTaxMasterAsync(string taxCode);
        Task<HsnwiseTaxCode> GetHSNTaxWithFallbackAsync(string hsnCode, string preferredFlag, DateTime poDate);
        Task<PurchaseOrderResponseViewModel> GetPOByNumberAsync(string poNumber);
        Task<PagedResponse<PurchaseOrderResponseViewModel>> GetPOListAsync(string? dealerCode, string orderType, int pageIndex, int pageSize, PurchaseOrderSearchViewModel purchaseOrderSearchViewModel);
        Task<PartsPurchaseOrderResponseViewModel> GetPartsPOByNumberAsync(string poNumber);
        Task<List<PartsPurchaseOrderResponseViewModel>> GetPartsPOListAsync();
        Task<decimal> GetSubsidyValue();
        Task UpdateStatus(string PoNumber);
        Task<List<PurchaseOrderDetail>> GetPODetails(string poNumber);
        Task DeletePODetailAsync(PurchaseOrderDetail detail);
        Task DeleteTaxByItemAsync(string poNumber, string itemCode);
        Task DeleteTaxesByPOAsync(string poNumber);
        Task DeleteDetailsByPOAsync(string poNumber);
        Task<bool> UpdatePOStatusAsync(UpdatePOStatusViewModel updatePOStatusViewModel);
    }
}