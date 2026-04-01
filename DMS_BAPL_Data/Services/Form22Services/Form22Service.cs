using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Form22MasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.Form22Services
{
    public class Form22Service : IForm22Service
    {
        private readonly IForm22MasterRepo _form22Repository;
        private readonly IExcelService _excelService;
        public Form22Service(IForm22MasterRepo form22Repository, IExcelService excelService)
        {
            _form22Repository = form22Repository;
            _excelService = excelService;
        }

        public async Task<Form22MasterViewModel> InsertForm22MasterAsync(Form22MasterViewModel form22MasterView)
        {
            return await _form22Repository.InsertForm22MasterAsync(form22MasterView);
        }

        public async Task<List<Form22Master>> GetForm22MastersAsync(string? search)
            {
                return await _form22Repository.GetForm22MastersAsync(search);
        }
        public  async Task<Form22Master> GetForm22MasterByIdAsync(int id)
        {
            return await _form22Repository.GetForm22MasterByIdAsync(id);
        }

        public async Task<Form22Master> UpdateForm22MasterAsync(int id, Form22MasterViewModel form22MasterViewModel)
        {
                return await _form22Repository.UpdateForm22MasterAsync(id, form22MasterViewModel);
        }

        public async Task<byte[]> DownloadForm22MasterExcel()
        {
            try
            {
                var data = await _form22Repository.GetForm22MastersAsync(null);

                // Get all DTO properties for columns
                var properties = typeof(Form22MasterViewModel)
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


    }
}
