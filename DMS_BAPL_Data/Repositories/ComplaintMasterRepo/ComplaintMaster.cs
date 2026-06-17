using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ComplaintMasterRepo
{
    public class ComplaintMaster : IComplaintMaster
    {
        private readonly BapldmsvadContext _context;
        private readonly IExcelService _excelService;

        public ComplaintMaster(BapldmsvadContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        public async Task<List<ComplaintMasterViewModel>> GetComplaintMasterList()
        {
            try
            {
              return await (
                       from c in _context.ComplaintMasters
                       join g in _context.GroupMasters
                           on c.GroupName equals g.Id
                       select new ComplaintMasterViewModel
                       {
                           Id = c.Id,
                           ComplaintName = c.ComplaintName,
                           ComplaintGroupName = g.GroupName,
                           GroupName = c.GroupName,
                           IsActive = c.Status
                       })
                       .OrderBy(x => x.ComplaintName)
                       .ToListAsync();
                 }
           catch (Exception)
           {
               throw;
            }
        }

        public async Task<ComplaintMasterViewModel?> GetComplaintMasterById(int id)
        {
            try
            {
                return await (
                    from c in _context.ComplaintMasters
                    join g in _context.GroupMasters
                        on c.GroupName equals g.Id
                    where c.Id == id
                    select new ComplaintMasterViewModel
                    {
                        Id = c.Id,
                        ComplaintName = c.ComplaintName,
                        ComplaintGroupName = g.GroupName,
                        GroupName= c.GroupName,
                        IsActive = c.Status
                    }
                ).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> InsertComplaintMaster(ComplaintMasterViewModel complaintMasterViewModel, string userId)
        {
            try
            {
                var AddComplaintentity = new DBModels.ComplaintMaster
                {
                    ComplaintName = complaintMasterViewModel.ComplaintName,
                    GroupName = complaintMasterViewModel.GroupName,
                    Status = complaintMasterViewModel.IsActive,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                _context.ComplaintMasters.Add(AddComplaintentity);
                await _context.SaveChangesAsync();

                return AddComplaintentity.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateComplaintMaster(ComplaintMasterViewModel complaintMasterViewModel, string userId)
        {
            try
            {
                var entity = await _context.ComplaintMasters
                    .FirstOrDefaultAsync(x => x.Id == complaintMasterViewModel.Id);

                if (entity == null)
                    return false;

                entity.ComplaintName = complaintMasterViewModel.ComplaintName;
                entity.GroupName = complaintMasterViewModel.GroupName;
                entity.Status = complaintMasterViewModel.IsActive;
                entity.UpdatedBy = userId;
                entity.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //soft Delete
        public async Task<bool> DeleteComplaintMaster(int id, string userName)
        {
            try
            {
                var entity = await _context.ComplaintMasters
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return false;

                entity.Status = false;
                entity.UpdatedBy = userName;
                entity.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Hard Delete 
        public async Task<bool> DeleteComplaintMaster(int id)
        {
            try
            {
                var entity = await _context.ComplaintMasters
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return false;

                _context.ComplaintMasters.Remove(entity);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> DownloadComplaintMasterExcel()
        {
            try
            {
                var data = await _context.ComplaintMasters.ToListAsync();
                // Get all DTO properties for columns
                var properties = typeof(ComplaintMasterViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);

                        if (entityProp != null)
                            dict[prop.Name] = entityProp.GetValue(d);
                        else
                            dict[prop.Name] = null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = StringConstants.DealerExcelSheetName,
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

    }
}
