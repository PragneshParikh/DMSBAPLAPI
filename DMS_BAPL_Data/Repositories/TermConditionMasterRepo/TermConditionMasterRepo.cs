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

namespace DMS_BAPL_Data.Repositories.TermConditionMasterRepo
{
    public class TermConditionMasterRepo : ITermConditionMasterRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IExcelService _excelService;

        public TermConditionMasterRepo(BapldmsvadContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        public async Task<int> AddTermCondition(TermConditionMasterViewModel conditionMasterViewModel,string userId)
        {
            try
            {
                var newTermCondition = new TermandConditionMaster
                {
                    TermCondition = conditionMasterViewModel.TermCondition,
                    ConditionModule = conditionMasterViewModel.ConditionModule,
                    ConditionEffectiveDate = conditionMasterViewModel.ConditionEffectiveDate,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow

                };
                _context.TermandConditionMasters.Add(newTermCondition);
                await _context.SaveChangesAsync();
                return newTermCondition.Id; // Return the ID of the newly inserted record
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while adding the term condition: {ex.Message}");
                return -1; // Indicate failure
            }
        }

        public async Task<int> UpdateTermCondition(TermConditionMasterViewModel conditionMasterViewModel,string userId)
        {
            try
            {
                var existingTermCondition = await _context.TermandConditionMasters.FindAsync(conditionMasterViewModel.ConditionId);
                if (existingTermCondition == null)
                {
                    return -1; // Term condition not found
                }
                // Update the properties of the existing term condition
                existingTermCondition.TermCondition = conditionMasterViewModel.TermCondition;
                existingTermCondition.ConditionModule = conditionMasterViewModel.ConditionModule;
                existingTermCondition.ConditionEffectiveDate = conditionMasterViewModel.ConditionEffectiveDate;
                existingTermCondition.UpdateBy = userId;
                existingTermCondition.UpdatedDate = DateTime.UtcNow;
                _context.TermandConditionMasters.Update(existingTermCondition);
                await _context.SaveChangesAsync();
                return 1; // Indicate success
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while updating the term condition: {ex.Message}");
                return -1; // Indicate failure
            }
        }

        public async Task<List<TermandConditionMaster>> GetAllTermCondition()
        {
            try
            {
                var termCondition = await _context.TermandConditionMasters.ToListAsync();
                return termCondition; // Return the found term condition (or null if not found)
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while retrieving the term condition: {ex.Message}");
                return null; // Indicate failure
            }
        }

        public async Task<int> DeleteTermCondition(int conditionId)
        {
            try
            {
                var termCondition = await _context.TermandConditionMasters.FindAsync(conditionId);
                if (termCondition == null)
                {
                    return -1; // Term condition not found
                }
                _context.TermandConditionMasters.Remove(termCondition);
                await _context.SaveChangesAsync();
                return 1; // Indicate success
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred while deleting the term condition: {ex.Message}");
                return -1; // Indicate failure
            }
        }

        public async Task<byte[]> DownloadTermConditionMasterExcel()
        {
            try
            {
                var data = await _context.TermandConditionMasters.ToListAsync();
                // Get all DTO properties for columns
                var properties = typeof(TermConditionMasterViewModel)
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
