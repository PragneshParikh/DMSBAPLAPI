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

/// <summary>A spare-parts line transferred between two dealer locations.</summary>
public sealed record PartsD2DLineRequest
{
    [Required, StringLength(500)]
    public required string PartCode { get; init; }

    [Range(1, int.MaxValue)]
    public int TransferQuantity { get; init; }
}

/// <summary>Payload for an immediate dealer-to-dealer spare-parts stock transfer.</summary>
public sealed record CreatePartsD2DRequest
{
    [Required, StringLength(100)]
    public required string Prefix { get; init; }

    [Required]
    public DateTimeOffset Date { get; init; }

    [Range(1, int.MaxValue)]
    public int FromLocationId { get; init; }

    [Range(1, int.MaxValue)]
    public int ToLocationId { get; init; }

    [Required, MinLength(1)]
    public required IReadOnlyList<PartsD2DLineRequest> Items { get; init; }
}

/// <summary>A saved dealer-to-dealer transfer line and both stock balances.</summary>
public sealed record PartsD2DLineResponse(
    string PartCode,
    string Description,
    int TransferQuantity,
    int FromPreviousStock,
    int FromClosingStock,
    int ToPreviousStock,
    int ToClosingStock,
    decimal Rate);

/// <summary>Confirmation returned after a dealer-to-dealer stock transfer.</summary>
public sealed record PartsD2DSaveResponse(
    string Prefix,
    string D2DBillNo,
    DateTimeOffset Date,
    string FromLocationCode,
    string ToLocationCode,
    IReadOnlyList<PartsD2DLineResponse> Items);

/// <summary>The next sequential bill number available for a dealer-to-dealer transfer.</summary>
public sealed record PartsD2DBillNoResponse(string D2DBillNo);

/// <summary>A saved dealer-to-dealer transfer line in location-pair history.</summary>
public sealed record PartsD2DTransactionResponse(
    int TransactionId,
    string D2DBillNo,
    string Prefix,
    DateTimeOffset Date,
    DateTimeOffset CreatedDate,
    DateTimeOffset ReceivedDate,
    string FromLocationCode,
    string FromLocationName,
    string FromDealerCode,
    string ToLocationCode,
    string ToLocationName,
    string ToDealerCode,
    string PartCode,
    string Description,
    int TransferQuantity,
    int FromPreviousStock,
    int FromClosingStock,
    int ToPreviousStock,
    int ToClosingStock,
    decimal Rate,
    bool IsIncomingForCaller,
    bool IsReceived);

/// <summary>A dealer-to-dealer parts receipt notification and its status.</summary>
public sealed record PartsD2DNotificationResponse(
    string D2DBillNo,
    string Prefix,
    DateTimeOffset InvoiceDate,
    DateTimeOffset ReceivedDate,
    string FromLocationName,
    string FromDealerCode,
    int NumberOfItems,
    bool IsReceived);

/// <summary>A dealer-to-dealer receipt header and its transferred parts.</summary>
public sealed record PartsD2DReceiptResponse(
    string D2DBillNo,
    string Prefix,
    DateTimeOffset InvoiceDate,
    DateTimeOffset ReceivedDate,
    string FromLocationCode,
    string FromLocationName,
    string FromDealerCode,
    string ToLocationCode,
    string ToLocationName,
    bool IsReceived,
    IReadOnlyList<PartsD2DReceiptItemResponse> Items);

/// <summary>A part line shown while receiving a dealer-to-dealer transfer.</summary>
public sealed record PartsD2DReceiptItemResponse(
    string PartCode,
    string Description,
    string HsnCode,
    int Quantity,
    decimal Rate);

/// <summary>Quantity update for one posted dealer-to-dealer transfer line.</summary>
public sealed record UpdatePartsD2DTransactionRequest
{
    [Range(1, int.MaxValue)]
    public int TransferQuantity { get; init; }
}

public sealed class LocalPartsValidationException(string message) : Exception(message);

public sealed class LocalPartsNotFoundException(string message) : Exception(message);

public sealed class LocalPartsConflictException(string message) : Exception(message);
