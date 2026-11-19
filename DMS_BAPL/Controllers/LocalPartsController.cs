using System.Security.Claims;
using DMS_BAPL_Data.Services.LocalParts;
using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class LocalPartsController : ControllerBase
{
    private readonly ILocalPartsService _localPartsService;
    private readonly ILogger<LocalPartsController> _logger;

    public LocalPartsController(
        ILocalPartsService localPartsService,
        ILogger<LocalPartsController> logger)
    {
        _localPartsService = localPartsService;
        _logger = logger;
    }

    [HttpGet("locations")]
    [ProducesResponseType(typeof(IReadOnlyList<LocalPartsLocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkshopLocations(CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        try
        {
            var locations = await _localPartsService.GetWorkshopLocationsAsync(
                dealerCode,
                canAccessAllDealers,
                cancellationToken);
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch local-parts workshop locations.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch workshop locations.");
        }
    }

    [HttpGet("parts")]
    [ProducesResponseType(typeof(IReadOnlyList<LocalPartOptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParts(
        [FromQuery] int locationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");
        if (locationId <= 0)
            return BadRequest("Select a workshop location.");

        try
        {
            var parts = await _localPartsService.GetPartsAsync(
                locationId,
                dealerCode,
                canAccessAllDealers,
                cancellationToken);
            return Ok(parts);
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (LocalPartsNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch local-parts options for location {LocationId}.", locationId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch spare parts.");
        }
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<LocalPartsTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int? locationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");
        if (locationId.HasValue && locationId.Value <= 0)
            return BadRequest("Select a valid workshop location.");

        try
        {
            var transactions = await _localPartsService.GetTransactionsAsync(
                fromDate,
                toDate,
                locationId,
                dealerCode,
                canAccessAllDealers,
                cancellationToken);
            return Ok(transactions);
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (LocalPartsNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch local-parts transaction history.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch local-parts transactions.");
        }
    }

    [HttpGet("transactions/{id:int}")]
    [ProducesResponseType(typeof(LocalPartsTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        try
        {
            var transaction = await _localPartsService.GetTransactionAsync(
                id,
                dealerCode,
                canAccessAllDealers,
                cancellationToken);
            return Ok(transaction);
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (LocalPartsNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch local-parts transaction {TransactionId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch local-parts transaction.");
        }
    }

    [HttpPut("transactions/{id:int}")]
    [ProducesResponseType(typeof(LocalPartsTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTransaction(
        int id,
        [FromBody] UpdateLocalPartsTransactionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(updatedBy))
            return Unauthorized("User not authorized.");

        try
        {
            var transaction = await _localPartsService.UpdateTransactionAsync(
                id,
                request,
                dealerCode,
                canAccessAllDealers,
                updatedBy,
                cancellationToken);
            return Ok(transaction);
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (LocalPartsNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (LocalPartsConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update local-parts transaction {TransactionId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to update local-parts transaction.");
        }
    }

    [HttpDelete("transactions/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteTransaction(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        var deletedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(deletedBy))
            return Unauthorized("User not authorized.");

        try
        {
            await _localPartsService.DeleteTransactionAsync(
                id,
                dealerCode,
                canAccessAllDealers,
                deletedBy,
                cancellationToken);
            return NoContent();
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (LocalPartsNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (LocalPartsConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete local-parts transaction {TransactionId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to delete local-parts transaction.");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(LocalPartsSaveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Save(
        [FromBody] CreateLocalPartsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        var createdBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(createdBy))
            return Unauthorized("User not authorized.");

        try
        {
            var result = await _localPartsService.SaveAsync(
                request,
                dealerCode ?? string.Empty,
                canAccessAllDealers,
                createdBy,
                cancellationToken);
            return Ok(result);
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (LocalPartsNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (LocalPartsConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post local-parts receipt.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save local-parts receipt.");
        }
    }

    private bool TryGetCallerScope(out string? dealerCode, out bool canAccessAllDealers)
    {
        var roles = User.Claims
            .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "role")
            .Select(claim => claim.Value);
        canAccessAllDealers = roles.Any(role =>
            role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));
        var isDealer = roles.Any(role => role.Equals("Dealer", StringComparison.OrdinalIgnoreCase));
        dealerCode = canAccessAllDealers
            ? null
            : GetUserInfoFromToken.GetDealerCodeFromToken(HttpContext);
        return canAccessAllDealers || isDealer;
    }
}
