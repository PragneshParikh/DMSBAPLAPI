using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.Repositories.LabourMasterRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabourMasterController : ControllerBase
    {

        private readonly ILabourMasterRepo _labourMasterRepo;

        public LabourMasterController(ILabourMasterRepo labourMasterRepo)
        {
            _labourMasterRepo = labourMasterRepo;
        }
        [HttpPost("ImportModelWiseLabourMasterExcelApi")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportModelWiseLabourMasterExcelApi([FromForm] LabourMasterViewModel labourMasterViewModel)
        {
            try
            {
                string? createdBy = User.Identity?.Name ?? "Admin";

                var result = await _labourMasterRepo.ImportLabourExcel(labourMasterViewModel, createdBy);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    LabourMasterData = new List<object>()
                });
            }
        }

        [HttpPost("ImportPartWiseLabourMasterExcelApi")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportPartWiseLabourMasterExcelApi([FromForm] LabourMasterViewModel labourMasterViewModel)
        {
            try
            {
                string? createdBy = User.Identity?.Name ?? "Admin";
                var result = await _labourMasterRepo.ImportPartWiseLabourExcel(labourMasterViewModel, createdBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    labourPartWiseData = new List<object>()
                });
            }
        }

        [HttpPut("UpdateLabourMasterDataApi")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLabourMasterDataApi([FromBody] LabourMasteUpdateViewModel labourMasterUpdateViewModel)
        {
            try
            {
                string? updatedBy = User.Identity?.Name ?? "Admin";
                var result = await _labourMasterRepo.UpdateLabourMaster(labourMasterUpdateViewModel, updatedBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    labourPartWiseData = new List<object>()
                });
            }
        }

        [HttpPut("UpdatePartWiseLabourMasterDataApi")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePartWiseLabourMasterDataApi([FromBody] PartWiseLabourMasterRateViewModel partWiseLabourMasterRateViewModel)
        {
            try
            {
                string? updatedBy = User.Identity?.Name ?? "Admin";
                var result = await _labourMasterRepo.UpdatePartWiseLabourMaster(partWiseLabourMasterRateViewModel, updatedBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    labourPartWiseData = new List<object>()
                });
            }
        }

        [HttpGet("GetLabourMasterModelwiseListApi")]
        [ProducesResponseType(typeof(PagedResponse<List<LabourMasteUpdateViewModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLabourMasterModelwiseListApi(string? searchText)
        {
            try
            {
                var result = await _labourMasterRepo.GetLabourMasterModelwiseList(searchText);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    Data = new List<LabourMasteUpdateViewModel>()
                });
            }
        }
        [HttpGet("GetLabourMasterPartwiseListApi")]
        [ProducesResponseType(typeof(PagedResponse<List<PartWiseLabourMasterRateViewModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLabourMasterPartwiseListApi(string? searchText)
        {
            try
            {
                var result = await _labourMasterRepo.GetLabourMasterPartwiseList(searchText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    Data = new List<PartWiseLabourMasterRateViewModel>()
                });
            }
        }

        [HttpGet("GetLabourRateDropDown/{oemmodelName}/{customerLedgerId}/{dealerCode}")]
        [ProducesResponseType(typeof(PagedResponse<List<PartWiseLabourMasterRateViewModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLabourRateDropDown(string oemmodelName, int customerLedgerId, string dealerCode)
        {
            try
            {
                var result = await _labourMasterRepo.GetLabourRateDropDowns(oemmodelName, customerLedgerId, dealerCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message,
                    TotalRecords = 0,
                    Data = new List<LabourRateDropDown>()
                });
            }
        }


        [HttpGet("download-excel")]
        public async Task<IActionResult> DownloadLabourRateMasterExcel(
                [FromQuery] string? rateType = null,
                [FromQuery] string? oemModelName = null,
                [FromQuery] int? cityTier = null)
        {
            try
            {
                using var workbook = new XLWorkbook();
                bool isModelWise = string.Equals(rateType, "Modelwise Labour Rate", StringComparison.OrdinalIgnoreCase);

                if (isModelWise)
                {
                    var result = await _labourMasterRepo.GetLabourMasterModelwiseList(null);
                    var data = (result?? new List<LabourMasteUpdateViewModel>())
                        .Where(x => oemModelName == null || x.OemModelName == oemModelName)
                        .Where(x => cityTier == null || x.CityTier == cityTier)
                        .ToList();

                    var sheet = workbook.Worksheets.Add("Modelwise Labour Rate");
                    var headers = new[]
                    {
                        "Labour Code", "Labour Description", "OEM Model", "City Tier",
                        "Labour Rate", "CGST", "SGST", "IGST", "HSN Code",
                        "Job Type", "Job Type Name", "Service Head", "Service Head Name",
                        "Service Type", "Service Type Name", "Is Active",
                        "Effective Date", "Created Date", "Updated By", "Updated Date"
                    };
                    for (int i = 0; i < headers.Length; i++)
                        sheet.Cell(1, i + 1).Value = headers[i];
                    sheet.Row(1).Style.Font.Bold = true;

                    int row = 2;
                    foreach (var item in data)
                    {
                        sheet.Cell(row, 1).Value = item.LabourCode;
                        sheet.Cell(row, 2).Value = item.LabourDescription;
                        sheet.Cell(row, 3).Value = item.OemModelName;
                        sheet.Cell(row, 4).Value = item.CityTier;
                        sheet.Cell(row, 5).Value = item.LabourRate;
                        sheet.Cell(row, 6).Value = item.Cgst;
                        sheet.Cell(row, 7).Value = item.Sgst;
                        sheet.Cell(row, 8).Value = item.Igst;
                        sheet.Cell(row, 9).Value = item.HSNCode;
                        sheet.Cell(row, 10).Value = item.JobType;
                        sheet.Cell(row, 11).Value = item.JobTypeName;
                        sheet.Cell(row, 12).Value = item.ServiceHead;
                        sheet.Cell(row, 13).Value = item.ServiceHeadName;
                        sheet.Cell(row, 14).Value = item.Servicetype;
                        sheet.Cell(row, 15).Value = item.ServicetypeName;
                        sheet.Cell(row, 16).Value = item.IsLabourRateActive == true ? "Yes" : "No";
                        sheet.Cell(row, 17).Value = item.EffectiveDate?.ToString("dd-MM-yyyy");
                        sheet.Cell(row, 18).Value = item.CreatedDate.ToString("dd-MM-yyyy");
                        sheet.Cell(row, 19).Value = item.UpdatedBy;
                        sheet.Cell(row, 20).Value = item.UpdatedDate?.ToString("dd-MM-yyyy");
                        row++;
                    }
                    sheet.Columns().AdjustToContents();
                }
                else
                {
                    var data = await _labourMasterRepo.GetLabourRateMasterExportDataAsync(oemModelName, cityTier);
                    var sheet = workbook.Worksheets.Add("Partwise Labour Rate");
                    var headers = new[]
                    {
                        "Labour Code", "Labour Name", "Part Code", "Part Description",
                        "OEM Model", "City Tier", "Labour Rate", "Labour Hours",
                        "CGST", "SGST", "IGST", "Job Type", "Job Type Name",
                        "Dealer Code", "HSN Code", "Effective Date",
                        "Service Head", "Service Head Name",
                        "Service Type", "Service Type Name",
                        "Is Active", "Created Date", "Updated By", "Updated Date"
                    };
                    for (int i = 0; i < headers.Length; i++)
                        sheet.Cell(1, i + 1).Value = headers[i];
                    sheet.Row(1).Style.Font.Bold = true;

                    int row = 2;
                    foreach (var item in data)
                    {
                        sheet.Cell(row, 1).Value = item.LabourCode;
                        sheet.Cell(row, 2).Value = item.LabourName;
                        sheet.Cell(row, 3).Value = item.PartCode;
                        sheet.Cell(row, 4).Value = item.PartDescription;
                        sheet.Cell(row, 5).Value = item.oemModelName;
                        sheet.Cell(row, 6).Value = item.CityTier;
                        sheet.Cell(row, 7).Value = item.LabourRate;
                        sheet.Cell(row, 8).Value = item.LabourHours;
                        sheet.Cell(row, 9).Value = item.Cgst;
                        sheet.Cell(row, 10).Value = item.Sgst;
                        sheet.Cell(row, 11).Value = item.Igst;
                        sheet.Cell(row, 12).Value = item.JobType;
                        sheet.Cell(row, 13).Value = item.JobTypeName;
                        sheet.Cell(row, 14).Value = item.DealerCode;
                        sheet.Cell(row, 15).Value = item.HSNCode;
                        sheet.Cell(row, 16).Value = item.EffectiveDate?.ToString("dd-MM-yyyy");
                        sheet.Cell(row, 17).Value = item.ServiceHead;
                        sheet.Cell(row, 18).Value = item.ServiceHeadName;
                        sheet.Cell(row, 19).Value = item.Servicetype;
                        sheet.Cell(row, 20).Value = item.ServicetypeName;
                        sheet.Cell(row, 21).Value = item.IsActive == true ? "Yes" : "No";
                        sheet.Cell(row, 22).Value = item.CreatedDate?.ToString("dd-MM-yyyy");
                        sheet.Cell(row, 23).Value = item.UpdatedBy;
                        sheet.Cell(row, 24).Value = item.UpdatedDate.ToString("dd-MM-yyyy");
                        row++;
                    }
                    sheet.Columns().AdjustToContents();
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileName = isModelWise
                    ? $"ModelWiseLabourRateMaster_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    : $"PartWiseLabourRateMaster_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }
}

