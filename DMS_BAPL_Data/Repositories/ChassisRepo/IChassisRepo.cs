using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ChassisRepo
{
    public interface IChassisRepo
    {
        Task<object> GetChassisDataAsync(string chassisNumber);

        Task<string> ImportChassisExcelAsync(IFormFile file);
    }
}
