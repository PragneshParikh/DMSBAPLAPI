using DMS_BAPL_Data.DBModels;
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
    }
}
