using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
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
                existing.LedgerCode = po.LedgerCode;
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
                    .Where(x => x.Hsncode == hsnCode && x.EffectiveDate <= poDate)
                    .OrderByDescending(x => x.StateFlag == preferredFlag)
                    .ThenByDescending(x => x.EffectiveDate)
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

                return new PurchaseOrderResponseViewModel
                {
                    PONumber = po.Ponumber,
                    PODate = po.PurchaseDate,
                    CustomerCode = po.CustomerCode,
                    TotalAmount = po.Amount,
                    IsSubmitted = po.Status,
                    TransactionType = po.TransactionType,
                    Remarks = po.Remarks,
                    LocCode = po.LocCode,
                    LocationName = _context.LocationMasters.FirstOrDefault(l => l.Loccode == po.LocCode)?.Locname,

                    Items = details.Select(d => new PurchaseOrderItemViewModel
                    {
                        ItemCode = d.ItemCode,
                        Qty = d.Qty,
                        Rate = d.Rate,
                        LineAmount = d.LineAmount,
                        Subsidy = d.Subsidy,

                        Taxes = taxes
                            .Where(t => t.ItemCode == d.ItemCode &&
                                        t.PodetailsLineNumber == d.LineNumber)
                            .Select(t => new TaxViewModel
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

        public async Task<List<PurchaseOrderResponseViewModel>> GetPOListAsync()
        {
            try
            {
                var poList = await _context.PurchaseOrders.ToListAsync();

                if (poList == null || !poList.Any())
                    return new List<PurchaseOrderResponseViewModel>();

                var resultList = new List<PurchaseOrderResponseViewModel>();

                foreach (var po in poList)
                {
                    var details = await _context.PurchaseOrderDetails
                        .Where(x => x.Ponumber == po.Ponumber)
                        .ToListAsync();

                    var taxes = await _context.TaxDetails
                        .Where(x => x.Ponumber == po.Ponumber)
                        .ToListAsync();

                    resultList.Add(new PurchaseOrderResponseViewModel
                    {
                        PONumber = po.Ponumber,
                        PODate = po.PurchaseDate,
                        CustomerCode = po.CustomerCode,
                        TotalAmount = po.Amount.GetValueOrDefault(),
                        IsSubmitted = po.Status,
                        TransactionType = po.TransactionType,
                        Remarks = po.Remarks,
                        LocCode = po.LocCode,
                        LocationName = _context.LocationMasters.FirstOrDefault(l => l.Loccode == po.LocCode)?.Locname,

                        Items = details.Select(d => new PurchaseOrderItemViewModel
                        {
                            ItemCode = d.ItemCode,
                            Qty = d.Qty,
                            Rate = d.Rate.GetValueOrDefault(),
                            LineAmount = d.LineAmount.GetValueOrDefault(),

                            Taxes = taxes
                                .Where(t => t.ItemCode == d.ItemCode &&
                                            t.PodetailsLineNumber == d.LineNumber)
                                .Select(t => new TaxViewModel
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
            var result = await _context.PurchaseOrders.Where(i=>i.Ponumber == PoNumber).FirstOrDefaultAsync();
            result.Status = true;
            await _context.SaveChangesAsync();
        }

    }
}