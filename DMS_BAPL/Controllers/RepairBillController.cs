using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.JobCardScanner;
using DMS_BAPL_Data.Repositories.RepairBillRepo;
using DMS_BAPL_Data.Services.B2BJobCardService;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DMS_BAPL_Utils.ViewModels.RepairBillViewModel;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairBillController : ControllerBase
    {
        private readonly IRepairBillRepo _repairBillRepo;
        private readonly ILogger<RepairBillController> _logger;
        private readonly IPrefixService _prefixService;
        private readonly IB2BJobCardService _b2bJobCardService;

        public RepairBillController(IRepairBillRepo repairBillRepo, ILogger<RepairBillController> logger,
            IPrefixService prefixService, IB2BJobCardService b2bJobCardService)
        {
            _repairBillRepo = repairBillRepo;
            _logger = logger;
            _prefixService = prefixService;
            _b2bJobCardService = b2bJobCardService;
        }

        private B2BCaller GetB2BCaller()
        {
            var isAdmin = GetUserInfoFromToken.GetUserGroup(HttpContext);
            return new B2BCaller
            {
                UserId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext) ?? "",
                IsAdmin = isAdmin,
                DealerCode = isAdmin ? null : GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext)?.Trim()
            };
        }

        private async Task<(int? StatusCode, string? Message, B2BJobCardDetailDto? JobCard)> ValidateB2BSourceAsync(
            RepairBillHeaderVM header)
        {
            if (!header.B2BJobCardId.HasValue)
                return (null, null, null);

            if (header.JobId.HasValue)
                return (StatusCodes.Status400BadRequest, "A repair bill cannot reference both a B2B and DMS job card.", null);

            try
            {
                var jobCard = await _b2bJobCardService.GetAsync(
                    GetB2BCaller(), header.B2BJobCardId.Value, HttpContext.RequestAborted);

                if (string.IsNullOrWhiteSpace(jobCard.DealerCode))
                    return (StatusCodes.Status400BadRequest, "The B2B job card has no mapped DMS dealer code.", null);

                if (!string.IsNullOrWhiteSpace(header.DealerCode)
                    && !string.Equals(header.DealerCode.Trim(), jobCard.DealerCode.Trim(), StringComparison.OrdinalIgnoreCase))
                    return (StatusCodes.Status403Forbidden, "The selected B2B job card belongs to a different dealer.", null);

                header.DealerCode = jobCard.DealerCode;
                header.B2BJobCardNumber = jobCard.JobCardNumber;
                return (null, null, jobCard);
            }
            catch (B2BNotFoundException)
            {
                return (StatusCodes.Status404NotFound, "B2B job card not found.", null);
            }
            catch (B2BValidationException ex)
            {
                return (StatusCodes.Status400BadRequest, ex.Message, null);
            }
        }

        private async Task<(int? StatusCode, string? Message, B2BJobCardDetailDto? JobCard)> ValidateB2BSourceAsync(
            RepairBillUpdateHeaderVM header)
        {
            var insertHeader = new RepairBillHeaderVM
            {
                JobId = header.JobId,
                B2BJobCardId = header.B2BJobCardId,
                B2BJobCardNumber = header.B2BJobCardNumber,
                DealerCode = header.DealerCode
            };
            var validation = await ValidateB2BSourceAsync(insertHeader);
            header.DealerCode = insertHeader.DealerCode;
            header.B2BJobCardNumber = insertHeader.B2BJobCardNumber;
            return validation;
        }

        private static void PopulateB2BRepairBillHeader(RepairBillUpdateHeaderVM header, B2BJobCardDetailDto jobCard)
        {
            header.B2BJobCardNumber = jobCard.JobCardNumber;
            header.JobTypeName = jobCard.JobTypeName;
            header.PartyName = jobCard.Customer?.Name;
            header.MobileNumber = jobCard.Customer?.Mobile;
            header.PartyState = jobCard.Customer?.State;
            header.JobInDate = DateOnly.FromDateTime(jobCard.CreatedAt);
            header.RegisterNo = jobCard.Vehicle?.RegNo;
            header.ModelName = jobCard.Vehicle?.Model;
            header.VehicleKms = (int)jobCard.OdometerAtCheckIn;
            header.ChassisNo = jobCard.Vehicle?.ChassisNo;
            header.Technician = jobCard.TechnicianName;
        }

        private static void PopulateB2BPerforma(RepairBillPerformaVM performa, B2BJobCardDetailDto jobCard)
        {
            performa.B2BJobCardNumber = jobCard.JobCardNumber;
            performa.JobTypeName = jobCard.JobTypeName ?? "";
            performa.JobSourceName = jobCard.JobSourceName ?? "";
            performa.CustomerName = jobCard.Customer?.Name ?? "";
            performa.CustomerPhoneNo = jobCard.Customer?.Mobile ?? "";
            performa.CustomerAddress = jobCard.Customer?.Address ?? "";
            performa.CustomerState = jobCard.Customer?.State ?? "";
            performa.CustomerCity = jobCard.Customer?.City ?? "";
            performa.ChassisNo = jobCard.Vehicle?.ChassisNo ?? "";
            performa.RegisterationNo = jobCard.Vehicle?.RegNo ?? "";
            performa.MotorNo = jobCard.Vehicle?.MotorNo ?? "";
            performa.ModelName = jobCard.Vehicle?.Model ?? "";
            performa.Color = jobCard.Vehicle?.Color ?? "";
            performa.JobNo = int.TryParse(jobCard.JobCardNumber, out var jobNo) ? jobNo : null;
            performa.JobInDate = DateOnly.FromDateTime(jobCard.CreatedAt);
            performa.ODOReading = (int)jobCard.OdometerAtCheckIn;
            performa.VehicleSaleDate = jobCard.Vehicle?.PurchaseDate?.ToDateTime(TimeOnly.MinValue);
            performa.VehicleExpiryDate = jobCard.Vehicle?.InsuranceExpiry?.ToDateTime(TimeOnly.MinValue);
        }

        [HttpPost("GetAllRepairBillList")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRepairBillList([FromBody] RepairBillSearchVM? search)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                search ??= new RepairBillSearchVM();
                if (!GetUserInfoFromToken.GetUserGroup(HttpContext))
                {
                    var tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (string.IsNullOrWhiteSpace(tokenDealerCode))
                        return Forbid();
                    search.DealerCode = tokenDealerCode;
                }
                var result = await _repairBillRepo.GetAllRepairBillList(search);
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchJobCard");
                return StatusCode(500, "An error occurred while searching job cards.");
            }

        }

        [HttpPost("InsertRepairBill")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertRepairBill(RepairBillViewModel.RepairBillInsertVM model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                if (!GetUserInfoFromToken.GetUserGroup(HttpContext))
                {
                    var tokenDealerCode = GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
                    if (string.IsNullOrWhiteSpace(tokenDealerCode))
                        return Forbid();
                    model.RepairBillheader.DealerCode = tokenDealerCode;
                }

                var sourceValidation = await ValidateB2BSourceAsync(model.RepairBillheader);
                if (sourceValidation.StatusCode.HasValue)
                    return StatusCode(sourceValidation.StatusCode.Value, new { Success = false, Message = sourceValidation.Message });
                if (!model.RepairBillheader.B2BJobCardId.HasValue
                    && (!model.RepairBillheader.JobId.HasValue || model.RepairBillheader.JobId.Value <= 0))
                    return BadRequest(new { Success = false, Message = "A valid B2C or B2B job card is required." });

                var result = await _repairBillRepo.InsertRepairBill(model, userId);
                if (result > 0)
                {
                    await _prefixService.UpdateNextNumberByDealerByModule(model.RepairBillheader.DealerCode, "Repair_bill");
                    return Ok(new { Success = true, RepairBillId = result });
                }
                else
                {
                    _logger.LogError("Failed to insert repair bill.");
                    return StatusCode(500, new { Success = false, Message = "An error occurred while inserting the repair bill." });

                }

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message, Details = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while inserting the repair bill.");
                // return StatusCode(500, new { Success = false, Message = "An error occurred while inserting the repair bill.", Details = ex.Message });
                return StatusCode(500, new { Success = false, Message = "An error occurred while inserting the repair bill.", Details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet("GetRepairBillById/{id}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRepairBillById(int id)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _repairBillRepo.GetRepairBillById(id);

                if (result == null)
                {
                    return NotFound($"Repair bill with ID {id} not found.");
                }

                if (result.RepairBillheader.B2BJobCardId is Guid b2bJobCardId)
                {
                    var source = await ValidateB2BSourceAsync(new RepairBillHeaderVM
                    {
                        B2BJobCardId = b2bJobCardId,
                        DealerCode = result.RepairBillheader.DealerCode
                    });
                    if (source.StatusCode.HasValue)
                        return StatusCode(source.StatusCode.Value, new { Message = source.Message });
                    if (source.JobCard is not null)
                        PopulateB2BRepairBillHeader(result.RepairBillheader, source.JobCard);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetRepairBillById for id: {id}");
                return StatusCode(500, "An error occurred while retrieving the repair bill.");
            }

        }

        [HttpPut("UpdateRepairBill")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRepairBill(RepairBillViewModel.RepairBillUpdateVM model)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var sourceValidation = await ValidateB2BSourceAsync(model.RepairBillheader);
                if (sourceValidation.StatusCode.HasValue)
                    return StatusCode(sourceValidation.StatusCode.Value, new { Success = false, Message = sourceValidation.Message });

                var result = await _repairBillRepo.UpdateRepairBill(model, userId);
                if (result)
                {
                    return Ok(new { Success = true, Message = "Repair bill updated successfully." });
                }
                else
                {
                    _logger.LogError("Failed to update repair bill.");
                    return StatusCode(500, new { Success = false, Message = "An error occurred while updating the repair bill." });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the repair bill.");
                return StatusCode(500, new { Success = false, Message = "An error occurred while updating the repair bill.", Details = ex.Message });
            }
        }

        [HttpPost("GenerateRepairBillPerformaDetails/{DealerCode}/{RepairBillId}")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateRepairBillPerformaDetails(string DealerCode, int RepairBillId)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");
                var result = await _repairBillRepo.generateRepairBillPerformaDetails(DealerCode, RepairBillId);
                if (result?.B2BJobCardId is Guid b2bJobCardId)
                {
                    var source = await ValidateB2BSourceAsync(new RepairBillHeaderVM
                    {
                        B2BJobCardId = b2bJobCardId,
                        DealerCode = GetUserInfoFromToken.GetUserGroup(HttpContext)
                            ? null
                            : GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext)
                    });
                    if (source.StatusCode.HasValue)
                        return StatusCode(source.StatusCode.Value, new { Message = source.Message });
                    if (source.JobCard is not null)
                        PopulateB2BPerforma(result, source.JobCard);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating repair bill performa details.");
                return StatusCode(500, new { Success = false, Message = "An error occurred while generating repair bill performa details.", Details = ex.Message });
            }
        }

        [HttpDelete("DeleteRepairbill/{id}/{role}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRepairbill(int id, string role)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                // CHANGED: case-insensitive, matching every other SuperAdmin check
                // in this codebase (e.g. GetUserGroup(...).ToLower() == "superadmin")
                if (!string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized("Only Super Admin can delete");

                var result = await _repairBillRepo.DeleteRepairbill(id, role, userId);

                if (result > 0)
                    return Ok(new { message = "Deleted Successfully" });

                return NotFound("Repair Bill not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteRepairBill");
                return StatusCode(500, "An error occurred while deleting the repair bill.");
            }
        }
    }
}