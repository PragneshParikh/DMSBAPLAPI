using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.HSNWiseTaxCodeRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.HSNWiseTaxcodeService
{
    public class HSNWiseTaxCodeService : IHSNWiseTaxcodeservice
    {
        private readonly IHSNWiseTaxcodeRepo _hsnWiseTaxcodeRepo;

        public HSNWiseTaxCodeService(IHSNWiseTaxcodeRepo hsnWiseTaxcodeRepo)
        {
            _hsnWiseTaxcodeRepo = hsnWiseTaxcodeRepo;
        }
        public async Task<List<HSNCodeList>> GetHsncodeList()
        {
            return await _hsnWiseTaxcodeRepo.GetHsncodeList();
        }
        public async Task<List<AggregateTaxCode>> GetAggregateTaxCodeList()
        {
            return await _hsnWiseTaxcodeRepo.GetAggregateTaxCodeList();
        }
        public async Task<HsnwiseTaxCodeViewModel> InsertHsnwiseTaxcodedetails(HsnwiseTaxCodeViewModel hsnwiseTaxCodeViewModel)
        {
            return await _hsnWiseTaxcodeRepo.InsertHsnwiseTaxcodedetails(hsnwiseTaxCodeViewModel);

        }
        public async Task<List<HsnwiseTaxCode>> GetHsnwiseTaxcodedetails(string? search)
        {
            return await _hsnWiseTaxcodeRepo.GetHsnwiseTaxcodedetails(search);
        }
    }
}
