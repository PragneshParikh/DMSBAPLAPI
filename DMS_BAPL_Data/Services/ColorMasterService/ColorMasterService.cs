using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ColorMasterService
{
    public class ColorMasterService : IColorMasterService
    {
        private readonly IColorMasterRepo _colorMasterRepo;
        private readonly IExcelService _excelService;

        public ColorMasterService(IColorMasterRepo colorMasterRepo, IExcelService excelService)
        {
            _colorMasterRepo = colorMasterRepo;
            _excelService = excelService;
        }

        Task<List<ColorMaster>> IColorMasterService.GetColorsAsync() => _colorMasterRepo.GetColorsAsync();

        Task<PagedResponse<ColorMaster>> IColorMasterService.getColorsByPagedAsync(string? searchTerms, int pageIndex, int pageSize) => _colorMasterRepo.getColorsByPagedAsync(searchTerms, pageIndex, pageSize);

        async Task<ColorMasterViewModel> IColorMasterService.CreateColorAsync(ColorMasterViewModel colorMasterViewModel) => await _colorMasterRepo.CreateColorAsync(colorMasterViewModel);

        public async Task<byte[]> downloadColorExcelAsync()
        {
            try
            {
                var data = await _colorMasterRepo.GetColorsAsync();

                // Get all DTO properties for columns
                var properties = typeof(ColorExcelViewModel)
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
                    SheetName = StringConstants.ColorExcelSheet,
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
