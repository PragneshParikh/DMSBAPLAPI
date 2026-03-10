using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.Color
{
    public class ColorRepo : IColorRepo
    {
        private readonly BapldmsvadContext _context;

        public ColorRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        Task<List<ColorMaster>> IColorRepo.GetColors()
        {
            try
            {

                var color = _context.ColorMasters.OrderBy(c => c.Colorname).ToList();

                return Task.FromResult(color);
            }
            catch (Exception)
            {
                throw;
            }
        }

        async Task<ColorMasterViewModel> IColorRepo.CreateColor(ColorMasterViewModel colorMasterViewModel)
        {
            try
            {

                var colorMaster = new ColorMaster
                {
                    Rrgcoloridno = colorMasterViewModel.rrgcoloridno,
                    Colorname = colorMasterViewModel.colorname,
                    Colorcode = colorMasterViewModel.colorcode,
                    CreatedBy = colorMasterViewModel.createdby,
                    CreatedDate = colorMasterViewModel.createddatetime,
                    UpdatedBy = colorMasterViewModel.updatedby,
                    UpdatedDate = colorMasterViewModel.updateddatetime
                };

                _context.ColorMasters.Add(colorMaster);
                await _context.SaveChangesAsync();

                // Return the inserted ID along with other details
                return new ColorMasterViewModel
                {
                    rrgcoloridno = colorMaster.Rrgcoloridno, // EF auto-generated ID
                    colorname = colorMaster.Colorname,
                    colorcode = colorMaster.Colorcode,
                    createdby = colorMaster.CreatedBy,
                    createddatetime = colorMaster.CreatedDate,
                    updatedby = colorMaster.UpdatedBy,
                    updateddatetime = colorMaster.UpdatedDate
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
