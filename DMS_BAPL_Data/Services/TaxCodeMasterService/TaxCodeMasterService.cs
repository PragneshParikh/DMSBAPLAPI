using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.TaxCodeMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.TaxCodeMasterService
{
    public class TaxCodeMasterService : ITaxCodeMasterService
    {

        private readonly ITaxCodeMasterRepo _taxCodeMasterRepo;
        private readonly IExcelService _excelService;

        public TaxCodeMasterService(ITaxCodeMasterRepo taxCodeMasterRepo, IExcelService excelService)
        {
            _taxCodeMasterRepo = taxCodeMasterRepo;
            _excelService = excelService;
        }

        public async Task<IEnumerable<TaxCodeMaster>> GetAllTaxCodes()
        {
            IEnumerable<TaxCodeMaster> taxCodeMasterList = await _taxCodeMasterRepo.GetAllTaxCodes();
            return taxCodeMasterList;
        }

        public async Task<TaxCodeMaster?> GetTaxCodeById(int id)
        {
            TaxCodeMaster? taxCodeMaster = await _taxCodeMasterRepo.GetTaxCodeById(id);
            return taxCodeMaster;
        }

        public async Task<int> AddTaxCode(TaxCodeMasterViewModel taxCodeMasterViewModel)
        {
            int taxCodeMasterId = await _taxCodeMasterRepo.AddTaxCode(taxCodeMasterViewModel);
            return taxCodeMasterId;
        }

        public async Task<int> UpdateTaxCode(TaxCodeMasterViewModel taxCodeMasterViewModel)
        {
            int affectedRows = await _taxCodeMasterRepo.UpdateTaxCode(taxCodeMasterViewModel);
            return affectedRows;
        }
        public async Task<byte[]> DownloadTaxCodeExcel()
        {
            try
            {
                var data = await _taxCodeMasterRepo.GetAllTaxCodes();

                var columns = new List<string>
        {
            "Id",
            "TaxCode",
            "Description",
            "TaxRate",
            "EffectiveDate",
            "CreatedBy",
            "CreatedDate",
            "UpdatedBy",
            "UpdatedDate"
        };

                var rows = data.Select(taxCodeMaster =>
                {
                    var dictionary = new Dictionary<string, object>();

                    dictionary["Id"] = taxCodeMaster.Id;
                    dictionary["TaxCode"] = taxCodeMaster.TaxCode;
                    dictionary["Description"] = taxCodeMaster.Description;
                    dictionary["TaxRate"] = taxCodeMaster.TaxRate;
                    dictionary["EffectiveDate"] = taxCodeMaster.EffectiveDate?.ToString("yyyy-MM-dd");
                    dictionary["CreatedBy"] = taxCodeMaster.CreatedBy;
                    dictionary["CreatedDate"] = taxCodeMaster.CreatedDate.ToString("yyyy-MM-dd HH:mm");
                    dictionary["UpdatedBy"] = taxCodeMaster.UpdatedBy;
                    dictionary["UpdatedDate"] = taxCodeMaster.UpdatedDate?.ToString("yyyy-MM-dd HH:mm");

                    return dictionary;

                }).ToList();

                var excelExportViewModel = new ExcelExportViewModel
                {
                    SheetName = "TaxCodeMaster",
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(excelExportViewModel);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }
        }
    }
}
