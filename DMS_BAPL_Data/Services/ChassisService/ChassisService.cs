using DMS_BAPL_Data.Repositories.ChassisRepo;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ChassisService
{
    public partial class ChassisService : IChassisService
    {
        private readonly IChassisRepo _chassisRepo;
        public ChassisService(IChassisRepo chassisRepo)
        {
            _chassisRepo = chassisRepo;
        }
        Task<object> IChassisService.GetChassisDataAsync(string chassisNumber) => _chassisRepo.GetChassisDataAsync(chassisNumber);

        public async Task<string>ImportChassisExcelAsync(IFormFile file)
        {
            return await _chassisRepo
                .ImportChassisExcelAsync(file);
        }
    }
}
