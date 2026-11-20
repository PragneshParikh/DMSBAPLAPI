using System.ComponentModel.DataAnnotations;

namespace DMS_BAPL_Data.Services.LocalParts;

/// <summary>A workshop location available for a local-parts receipt.</summary>
public sealed record LocalPartsLocationResponse(
    int LocationId,
    string LocationCode,
    string LocationName,
    string DealerCode);

/// <summary>A dealer-owned spare part and its stock at a workshop location.</summary>
public sealed record LocalPartOptionResponse(
    string PartCode,
    string PartName,
    string Description,
    string? DisplayName,
    string? ModelName,
    string HsnCode,
    int? Uom,
    decimal PurchaseRate,
    int CurrentStock);

/// <summary>A part and purchase quantity on a local-parts receipt.</summary>
public sealed record LocalPartsLineRequest
{
    [Required, StringLength(500)]
    public required string PartCode { get; init; }

    [Range(1, int.MaxValue)]
    public int PurchaseQuantity { get; init; }
}

/// <summary>Payload for posting a local-parts receipt into stock.</summary>
public sealed record CreateLocalPartsRequest
{
    [Required, StringLength(100)]
    public required string Prefix { get; init; }

    [Required]
    public DateTimeOffset Date { get; init; }

    [Range(1, int.MaxValue)]
    public int LocationId { get; init; }

    [Required, MinLength(1)]
    public required IReadOnlyList<LocalPartsLineRequest> Items { get; init; }
}

/// <summary>A posted local-parts line with its resulting stock.</summary>
public sealed record LocalPartsSavedLineResponse(
    string PartCode,
    string Description,
    int PreviousStock,
    int PurchaseQuantity,
    int TotalStock);

/// <summary>Confirmation returned after local-parts stock is posted.</summary>
public sealed record LocalPartsSaveResponse(
    string Prefix,
    DateTimeOffset Date,
    int LocationId,
    string LocationName,
    IReadOnlyList<LocalPartsSavedLineResponse> Items);

/// <summary>Payload for editing a posted local-parts transaction line.</summary>
public sealed record UpdateLocalPartsTransactionRequest
{
    [Required, StringLength(100)]
    public required string Prefix { get; init; }

    [Range(1, int.MaxValue)]
    public int PurchaseQuantity { get; init; }
}

/// <summary>A posted local-parts receipt line shown in transaction history.</summary>
public sealed record LocalPartsTransactionResponse(
    int Id,
    string ReceiptNumber,
    DateTimeOffset Date,
    string DealerCode,
    string DealerName,
    int LocationId,
    string LocationCode,
    string LocationName,
    string PartCode,
    string PartName,
    string Description,
    int PreviousStock,
    int PurchaseQuantity,
    int TotalStock);

public sealed class LocalPartsValidationException(string message) : Exception(message);

public sealed class LocalPartsNotFoundException(string message) : Exception(message);

public sealed class LocalPartsConflictException(string message) : Exception(message);
