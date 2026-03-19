using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.HSNWiseTaxcodeService
{
    public interface IHSNWiseTaxcodeservice
    {
        Task<List<HSNCodeList>> GetHsncodeList();
        Task<List<AggregateTaxCode>> GetAggregateTaxCodeList();
        Task<HsnwiseTaxCodeViewModel> InsertHsnwiseTaxcodedetails(HsnwiseTaxCodeViewModel hsnwiseTaxCodeViewModel);
        Task<List<HsnwiseTaxCode>> GetHsnwiseTaxcodedetails(string? search);
    }
}
