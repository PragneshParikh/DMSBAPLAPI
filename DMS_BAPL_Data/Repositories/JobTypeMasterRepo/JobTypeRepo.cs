using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.GroupMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.JobTypeMasterRepo
{
    public class JobTypeRepo : IJobTypeMasterRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IExcelService _excelService;

        public JobTypeRepo(BapldmsvadContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        public async Task<int> InsertJobType(JobTypeMasterViewModel jobtypeMasterViewModel, string userId)
        {
            try
            {
                var jobtypeMaster = new JobType
                {
                    JobTypeName = jobtypeMasterViewModel.JobTypeName,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                };
                _context.JobTypes.Add(jobtypeMaster);
                await _context.SaveChangesAsync();
                return jobtypeMaster.Id; // Return the ID of the newly inserted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while inserting the group: {ex.Message}");
                return -1; // Indicate failure

            }
        }

        public async Task<int> UpdateJobTypeName(JobTypeMasterViewModel jobtypeMasterViewModel, string userId)
        {
            try
            {
                var jobtypeMaster = await _context.JobTypes.FindAsync(jobtypeMasterViewModel.Id);
                if (jobtypeMaster == null)
                {
                    return -1; // Group not found
                }
                jobtypeMaster.JobTypeName = jobtypeMasterViewModel.JobTypeName;
                jobtypeMaster.UpdatedBy = userId;
                jobtypeMaster.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return jobtypeMaster.Id; // Return the ID of the updated record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while updating the group name: {ex.Message}");
                return -1; // Indicate failure

            }
        }

        public async Task<int> DeleteJobType(int jobTypeId)
        {
            try
            {
                var jobtypeMaster = await _context.JobTypes.FindAsync(jobTypeId);
                if (jobtypeMaster == null)
                {
                    return -1; // Group not found
                }
                _context.JobTypes.Remove(jobtypeMaster);
                await _context.SaveChangesAsync();
                return jobTypeId; // Return the ID of the deleted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while deleting the group: {ex.Message}");
                return -1; // Indicate failure
            }
        }

        public async Task<List<JobTypeMasterViewModel>> GetAllJobType()
        {
            try
            {
                var groups = await _context.JobTypes.ToListAsync();
                return groups.Select(jt => new JobTypeMasterViewModel
                {
                    Id = jt.Id,
                    JobTypeName = jt.JobTypeName,
                    CreatedBy = jt.CreatedBy,
                    CreatedDate = jt.CreatedDate,
                    UpdatedBy = jt.UpdatedBy,
                    UpdatedDate = jt.UpdatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while retrieving groups: {ex.Message}");
                return new List<JobTypeMasterViewModel>(); // Return an empty list on failure
            }
        }

        public async Task<byte[]> DownloadJobTypeMasterExcel()
        {
            try
            {
                var data = await _context.JobTypes.ToListAsync();
                // Get all DTO properties for columns
                var properties = typeof(JobTypeMasterViewModel)
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

