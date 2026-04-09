using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.CityRepo;
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
        public CityService(ICityRepo cityRepo)
        {
            _cityRepo = cityRepo;
        }
        Task<IEnumerable<City>> ICityService.GetCitiesAsync() => _cityRepo.GetCitiesAsync();
    }
}
