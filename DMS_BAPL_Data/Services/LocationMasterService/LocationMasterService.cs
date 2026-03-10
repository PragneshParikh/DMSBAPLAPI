using DMS_BAPL_Data.Repositories.LocationMasterRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LocationMasterService
{
    public class LocationMasterService : ILocationMasterService
    {
        private readonly ILocationMasterRepo _repo;
        public LocationMasterService(ILocationMasterRepo repo)
        {
            _repo = repo;
        }
        public async Task<List<LocationMasterViewModel>> GetAllLocationMaster()
        {
            return await _repo.GetAllLocationMaster();
        }
        public async Task<LocationMasterViewModel> GetLocationMasterById(int id)
        {
            return await _repo.GetLocationMasterById(id);
        }

        public async Task<bool> AddLocationMaster(LocationMasterViewModel model)
        {
            return await _repo.AddLocationMaster(model);
        }

        public async Task<bool> UpdateLocationMaster(LocationMasterViewModel model)
        {
            return await _repo.UpdateLocationMaster(model);
        }
       
    }
}
