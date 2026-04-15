using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.OEMModelWarrantyRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.OEMModelWarrantyService
{
    public class OEMModelWarrantyService : IOEMModelWarrantyService
    {
        private readonly IOEMModelWarrantyRepo _repo;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IExcelService _excelService;
        public OEMModelWarrantyService(IOEMModelWarrantyRepo repo, IHttpContextAccessor contextAccessor, IExcelService excelService)
        {
            _repo = repo;
            _contextAccessor = contextAccessor;
            _excelService = excelService;
        }

        public async Task<OemmodelWarranty> CreateAsync(OemModelWarrantyViewModel model)
        {
            try
            {
               var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
                var newOemModelWarranty = new OemmodelWarranty
                {
                    OemmodelId = model.OemmodelId,
                    EffectiveDate = model.EffectiveDate,
                    Odoreading = model.Odoreading,
                    DurationType = model.DurationType,
                    Duration = model.Duration,
                    IsB2b = model.IsB2b,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };
                return await _repo.CreateAsync(newOemModelWarranty);
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<OemmodelWarranty?> GetByIdAsync(int id)
        {
            try
            {
                var result = await _repo.GetByIdAsync(id);
                return result;
            }
            catch
            {
                throw;
            }

        }
        public async Task<OemModelWarrantyResponseViewModel?> GetDetailsByIdAsync(int id)
        {
            try
            {
                return await _repo.GetDetailsByIdAsync(id);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<OemModelWarrantyResponseViewModel>> GetAllAsync(string? searchTerm, DateOnly? effectiveDateFrom, DateOnly? effectiveDateTo)
        {
            try
            {
                var result = await _repo.GetAllAsync(searchTerm, effectiveDateFrom, effectiveDateTo);
                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<OemmodelWarranty> UpdateAsync(int id, OemModelWarrantyViewModel model)
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
                var existing = await _repo.GetByIdAsync(id);
                if (existing == null)
                    throw new Exception("Record not found");

                
                existing.OemmodelId = model.OemmodelId;
                existing.EffectiveDate = model.EffectiveDate;
                existing.Odoreading = model.Odoreading;
                existing.DurationType = model.DurationType;
                existing.Duration = model.Duration;
                existing.IsB2b = model.IsB2b;
                existing.UpdatedBy = userId;
                existing.UpdatedDate = DateTime.Now;

                var result =await _repo.UpdateAsync(existing);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async  Task<bool> DeleteAsync(int id)
        {
            try
            {
               return await _repo.DeleteAsync(id);
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> LastEffectiveDate(int oemmodelId)
        {
            try
            {
                return await _repo.LastEffectiveDate(oemmodelId);
            }
            catch
            {
                throw;
            }
        }


        public async Task<byte[]> downloadExcel()
        {
            try
            {
                var data = await _repo.GetAllAsync(null,null,null);

                var properties = typeof(OemmodelWarranty)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);
                        dict[prop.Name] = entityProp != null
                            ? entityProp.GetValue(d)
                            : null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = StringConstants.OEMModelWarranty,
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); // optional logging
                throw;
            }
        }
    }
}
