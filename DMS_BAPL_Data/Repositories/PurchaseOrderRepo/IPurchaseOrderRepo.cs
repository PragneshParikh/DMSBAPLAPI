using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.PurchaseOrderRepo
{
    public interface IPurchaseOrderRepo
    {
        Task AddPODetailAsync(PurchaseOrderDetail detail);
        
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        
        Task AddPOAsync(PurchaseOrder po);
        //Task AddPODetailAsync(PurchaseOrderDetail detail);
        Task AddTaxAsync(TaxDetail tax);

       
        Task UpdatePOAmountAsync(string poNumber, decimal amount);

        
        Task<ItemMaster> GetItemAsync(string itemCode);
        Task<HsncodeMaster> GetHSNByCodeAsync(string? hsnCode);
        Task<HsnwiseTaxCode> GetHSNTaxAsync(string hsnCode);
        Task<List<AggregateTaxCode>> GetAggregateTaxesAsync(string aTaxCode);
        Task<TaxCodeMaster> GetTaxMasterAsync(string taxCode);
        Task<HsnwiseTaxCode> GetHSNTaxWithFallbackAsync(string hsnCode, string preferredFlag, DateTime poDate);
        Task<PurchaseOrderResponseViewModel> GetPOByNumberAsync(string poNumber);
        Task<List<PurchaseOrderResponseViewModel>> GetPOListAsync();
        Task<decimal> GetSubsidyValue();



    }
}