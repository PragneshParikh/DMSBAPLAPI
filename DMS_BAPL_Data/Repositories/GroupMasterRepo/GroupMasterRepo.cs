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

namespace DMS_BAPL_Data.Repositories.GroupMasterRepo
{
    public class GroupMasterRepo : IGroupMasterRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IExcelService _excelService;

        public GroupMasterRepo(BapldmsvadContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        public async Task<int> InsertGroup(GroupMasterViewModel groupMasterViewModel, string userId)
        {
            try
            {
                var groupMaster = new GroupMaster
                {
                    GroupName = groupMasterViewModel.GroupName,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                };
                _context.GroupMasters.Add(groupMaster);
                await _context.SaveChangesAsync();
                return groupMaster.Id; // Return the ID of the newly inserted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while inserting the group: {ex.Message}");
                return -1; // Indicate failure

            }
        }

        public async Task<int> UpdateGroupName(int id, string groupName, string userId)
        {
            try
            {
                var groupMaster = await _context.GroupMasters.FindAsync(id);
                if (groupMaster == null)
                {
                    return -1; // Group not found
                }
                groupMaster.GroupName = groupName;
                groupMaster.UpdateBy = userId;
                groupMaster.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return groupMaster.Id; // Return the ID of the updated record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while updating the group name: {ex.Message}");
                return -1; // Indicate failure

            }
        }

        public async Task<int> DeleteGroup(int groupId)
        {
            try
            {
                var groupMaster = await _context.GroupMasters.FindAsync(groupId);
                if (groupMaster == null)
                {
                    return -1; // Group not found
                }
                _context.GroupMasters.Remove(groupMaster);
                await _context.SaveChangesAsync();
                return groupId; // Return the ID of the deleted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while deleting the group: {ex.Message}");
                return -1; // Indicate failure
            }
        }

        public async Task<List<GroupMasterViewModel>> GetAllGroups()
        {
            try
            {
                var groups = await _context.GroupMasters.ToListAsync();
                return groups.Select(g => new GroupMasterViewModel
                {
                    Id = g.Id,
                    GroupName = g.GroupName,
                    CreatedBy = g.CreatedBy,
                    CreatedDate = g.CreatedDate,
                    UpdatedBy = g.UpdateBy,
                    UpdatedDate = g.UpdatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while retrieving groups: {ex.Message}");
                return new List<GroupMasterViewModel>(); // Return an empty list on failure
            }
        }

        public async Task<byte[]> DownloadGroupMasterExcel()
        {
            try
            {
                var data = await _context.GroupMasters.ToListAsync();
                // Get all DTO properties for columns
                var properties = typeof(GroupMasterViewModel)
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
