using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ServiceHeadRepo
{
    public class ServiceHeadMasterRepo : IServiceHeadRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IExcelService _excelService;

        public ServiceHeadMasterRepo(BapldmsvadContext bapldmsvadContext, IExcelService excelService)
        {
            _context = bapldmsvadContext;
            _excelService = excelService;
        }

        public async Task<int> InsertServiceHead(ServiceHeadMasterViewModel serviceHeadMasterView, string userId)
        {
            try
            {
                var serviceHeadMaster = new ServiceHead
                {
                    JobTypeId = serviceHeadMasterView.JobtypeId,
                    ServiceHeadName = serviceHeadMasterView.ServiceHeadName,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                };
                _context.ServiceHeads.Add(serviceHeadMaster);
                await _context.SaveChangesAsync();
                return serviceHeadMasterView.Id; // Return the ID of the newly inserted record

            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while inserting the serviceHead: {ex.Message}");
                return -1; // Indicate failure
            }
           
        }

        public async Task<int> UpdateServiceHeadName(ServiceHeadMasterViewModel serviceHeadMasterView, string userId)
        {
            try
            {
                var serviceHeadMaster = await _context.ServiceHeads.FindAsync(serviceHeadMasterView.Id);
                if (serviceHeadMaster == null)
                {
                    return -1; // Group not found
                }
                serviceHeadMaster.JobTypeId = serviceHeadMasterView.JobtypeId;
                serviceHeadMaster.ServiceHeadName = serviceHeadMasterView.ServiceHeadName;
                serviceHeadMaster.UpdatedBy = userId;
                serviceHeadMaster.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return serviceHeadMaster.Id; // Return the ID of the updated record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while updating the serviceHead name: {ex.Message}");
                return -1; // Indicate failure

            }
        }

        public async Task<int> DeleteServiceHead(int serviceHeadId)
        {
            try
            {
               
                var serviceHeadMaster = await _context.ServiceHeads.FindAsync(serviceHeadId);
                if (serviceHeadMaster == null)
                {
                    return -1; // Group not found
                }
                
                _context.ServiceHeads.Remove(serviceHeadMaster);
                await _context.SaveChangesAsync();
                return serviceHeadId; // Return the ID of the deleted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while deleting the serviceHead: {ex.Message}");
                return -1; // Indicate failure
            }
        }

        public async Task<List<ServiceHeadMasterViewModel>> GetAllServiceHead()
        {
            try
            {
                var serviceHeads = await (
                    from sh in _context.ServiceHeads
                    join jt in _context.JobTypes
                        on sh.JobTypeId equals jt.Id into jtGroup
                    from jt in jtGroup.DefaultIfEmpty()

                    select new ServiceHeadMasterViewModel
                    {
                        Id = sh.Id,
                        JobtypeId = sh.JobTypeId,
                        JobTypeName = jt != null ? jt.JobTypeName : null,
                        ServiceHeadName = sh.ServiceHeadName,
                        CreatedBy = sh.CreatedBy,
                        CreatedDate = sh.CreatedDate,
                        UpdatedBy = sh.UpdatedBy,
                        UpdatedDate = sh.UpdatedDate
                    }
                ).ToListAsync();

                return serviceHeads;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving service heads: {ex.Message}");
                return new List<ServiceHeadMasterViewModel>();
            }
        }

        public async Task<byte[]> DownloadServiceHeadMasterExcel()
        {
            try
            {
                var data = await _context.ServiceHeads.ToListAsync();
                // Get all DTO properties for columns
                var properties = typeof(ServiceHeadMasterViewModel)
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
