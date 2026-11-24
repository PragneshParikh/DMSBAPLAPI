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

    [HttpPost("d2d")]
    [ProducesResponseType(typeof(PartsD2DSaveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SavePartsD2D(
        [FromBody] CreatePartsD2DRequest request,
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
            var result = await _localPartsService.SaveD2DAsync(
                request,
                dealerCode,
                canAccessAllDealers,
                createdBy,
                cancellationToken);
            return Ok(result);
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (LocalPartsConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save parts D2D transfer.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save parts D2D transfer.");
        }
    }

    [HttpGet("d2d/next-bill-no")]
    [ProducesResponseType(typeof(PartsD2DBillNoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetNextPartsD2DBillNo(CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        try
        {
            var d2DBillNo = await _localPartsService.GetNextD2DBillNoAsync(cancellationToken);
            return Ok(new PartsD2DBillNoResponse(d2DBillNo));
        }
        catch (LocalPartsConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the next Parts D2D bill number.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to get the next D2D bill number.");
        }
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

    [HttpGet("d2d/locations")]
    [ProducesResponseType(typeof(IReadOnlyList<LocalPartsLocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetD2DWorkshopLocations(CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        try
        {
            var locations = await _localPartsService.GetWorkshopLocationsAsync(
                null,
                true,
                cancellationToken);
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch D2D destination workshop locations.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch workshop locations.");
        }
    }

    [HttpGet("d2d/transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<PartsD2DTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetD2DTransactions(
        [FromQuery] int fromLocationId,
        [FromQuery] int toLocationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        try
        {
            var transactions = await _localPartsService.GetD2DTransactionsAsync(
                fromLocationId,
                toLocationId,
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
            _logger.LogError(ex, "Failed to fetch D2D transactions for locations {FromLocationId} and {ToLocationId}.",
                fromLocationId,
                toLocationId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch D2D transactions.");
        }
    }

    [HttpGet("d2d/history")]
    [ProducesResponseType(typeof(IReadOnlyList<PartsD2DTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetD2DHistory(CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        try
        {
            var transactions = await _localPartsService.GetD2DHistoryAsync(
                dealerCode,
                canAccessAllDealers,
                cancellationToken);
            return Ok(transactions);
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Parts D2D history.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch Parts D2D history.");
        }
    }

    [HttpGet("d2d/notifications")]
    [ProducesResponseType(typeof(IReadOnlyList<PartsD2DNotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetD2DNotifications(CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out var canAccessAllDealers))
            return Forbid();
        if (canAccessAllDealers)
            return Ok(Array.Empty<PartsD2DNotificationResponse>());
        if (string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        try
        {
            var notifications = await _localPartsService.GetD2DNotificationsAsync(
                dealerCode,
                cancellationToken);
            return Ok(notifications);
        }
        catch (LocalPartsValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Parts D2D notifications.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch Parts D2D notifications.");
        }
    }

    [HttpGet("d2d/receipt/{d2dBillNo}")]
    [ProducesResponseType(typeof(PartsD2DReceiptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetD2DReceipt(
        string d2dBillNo,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out _))
            return Forbid();
        if (string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        try
        {
            var receipt = await _localPartsService.GetD2DReceiptAsync(
                d2dBillNo,
                dealerCode,
                cancellationToken);
            return Ok(receipt);
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
            _logger.LogError(ex, "Failed to fetch D2D receipt {D2DBillNo}.", d2dBillNo);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to fetch D2D receipt.");
        }
    }

    [HttpPost("d2d/receipt/{d2dBillNo}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptD2DReceipt(
        string d2dBillNo,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out _))
            return Forbid();
        if (string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        var receivedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(receivedBy))
            return Unauthorized("User not authorized.");

        try
        {
            await _localPartsService.AcceptD2DReceiptAsync(
                d2dBillNo,
                dealerCode,
                receivedBy,
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to accept D2D receipt {D2DBillNo}.", d2dBillNo);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to accept D2D receipt.");
        }
    }

    [HttpDelete("d2d/receipt/{d2dBillNo}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectD2DReceipt(
        string d2dBillNo,
        CancellationToken cancellationToken)
    {
        if (!TryGetCallerScope(out var dealerCode, out _))
            return Forbid();
        if (string.IsNullOrWhiteSpace(dealerCode))
            return Unauthorized("Dealer code not found.");

        var rejectedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(rejectedBy))
            return Unauthorized("User not authorized.");

        try
        {
            await _localPartsService.RejectD2DReceiptAsync(
                d2dBillNo,
                dealerCode,
                rejectedBy,
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
            _logger.LogError(ex, "Failed to reject D2D receipt {D2DBillNo}.", d2dBillNo);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to reject D2D receipt.");
        }
    }

    [HttpPut("d2d/transactions/{transactionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateD2DTransaction(
        int transactionId,
        [FromBody] UpdatePartsD2DTransactionRequest request,
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
            await _localPartsService.UpdateD2DTransactionAsync(
                transactionId,
                request,
                dealerCode,
                canAccessAllDealers,
                updatedBy,
                cancellationToken);
            return Ok(new { message = "D2D transfer line updated." });
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
            _logger.LogError(ex, "Failed to update D2D transaction {TransactionId}.", transactionId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to update D2D transaction.");
        }
    }

    [HttpDelete("d2d/transactions/{transactionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteD2DTransaction(
        int transactionId,
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
            await _localPartsService.DeleteD2DTransactionAsync(
                transactionId,
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
            _logger.LogError(ex, "Failed to delete D2D transaction {TransactionId}.", transactionId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to delete D2D transaction.");
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
