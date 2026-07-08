using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LabourMasterRepo
{
    public class LabourMasterRepo : ILabourMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public LabourMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<object> ImportLabourExcel(LabourMasterViewModel labourMasterViewModel, string? createdBy)
        {
            try
            {
                if (labourMasterViewModel.File == null || labourMasterViewModel.File.Length == 0)
                {
                    return new
                    {
                        Success = false,
                        Message = "File not found",
                        TotalRecords = 0,
                        LabourMasterData = new List<object>()
                    };
                }
                ExcelPackage.License.SetNonCommercialPersonal("Admin");
                var labourList = new List<LabourMaster>();

                using (var stream = new MemoryStream())
                {
                    await labourMasterViewModel
                        .File
                        .CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        // Restrict multiple sheets
                        if (package.Workbook.Worksheets.Count > 1)
                        {
                            return new
                            {
                                Success = false,
                                Message =
                                "Multiple sheets are not allowed",
                                TotalRecords = 0,
                                LabourMasterData =
                                new List<object>()
                            };
                        }

                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            // Skip blank rows
                            if (string.IsNullOrWhiteSpace(
                                worksheet.Cells[row, 1].Text))
                            {
                                continue;
                            }
                            var labour = new LabourMaster();

                            labour.LabourCode = worksheet.Cells[row, 1].Text.Trim();

                            labour.LabourDescription = worksheet.Cells[row, 2].Text.Trim();

                            labour.ModelCode = worksheet.Cells[row, 3].Text.Trim();

                            labour.ModelCc = int.TryParse(worksheet.Cells[row, 4].Text, out int modelcc) ? modelcc : 0;

                            labour.CityTier = int.TryParse(worksheet.Cells[row, 5].Text, out int cityTier) ? cityTier : 0;

                            labour.LabourRate = decimal.TryParse(worksheet.Cells[row, 6].Text, out decimal labourRate) ? labourRate : 0;

                            labour.Sgst = decimal.TryParse(worksheet.Cells[row, 7].Text, out decimal sgst) ? sgst : 0;

                            labour.Cgst = decimal.TryParse(worksheet.Cells[row, 8].Text, out decimal cgst) ? cgst : 0;

                            labour.Igst = decimal.TryParse(worksheet.Cells[row, 9].Text, out decimal igst) ? igst : 0;

                            labour.Category = worksheet.Cells[row, 10].Text.Trim();

                            labour.Hsncode = worksheet.Cells[row, 11].Text.Trim();

                            // Extra frontend values
                            labour.EffectiveDate = labourMasterViewModel.effectiveDate;

                            labour.Oemmodelname = labourMasterViewModel.oemmodelname;

                            // Backend handled fields
                            labour.CreatedBy = createdBy;

                            labour.CreatedDate = DateTime.Now;

                            labour.IsLabourActive = true;

                            labourList.Add(labour);
                        }
                    }
                }
                // Bulk Insert
                await _context.LabourMasters.AddRangeAsync(labourList);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Success = true,
                    Message = StringConstants.LabourMasterExcelSheetName,

                    TotalRecords = labourList.Count,

                    LabourMasterData = labourList.Select(x => new
                    {
                        LabourCode = x.LabourCode,
                        LabourDescription = x.LabourDescription,
                        ModelCode = x.ModelCode,
                        ModelCc = x.ModelCc,
                        CityTier = x.CityTier,
                        LabourRate = x.LabourRate,
                        Sgst = x.Sgst,
                        Cgst = x.Cgst,
                        Igst = x.Igst,
                        Category = x.Category,
                        Hsncode = x.Hsncode,
                        EffectiveDate = x.EffectiveDate,
                        Oemmodelname = x.Oemmodelname,
                        IsLabourActive = x.IsLabourActive
                    }).ToList()
                };
                return response;
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = $"Error Occurred: {ex.Message}",
                    TotalRecords = 0,
                    LabourMasterData = new List<object>()
                };
            }
        }
        public async Task<object> ImportPartWiseLabourExcel(LabourMasterViewModel labourMasterViewModel, string? createdBy)
        {
            try
            {
                if (labourMasterViewModel.File == null ||
                    labourMasterViewModel.File.Length == 0)
                {
                    return new
                    {
                        Success = false,
                        Message = "File not found",
                        TotalRecords = 0,
                        Data = new List<object>()
                    };
                }

                ExcelPackage.License.SetNonCommercialPersonal("Admin");

                var labourList = new List<PartWiseLabourMaster>();

                using (var stream = new MemoryStream())
                {
                    await labourMasterViewModel.File.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        if (package.Workbook.Worksheets.Count > 1)
                        {
                            return new
                            {
                                Success = false,
                                Message = "Multiple sheets are not allowed",
                                TotalRecords = 0,
                                Data = new List<object>()
                            };
                        }

                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                            {
                                continue;
                            }

                            var labour = new PartWiseLabourMaster();
                            labour.LabourCode = worksheet.Cells[row, 1].Text.Trim();
                            labour.LabourName = worksheet.Cells[row, 2].Text.Trim();
                            labour.PartCode = worksheet.Cells[row, 3].Text.Trim();
                            labour.PartDescription = worksheet.Cells[row, 4].Text.Trim();
                            labour.ModelName = worksheet.Cells[row, 5].Text.Trim();
                            labour.CityTier = int.TryParse(worksheet.Cells[row, 6].Text, out int cityTier) ? cityTier : 0;
                            labour.LabourRate = decimal.TryParse(worksheet.Cells[row, 7].Text, out decimal labourRate) ? labourRate : 0;
                            labour.LabourHrs = decimal.TryParse(worksheet.Cells[row, 8].Text, out decimal labourHrs) ? labourHrs : 0;
                            labour.Cgst = decimal.TryParse(worksheet.Cells[row, 9].Text, out decimal cgst) ? cgst : 0;
                            labour.Sgst = decimal.TryParse(worksheet.Cells[row, 10].Text, out decimal sgst) ? sgst : 0;
                            labour.Igst = decimal.TryParse(worksheet.Cells[row, 11].Text, out decimal igst) ? igst : 0;
                            labour.JobType = int.TryParse(worksheet.Cells[row, 12].Text,out int JobType)?JobType:0;
                            labour.DealerCode = worksheet.Cells[row, 13].Text.Trim();
                            labour.Hsncode = worksheet.Cells[row, 14].Text.Trim();
                            labour.EffectiveDate = labourMasterViewModel.effectiveDate;
                            labour.CreatedBy = createdBy;
                            labour.CreatedDate = DateTime.Now;
                            labour.IsActive = true;
                            labourList.Add(labour);
                        }
                    }
                }

                await _context.PartWiseLabourMasters.AddRangeAsync(labourList);
                await _context.SaveChangesAsync();
                return new
                {
                    Success = true,
                    Message = StringConstants.LabourMasterExcelSheetName,
                    TotalRecords = labourList.Count,
                    LabourPartWiseData = labourList.Select(x => new
                    {
                        x.LabourCode,
                        x.LabourName,
                        x.PartCode,
                        x.PartDescription,
                        x.ModelName,
                        x.CityTier,
                        x.LabourRate,
                        x.LabourHrs,
                        x.Cgst,
                        x.Sgst,
                        x.Igst,
                        x.JobType,
                        x.DealerCode,
                        x.Hsncode,
                        x.IsActive
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    LabourPartWiseData = new List<object>()
                };
            }
        }
        public async Task<object> UpdateLabourMaster(LabourMasteUpdateViewModel labourMasteUpdateViewModel, string? updatedBy)
        {
            try
            {
                var data = await _context.LabourMasters
                    .FirstOrDefaultAsync(x => x.Id == labourMasteUpdateViewModel.Id);

                if (data == null)
                {
                    return new
                    {
                        Success = false,
                        Message = "Record not found"
                    };
                }

                // ============================
                // Update Only Required Fields
                // ============================

                data.LabourCode = labourMasteUpdateViewModel.LabourCode;

                data.LabourDescription = labourMasteUpdateViewModel.LabourDescription;

                data.LabourRate = labourMasteUpdateViewModel.LabourRate;

                data.IsLabourActive = labourMasteUpdateViewModel.IsLabourRateActive;

                data.Cgst = labourMasteUpdateViewModel.Cgst;

                data.Sgst = labourMasteUpdateViewModel.Sgst;

                data.Igst = labourMasteUpdateViewModel.Igst;

                data.EffectiveDate = labourMasteUpdateViewModel.EffectiveDate;

                data.Oemmodelname = labourMasteUpdateViewModel.OemModelName;

                data.CityTier = labourMasteUpdateViewModel.CityTier;

                data.Jobtype = labourMasteUpdateViewModel.JobType;
                data.ServiceHead = labourMasteUpdateViewModel.ServiceHead;
                data.ServiceType = labourMasteUpdateViewModel.Servicetype;

                // ============================
                // Backend Handle Fields
                // ============================

                data.UpdatedDate = DateTime.Now;

                data.UpdateBy = updatedBy;

                await _context.SaveChangesAsync();

                return new
                {
                    Success = true,
                    Message = "Record Updated Successfully",
                    Data = new
                    {
                        data.Id,
                        data.LabourCode,
                        data.LabourDescription,
                        data.LabourRate,
                        data.Cgst,
                        data.Sgst,
                        data.Igst,
                        data.EffectiveDate,
                        data.Oemmodelname,
                        data.CityTier,
                        data.UpdatedDate,
                        data.UpdateBy
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<object> UpdatePartWiseLabourMaster(PartWiseLabourMasterRateViewModel partWiseLabourMasterRateViewModel, string? updatedBy)
        {
            try
            {
                var data = await _context.PartWiseLabourMasters
                    .FirstOrDefaultAsync(x => x.Id == partWiseLabourMasterRateViewModel.Id);

                if (data == null)
                {
                    return new
                    {
                        Success = false,
                        Message = "Record not found"
                    };
                }

                // ============================
                // Update Only Required Fields
                // ============================

                data.LabourCode = partWiseLabourMasterRateViewModel.LabourCode;
                data.LabourName = partWiseLabourMasterRateViewModel.LabourName;

                data.PartCode = partWiseLabourMasterRateViewModel.PartCode;
                data.PartDescription = partWiseLabourMasterRateViewModel.PartDescription;

                data.ModelName = partWiseLabourMasterRateViewModel.oemModelName;
                data.CityTier = partWiseLabourMasterRateViewModel.CityTier;

                data.LabourRate = partWiseLabourMasterRateViewModel.LabourRate;
                data.LabourHrs = partWiseLabourMasterRateViewModel.LabourHours;

                data.Cgst = partWiseLabourMasterRateViewModel.Cgst;
                data.Sgst = partWiseLabourMasterRateViewModel.Sgst;
                data.Igst = partWiseLabourMasterRateViewModel.Igst;

                data.JobType = partWiseLabourMasterRateViewModel.JobType;
                data.ServiceHead = partWiseLabourMasterRateViewModel.ServiceHead;
                data.ServiceType = partWiseLabourMasterRateViewModel.Servicetype;
                data.Hsncode = partWiseLabourMasterRateViewModel.HSNCode;
                data.EffectiveDate = partWiseLabourMasterRateViewModel.EffectiveDate;

                // ============================
                // Backend Handle Fields
                // ============================
                data.UpdatedBy = updatedBy;
                data.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return new
                {
                    Success = true,
                    Message = "Record Updated Successfully",
                    Data = new
                    {
                        data.Id,
                        data.LabourCode,
                        data.LabourName,
                        data.PartCode,
                        data.PartDescription,
                        data.ModelName,
                        data.CityTier,
                        data.LabourRate,
                        data.LabourHrs,
                        data.Cgst,
                        data.Sgst,
                        data.Igst,
                        data.JobType,
                        data.DealerCode,
                        data.Hsncode,
                        data.EffectiveDate,
                        data.UpdatedBy,
                        data.UpdatedDate
                    }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<List<LabourMasteUpdateViewModel>> GetLabourMasterModelwiseList()
        {
            var LaboorRateModelwiseListingdata = await(from labour in _context.LabourMasters
                join jobType in _context.JobTypes on labour.Jobtype equals jobType.Id into jobTypeJoin
                from jobType in jobTypeJoin.DefaultIfEmpty()
                join serviceHead in _context.ServiceHeads on labour.ServiceHead equals serviceHead.Id into serviceHeadJoin
                from serviceHead in serviceHeadJoin.DefaultIfEmpty()
                join serviceType in _context.ServiceTypes on labour.ServiceType equals serviceType.Id into serviceTypeJoin
                from serviceType in serviceTypeJoin.DefaultIfEmpty()

                select new LabourMasteUpdateViewModel
                {
                    Id = labour.Id,
                    LabourCode = labour.LabourCode,
                    LabourDescription = labour.LabourDescription,
                    LabourRate = labour.LabourRate,
                    HSNCode = labour.Hsncode,
                    Cgst = labour.Cgst,
                    Sgst = labour.Sgst,
                    Igst = labour.Igst,
                    EffectiveDate = labour.EffectiveDate,
                    CreatedDate = labour.CreatedDate,
                    OemModelName = labour.Oemmodelname,
                    CityTier = labour.CityTier,
                    JobTypeName = jobType != null ? jobType.JobTypeName : null,
                    ServiceHeadName = serviceHead != null ? serviceHead.ServiceHeadName : null,
                    ServicetypeName = serviceType != null ? serviceType.ServiceTypeName : null,
                    IsLabourRateActive = labour.IsLabourActive
                }).ToListAsync();

            return LaboorRateModelwiseListingdata;


        }

        public async Task<List<PartWiseLabourMasterRateViewModel>> GetLabourMasterPartwiseList()
        {
            var LaboorRatePartwiseListingdata = await( from partwiselabour in _context.PartWiseLabourMasters
                                                       join jobType in _context.JobTypes on partwiselabour.JobType equals jobType.Id into jobTypeJoin
                                                         from jobType in jobTypeJoin.DefaultIfEmpty()
                                                       join serviceHead in _context.ServiceHeads on partwiselabour.ServiceHead equals serviceHead.Id into serviceHeadJoin
                                                         from serviceHead in serviceHeadJoin.DefaultIfEmpty()
                                                    join serviceType in _context.ServiceTypes on partwiselabour.ServiceType equals serviceType.Id into serviceTypeJoin
                                                         from serviceType in serviceTypeJoin.DefaultIfEmpty()
                select new PartWiseLabourMasterRateViewModel
                {
                    Id = partwiselabour.Id,
                    LabourCode = partwiselabour.LabourCode,
                    LabourName = partwiselabour.LabourName,
                    PartCode = partwiselabour.PartCode,
                    PartDescription = partwiselabour.PartDescription,
                    oemModelName = partwiselabour.ModelName,
                    CityTier = partwiselabour.CityTier,
                    LabourRate = partwiselabour.LabourRate,
                    LabourHours = partwiselabour.LabourHrs,
                    Cgst = partwiselabour.Cgst,
                    Sgst = partwiselabour.Sgst,
                    Igst = partwiselabour.Igst,
                    JobTypeName = jobType != null ? jobType.JobTypeName : null,
                    ServiceHeadName = serviceHead != null ? serviceHead.ServiceHeadName : null,
                    ServicetypeName = serviceType != null ? serviceType.ServiceTypeName : null,
                    DealerCode = partwiselabour.DealerCode,
                    HSNCode = partwiselabour.Hsncode,
                    EffectiveDate = partwiselabour.EffectiveDate,
                    CreatedDate = partwiselabour.CreatedDate,
                    IsActive = partwiselabour.IsActive
                }).ToListAsync();
            return LaboorRatePartwiseListingdata;


        }

        public async Task<List<LabourRateDropDown>> GetLabourRateDropDowns(string oemmodelName, int customerLedgerId, string dealerCode)
        {
            var itemName = oemmodelName;
            var oemmodel = await _context.ItemMasters
                .Where(x => x.Itemname == itemName)
                .Select(x => x.Oemmodelname)
                .FirstOrDefaultAsync();
            var modelName = (oemmodel ?? "").Trim();

            var CustomerStateId = await _context.LedgerMasters
                .Where(cs => cs.Id == customerLedgerId)
                .Select(cs => cs.State)
                .FirstOrDefaultAsync();
            var custState = await _context.States
                .Where(custst => custst.StateId == CustomerStateId)
                .Select(custst  => custst.StateName)
                .FirstOrDefaultAsync();
            var DealerState = await _context.DealerMasters
                .Where(ds => ds.Dealercode == dealerCode)
                .Select(ds => ds.State)
                .FirstOrDefaultAsync();


            var city = await _context.LedgerMasters
                .Where(y => y.Id == customerLedgerId)
                .Select(y => y.City)
                .FirstOrDefaultAsync();
            
            var cityTier = await _context.Cities
                .Where(ct => ct.CityId == city)
                .Select(ct => ct.TierLevel)
                .FirstOrDefaultAsync();

            //Model Wise Labour
            var labourRateDropDowns = await _context.LabourMasters.Where(x => x.IsLabourActive == true && cityTier == x.CityTier)
                .Select(x => new LabourRateDropDown
                {
                    LabourId = x.Id,
                    LabourCode = x.LabourCode,
                    LabourDescription = x.LabourDescription,
                    LabourRate = x.LabourRate,
                    LabourHsnCode = x.Hsncode,
                    Cgst = x.Cgst,
                    Sgst = x.Sgst,
                    Igst = x.Igst,
                    OemModelName = x.Oemmodelname,
                    custState = custState,
                    DealerState = DealerState,


                }).ToListAsync();

            labourRateDropDowns = labourRateDropDowns
         .Where(x =>
             !string.IsNullOrWhiteSpace(x.OemModelName) &&
             (
                 modelName.Contains(x.OemModelName.Trim())
                 ||
                 x.OemModelName.Trim().Contains(modelName)
             )
         )
         .ToList();
            // Part Wise Labour
            var partWiseLabourRateDropDowns = await _context.PartWiseLabourMasters.Where(x => x.IsActive == true && cityTier == x.CityTier)
                .Select(x => new LabourRateDropDown
                {
                    LabourCode = x.LabourCode,
                    LabourDescription = x.LabourName,
                    LabourRate = x.LabourRate,
                    LabourHsnCode = x.Hsncode,
                    Cgst = x.Cgst,
                    Sgst = x.Sgst,
                    Igst = x.Igst,
                    OemModelName = x.ModelName,
                    custState = custState,
                    DealerState = DealerState,
                }).ToListAsync();
            
            partWiseLabourRateDropDowns = partWiseLabourRateDropDowns
        .Where(x =>
            !string.IsNullOrWhiteSpace(x.OemModelName) &&
            (
                modelName.Contains(x.OemModelName.Trim())
                ||
                x.OemModelName.Trim().Contains(modelName)
            )
        )
        .ToList();
            return labourRateDropDowns.Concat(partWiseLabourRateDropDowns).ToList();
        }
    }
}

