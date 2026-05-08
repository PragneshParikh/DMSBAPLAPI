using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ChassisService
{
    public interface IChassisService
    {
        Task<object> GetChassisDataAsync(string chassisNumber);
    }
}
