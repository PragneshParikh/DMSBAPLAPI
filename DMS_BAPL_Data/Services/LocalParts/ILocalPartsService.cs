namespace DMS_BAPL_Data.Services.LocalParts;

public interface ILocalPartsService
{
    Task<IReadOnlyList<LocalPartsLocationResponse>> GetWorkshopLocationsAsync(
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LocalPartOptionResponse>> GetPartsAsync(
        int locationId,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LocalPartsTransactionResponse>> GetTransactionsAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        int? locationId,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken);

    Task<LocalPartsTransactionResponse> GetTransactionAsync(
        int id,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken);

    Task<LocalPartsTransactionResponse> UpdateTransactionAsync(
        int id,
        UpdateLocalPartsTransactionRequest request,
        string? dealerScope,
        bool canAccessAllDealers,
        string createdBy,
        CancellationToken cancellationToken);

    Task DeleteTransactionAsync(
        int id,
        string? dealerScope,
        bool canAccessAllDealers,
        string deletedBy,
        CancellationToken cancellationToken);

    Task<LocalPartsSaveResponse> SaveAsync(
        CreateLocalPartsRequest request,
        string dealerScope,
        bool canAccessAllDealers,
        string createdBy,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PartsD2DTransactionResponse>> GetD2DTransactionsAsync(
        int fromLocationId,
        int toLocationId,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PartsD2DTransactionResponse>> GetD2DHistoryAsync(
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken);

    Task<string> GetNextD2DBillNoAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<PartsD2DNotificationResponse>> GetD2DNotificationsAsync(
        string dealerScope,
        CancellationToken cancellationToken);

    Task<PartsD2DReceiptResponse> GetD2DReceiptAsync(
        string d2dBillNo,
        string dealerScope,
        CancellationToken cancellationToken);

    Task AcceptD2DReceiptAsync(
        string d2dBillNo,
        string dealerScope,
        string receivedBy,
        CancellationToken cancellationToken);

    Task RejectD2DReceiptAsync(
        string d2dBillNo,
        string dealerScope,
        string rejectedBy,
        CancellationToken cancellationToken);

    Task UpdateD2DTransactionAsync(
        int transactionId,
        UpdatePartsD2DTransactionRequest request,
        string? dealerScope,
        bool canAccessAllDealers,
        string updatedBy,
        CancellationToken cancellationToken);

    Task DeleteD2DTransactionAsync(
        int transactionId,
        string? dealerScope,
        bool canAccessAllDealers,
        string deletedBy,
        CancellationToken cancellationToken);

    Task<PartsD2DSaveResponse> SaveD2DAsync(
        CreatePartsD2DRequest request,
        string? dealerScope,
        bool canAccessAllDealers,
        string createdBy,
        CancellationToken cancellationToken);
}
