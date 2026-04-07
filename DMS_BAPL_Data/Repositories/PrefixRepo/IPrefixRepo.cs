using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PrefixRepo
{
    public interface IPrefixRepo
    {
        Task<PagedResponse<NumberSequence>> GetPrefixByPagedAsync(string? searchTerm, int pageIndex, int pageSize);
        Task<IEnumerable<NumberSequence>> GetPrefixByDealerCode(string dealerCode);
        Task<int> InsertPrefix(NumberSequence numberSequence);
    }
}
