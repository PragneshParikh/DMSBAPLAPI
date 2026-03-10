using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LocationMasterRepo
{
    public interface ILocationMasterRepo
    {
        Task<List<LocationMasterViewModel>> GetAllLocationMaster();
        Task<LocationMasterViewModel> GetLocationMasterById(int id);
        Task<bool> AddLocationMaster(LocationMasterViewModel model);
        Task<bool> UpdateLocationMaster(LocationMasterViewModel model);
    }
}
