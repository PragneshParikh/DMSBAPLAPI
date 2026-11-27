using System.Data;
using System.Globalization;
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
        if (request.PurchaseQuantity <= 0)
            throw new LocalPartsValidationException("Purchase quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new LocalPartsValidationException("Authenticated user information is required.");
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        var prefix = request.Prefix?.Trim();
        if (string.IsNullOrWhiteSpace(prefix))
            throw new LocalPartsValidationException("Prefix is required.");
        if (prefix.Length > 100)
            throw new LocalPartsValidationException("Prefix must be at most 100 characters.");
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

        var prefix = request.Prefix?.Trim();
        if (string.IsNullOrWhiteSpace(prefix))
            throw new LocalPartsValidationException("Prefix is required.");
        if (prefix.Length > 100)
            throw new LocalPartsValidationException("Prefix must be at most 100 characters.");
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

            var prefixAlreadyUsed = await _context.PartsInventories.AnyAsync(stock =>
                stock.VendorCode == location.Dealercode &&
                stock.DealerLocation == location.Loccode &&
                stock.VoucherNo == prefix &&
                stock.TransType == "P",
                cancellationToken);
            if (prefixAlreadyUsed)
                throw new LocalPartsConflictException(
                    $"Prefix '{prefix}' has already been used for a purchase at this location.");

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

    public async Task<IReadOnlyList<PartsD2DTransactionResponse>> GetD2DTransactionsAsync(
        int fromLocationId,
        int toLocationId,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken)
    {
        if (fromLocationId <= 0 || toLocationId <= 0)
            throw new LocalPartsValidationException("Select both from and to workshop locations.");
        if (fromLocationId == toLocationId)
            throw new LocalPartsValidationException("From and to locations must be different.");

        var fromLocation = await GetWorkshopLocationAsync(
            fromLocationId,
            null,
            true,
            cancellationToken);
        var toLocation = await GetWorkshopLocationAsync(
            toLocationId,
            null,
            true,
            cancellationToken);
        if (!canAccessAllDealers &&
            !string.Equals(fromLocation.Dealercode, dealerScope, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(toLocation.Dealercode, dealerScope, StringComparison.OrdinalIgnoreCase))
            throw new LocalPartsNotFoundException(
                "D2D transaction was not found or is not accessible.");
        if (string.Equals(fromLocation.Loccode, toLocation.Loccode, StringComparison.OrdinalIgnoreCase))
            throw new LocalPartsValidationException("From and to locations must be different.");

        var transferRows = await (
            from source in _context.PartsInventories.AsNoTracking()
            join target in _context.PartsInventories.AsNoTracking()
                on new { source.BatchNo, source.ItemCode } equals new { target.BatchNo, target.ItemCode }
            where source.VendorCode == fromLocation.Dealercode &&
                  source.DealerLocation == fromLocation.Loccode &&
                  source.Potype == "D2D-SOURCE" &&
                  source.BatchNo != null &&
                  target.VendorCode == toLocation.Dealercode &&
                  target.DealerLocation == toLocation.Loccode &&
                  target.Potype == "D2D-TARGET"
            orderby source.TransDate descending, source.Id descending
            select new { Source = source, Target = target })
            .ToListAsync(cancellationToken);

        if (transferRows.Count == 0)
            return Array.Empty<PartsD2DTransactionResponse>();
        var sourceRows = transferRows.Select(row => row.Source).ToList();

        var partCodes = sourceRows
            .Select(stock => stock.ItemCode)
            .Distinct()
            .ToList();
        var itemDescriptions = await _context.ItemMasters
            .AsNoTracking()
            .Where(item =>
                item.DealerCode == fromLocation.Dealercode &&
                partCodes.Contains(item.Itemcode))
            .Select(item => new { item.Itemcode, item.Itemdesc })
            .ToListAsync(cancellationToken);
        var descriptionByPart = itemDescriptions
            .GroupBy(item => item.Itemcode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().Itemdesc,
                StringComparer.OrdinalIgnoreCase);

        var transactions = new List<PartsD2DTransactionResponse>(sourceRows.Count);
        foreach (var transferRow in transferRows)
        {
            var source = transferRow.Source;
            var target = transferRow.Target;

            var description = descriptionByPart.TryGetValue(source.ItemCode, out var itemDescription)
                ? itemDescription
                : source.ItemCode;
            var isIncomingForCaller =
                !canAccessAllDealers &&
                string.Equals(toLocation.Dealercode, dealerScope, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(fromLocation.Dealercode, dealerScope, StringComparison.OrdinalIgnoreCase);
            transactions.Add(new PartsD2DTransactionResponse(
                source.Id,
                source.BatchNo!,
                source.VoucherNo,
                new DateTimeOffset(source.TransDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                new DateTimeOffset(source.CreatedDate),
                new DateTimeOffset(target.PostTransaction == 1
                    ? target.UpdatedDate ?? target.CreatedDate
                    : target.CreatedDate),
                fromLocation.Loccode,
                fromLocation.Locname,
                fromLocation.Dealercode,
                toLocation.Loccode,
                toLocation.Locname,
                toLocation.Dealercode,
                source.ItemCode,
                description,
                source.BatchTransQty,
                source.BatchOpeningQty,
                source.BatchClosingQty,
                target.BatchOpeningQty,
                target.BatchClosingQty,
                isIncomingForCaller ? target.PurchaseRate : source.PurchaseRate,
                isIncomingForCaller,
                target.PostTransaction == 1));
        }

        return transactions;
    }

    public async Task<IReadOnlyList<PartsD2DTransactionResponse>> GetD2DHistoryAsync(
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken)
    {
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        // 26.11.2026 - dealerScope may hold several codes (BG employee mapped to many dealers)
        var scopeCodes = (dealerScope ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var transferRows = await (
            from source in _context.PartsInventories.AsNoTracking()
            join target in _context.PartsInventories.AsNoTracking()
                on new { source.BatchNo, source.ItemCode } equals new { target.BatchNo, target.ItemCode }
            where source.Potype == "D2D-SOURCE" &&
                  source.BatchNo != null &&
                  target.Potype == "D2D-TARGET" &&
                  (canAccessAllDealers ||
                   scopeCodes.Contains(source.VendorCode) ||
                   scopeCodes.Contains(target.VendorCode))
            orderby source.TransDate descending, source.Id descending
            select new { Source = source, Target = target })
            .ToListAsync(cancellationToken);

        if (transferRows.Count == 0)
            return Array.Empty<PartsD2DTransactionResponse>();

        var locations = await _context.LocationMasters
            .AsNoTracking()
            .Where(location => location.Locareaidno == 2)
            .Select(location => new
            {
                location.Dealercode,
                location.Loccode,
                location.Locname
            })
            .ToListAsync(cancellationToken);
        var locationNameByKey = locations
            .GroupBy(
                location => $"{location.Dealercode}\u001f{location.Loccode}",
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().Locname,
                StringComparer.OrdinalIgnoreCase);

        var partCodes = transferRows.Select(row => row.Source.ItemCode).Distinct().ToList();
        var itemDescriptions = new List<(string? DealerCode, string Itemcode, string Itemdesc)>();
        foreach (var partCodeChunk in partCodes.Chunk(1000))
        {
            var chunkDescriptions = await _context.ItemMasters
                .AsNoTracking()
                .Where(item => partCodeChunk.Contains(item.Itemcode))
                .Select(item => new { item.DealerCode, item.Itemcode, item.Itemdesc })
                .ToListAsync(cancellationToken);
            itemDescriptions.AddRange(chunkDescriptions.Select(item =>
                (item.DealerCode, item.Itemcode, item.Itemdesc)));
        }

        var descriptionByKey = itemDescriptions
            .GroupBy(item => $"{item.DealerCode}\u001f{item.Itemcode}", StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().Itemdesc,
                StringComparer.OrdinalIgnoreCase);

        return transferRows.Select(row =>
        {
            var source = row.Source;
            var target = row.Target;
            var fromLocationKey = $"{source.VendorCode}\u001f{source.DealerLocation}";
            var toLocationKey = $"{target.VendorCode}\u001f{target.DealerLocation}";
            var fromLocationName = locationNameByKey.TryGetValue(fromLocationKey, out var fromName)
                ? fromName
                : source.DealerLocation;
            var toLocationName = locationNameByKey.TryGetValue(toLocationKey, out var toName)
                ? toName
                : target.DealerLocation;
            var itemKey = $"{source.VendorCode}\u001f{source.ItemCode}";
            var description = descriptionByKey.TryGetValue(itemKey, out var itemDescription)
                ? itemDescription
                : source.ItemCode;
            var isIncomingForCaller =
                !canAccessAllDealers &&
                scopeCodes.Contains(target.VendorCode ?? string.Empty, StringComparer.OrdinalIgnoreCase) &&
                !scopeCodes.Contains(source.VendorCode ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            return new PartsD2DTransactionResponse(
                source.Id,
                source.BatchNo!,
                source.VoucherNo,
                new DateTimeOffset(source.TransDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                new DateTimeOffset(source.CreatedDate),
                new DateTimeOffset(target.PostTransaction == 1
                    ? target.UpdatedDate ?? target.CreatedDate
                    : target.CreatedDate),
                source.DealerLocation,
                fromLocationName,
                source.VendorCode ?? string.Empty,
                target.DealerLocation,
                toLocationName,
                target.VendorCode ?? string.Empty,
                source.ItemCode,
                description,
                source.BatchTransQty,
                source.BatchOpeningQty,
                source.BatchClosingQty,
                target.BatchOpeningQty,
                target.BatchClosingQty,
                isIncomingForCaller ? target.PurchaseRate : source.PurchaseRate,
                isIncomingForCaller,
                target.PostTransaction == 1);
        }).ToList();
    }

    public async Task<IReadOnlyList<PartsD2DNotificationResponse>> GetD2DNotificationsAsync(
        string dealerScope,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        var transactions = await GetD2DHistoryAsync(dealerScope, false, cancellationToken);
        return transactions
            .Where(transaction => transaction.IsIncomingForCaller)
            .GroupBy(transaction => transaction.D2DBillNo, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var first = group.First();
                return new PartsD2DNotificationResponse(
                    first.D2DBillNo,
                    first.Prefix,
                    first.Date,
                    first.ReceivedDate,
                    first.FromLocationName,
                    first.FromDealerCode,
                    group.Count(),
                    group.All(transaction => transaction.IsReceived));
            })
            .OrderByDescending(notification => notification.ReceivedDate)
            .ToList();
    }

    public async Task<PartsD2DReceiptResponse> GetD2DReceiptAsync(
        string d2dBillNo,
        string dealerScope,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(d2dBillNo))
            throw new LocalPartsValidationException("D2D bill number is required.");

        var transactions = await GetD2DHistoryAsync(dealerScope, false, cancellationToken);
        var receiptLines = transactions
            .Where(transaction =>
                string.Equals(transaction.D2DBillNo, d2dBillNo, StringComparison.OrdinalIgnoreCase) &&
                transaction.IsIncomingForCaller)
            .ToList();
        if (receiptLines.Count == 0)
            throw new LocalPartsNotFoundException("D2D receipt was not found or is not accessible.");

        var toDealerCode = receiptLines[0].ToDealerCode;
        var partCodes = receiptLines.Select(line => line.PartCode).ToList();
        var items = await _context.ItemMasters
            .AsNoTracking()
            .Where(item => item.DealerCode == toDealerCode && partCodes.Contains(item.Itemcode))
            .Select(item => new { item.Itemcode, item.Hsncode })
            .ToListAsync(cancellationToken);
        var hsnByPart = items
            .GroupBy(item => item.Itemcode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().Hsncode ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        var firstLine = receiptLines[0];
        return new PartsD2DReceiptResponse(
            firstLine.D2DBillNo,
            firstLine.Prefix,
            firstLine.Date,
            firstLine.CreatedDate,
            firstLine.FromLocationCode,
            firstLine.FromLocationName,
            firstLine.FromDealerCode,
            firstLine.ToLocationCode,
            firstLine.ToLocationName,
            receiptLines.All(line => line.IsReceived),
            receiptLines.Select(line => new PartsD2DReceiptItemResponse(
                line.PartCode,
                line.Description,
                hsnByPart.TryGetValue(line.PartCode, out var hsnCode) ? hsnCode : string.Empty,
                line.TransferQuantity,
                line.Rate)).ToList());
    }

    public async Task AcceptD2DReceiptAsync(
        string d2dBillNo,
        string dealerScope,
        string receivedBy,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(d2dBillNo) ||
            string.IsNullOrWhiteSpace(dealerScope) ||
            string.IsNullOrWhiteSpace(receivedBy))
            throw new LocalPartsValidationException("D2D receipt, dealer and user information are required.");

        var targets = await _context.PartsInventories
            .Where(stock =>
                stock.BatchNo == d2dBillNo &&
                stock.Potype == "D2D-TARGET" &&
                stock.VendorCode == dealerScope)
            .ToListAsync(cancellationToken);
        if (targets.Count == 0)
            throw new LocalPartsNotFoundException("D2D receipt was not found or is not accessible.");

        foreach (var target in targets)
        {
            target.PostTransaction = 1;
            target.UpdatedBy = receivedBy;
            target.UpdatedDate = DateTime.Now;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectD2DReceiptAsync(
        string d2dBillNo,
        string dealerScope,
        string rejectedBy,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(d2dBillNo) ||
            string.IsNullOrWhiteSpace(dealerScope) ||
            string.IsNullOrWhiteSpace(rejectedBy))
            throw new LocalPartsValidationException("D2D receipt, dealer and user information are required.");

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var targets = await _context.PartsInventories
                .Where(stock =>
                    stock.BatchNo == d2dBillNo &&
                    stock.Potype == "D2D-TARGET" &&
                    stock.VendorCode == dealerScope)
                .ToListAsync(cancellationToken);
            if (targets.Count == 0)
                throw new LocalPartsNotFoundException("D2D receipt was not found or is not accessible.");
            if (targets.Any(target => target.PostTransaction == 1))
                throw new LocalPartsConflictException("This D2D receipt has already been marked as received.");

            var sources = await _context.PartsInventories
                .Where(stock =>
                    stock.BatchNo == d2dBillNo &&
                    stock.Potype == "D2D-SOURCE")
                .ToListAsync(cancellationToken);
            if (sources.Count != targets.Count)
                throw new LocalPartsConflictException(
                    "The matching source entries were not found; this transfer cannot be rejected.");

            foreach (var target in targets)
            {
                var source = sources.FirstOrDefault(row =>
                    string.Equals(row.ItemCode, target.ItemCode, StringComparison.OrdinalIgnoreCase));
                if (source is null)
                    throw new LocalPartsConflictException(
                        "A matching source entry was not found; this transfer cannot be rejected.");
                await EnsureD2DLineIsLatestAsync(source, target, cancellationToken);
            }

            var rejectedAt = DateTime.Now;
            foreach (var source in sources)
            {
                source.Potype = "D2D-VOID-SOURCE";
                source.BatchTransQty = 0;
                source.BatchClosingQty = source.BatchOpeningQty;
                source.TotalRate = 0;
                source.UpdatedBy = rejectedBy;
                source.UpdatedDate = rejectedAt;
            }

            foreach (var target in targets)
            {
                target.Potype = "D2D-VOID-TARGET";
                target.BatchTransQty = 0;
                target.BatchClosingQty = target.BatchOpeningQty;
                target.TotalRate = 0;
                target.UpdatedBy = rejectedBy;
                target.UpdatedDate = rejectedAt;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateD2DTransactionAsync(
        int transactionId,
        UpdatePartsD2DTransactionRequest request,
        string? dealerScope,
        bool canAccessAllDealers,
        string updatedBy,
        CancellationToken cancellationToken)
    {
        if (transactionId <= 0)
            throw new LocalPartsValidationException("Transaction ID is invalid.");
        if (request is null || request.TransferQuantity <= 0)
            throw new LocalPartsValidationException("Transfer quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new LocalPartsValidationException("Authenticated user information is required.");
        if (!canAccessAllDealers && string.IsNullOrWhiteSpace(dealerScope))
            throw new LocalPartsValidationException("Dealer code is required.");

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        try
        {
            var (source, target) = await GetAccessibleD2DLineAsync(
                transactionId,
                dealerScope,
                canAccessAllDealers,
                cancellationToken);
            await EnsureD2DLineIsLatestAsync(source, target, cancellationToken);

            if (source.BatchOpeningQty < request.TransferQuantity)
                throw new LocalPartsValidationException(
                    $"Transfer quantity cannot exceed source stock before this transfer ({source.BatchOpeningQty}).");

            int sourceClosing;
            int targetClosing;
            decimal sourceTotalRate;
            decimal targetTotalRate;
            try
            {
                sourceClosing = checked(source.BatchOpeningQty - request.TransferQuantity);
                targetClosing = checked(target.BatchOpeningQty + request.TransferQuantity);
                sourceTotalRate = checked(source.PurchaseRate * request.TransferQuantity);
                targetTotalRate = checked(target.PurchaseRate * request.TransferQuantity);
            }
            catch (OverflowException)
            {
                throw new LocalPartsValidationException(
                    "The updated stock quantity or amount exceeds the supported range.");
            }

            source.BatchTransQty = request.TransferQuantity;
            source.BatchClosingQty = sourceClosing;
            source.TotalRate = sourceTotalRate;
            source.UpdatedBy = updatedBy;
            source.UpdatedDate = DateTime.Now;

            target.BatchTransQty = request.TransferQuantity;
            target.BatchClosingQty = targetClosing;
            target.TotalRate = targetTotalRate;
            target.UpdatedBy = updatedBy;
            target.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteD2DTransactionAsync(
        int transactionId,
        string? dealerScope,
        bool canAccessAllDealers,
        string deletedBy,
        CancellationToken cancellationToken)
    {
        if (transactionId <= 0)
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
            var (source, target) = await GetAccessibleD2DLineAsync(
                transactionId,
                dealerScope,
                canAccessAllDealers,
                cancellationToken);
            await EnsureD2DLineIsLatestAsync(source, target, cancellationToken);

            source.Potype = "D2D-VOID-SOURCE";
            source.BatchTransQty = 0;
            source.BatchClosingQty = source.BatchOpeningQty;
            source.TotalRate = 0;
            source.UpdatedBy = deletedBy;
            source.UpdatedDate = DateTime.Now;

            target.Potype = "D2D-VOID-TARGET";
            target.BatchTransQty = 0;
            target.BatchClosingQty = target.BatchOpeningQty;
            target.TotalRate = 0;
            target.UpdatedBy = deletedBy;
            target.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<(PartsInventory Source, PartsInventory Target)> GetAccessibleD2DLineAsync(
        int transactionId,
        string? dealerScope,
        bool canAccessAllDealers,
        CancellationToken cancellationToken)
    {
        var source = await _context.PartsInventories
            .Where(stock => stock.Id == transactionId && stock.Potype == "D2D-SOURCE")
            .FirstOrDefaultAsync(cancellationToken);
        if (source is null || string.IsNullOrWhiteSpace(source.BatchNo) ||
            string.IsNullOrWhiteSpace(source.VendorCode))
            throw new LocalPartsNotFoundException(
                "D2D transaction was not found or is not accessible.");

        var target = await _context.PartsInventories
            .Where(stock =>
                stock.Potype == "D2D-TARGET" &&
                stock.BatchNo == source.BatchNo &&
                stock.ItemCode == source.ItemCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (target is null || string.IsNullOrWhiteSpace(target.VendorCode) ||
            string.IsNullOrWhiteSpace(target.DealerLocation))
            throw new LocalPartsConflictException(
                "The matching destination stock entry was not found; this transfer cannot be changed.");
        if (!canAccessAllDealers &&
            !string.Equals(source.VendorCode, dealerScope, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(target.VendorCode, dealerScope, StringComparison.OrdinalIgnoreCase))
            throw new LocalPartsNotFoundException(
                "D2D transaction was not found or is not accessible.");

        return (source, target);
    }

    private async Task EnsureD2DLineIsLatestAsync(
        PartsInventory source,
        PartsInventory target,
        CancellationToken cancellationToken)
    {
        var sourceHasLaterMovement = await HasLaterStockMovementAsync(source, cancellationToken);
        var targetHasLaterMovement = await HasLaterStockMovementAsync(target, cancellationToken);
        var sourceHasOtherFinalRow = await HasOtherFinalStockRowAsync(source, cancellationToken);
        var targetHasOtherFinalRow = await HasOtherFinalStockRowAsync(target, cancellationToken);
        if (source.FinalStockFlag != "Y" ||
            target.FinalStockFlag != "Y" ||
            sourceHasLaterMovement ||
            targetHasLaterMovement ||
            sourceHasOtherFinalRow ||
            targetHasOtherFinalRow)
            throw new LocalPartsConflictException(
                "This transfer cannot be edited or deleted because later stock movements exist, or the stock ledger is no longer at this transfer.");
    }

    private Task<bool> HasOtherFinalStockRowAsync(
        PartsInventory stockRow,
        CancellationToken cancellationToken) =>
        _context.PartsInventories.AnyAsync(stock =>
            stock.Id != stockRow.Id &&
            stock.VendorCode == stockRow.VendorCode &&
            stock.DealerLocation == stockRow.DealerLocation &&
            stock.ItemCode == stockRow.ItemCode &&
            stock.FinalStockFlag == "Y",
            cancellationToken);

    private Task<bool> HasLaterStockMovementAsync(
        PartsInventory stockRow,
        CancellationToken cancellationToken) =>
        _context.PartsInventories.AnyAsync(stock =>
            stock.Id != stockRow.Id &&
            stock.VendorCode == stockRow.VendorCode &&
            stock.DealerLocation == stockRow.DealerLocation &&
            stock.ItemCode == stockRow.ItemCode &&
            (stock.TransDate > stockRow.TransDate ||
             (stock.TransDate == stockRow.TransDate && stock.Id > stockRow.Id)),
            cancellationToken);

    public async Task<string> GetNextD2DBillNoAsync(CancellationToken cancellationToken)
    {
        var existingBillNumbers = await _context.PartsInventories
            .Where(stock => stock.Potype == "D2D-SOURCE" || stock.Potype == "D2D-VOID-SOURCE")
            .Select(stock => stock.BatchNo)
            .ToListAsync(cancellationToken);
        var latestBillNumber = existingBillNumbers
            .Select(value => long.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var number) ? number : 0)
            .DefaultIfEmpty()
            .Max();
        if (latestBillNumber == long.MaxValue)
            throw new LocalPartsConflictException("The D2D bill number sequence is exhausted.");

        return (latestBillNumber + 1).ToString(CultureInfo.InvariantCulture);
    }

    public async Task<PartsD2DSaveResponse> SaveD2DAsync(
        CreatePartsD2DRequest request,
        string? dealerScope,
        bool canAccessAllDealers,
        string createdBy,
        CancellationToken cancellationToken)
    {
        if (request is null)
            throw new LocalPartsValidationException("D2D transfer data is required.");

        var prefix = request.Prefix?.Trim();
        if (string.IsNullOrWhiteSpace(prefix))
            throw new LocalPartsValidationException("Prefix is required.");
        if (prefix.Length > 100)
            throw new LocalPartsValidationException("Prefix must be at most 100 characters.");
        if (request.Date == default)
            throw new LocalPartsValidationException("Date is required.");
        if (request.FromLocationId <= 0)
            throw new LocalPartsValidationException("Select the from workshop location.");
        if (request.ToLocationId <= 0)
            throw new LocalPartsValidationException("Select the to workshop location.");
        if (request.FromLocationId == request.ToLocationId)
            throw new LocalPartsValidationException("From and to locations must be different.");
        if (request.Items is null || request.Items.Count == 0)
            throw new LocalPartsValidationException("Add at least one part.");
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new LocalPartsValidationException("Authenticated user information is required.");

        var quantitiesByPart = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in request.Items)
        {
            if (line is null)
                throw new LocalPartsValidationException("Every transfer line must contain a part.");

            var partCode = line.PartCode?.Trim();
            if (string.IsNullOrWhiteSpace(partCode))
                throw new LocalPartsValidationException("Part code is required for every line.");
            if (line.TransferQuantity <= 0)
                throw new LocalPartsValidationException($"Transfer quantity for '{partCode}' must be greater than zero.");

            try
            {
                quantitiesByPart[partCode] = checked(
                    quantitiesByPart.GetValueOrDefault(partCode) + line.TransferQuantity);
            }
            catch (OverflowException)
            {
                throw new LocalPartsValidationException(
                    $"Combined transfer quantity for '{partCode}' exceeds the supported range.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var fromLocation = await GetWorkshopLocationAsync(
                request.FromLocationId,
                dealerScope,
                canAccessAllDealers,
                cancellationToken);
            var toLocation = await GetWorkshopLocationAsync(
                request.ToLocationId,
                dealerScope,
                true,
                cancellationToken);

            var fromLocationCode = fromLocation.Loccode;
            var toLocationCode = toLocation.Loccode;

            if (string.Equals(fromLocationCode, toLocationCode, StringComparison.OrdinalIgnoreCase))
                throw new LocalPartsValidationException("From and to locations must be different.");

            var prefixAlreadyUsed = await _context.PartsInventories.AnyAsync(stock =>
                stock.VoucherNo == prefix &&
                (stock.Potype == "D2D-SOURCE" || stock.Potype == "D2D-VOID-SOURCE"),
                cancellationToken);
            if (prefixAlreadyUsed)
                throw new LocalPartsConflictException(
                    $"Prefix '{prefix}' has already been used for a D2D transfer.");

            var d2dBillNo = await GetNextD2DBillNoAsync(cancellationToken);

            var partCodes = quantitiesByPart.Keys.ToList();
            var fromItems = await _context.ItemMasters
                .Where(item =>
                    item.DealerCode == fromLocation.Dealercode &&
                    item.Grpidno == 1 &&
                    item.Status &&
                    partCodes.Contains(item.Itemcode))
                .ToListAsync(cancellationToken);
            var fromItemsByPart = fromItems
                .GroupBy(item => item.Itemcode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var toItems = await _context.ItemMasters
                .Where(item =>
                    item.DealerCode == toLocation.Dealercode &&
                    item.Grpidno == 1 &&
                    partCodes.Contains(item.Itemcode))
                .ToListAsync(cancellationToken);
            var toItemsByPart = toItems
                .GroupBy(item => item.Itemcode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var partCode in partCodes)
            {
                if (!fromItemsByPart.ContainsKey(partCode))
                    throw new LocalPartsValidationException(
                        $"Part '{partCode}' is not an active Spares item for the from dealer.");
            }

            var fromRows = await _context.PartsInventories
                .Where(stock =>
                    stock.VendorCode == fromLocation.Dealercode &&
                    stock.DealerLocation == fromLocationCode &&
                    stock.FinalStockFlag == "Y" &&
                    partCodes.Contains(stock.ItemCode))
                .ToListAsync(cancellationToken);
            var toRows = await _context.PartsInventories
                .Where(stock =>
                    stock.VendorCode == toLocation.Dealercode &&
                    stock.DealerLocation == toLocationCode &&
                    stock.FinalStockFlag == "Y" &&
                    partCodes.Contains(stock.ItemCode))
                .ToListAsync(cancellationToken);

            var fromStock = fromRows
                .GroupBy(stock => stock.ItemCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Sum(stock => (long)stock.BatchClosingQty), StringComparer.OrdinalIgnoreCase);
            var toStock = toRows
                .GroupBy(stock => stock.ItemCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Sum(stock => (long)stock.BatchClosingQty), StringComparer.OrdinalIgnoreCase);

            var transferBatchId = d2dBillNo;
            var transferDate = DateOnly.FromDateTime(request.Date.UtcDateTime);
            var createdDate = DateTime.Now;
            var responses = new List<PartsD2DLineResponse>(partCodes.Count);

            foreach (var partCode in partCodes)
            {
                var quantity = quantitiesByPart[partCode];
                var sourceOpening = fromStock.TryGetValue(partCode, out var sourceStock) ? sourceStock : 0;
                var targetOpening = toStock.TryGetValue(partCode, out var targetStock) ? targetStock : 0;

                if (sourceOpening < quantity)
                    throw new LocalPartsValidationException(
                        $"Insufficient stock for part '{partCode}'. Available: {sourceOpening}, Transfer: {quantity}.");

                int sourceClosing;
                int targetClosing;
                decimal sourceRate;
                decimal targetRate;
                var targetItemRate = toItemsByPart.TryGetValue(partCode, out var targetItem)
                    ? targetItem.Ipurrate
                    : fromItemsByPart[partCode].Ipurrate;
                try
                {
                    sourceClosing = checked((int)sourceOpening - quantity);
                    targetClosing = checked((int)targetOpening + quantity);
                    sourceRate = checked(fromItemsByPart[partCode].Ipurrate * quantity);
                    targetRate = checked(targetItemRate * quantity);
                }
                catch (OverflowException)
                {
                    throw new LocalPartsValidationException(
                        $"Stock total or amount for part '{partCode}' exceeds the supported range.");
                }

                responses.Add(new PartsD2DLineResponse(
                    partCode,
                    fromItemsByPart[partCode].Itemdesc,
                    quantity,
                    (int)sourceOpening,
                    sourceClosing,
                    (int)targetOpening,
                    targetClosing,
                    fromItemsByPart[partCode].Ipurrate));

                _context.PartsInventories.Add(new PartsInventory
                {
                    TransId = Guid.NewGuid().ToString(),
                    ItemCode = partCode,
                    VoucherNo = prefix,
                    TransType = "TO",
                    BatchNo = transferBatchId,
                    BatchOpeningQty = (int)sourceOpening,
                    BatchTransQty = quantity,
                    BatchClosingQty = sourceClosing,
                    TransDate = transferDate,
                    DealerLocation = fromLocationCode,
                    VendorCode = fromLocation.Dealercode,
                    FinalStockFlag = "Y",
                    TotalRate = sourceRate,
                    PurchaseRate = fromItemsByPart[partCode].Ipurrate,
                    Potype = "D2D-SOURCE",
                    PostTransaction = 0,
                    CreatedBy = createdBy,
                    CreatedDate = createdDate
                });

                _context.PartsInventories.Add(new PartsInventory
                {
                    TransId = Guid.NewGuid().ToString(),
                    ItemCode = partCode,
                    VoucherNo = prefix,
                    TransType = "TI",
                    BatchNo = transferBatchId,
                    BatchOpeningQty = (int)targetOpening,
                    BatchTransQty = quantity,
                    BatchClosingQty = targetClosing,
                    TransDate = transferDate,
                    DealerLocation = toLocationCode,
                    VendorCode = toLocation.Dealercode,
                    FinalStockFlag = "Y",
                    TotalRate = targetRate,
                    PurchaseRate = targetItemRate,
                    Potype = "D2D-TARGET",
                    PostTransaction = 0,
                    CreatedBy = createdBy,
                    CreatedDate = createdDate
                });
            }

            foreach (var row in fromRows)
                row.FinalStockFlag = "N";
            foreach (var row in toRows)
                row.FinalStockFlag = "N";

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new PartsD2DSaveResponse(
                prefix,
                transferBatchId,
                request.Date,
                fromLocationCode,
                toLocationCode,
                responses);
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
