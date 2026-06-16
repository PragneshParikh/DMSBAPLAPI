using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.OccupationMasterRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.OccupationMasterService
{
    
    public class OccupationMasterService:IOccupationMasterService
    {
        private readonly IOccupationMasterRepo _occupationRepo;
        public OccupationMasterService(IOccupationMasterRepo occupationRepo)
        {
          _occupationRepo = occupationRepo;  
        }

        public async Task<OccupationMaster> AddOccupationMasterAsync(OccupationViewModel occupationViewModel, string userId)
        {
            try
            {
                return await _occupationRepo.AddOccupationMasterAsync(occupationViewModel, userId);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<OccupationMaster>> GetOccupationMastersAsync()
        {
            try
            {
                return await _occupationRepo.GetOccupationMastersAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<OccupationMaster?> UpdateOccupationMasterAsync(int id, OccupationViewModel occupationViewModel, string userId)
        {
            try
            {
                return await _occupationRepo.UpdateOccupationMasterAsync(id, occupationViewModel, userId);
            }
            catch
            {
                throw;
            }
        }
        public async Task<PagedResponseBattery<OccupationMaster>> GetPaginatedOccupationMastersAsync(string? occupationName, int? page, int? pageSize)
        {
            try
            {
                return await _occupationRepo.GetPaginatedOccupationMastersAsync(occupationName, page, pageSize);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<OccupationMaster>> GetAllActiveOccupation()
        {
            try
            {
                return await _occupationRepo.GetAllActiveOccupation();
            }
            catch
            {
                throw;
            }
        }
    }
}
