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
        Task<int> InsertPrefix(NumberSequenceViewModel numberSequence);
        Task<int> AddPrefixForDealers(NumberSequenceViewModel numberSequenceViewModel);
        Task<int> UpdateNextNumberByDealerByModule(string dealerCode, string moduleName);
    }
}
