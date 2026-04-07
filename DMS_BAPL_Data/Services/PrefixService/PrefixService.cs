using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PrefixRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PrefixService
{
    public partial class PrefixService : IPrefixService
    {
        private readonly IPrefixRepo _prefixRepo;

        public PrefixService(IPrefixRepo prefixRep)
        {
            _prefixRepo = prefixRep;
        }

        Task<PagedResponse<NumberSequence>> IPrefixService.GetPrefixByPagedAsync(string? searchTerms, int pageIndex, int pageSize) => _prefixRepo.GetPrefixByPagedAsync(searchTerms, pageIndex, pageSize);
        Task<IEnumerable<NumberSequence>> IPrefixService.GetPrefixByDealerCode(String dealerCode) => _prefixRepo.GetPrefixByDealerCode(dealerCode);
        Task<int> IPrefixService.InsertPrefix(NumberSequence numberSequence) => _prefixRepo.InsertPrefix(numberSequence);

    }
}
