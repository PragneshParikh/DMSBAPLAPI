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

namespace DMS_BAPL_Data.Repositories.ServiceTypeRepo
{
    public class ServiceTypeMasterRepo : IServiceTypeMasterRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IExcelService _excelService;

        public ServiceTypeMasterRepo(BapldmsvadContext bapldmsvadContext, IExcelService excelService)
        {
            _context = bapldmsvadContext;
            _excelService = excelService;
        }

        public async Task<int> InsertserviceType(ServiceTypeMasterViewModel serviceTypeMasterViewModel, string userId)
        {
            try
            {
                var serviceTypeMaster = new ServiceType
                {
                    ServiceHeadId = serviceTypeMasterViewModel.ServiceHeadId,
                    ServiceTypeName = serviceTypeMasterViewModel.ServiceTypeName,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                };
                _context.ServiceTypes.Add(serviceTypeMaster);
                await _context.SaveChangesAsync();
                return serviceTypeMasterViewModel.Id; // Return the ID of the newly inserted record

            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while inserting the serviceType: {ex.Message}");
                return -1; // Indicate failure
            }

        }

        public async Task<int> UpdateserviceTypeName(ServiceTypeMasterViewModel serviceTypeMasterViewModel, string userId)
        {
            try
            {
                var serviceTypeMaster = await _context.ServiceTypes.FindAsync(serviceTypeMasterViewModel.Id);
                if (serviceTypeMaster == null)
                {
                    return -1; // Group not found
                }
                serviceTypeMaster.ServiceHeadId = serviceTypeMasterViewModel.ServiceHeadId;
                serviceTypeMaster.ServiceTypeName = serviceTypeMasterViewModel.ServiceTypeName;
                serviceTypeMaster.UpdatedBy = userId;
                serviceTypeMaster.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return serviceTypeMaster.Id; // Return the ID of the updated record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while updating the serviceType name: {ex.Message}");
                return -1; // Indicate failure

            }
        }

        public async Task<int> DeleteserviceType(int serviceTypeId)
        {
            try
            {

                var serviceTypeMaster = await _context.ServiceTypes.FindAsync(serviceTypeId);
                if (serviceTypeMaster == null)
                {
                    return -1; // Group not found
                }

                _context.ServiceTypes.Remove(serviceTypeMaster);
                await _context.SaveChangesAsync();
                return serviceTypeId; // Return the ID of the deleted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while deleting the serviceType: {ex.Message}");
                return -1; // Indicate failure
            }
        }

        public async Task<List<ServiceTypeMasterViewModel>> GetAllServiceType()
        {
            try
            {
                var serviceHeads = await (
                    from st in _context.ServiceTypes
                    join sh in _context.ServiceHeads
                        on st.ServiceHeadId equals sh.Id into shGroup
                    from sh in shGroup.DefaultIfEmpty()

                    select new ServiceTypeMasterViewModel
                    {
                        Id = st.Id,
                        ServiceHeadId = st.ServiceHeadId,
                        ServiceHeadName = sh != null ? sh.ServiceHeadName : null,
                        ServiceTypeName = st.ServiceTypeName,
                        CreatedBy = st.CreatedBy,
                        CreatedDate = st.CreatedDate,
                        UpdatedBy = st.UpdatedBy,
                        UpdatedDate = st.UpdatedDate
                    }
                ).ToListAsync();

                return serviceHeads;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving service type: {ex.Message}");
                return new List<ServiceTypeMasterViewModel>();
            }
        }

        public async Task<byte[]> DownloadserviceTypeMasterExcel()
        {
            try
            {
                var data = await _context.ServiceTypes.ToListAsync();
                // Get all DTO properties for columns
                var properties = typeof(ServiceTypeMasterViewModel)
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
