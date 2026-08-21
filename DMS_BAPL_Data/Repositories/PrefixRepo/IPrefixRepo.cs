using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.PrefixRepo
{
    public interface IPrefixRepo
    {
        Task<IEnumerable<NumberSequence>> Get();
        Task<PagedResponse<NumberSequence>> GetPrefixByPagedAsync(string? searchTerm, int pageIndex, int pageSize);
        Task<IEnumerable<NumberSequence>> GetPrefixByDealerCode(string dealerCode);
        Task<NumberSequence> GetPrefixByDealerCodeModuleName(string dealerCode, string moduleName);
        Task<NumberSequence?> GetPrefixByDealerCodeModuleNameBillingType(string dealerCode, string moduleName, int? billingType);   // ADDED
        Task<NumberSequence?> GetById(int id);
        Task<int> InsertPrefix(NumberSequenceViewModel numberSequence);
        Task<int> UpdatePrefix(int id, NumberSequenceViewModel numberSequence);
        Task<bool> DeletePrefix(int id);
        Task<bool> CheckDuplicate(string dealerCode, string moduleName, string year, string prefix, int? billingType, int? excludeId);
        Task<int> AddPrefixForDealers(NumberSequenceViewModel numberSequenceViewModel);
        Task<int> UpdateNextNumberByDealerByModule(string dealerCode, string moduleName);
        Task<int> UpdateNextNumberByDealerByModuleBillingType(string dealerCode, string moduleName, int? billingType);   // ADDED
        Task<PagedResponse<NumberSequence>> GetPrefixByPagedByDealer(int pageIndex, int pageSize, string? searchTerms, string? dealerCode);
    }
}