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
}
