using DMS_BAPL_Data.DBModels;

namespace DMS_BAPL_Data.Services.PartDispWarrantyService
{
    public interface IPartDispWarrantyService
    {
        Task<List<ZDmsPartDispWarranty>> GetAllAsync();
        Task<ZDmsPartDispWarranty?> GetByIdAsync(int id);
        Task<ZDmsPartDispWarranty> CreateAsync(ZDmsPartDispWarranty item, string userId);
        Task<ZDmsPartDispWarranty?> UpdateAsync(ZDmsPartDispWarranty item, string userId);
        Task<List<string>> GetSerialNosByItemCodeAsync(string itemCode, int? excludeInvoiceId = null);
        Task<List<ZDmsPartDispWarranty>> GetByItemCodesAsync(List<string> itemCodes);
        Task<ZDmsPartDispWarranty?> GetBySerialNoAsync(string serialNo);
        Task<bool> DeleteAsync(int id);
        Task<int> ImportFromExcelAsync(Stream fileStream, string userId);
    }
}