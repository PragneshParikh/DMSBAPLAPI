using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.CityService
{
    public interface ICityService
    {
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<List<CityResponseViewModel>> GetAllCitiesWithStateAsync();
        Task<CityResponseViewModel> GetCityByIdAsync(int id);
        Task<City> CreateCityAsync(CityCreateEditViewModel model);
        Task<bool> UpdateCityAsync(int id, CityCreateEditViewModel model);
        Task<byte[]> downloadReceiptExcel();
    }
}
