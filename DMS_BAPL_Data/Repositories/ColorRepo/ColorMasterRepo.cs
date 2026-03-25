using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DMS_BAPL_Data.Services.ColorMasterService;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.Color
{
    public class ColorMasterRepo : IColorMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public ColorMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        Task<List<ColorMaster>> IColorMasterRepo.GetColors()
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

        Task<ColorMaster> IColorMasterRepo.GetColorByCode(string colorCode)
        {
            try
            {

                var color = _context.ColorMasters.Where(c => c.Colorcode == colorCode).FirstOrDefaultAsync();

                return (color);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PagedResponse<ColorMaster>> getColorsByPaged(string? searchTerms, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.ColorMasters.AsNoTracking();

                // 1. Apply Search Filter if searchTerms is provided
                if (!string.IsNullOrWhiteSpace(searchTerms))
                {
                    // Update these property names to match your actual database columns
                    query = query.Where(c => c.Colorname.Contains(searchTerms) ||
                                             c.Colorcode.Contains(searchTerms));
                }

                // 2. Get total count AFTER filtering, but BEFORE paging
                int totalRecords = await query.CountAsync();

                // 3. Get paged data
                var items = await query
                    .OrderBy(c => c.Rrgcoloridno)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Calculate starting serial number
                int startSrNo = (pageIndex * pageSize) + 1;

                var viewModelItems = items.Select((item, index) => new ColorMaster
                {
                    Id = startSrNo + index,
                    Rrgcoloridno = item.Rrgcoloridno,
                    Colorname = item.Colorname,
                    Colorcode = item.Colorcode,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = item.CreatedDate,
                    UpdatedBy = item.UpdatedBy,
                    UpdatedDate = item.UpdatedDate
                }).ToList();

                return new PagedResponse<ColorMaster>
                {
                    Data = viewModelItems,
                    TotalRecords = totalRecords
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        async Task<ColorMasterViewModel> IColorMasterRepo.CreateColor(ColorMasterViewModel colorMasterViewModel)
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
            catch (DbUpdateException ex)
            {
                //Access the SQL - specific exception
                if (ex.InnerException is SqlException sqlEx)
                {
                    int errorCode = sqlEx.Number;

                    if (errorCode == 2601 || errorCode == 2627)
                    {
                        // Handle duplicate logic here
                        return null;
                    }
                }
                throw;
            }
        }
    }
}
