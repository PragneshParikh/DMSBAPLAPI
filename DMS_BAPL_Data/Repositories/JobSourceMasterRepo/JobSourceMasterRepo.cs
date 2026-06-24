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

namespace DMS_BAPL_Data.Repositories.JobSourceMasterRepo
{
    public class JobSourceMasterRepo : IJobSourceMasterRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IExcelService _excelService;

        public JobSourceMasterRepo(BapldmsvadContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        public async Task<int> InsertJobSource(JobSourceMasterViewModel jobSourceMasterViewModel, string userId)
        {
            try
            {
                var jobsourceMaster = new JobSource
                {
                    JobSourceName = jobSourceMasterViewModel.JobSourceName,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                };
                _context.JobSources.Add(jobsourceMaster);
                await _context.SaveChangesAsync();
                return jobsourceMaster.Id; // Return the ID of the newly inserted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while inserting the group: {ex.Message}");
                return -1; // Indicate failure

            }
        }

        public async Task<int> UpdateJobSourceName(JobSourceMasterViewModel jobSourceMasterViewModel, string userId)
        {
            try
            {
                var jobsourceMaster = await _context.JobSources.FindAsync(jobSourceMasterViewModel.Id);
                if (jobsourceMaster == null)
                {
                    return -1; // Group not found
                }
                jobsourceMaster.JobSourceName = jobSourceMasterViewModel.JobSourceName;
                jobsourceMaster.UpdatedBy = userId;
                jobsourceMaster.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return jobsourceMaster.Id; // Return the ID of the updated record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while updating the group name: {ex.Message}");
                return -1; // Indicate failure

            }
        }

        public async Task<int> DeleteJobSource(int jobSourceId)
        {
            try
            {
                var jobsourceMaster = await _context.JobSources.FindAsync(jobSourceId);
                if (jobsourceMaster == null)
                {
                    return -1; // Group not found
                }
                _context.JobSources.Remove(jobsourceMaster);
                await _context.SaveChangesAsync();
                return jobSourceId; // Return the ID of the deleted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while deleting the group: {ex.Message}");
                return -1; // Indicate failure
            }
        }

        public async Task<List<JobSourceMasterViewModel>> GetAllJobSource()
        {
            try
            {
                var groups = await _context.JobSources.ToListAsync();
                return groups.Select(jt => new JobSourceMasterViewModel
                {
                    Id = jt.Id,
                    JobSourceName = jt.JobSourceName,
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
                return new List<JobSourceMasterViewModel>(); // Return an empty list on failure
            }
        }

        public async Task<byte[]> DownloadjobsourceMasterExcel()
        {
            try
            {
                var data = await _context.JobSources.ToListAsync();
                // Get all DTO properties for columns
                var properties = typeof(JobSourceMasterViewModel)
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
