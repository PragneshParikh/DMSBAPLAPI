using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;

namespace DMS_BAPL_Data.Repositories.PurchaseOrderRepo
{
    public class PurchaseOrderRepo : IPurchaseOrderRepo
    {
        private readonly BapldmsvadContext _context;
        private IDbContextTransaction? _transaction;

        public PurchaseOrderRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        // Transaction Handling
        public async Task BeginTransactionAsync()
        {
            try
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                    await _transaction.CommitAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                    await _transaction.RollbackAsync();
            }
            catch
            {
                throw;
            }
        }

        // Insert PO
        public async Task AddPOAsync(PurchaseOrder po)
        {
            try
            {
                await _context.PurchaseOrders.AddAsync(po);
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        // Insert Tax
        public async Task AddTaxAsync(TaxDetail tax)
        {
            try
            {
                await _context.TaxDetails.AddAsync(tax);
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        // Update Amount
        public async Task UpdatePOHeaderAsync(PurchaseOrder po)
        {
            try
            {
                var existing = await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x => x.Ponumber == po.Ponumber);

                if (existing == null)
                    throw new Exception(StringConstants.PONotFound);

                existing.CustomerCode = po.CustomerCode;
                existing.PurchaseDate = po.PurchaseDate;
                existing.OrderType = po.OrderType;
                existing.TransactionType = po.TransactionType;
                existing.Remarks = po.Remarks;
                existing.LocCode = po.LocCode;
                existing.ConsigneeCode = po.ConsigneeCode;
                existing.LedgerCode = po.LedgerCode;
                existing.SubOrderType = po.SubOrderType;
                existing.UpdatedBy = po.UpdatedBy;
                existing.UpdatedDate = po.UpdatedDate;

                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdatePOAmountAsync(string poNumber, decimal amount)
        {
            try
            {
                var po = await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x => x.Ponumber == poNumber);

                if (po == null)
                    throw new Exception(StringConstants.PONotFound);

                po.Amount = amount;
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        // Get Item
        public async Task<ItemMaster> GetItemAsync(string itemCode)
        {
            try
            {
                return await _context.ItemMasters
                    .FirstOrDefaultAsync(x => x.Itemcode == itemCode)
                    ?? throw new Exception(StringConstants.ItemNotFound);
            }
            catch
            {
                throw;
            }
        }

        // Get HSN
        public async Task<HsncodeMaster> GetHSNByCodeAsync(string? hsnCode)
        {
            try
            {
                return await _context.HsncodeMasters
                    .FirstOrDefaultAsync(x => x.Hsncode == hsnCode)
                    ?? throw new Exception(StringConstants.HSNNotFound);
            }
            catch
            {
                throw;
            }
        }

        // Get ParameterValue of Subsidy
        public async Task<decimal> GetSubsidyValue()
        {
            var param = await _context.ParameterMasterTables
                .FirstOrDefaultAsync(x => x.ParameterName == StringConstants.SubsidyParam);

            if (param == null)
                throw new Exception(StringConstants.SubsidyParameterNotFound);

            return param.ParameterValue;
        }
        //Add PO Details
        public async Task AddPODetailAsync(PurchaseOrderDetail detail)
        {
            try
            {
                await _context.PurchaseOrderDetails.AddAsync(detail);
            }
            catch
            {
                throw;
            }
        }

        // Get HSN Tax Mapping
        public async Task<HsnwiseTaxCode> GetHSNTaxAsync(string hsnCode)
        {
            try
            {
                return await _context.HsnwiseTaxCodes
                    .Where(x => x.Hsncode == hsnCode)
                    .OrderByDescending(x => x.EffectiveDate)
                    .FirstOrDefaultAsync()
                    ?? throw new Exception(StringConstants.HSNTaxMapMissing);
            }
            catch
            {
                throw;
            }
        }

        // Get Aggregate Taxes
        public async Task<List<AggregateTaxCode>> GetAggregateTaxesAsync(string aTaxCode)
        {
            try
            {
                return await _context.AggregateTaxCodes
                    .Where(x => x.AtaxCode == aTaxCode)
                    .OrderBy(x => x.SrNo)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        // Get Tax Master
        public async Task<TaxCodeMaster> GetTaxMasterAsync(string taxCode)
        {
            try
            {
                return await _context.TaxCodeMasters
                    .Where(x => x.TaxCode == taxCode)
                    .OrderByDescending(x => x.EffectiveDate)
                    .FirstOrDefaultAsync()
                    ?? throw new Exception(StringConstants.TaxCodeNotFound);
            }
            catch
            {
                throw;
            }
        }

        public async Task<HsnwiseTaxCode> GetHSNTaxWithFallbackAsync(string hsnCode, string preferredFlag, DateTime poDate)
        {
            try
            {
                var result = await _context.HsnwiseTaxCodes
                    .Where(x => x.Hsncode == hsnCode && x.EffectiveDate.Date <= poDate.Date)
                    .OrderByDescending(x => x.StateFlag == preferredFlag)
                    .ThenByDescending(x => x.EffectiveDate)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                if (result == null)
                    throw new Exception(StringConstants.PONotFound);

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<PurchaseOrderResponseViewModel> GetPOByNumberAsync(string poNumber)
        {
            try
            {
                var po = await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x => x.Ponumber == poNumber);

                if (po == null)
                    return null;

                var details = await _context.PurchaseOrderDetails
                    .Where(x => x.Ponumber == poNumber)
                    .ToListAsync();

                var taxes = await _context.TaxDetails
                    .Where(x => x.Ponumber == poNumber)
                    .ToListAsync();

                // Get all item codes
                var itemCodes = details
                    .Select(x => x.ItemCode)
                    .Distinct()
                    .ToList();

                // Fetch all items
                var items = await _context.ItemMasters
                    .Where(x => itemCodes.Contains(x.Itemcode))
                    .ToListAsync();

                return new PurchaseOrderResponseViewModel
                {
                    Id = po.Id,
                    PONumber = po.Ponumber,
                    PODate = po.PurchaseDate,
                    CustomerCode = po.CustomerCode,
                    TotalAmount = po.Amount,
                    IsSubmitted = po.Status,
                    TransactionType = po.TransactionType,
                    Remarks = po.Remarks,
                    LocCode = po.LocCode,
                    SubOrderType = po.SubOrderType,
                    LedgerCode = po.LedgerCode,
                    IsAgainstKit = po.IsAgainstKit,
                    LocationName = _context.LocationMasters
        .FirstOrDefault(l => l.Loccode == po.LocCode)?.Locname,

                    Items = details.Select(d =>
                    {
                        var item = items.FirstOrDefault(i => i.Itemcode == d.ItemCode);

                        return new PurchaseOrderItemViewModel
                        {
                            ItemCode = d.ItemCode,
                            Qty = d.Qty,
                            Rate = d.Rate,
                            MRP = d.Mrp,
                            LineAmount = d.LineAmount,
                            Subsidy = d.Subsidy,
                            ItemDescription = item?.Itemdesc,
                            LineNumber = d.LineNumber,

                            Taxes = taxes
                                .Where(t => t.ItemCode == d.ItemCode &&
                                            t.PodetailsLineNumber == d.LineNumber)
                                .Select(t => new TaxViewModel
                                {
                                    TaxCode = t.TaxCode,
                                    TaxRate = t.TaxRate,
                                    TaxAmount = t.TaxAmount
                                }).ToList()
                        };
                    }).ToList()
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResponse<PurchaseOrderResponseViewModel>> GetPOListAsync(string? dealerCode, string orderType, int pageIndex, int pageSize, PurchaseOrderSearchViewModel purchaseOrderSearchViewModel)
        {
            IQueryable<PurchaseOrder> query = _context.PurchaseOrders
                .AsNoTracking()
                .Where(x => x.OrderType == orderType);

            if (!string.IsNullOrWhiteSpace(dealerCode))
            {
                query = query.Where(x => x.CustomerCode == dealerCode);
            }

            if (purchaseOrderSearchViewModel.DateFrom.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= purchaseOrderSearchViewModel.DateFrom.Value);
            }

            if (purchaseOrderSearchViewModel.DateTo.HasValue)
            {
                var endDate = purchaseOrderSearchViewModel.DateTo.Value.Date.AddDays(1);

                query = query.Where(x => x.CreatedDate < endDate);
            }

            if (!string.IsNullOrEmpty(purchaseOrderSearchViewModel.IsSubmitted))
            {
                bool status = purchaseOrderSearchViewModel.IsSubmitted == "Submited To ERP" ? true : false;
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrEmpty(purchaseOrderSearchViewModel.PurchaseNo))
            {
                query = query.Where(x => x.Ponumber == purchaseOrderSearchViewModel.PurchaseNo);
            }

            var poList = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!poList.Any())
                return new PagedResponse<PurchaseOrderResponseViewModel>();

            var poNumbers = poList
                .Select(x => x.Ponumber)
                .ToList();

            var details = await _context.PurchaseOrderDetails
                .AsNoTracking()
                .Where(x => poNumbers.Contains(x.Ponumber))
                .ToListAsync();

            var taxes = await _context.TaxDetails
                .AsNoTracking()
                .Where(x => poNumbers.Contains(x.Ponumber))
                .ToListAsync();

            var ledgerMasters = await _context.LedgerMasters
                .AsNoTracking()
                .ToListAsync();

            var locationMasters = await _context.LocationMasters
                .AsNoTracking()
                .ToListAsync();

            var result = poList.Select(po =>
            {
                var poDetails = details
                    .Where(x => x.Ponumber == po.Ponumber)
                    .ToList();

                return new PurchaseOrderResponseViewModel
                {
                    PONumber = po.Ponumber,
                    PODate = po.PurchaseDate,
                    CustomerCode = po.CustomerCode,
                    TotalAmount = po.Amount.GetValueOrDefault(),
                    IsSubmitted = po.Status,
                    TransactionType = po.TransactionType,
                    Remarks = po.Remarks,
                    LocCode = po.LocCode,
                    LedgerCode = po.LedgerCode,

                    LedgerName = ledgerMasters
                        .FirstOrDefault(x => x.LedgerCode == po.LedgerCode)
                        ?.LedgerName,

                    LocationName = locationMasters
                        .FirstOrDefault(x => x.Loccode == po.LocCode)
                        ?.Locname,

                    Items = poDetails.Select(d => new PurchaseOrderItemViewModel
                    {
                        ItemCode = d.ItemCode,
                        Qty = d.Qty,
                        Rate = d.Rate.GetValueOrDefault(),
                        MRP = d.Mrp.GetValueOrDefault(),
                        LineAmount = d.LineAmount.GetValueOrDefault(),
                        Taxes = taxes
                        .Where(t => t.Ponumber == po.Ponumber &&
                                    t.ItemCode == d.ItemCode &&
                                    t.PodetailsLineNumber == d.LineNumber)
                        .Select(t => new TaxViewModel
                        {
                            TaxCode = t.TaxCode,
                            TaxRate = t.TaxRate,
                            TaxAmount = t.TaxAmount
                        }).ToList()

                    }).ToList()
                };
            }).ToList();

            int totalRecords = await query.CountAsync();

            return new PagedResponse<PurchaseOrderResponseViewModel>
            {
                Data = result,
                TotalRecords = totalRecords
            };
        }
        public async Task<List<PurchaseOrderDetail>> GetPODetails(string poNumber)
        {
            return await _context.PurchaseOrderDetails
                .Where(x => x.Ponumber == poNumber)
                .ToListAsync();
        }

        public async Task DeletePODetailAsync(PurchaseOrderDetail detail)
        {
            _context.PurchaseOrderDetails.Remove(detail);
        }

        public async Task DeleteTaxByItemAsync(string poNumber, string itemCode)
        {
            var taxes = await _context.TaxDetails
                .Where(x => x.Ponumber == poNumber && x.ItemCode == itemCode)
                .ToListAsync();

            _context.TaxDetails.RemoveRange(taxes);
        }

        public async Task DeleteTaxesByPOAsync(string poNumber)
        {
            var taxes = await _context.TaxDetails
                .Where(x => x.Ponumber == poNumber)
                .ToListAsync();
            _context.TaxDetails.RemoveRange(taxes);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDetailsByPOAsync(string poNumber)
        {
            var details = await _context.PurchaseOrderDetails
                .Where(x => x.Ponumber == poNumber)
                .ToListAsync();
            _context.PurchaseOrderDetails.RemoveRange(details);
            await _context.SaveChangesAsync();
        }

        public async Task<PartsPurchaseOrderResponseViewModel> GetPartsPOByNumberAsync(string poNumber)
        {
            try
            {
                var po = await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x => x.Ponumber == poNumber);

                if (po == null)
                    return null;

                var details = await _context.PurchaseOrderDetails
                    .Where(x => x.Ponumber == poNumber)
                    .ToListAsync();

                var taxes = await _context.TaxDetails
                    .Where(x => x.Ponumber == poNumber)
                    .ToListAsync();

                return new PartsPurchaseOrderResponseViewModel
                {
                    PONumber = po.Ponumber,
                    PODate = po.PurchaseDate,
                    CustomerCode = po.CustomerCode,
                    TotalAmount = po.Amount,
                    IsSubmitted = po.Status,
                    TransactionType = po.TransactionType,

                    Items = details.Select(d => new PartsPurchaseOrderItemViewModel
                    {
                        ItemCode = d.ItemCode,
                        Qty = d.Qty,
                        Rate = d.Rate,
                        LineAmount = d.LineAmount,
                        Subsidy = d.Subsidy,

                        Taxes = taxes
                            .Where(t => t.ItemCode == d.ItemCode &&
                                        t.PodetailsLineNumber == d.LineNumber)
                            .Select(t => new PartsTaxViewModel
                            {
                                TaxCode = t.TaxCode,
                                TaxRate = t.TaxRate,
                                TaxAmount = t.TaxAmount
                            }).ToList()
                    }).ToList()
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<PartsPurchaseOrderResponseViewModel>> GetPartsPOListAsync()
        {
            try
            {
                // Note: We might want a filter here for Parts POs specifically.
                // For now, returning all but mapped to Parts ViewModel as requested for separation.
                var poList = await _context.PurchaseOrders.ToListAsync();

                if (poList == null || !poList.Any())
                    return new List<PartsPurchaseOrderResponseViewModel>();

                var resultList = new List<PartsPurchaseOrderResponseViewModel>();

                foreach (var po in poList)
                {
                    var details = await _context.PurchaseOrderDetails
                        .Where(x => x.Ponumber == po.Ponumber)
                        .ToListAsync();

                    var taxes = await _context.TaxDetails
                        .Where(x => x.Ponumber == po.Ponumber)
                        .ToListAsync();

                    resultList.Add(new PartsPurchaseOrderResponseViewModel
                    {
                        PONumber = po.Ponumber,
                        PODate = po.PurchaseDate,
                        CustomerCode = po.CustomerCode,
                        TotalAmount = po.Amount.GetValueOrDefault(),
                        IsSubmitted = po.Status,
                        TransactionType = po.TransactionType,

                        Items = details.Select(d => new PartsPurchaseOrderItemViewModel
                        {
                            ItemCode = d.ItemCode,
                            Qty = d.Qty,
                            Rate = d.Rate.GetValueOrDefault(),
                            LineAmount = d.LineAmount.GetValueOrDefault(),

                            Taxes = taxes
                                .Where(t => t.ItemCode == d.ItemCode &&
                                            t.PodetailsLineNumber == d.LineNumber)
                                .Select(t => new PartsTaxViewModel
                                {
                                    TaxCode = t.TaxCode,
                                    TaxRate = t.TaxRate,
                                    TaxAmount = t.TaxAmount
                                }).ToList()
                        }).ToList()
                    });
                }

                return resultList;
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateStatus(string PoNumber)
        {
            var result = await _context.PurchaseOrders.Where(i => i.Ponumber == PoNumber).FirstOrDefaultAsync();
            result.Status = true;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdatePOStatusAsync(UpdatePOStatusViewModel updatePOStatusViewModel)
        {
            try
            {
                var po = await _context.PurchaseOrders
                    .FirstOrDefaultAsync(x => x.Ponumber == updatePOStatusViewModel.PONumber);

                if (po == null)
                    throw new Exception(StringConstants.PONotFound);

                po.Status = updatePOStatusViewModel.Status;
                po.ReferenceNo = updatePOStatusViewModel.SaleOrderNo;
                po.ConsigneeCode = updatePOStatusViewModel.ConsigneeCode;

                await _context.SaveChangesAsync();
                return true;
            }
            catch { throw; }
        }
        public async Task<object> GetOrderDetailsByItemCode(string itemCode, string dealerCode)
        {
            var result = await (from PO in _context.PurchaseOrders

                                join PD in _context.PurchaseOrderDetails
                                on PO.Ponumber equals PD.Ponumber

                                join L in _context.LedgerMasters
                                on PO.LedgerCode equals L.LedgerCode

                                where PO.CustomerCode == dealerCode
                                   && PD.ItemCode == itemCode

                                let gstAmount = _context.TaxDetails
                                .Where(TD => TD.Ponumber == PO.Ponumber && TD.ItemCode == PD.ItemCode)
                                .Sum(TD => (decimal?)TD.TaxAmount) ?? 0

                                select new
                                {
                                    PONumber = PO.Ponumber,
                                    PO.PurchaseDate,

                                    L.LedgerName,

                                    Rate = PD.Rate - gstAmount,
                                    Amount = PD.Rate,

                                    PD.Qty,

                                    GST = gstAmount
                                })
                                .GroupBy(x => x.Rate)
                                .Select(g => g.First())
                                .ToListAsync();

            return result;
        }

        public async Task<TaxCodeMaster> GetTaxMasterAsync(string taxCode, DateTime effectiveAsOf)
        {
            try
            {
                return await _context.TaxCodeMasters
                    .Where(x => x.TaxCode == taxCode
                             && x.EffectiveDate.HasValue
                             && x.EffectiveDate.Value.Date <= effectiveAsOf.Date)
                    .OrderByDescending(x => x.EffectiveDate)
                    .FirstOrDefaultAsync()
                    ?? throw new Exception(StringConstants.TaxCodeNotFound);
            }
            catch
            {
                throw;
            }
        }

        // --- ERP Purchase Order two-way sync ---------------------------------
        // Builds the poHeader/poLine payload the ERP expects for its PO creation
        // endpoint. Field names are locked to the ERP's own contract (e.g.
        // "Ref_No", lower-case "descriptions") - see ErpPurchaseOrderViewModel.cs
        // for what's confirmed vs. assumed. The actual POST + retry + APITracking
        // logging lives in the controller, same split as the warranty ERP flow.
        public async Task<ErpPurchaseOrderRequest> BuildErpPurchaseOrderPayload(string poNumber)
        {
            var po = await _context.PurchaseOrders
                .FirstOrDefaultAsync(x => x.Ponumber == poNumber);

            if (po == null)
                throw new Exception(StringConstants.PONotFound);

            var details = await _context.PurchaseOrderDetails
                .Where(x => x.Ponumber == poNumber)
                .ToListAsync();

            var taxes = await _context.TaxDetails
                .Where(x => x.Ponumber == poNumber)
                .ToListAsync();

            var itemCodes = details.Select(d => d.ItemCode).Distinct().ToList();
            var items = await _context.ItemMasters
                .Where(i => itemCodes.Contains(i.Itemcode))
                .ToListAsync();

            var request = new ErpPurchaseOrderRequest
            {
                PoHeader = new ErpPoHeaderViewModel
                {
                    // "SupplierCode" is the ERP's own field name, but it carries
                    // the dealer/customer code (matches the CUS-prefixed sample
                    // value) - same value used as CustomerCode elsewhere in this repo.
                    SupplierCode = po.CustomerCode ?? "",
                    // DMS's own PO number, sent as the reference the ERP correlates
                    // its newly issued PO No/Date back to.
                    // UNCONFIRMED - swap to po.Id.ToString() if the ERP expects that instead.
                    RefNo = po.Ponumber ?? "",
                    Remark = po.Remarks ?? "",
                    Amount = (po.Amount ?? 0).ToString("0.00")
                }
            };

            foreach (var d in details)
            {
                var item = items.FirstOrDefault(i => i.Itemcode == d.ItemCode);

                var lineTax = taxes
                    .Where(t => t.ItemCode == d.ItemCode && t.PodetailsLineNumber == d.LineNumber)
                    .Sum(t => (decimal?)t.TaxAmount) ?? 0;

                // Assessable value = the line total with tax stripped back out,
                // same "subtract tax to get the taxable value" approach
                // GetOrderDetailsByItemCode already uses elsewhere in this repo.
                // NOTE: if PurchaseOrderDetail.Qty/LineAmount are non-nullable in
                // your actual model, drop the .GetValueOrDefault() calls below.
                decimal lineAmount = d.LineAmount.GetValueOrDefault();
                decimal qty = d.Qty.GetValueOrDefault();
                decimal assessableValue = lineAmount - lineTax;
                decimal unitRate = qty > 0 ? Math.Round(assessableValue / qty, 2) : assessableValue;

                request.PoLine.Add(new ErpPoLineViewModel
                {
                    // ERP calls this "ItemName" but the sample value
                    // ("22DX340010AS") is clearly an item code, not a name.
                    ItemName = d.ItemCode ?? "",
                    Descriptions = item?.Itemdesc ?? item?.Itemname ?? item?.Displayname ?? "",
                    // Confirmed real field (PurchaseOrderDetail.Unit) - no
                    // longer a hardcoded guess. Falls back to "NOS" only
                    // when the detail row itself has no unit recorded.
                    Unit = string.IsNullOrWhiteSpace(d.Unit) ? "NOS" : d.Unit,
                    Qty = qty.ToString("0.##"),
                    Rate = unitRate.ToString("0.00"),
                    AssValue = assessableValue.ToString("0.00")
                });
            }

            return request;
        }

        // Persists what the ERP hands back for this PO after a successful
        // two-way POST (see PostPurchaseOrderToErpAsync in the controller).
        public async Task SaveErpPurchaseOrderResultAsync(string poNumber, string? erpPoNumber, DateTime? erpPoDate)
        {
            var po = await _context.PurchaseOrders
                .FirstOrDefaultAsync(x => x.Ponumber == poNumber);

            if (po == null)
                throw new Exception(StringConstants.PONotFound);

            po.ErpPoNumber = erpPoNumber;
            po.ErpPoDate = erpPoDate;
            po.ErpSubmittedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        // Shared APITracking logger - same INSERT pattern WarrantyInvoiceController
        // already uses, added here because PurchaseOrderService doesn't have
        // direct DbContext access. PurchaseOrderService.SendToERP previously had
        // NO logging at all, which meant there was no way to retroactively check
        // what the ERP actually returned for a PO submission.
        // NOTE: no ILogger is injected into this repo, so a logging failure here
        // is silently swallowed rather than reported - add ILogger<PurchaseOrderRepo>
        // if you want that surfaced.
        public async Task LogApiTrackingAsync(string endpoint, string? payload, string? status, string? response)
        {
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO APITracking (endpoint, dateofhit, payload, status, response)
            VALUES ({endpoint}, {DateTime.Now}, {payload}, {status}, {response})");
            }
            catch
            {
                // Swallowed intentionally - a logging failure shouldn't break the ERP call itself.
            }
        }
    }


}