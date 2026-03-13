using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ColorMasterService
{
    public class ColorMasterService : IColorMasterService
    {
        private readonly IColorMasterRepo _colorMasterRepo;

        public ColorMasterService(IColorMasterRepo colorMasterRepo)
        {
            _colorMasterRepo = colorMasterRepo;
        }

        Task<List<ColorMaster>> IColorMasterService.GetColors() => _colorMasterRepo.GetColors();

        Task<PagedResponse<ColorMaster>> IColorMasterService.getColorsByPaged(string? searchTerms, int pageIndex, int pageSize) => _colorMasterRepo.getColorsByPaged(searchTerms, pageIndex, pageSize);

        async Task<ColorMasterViewModel> IColorMasterService.CreateColor(ColorMasterViewModel colorMasterViewModel) => await _colorMasterRepo.CreateColor(colorMasterViewModel);
    }
}
