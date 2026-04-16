using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.CityRepo
{
    public interface ICityRepo
    {
        Task<IEnumerable<City>> GetCitiesAsync();

        Task<List<CityResponseViewModel>> GetAllCitiesWithStateAsync();
        Task<CityResponseViewModel> GetCityByIdAsync(int id);
        Task<City> CreateCityAsync(CityCreateEditViewModel model, string userId);
        Task<bool> UpdateCityAsync(int id, CityCreateEditViewModel model, string userId);
    }
}
