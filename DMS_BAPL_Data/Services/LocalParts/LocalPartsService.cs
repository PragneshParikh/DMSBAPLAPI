using System.Data;
using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Services.LocalParts;

public sealed class LocalPartsService : ILocalPartsService
{
    private readonly BapldmsvadContext _context;

    public LocalPartsService(BapldmsvadContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LocalPartsLocationResponse>> GetWorkshopLocationsAsync(
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken)
    {
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        var query = _context.LocationMasters
            .AsNoTracking()
            .Where(location => location.Active == "Y" && location.Locareaidno == 2);

        if (!canAccessAllDealers)
            query = query.Where(location => location.Dealercode == dealerScope);

        return await query
            .OrderBy(location => location.Locname)
            .Select(location => new LocalPartsLocationResponse(
                location.Id,
                location.Loccode,
                location.Locname,
                location.Dealercode))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LocalPartOptionResponse>> GetPartsAsync(
        int locationId,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken)
    {
        var location = await GetWorkshopLocationAsync(
            locationId,
            dealerScope,
            canAccessAllDealers,
            cancellationToken);

        var items = await _context.ItemMasters
            .AsNoTracking()
            .Where(item =>
                item.DealerCode == location.Dealercode &&
                item.Grpidno == 1 &&
                item.Status)
            .OrderBy(item => item.Itemcode)
            .Select(item => new
            {
                item.Itemcode,
                item.Itemname,
                item.Itemdesc,
                item.Displayname,
                item.Oemmodelname,
                item.Hsncode,
                item.Uom,
                item.Ipurrate
            })
            .ToListAsync(cancellationToken);

        var itemCodes = items.Select(item => item.Itemcode).ToList();
        var finalStockRows = await _context.PartsInventories
            .AsNoTracking()
            .Where(stock =>
                stock.VendorCode == location.Dealercode &&
                stock.DealerLocation == location.Loccode &&
                stock.FinalStockFlag == "Y" &&
                itemCodes.Contains(stock.ItemCode))
            .Select(stock => new
            {
                stock.ItemCode,
                ClosingQty = stock.BatchClosingQty
            })
            .ToListAsync(cancellationToken);

        var stockByPart = finalStockRows
            .GroupBy(row => row.ItemCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(row => (long)row.ClosingQty),
                StringComparer.OrdinalIgnoreCase);

        return items.Select(item =>
        {
            var stock = stockByPart.TryGetValue(item.Itemcode, out var quantity) ? quantity : 0;
            if (stock > int.MaxValue)
                throw new LocalPartsValidationException(
                    $"Current stock for part '{item.Itemcode}' exceeds the supported quantity.");

            return new LocalPartOptionResponse(
                item.Itemcode,
                item.Itemname,
                item.Itemdesc,
                item.Displayname,
                item.Oemmodelname,
                item.Hsncode,
                item.Uom,
                item.Ipurrate,
                (int)stock);
        }).ToList();
    }

    public async Task<IReadOnlyList<LocalPartsTransactionResponse>> GetTransactionsAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        int? locationId,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            throw new LocalPartsValidationException("From date cannot be later than to date.");
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        var receipts = _context.PartsInventories
            .AsNoTracking()
            .Where(stock => stock.Potype == "LOCAL" && stock.TransType == "P");

        if (!canAccessAllDealers)
            receipts = receipts.Where(stock => stock.VendorCode == dealerScope);
        if (fromDate.HasValue)
            receipts = receipts.Where(stock => stock.TransDate >= fromDate.Value);
        if (toDate.HasValue)
            receipts = receipts.Where(stock => stock.TransDate <= toDate.Value);

        if (locationId.HasValue)
        {
            var location = await _context.LocationMasters
                .AsNoTracking()
                .Where(candidate =>
                    candidate.Id == locationId.Value &&
                    candidate.Active == "Y" &&
                    candidate.Locareaidno == 2)
                .Where(candidate => canAccessAllDealers || candidate.Dealercode == dealerScope)
                .Select(candidate => new { candidate.Dealercode, candidate.Loccode })
                .FirstOrDefaultAsync(cancellationToken);

            if (location is null)
                throw new LocalPartsNotFoundException("Workshop location was not found or is not accessible.");

            receipts = receipts.Where(stock =>
                stock.VendorCode == location.Dealercode &&
                stock.DealerLocation == location.Loccode);
        }

        var rows = await (
            from receipt in receipts
            join itemRow in _context.ItemMasters.AsNoTracking()
                on new { ItemCode = receipt.ItemCode, DealerCode = receipt.VendorCode }
                equals new { ItemCode = itemRow.Itemcode, DealerCode = itemRow.DealerCode }
                into matchingItems
            from item in matchingItems.DefaultIfEmpty()
            join locationRow in _context.LocationMasters.AsNoTracking()
                on new { LocationCode = receipt.DealerLocation, DealerCode = receipt.VendorCode }
                equals new { LocationCode = locationRow.Loccode, DealerCode = (string?)locationRow.Dealercode }
                into matchingLocations
            from location in matchingLocations.DefaultIfEmpty()
            join dealerRow in _context.DealerMasters.AsNoTracking()
                on receipt.VendorCode equals dealerRow.Dealercode
                into matchingDealers
            from dealer in matchingDealers.DefaultIfEmpty()
            orderby receipt.TransDate descending, receipt.Id descending
            select new
            {
                receipt.Id,
                receipt.VoucherNo,
                receipt.TransDate,
                receipt.VendorCode,
                receipt.DealerLocation,
                receipt.ItemCode,
                receipt.BatchOpeningQty,
                receipt.BatchTransQty,
                receipt.BatchClosingQty,
                LocationId = location == null ? 0 : location.Id,
                PartName = item == null ? null : item.Itemname,
                Description = item == null ? null : item.Itemdesc,
                LocationName = location == null ? null : location.Locname,
                DealerName = dealer == null ? null : dealer.Compname
            })
            .ToListAsync(cancellationToken);

        return rows.Select(row =>
        {
            var dealerCode = row.VendorCode ?? string.Empty;
            return new LocalPartsTransactionResponse(
                row.Id,
                row.VoucherNo,
                new DateTimeOffset(row.TransDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                dealerCode,
                row.DealerName ?? dealerCode,
                row.LocationId,
                row.DealerLocation,
                row.LocationName ?? row.DealerLocation,
                row.ItemCode,
                row.PartName ?? row.ItemCode,
                row.Description ?? string.Empty,
                row.BatchOpeningQty,
                row.BatchTransQty,
                row.BatchClosingQty);
        }).ToList();
    }

    public async Task<LocalPartsTransactionResponse> GetTransactionAsync(
        int id,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            throw new LocalPartsValidationException("Transaction ID is invalid.");
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        var receipt = await _context.PartsInventories
            .AsNoTracking()
            .Where(stock => stock.Id == id && stock.Potype == "LOCAL" && stock.TransType == "P")
            .Where(stock => canAccessAllDealers || stock.VendorCode == dealerScope)
            .FirstOrDefaultAsync(cancellationToken);

        if (receipt is null || string.IsNullOrWhiteSpace(receipt.VendorCode))
            throw new LocalPartsNotFoundException(
                "Local-parts transaction was not found or is not accessible.");

        var location = await _context.LocationMasters
            .AsNoTracking()
            .Where(candidate =>
                candidate.Dealercode == receipt.VendorCode &&
                candidate.Loccode == receipt.DealerLocation &&
                candidate.Locareaidno == 2)
            .Select(candidate => new { candidate.Id, candidate.Locname })
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
            throw new LocalPartsNotFoundException("Workshop location for this transaction was not found.");

        var item = await _context.ItemMasters
            .AsNoTracking()
            .Where(candidate =>
                candidate.DealerCode == receipt.VendorCode &&
                candidate.Itemcode == receipt.ItemCode)
            .Select(candidate => new { candidate.Itemname, candidate.Itemdesc })
            .FirstOrDefaultAsync(cancellationToken);

        var dealerName = await _context.DealerMasters
            .AsNoTracking()
            .Where(dealer => dealer.Dealercode == receipt.VendorCode)
            .Select(dealer => dealer.Compname)
            .FirstOrDefaultAsync(cancellationToken);

        return new LocalPartsTransactionResponse(
            receipt.Id,
            receipt.VoucherNo,
            new DateTimeOffset(receipt.TransDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            receipt.VendorCode,
            dealerName ?? receipt.VendorCode,
            location.Id,
            receipt.DealerLocation,
            location.Locname,
            receipt.ItemCode,
            item?.Itemname ?? receipt.ItemCode,
            item?.Itemdesc ?? string.Empty,
            receipt.BatchOpeningQty,
            receipt.BatchTransQty,
            receipt.BatchClosingQty);
    }

    public async Task<LocalPartsTransactionResponse> UpdateTransactionAsync(
        int id,
        UpdateLocalPartsTransactionRequest request,
        string? dealerScope,
        bool canAccessAllDealers,
        string createdBy,
        CancellationToken cancellationToken)
    {
        if (request is null)
            throw new LocalPartsValidationException("Transaction data is required.");
        if (id <= 0)
            throw new LocalPartsValidationException("Transaction ID is invalid.");
        if (request.Date == default)
            throw new LocalPartsValidationException("Date is required.");
        if (request.PurchaseQuantity <= 0)
            throw new LocalPartsValidationException("Purchase quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new LocalPartsValidationException("Authenticated user information is required.");
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        var providedPrefix = request.Prefix?.Trim();
        if (providedPrefix?.Length > 100)
            throw new LocalPartsValidationException("Prefix must be at most 100 characters.");
        var prefix = string.IsNullOrWhiteSpace(providedPrefix)
            ? $"LOCAL-{Guid.NewGuid():N}"
            : providedPrefix;
        var transactionDate = DateOnly.FromDateTime(request.Date.UtcDateTime);

        await using (var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken))
        {
            try
            {
                var receipt = await _context.PartsInventories
                    .Where(stock => stock.Id == id && stock.Potype == "LOCAL" && stock.TransType == "P")
                    .Where(stock => canAccessAllDealers || stock.VendorCode == dealerScope)
                    .FirstOrDefaultAsync(cancellationToken);

                if (receipt is null || string.IsNullOrWhiteSpace(receipt.VendorCode))
                    throw new LocalPartsNotFoundException(
                        "Local-parts transaction was not found or is not accessible.");

                if (receipt.FinalStockFlag != "Y")
                    throw new LocalPartsConflictException(
                        "This transaction cannot be edited because later stock movements already exist for this part and location.");

                var hasLaterMovement = await _context.PartsInventories.AnyAsync(stock =>
                    stock.Id != id &&
                    stock.VendorCode == receipt.VendorCode &&
                    stock.DealerLocation == receipt.DealerLocation &&
                    stock.ItemCode == receipt.ItemCode &&
                    (stock.TransDate > receipt.TransDate ||
                     (stock.TransDate == receipt.TransDate && stock.Id > receipt.Id)),
                    cancellationToken);
                if (hasLaterMovement)
                    throw new LocalPartsConflictException(
                        "This transaction cannot be edited because later stock movements already exist for this part and location.");

                var hasMovementAfterNewDate = await _context.PartsInventories.AnyAsync(stock =>
                    stock.Id != id &&
                    stock.VendorCode == receipt.VendorCode &&
                    stock.DealerLocation == receipt.DealerLocation &&
                    stock.ItemCode == receipt.ItemCode &&
                    stock.TransDate > transactionDate,
                    cancellationToken);
                if (hasMovementAfterNewDate)
                    throw new LocalPartsConflictException(
                        "The transaction date cannot be moved before a later stock movement.");

                if (!string.Equals(prefix, receipt.VoucherNo, StringComparison.Ordinal))
                {
                    var prefixAlreadyUsed = await _context.PartsInventories.AnyAsync(stock =>
                        stock.Id != id &&
                        stock.VendorCode == receipt.VendorCode &&
                        stock.DealerLocation == receipt.DealerLocation &&
                        stock.VoucherNo == prefix &&
                        stock.TransType == "P",
                        cancellationToken);
                    if (prefixAlreadyUsed)
                        throw new LocalPartsConflictException(
                            $"Prefix '{prefix}' has already been used for a purchase at this location.");
                }

                int totalStock;
                decimal totalRate;
                try
                {
                    totalStock = checked(receipt.BatchOpeningQty + request.PurchaseQuantity);
                    totalRate = checked(receipt.PurchaseRate * request.PurchaseQuantity);
                }
                catch (OverflowException)
                {
                    throw new LocalPartsValidationException(
                        "The updated stock quantity or amount exceeds the supported range.");
                }

                receipt.VoucherNo = prefix;
                receipt.TransDate = transactionDate;
                receipt.BatchTransQty = request.PurchaseQuantity;
                receipt.BatchClosingQty = totalStock;
                receipt.TotalRate = totalRate;
                receipt.UpdatedBy = createdBy;
                receipt.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        return await GetTransactionAsync(id, dealerScope, canAccessAllDealers, cancellationToken);
    }

    public async Task DeleteTransactionAsync(
        int id,
        string? dealerScope,
        bool canAccessAllDealers,
        string deletedBy,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            throw new LocalPartsValidationException("Transaction ID is invalid.");
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new LocalPartsValidationException("Authenticated user information is required.");
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var receipt = await _context.PartsInventories
                .Where(stock => stock.Id == id && stock.Potype == "LOCAL" && stock.TransType == "P")
                .Where(stock => canAccessAllDealers || stock.VendorCode == dealerScope)
                .FirstOrDefaultAsync(cancellationToken);

            if (receipt is null || string.IsNullOrWhiteSpace(receipt.VendorCode))
                throw new LocalPartsNotFoundException(
                    "Local-parts transaction was not found or is not accessible.");

            if (receipt.FinalStockFlag != "Y")
                throw new LocalPartsConflictException(
                    "This transaction cannot be deleted because later stock movements already exist for this part and location.");

            var hasLaterMovement = await _context.PartsInventories.AnyAsync(stock =>
                stock.Id != id &&
                stock.VendorCode == receipt.VendorCode &&
                stock.DealerLocation == receipt.DealerLocation &&
                stock.ItemCode == receipt.ItemCode &&
                (stock.TransDate > receipt.TransDate ||
                 (stock.TransDate == receipt.TransDate && stock.Id > receipt.Id)),
                cancellationToken);
            if (hasLaterMovement)
                throw new LocalPartsConflictException(
                    "This transaction cannot be deleted because later stock movements already exist for this part and location.");

            receipt.Potype = "LOCAL-VOID";
            receipt.BatchTransQty = 0;
            receipt.BatchClosingQty = receipt.BatchOpeningQty;
            receipt.TotalRate = 0;
            receipt.UpdatedBy = deletedBy;
            receipt.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<LocalPartsSaveResponse> SaveAsync(
        CreateLocalPartsRequest request,
        string dealerScope,
        bool canAccessAllDealers,
        string createdBy,
        CancellationToken cancellationToken)
    {
        if (request is null)
            throw new LocalPartsValidationException("Receipt data is required.");

        var providedPrefix = request.Prefix?.Trim();
        if (providedPrefix?.Length > 100)
            throw new LocalPartsValidationException("Prefix must be at most 100 characters.");
        var prefix = string.IsNullOrWhiteSpace(providedPrefix)
            ? $"LOCAL-{Guid.NewGuid():N}"
            : providedPrefix;
        if (request.Date == default)
            throw new LocalPartsValidationException("Date is required.");
        if (request.LocationId <= 0)
            throw new LocalPartsValidationException("Select a workshop location.");
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new LocalPartsValidationException("Authenticated user information is required.");
        if (request.Items is null || request.Items.Count == 0)
            throw new LocalPartsValidationException("Add at least one part.");

        var quantitiesByPart = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in request.Items)
        {
            if (line is null)
                throw new LocalPartsValidationException("Every receipt line must contain a part.");

            var partCode = line.PartCode?.Trim();
            if (string.IsNullOrWhiteSpace(partCode))
                throw new LocalPartsValidationException("Part code is required for every line.");
            if (partCode.Length > 500)
                throw new LocalPartsValidationException("Part code is too long.");
            if (line.PurchaseQuantity <= 0)
                throw new LocalPartsValidationException($"Purchase quantity for '{partCode}' must be greater than zero.");

            try
            {
                quantitiesByPart[partCode] = checked(
                    quantitiesByPart.GetValueOrDefault(partCode) + line.PurchaseQuantity);
            }
            catch (OverflowException)
            {
                throw new LocalPartsValidationException(
                    $"Combined purchase quantity for '{partCode}' exceeds the supported range.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var location = await GetWorkshopLocationAsync(
                request.LocationId,
                dealerScope,
                canAccessAllDealers,
                cancellationToken);

            var partCodes = quantitiesByPart.Keys.ToList();
            var itemRows = await _context.ItemMasters
                .Where(item =>
                    item.DealerCode == location.Dealercode &&
                    item.Grpidno == 1 &&
                    item.Status &&
                    partCodes.Contains(item.Itemcode))
                .ToListAsync(cancellationToken);

            var itemsByPart = itemRows
                .GroupBy(item => item.Itemcode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var missingPart = partCodes.FirstOrDefault(partCode => !itemsByPart.ContainsKey(partCode));
            if (missingPart is not null)
                throw new LocalPartsValidationException(
                    $"Part '{missingPart}' is not an active Spares item owned by this dealer.");

            if (!string.IsNullOrWhiteSpace(providedPrefix))
            {
                var prefixAlreadyUsed = await _context.PartsInventories.AnyAsync(stock =>
                    stock.VendorCode == location.Dealercode &&
                    stock.DealerLocation == location.Loccode &&
                    stock.VoucherNo == prefix &&
                    stock.TransType == "P",
                    cancellationToken);
                if (prefixAlreadyUsed)
                    throw new LocalPartsConflictException(
                        $"Prefix '{prefix}' has already been used for a purchase at this location.");
            }

            var currentRows = await _context.PartsInventories
                .Where(stock =>
                    stock.VendorCode == location.Dealercode &&
                    stock.DealerLocation == location.Loccode &&
                    stock.FinalStockFlag == "Y" &&
                    partCodes.Contains(stock.ItemCode))
                .ToListAsync(cancellationToken);

            var currentByPart = currentRows
                .GroupBy(stock => stock.ItemCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(stock => (long)stock.BatchClosingQty),
                    StringComparer.OrdinalIgnoreCase);

            var transactionDate = DateOnly.FromDateTime(request.Date.UtcDateTime);
            var createdDate = DateTime.Now;
            var savedLines = new List<LocalPartsSavedLineResponse>(partCodes.Count);

            foreach (var (partCode, purchaseQuantity) in quantitiesByPart)
            {
                var item = itemsByPart[partCode];
                var currentStockLong = currentByPart.TryGetValue(partCode, out var stock) ? stock : 0;
                if (currentStockLong < 0 || currentStockLong > int.MaxValue)
                    throw new LocalPartsValidationException(
                        $"Current stock for part '{partCode}' is outside the supported range.");

                int totalStock;
                decimal totalRate;
                try
                {
                    totalStock = checked((int)currentStockLong + purchaseQuantity);
                    totalRate = checked(item.Ipurrate * purchaseQuantity);
                }
                catch (OverflowException)
                {
                    throw new LocalPartsValidationException(
                        $"Stock total for part '{partCode}' exceeds the supported range.");
                }

                savedLines.Add(new LocalPartsSavedLineResponse(
                    item.Itemcode,
                    item.Itemdesc,
                    (int)currentStockLong,
                    purchaseQuantity,
                    totalStock));

                _context.PartsInventories.Add(new PartsInventory
                {
                    TransId = Guid.NewGuid().ToString(),
                    ItemCode = item.Itemcode,
                    VoucherNo = prefix,
                    TransType = "P",
                    BatchNo = "LOCAL",
                    BatchOpeningQty = (int)currentStockLong,
                    BatchTransQty = purchaseQuantity,
                    BatchClosingQty = totalStock,
                    TransDate = transactionDate,
                    DealerLocation = location.Loccode,
                    VendorCode = location.Dealercode,
                    FinalStockFlag = "Y",
                    TotalRate = totalRate,
                    PurchaseRate = item.Ipurrate,
                    Potype = "LOCAL",
                    PostTransaction = 0,
                    CreatedBy = createdBy,
                    CreatedDate = createdDate
                });
            }

            foreach (var currentRow in currentRows)
                currentRow.FinalStockFlag = "N";

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new LocalPartsSaveResponse(
                prefix,
                request.Date,
                location.Id,
                location.Locname,
                savedLines);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<LocationMaster> GetWorkshopLocationAsync(
        int locationId,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken)
    {
        var query = _context.LocationMasters
            .Where(location =>
                location.Id == locationId &&
                location.Active == "Y" &&
                location.Locareaidno == 2);

        if (!canAccessAllDealers)
        {
            if (string.IsNullOrWhiteSpace(dealerScope))
                throw new LocalPartsValidationException("Dealer code is required.");

            query = query.Where(location => location.Dealercode == dealerScope);
        }

        return await query.FirstOrDefaultAsync(cancellationToken)
            ?? throw new LocalPartsNotFoundException("Workshop location was not found or is not accessible.");
    }
}
