using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.HSNCodeMaterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.HSNCodeMaterService
{
    public class HSNCodeMaterService : IHSNCodeMaterService
    {
        private readonly IHSNCodeMaterRepo _hSNCodeMaterRepo;
        private readonly IExcelService _excelService;
        public HSNCodeMaterService(IHSNCodeMaterRepo hSNCodeMaterRepo, IExcelService excelService)
        {
            _hSNCodeMaterRepo = hSNCodeMaterRepo;
            _excelService = excelService;
        }

        public async Task<List<HsncodeMaster>> GetAllHSNCodeListAsync(string? search)
        {
            return await _hSNCodeMaterRepo.GetAllHSNCodeListAsync(search);
        }
        public async Task<HsncodeMaster?> GetByIdAsync(int id)
        {
            try
            {
            return await _hSNCodeMaterRepo.GetByIdAsync(id);
            }
            catch
            {
                throw;
            }

        }
        public async Task<HsncodeMaster> AddAsync(HSNCodeMasterViewModel entity)
        {
            try
            {
            return await _hSNCodeMaterRepo.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> UpdateAsync(int id, HSNCodeMasterViewModel entity)
        {
            try
            {
            return await _hSNCodeMaterRepo.UpdateAsync(id, entity);
            }
            catch
            {
                throw;
            }

        }

        public async Task<byte[]> downloadHSNCodeExcel()
        {
            try
            {
                var data = await _hSNCodeMaterRepo.GetAllHSNCodeListAsync(null);

                // Get all DTO properties for columns
                var properties = typeof(HSNCodeMasterViewModel)
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
                    SheetName = StringConstants.HSNCodeExcelSheetName,
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
