using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.CityRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.CityService
{
    public partial class CityService : ICityService
    {
        private readonly ICityRepo _cityRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExcelService _excelService;
        public CityService(ICityRepo cityRepo,IHttpContextAccessor httpContextAccessor, IExcelService excelService)
        {
            _cityRepo = cityRepo;
            _httpContextAccessor = httpContextAccessor;
            _excelService = excelService;
        }
        Task<IEnumerable<City>> ICityService.GetCitiesAsync() => _cityRepo.GetCitiesAsync();
        public async Task<List<CityResponseViewModel>> GetAllCitiesWithStateAsync()
        {
            try
            {
                return await _cityRepo.GetAllCitiesWithStateAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<CityResponseViewModel> GetCityByIdAsync(int id)
        {
            try
            {
                return await _cityRepo.GetCityByIdAsync(id);
            }
            catch
            {
                throw;
            }
        }
        public async Task<City> CreateCityAsync(CityCreateEditViewModel model)
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);
                return await _cityRepo.CreateCityAsync(model, userId);
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> UpdateCityAsync(int id, CityCreateEditViewModel model)
        {
            try
            {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);
            return await _cityRepo.UpdateCityAsync(id, model, userId);
            }
            catch
            {
                throw;
            }
        }

        public async Task<byte[]> downloadReceiptExcel()
        {
            try
            {
                var data = await _cityRepo.GetAllCitiesWithStateAsync();

                var properties = typeof(CityResponseViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);
                        dict[prop.Name] = entityProp != null
                            ? entityProp.GetValue(d)
                            : null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = StringConstants.CityMaster,
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); // optional logging
                throw;
            }
        }
    }
}
