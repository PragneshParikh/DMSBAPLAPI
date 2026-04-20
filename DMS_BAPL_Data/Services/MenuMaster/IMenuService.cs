using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.MenuMasterService
{
    public interface IMenuService
    {
        Task<List<MenuMasterViewModel>> GetMenuItems();
    }
}
