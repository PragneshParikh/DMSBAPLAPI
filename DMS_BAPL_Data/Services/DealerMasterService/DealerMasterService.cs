using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DealerMasterService
{
    public class DealerMasterService : IDealerMasterService
    {
        private readonly IDealerMasterRepo _dealerMasterRepo;
        private readonly IExcelService _excelService;

        public DealerMasterService(IDealerMasterRepo dealerMasterRepo, IExcelService excelService)
        {
            _dealerMasterRepo = dealerMasterRepo;
            _excelService = excelService;

        }


        public async Task<DealerMaster> AddDealerAsync(DealerMasterViewModel dealer)
        {
            return await _dealerMasterRepo.AddDealerAsync(dealer);
        }

        public async Task<List<DealerMaster>> GetAllDealersAsync(string? search)
        {
            return await _dealerMasterRepo.GetAllDealersAsync(search);
        }

        public async Task<DealerMaster> GetDealerById(int id)
        {
            return await _dealerMasterRepo.GetDealerById(id);
        }

        public async Task<DealerMaster?> UpdateDealerAsync(int id, DealerMasterViewModel dealer)
        {
            return await _dealerMasterRepo.UpdateDealerAsync(id, dealer);
        }

      
        public async Task<byte[]> DownloadDealerExcel()
        {
            try
            {
                var data = await _dealerMasterRepo.GetAllDealersAsync(null);

                // Get all DTO properties for columns
                var properties = typeof(DealerMasterViewModel)
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
                    SheetName = StringConstants.DealerExcelSheetName,
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
        public async Task<List<DealerDropdownViewModel>> GetDealerDropdown()
        {
            return await _dealerMasterRepo.GetDealerDropdown();
        }
    }
}
