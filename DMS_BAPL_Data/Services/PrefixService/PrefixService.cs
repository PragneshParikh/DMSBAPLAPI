using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PrefixRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
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
        private readonly IExcelService _excelService;

        public PrefixService(IPrefixRepo prefixRep, IExcelService excelService)
        {
            _prefixRepo = prefixRep;
            _excelService = excelService;
        }

        Task<IEnumerable<NumberSequence>> IPrefixService.Get() => _prefixRepo.Get();
        Task<PagedResponse<NumberSequence>> IPrefixService.GetPrefixByPagedAsync(string? searchTerms, int pageIndex, int pageSize) => _prefixRepo.GetPrefixByPagedAsync(searchTerms, pageIndex, pageSize);
        Task<IEnumerable<NumberSequence>> IPrefixService.GetPrefixByDealerCode(String dealerCode) => _prefixRepo.GetPrefixByDealerCode(dealerCode);
        Task<NumberSequence> IPrefixService.GetPrefixByDealerCodeModuleName(string dealerCode, string moduleName) => _prefixRepo.GetPrefixByDealerCodeModuleName(dealerCode, moduleName);
        Task<int> IPrefixService.InsertPrefix(NumberSequenceViewModel numberSequenceViewModel) => _prefixRepo.InsertPrefix(numberSequenceViewModel);
        Task<int> IPrefixService.AddPrefixForDealers(NumberSequenceViewModel numberSequenceViewModel) => _prefixRepo.AddPrefixForDealers(numberSequenceViewModel);
        Task<int> IPrefixService.UpdateNextNumberByDealerByModule(string dealerCode, string moduleName) => _prefixRepo.UpdateNextNumberByDealerByModule(dealerCode, moduleName);
        public async Task<byte[]> DownloadExcel()
        {
            try
            {
                var data = await _prefixRepo.Get();

                // Get all DTO properties for columns
                var properties = typeof(PrefixExcelViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);

                        if (entityProp != null)
                            dict[prop.Name] = entityProp.GetValue(d);
                        else
                            dict[prop.Name] = null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = "Prefix",
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
