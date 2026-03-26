using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ColorMasterService
{
    public interface IColorMasterService
    {
        Task<List<ColorMaster>> GetColorsAsync();
        Task<PagedResponse<ColorMaster>> getColorsByPagedAsync(string? searchTerms, int pageIndex, int pageSize);
        Task<ColorMasterViewModel> CreateColorAsync(ColorMasterViewModel colorMasterViewModel);
        Task<byte[]> downloadColorExcelAsync();

    }
}
