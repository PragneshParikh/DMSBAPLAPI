using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ColorMasterService
{
    public interface IColorMasterService
    {
        Task<List<ColorMaster>> GetColors();

        Task<ColorMasterViewModel> CreateColor(ColorMasterViewModel colorMasterViewModel);

    }
}
