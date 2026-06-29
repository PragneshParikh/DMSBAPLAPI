using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.CounterBillRepo
{
    public class CounterBillRepo : ICounterBillRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IPartInventoryService _partInventoryService;
        public CounterBillRepo(BapldmsvadContext context, IPartInventoryService partInventoryService)
        {
            _context = context;
            _partInventoryService = partInventoryService;
        }

        public async Task<int> SaveCounterBillAsync(CounterBillViewModel model, string userName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = new CounterBillHeader
                {
                    DealerCode = model.Header.DealerCode,
                    BillNo = model.Header.BillNo,
                    MobileNo = model.Header.MobileNo,
                    BillDate = model.Header.BillDate,
                    BillType = model.Header.BillType,
                    LocCode = model.Header.LocCode,
                    CashCreditAcc = model.Header.CashCreditAcc,
                    PartyName = model.Header.PartyName,
                    PartyState = model.Header.PartyState,
                    ChassisNo = model.Header.ChassisNo,
                    BillAmount = model.Header.BillAmount,
                    Remarks = model.Header.Remarks,
                    CustomerLedgerId = model.Header.CustomerLedgerId,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now
                };

                await _context.CounterBillHeaders.AddAsync(header);
                await _context.SaveChangesAsync();

                var details = model.Details.Select(x =>
                    new CounterBillDetail
                    {
                        CounterBillId = header.Id,
                        PartCode = x.PartCode,
                        SaleType = x.SaleType,
                        Qty = x.Qty,
                        Rate = x.Rate,
                        DiscType = x.DiscType,
                        Discount = x.Discount,
                        Mrp = x.Mrp,
                        Igstper = x.Igstper,
                        Igstamnt = x.Igstamnt,
                        Cgstper = x.Cgstper,
                        Cgstamnt = x.Cgstamnt,
                        Sgstper = x.Sgstper,
                        Sgstamnt = x.Sgstamnt,
                        CreatedBy = userName,
                        CreatedDate = DateTime.Now
                    }).ToList();

                await _context.CounterBillDetails.AddRangeAsync(details);

                // GROUP SAME PARTS
                var groupedParts = model.Details.GroupBy(x => x.PartCode).Select(g => new
                {
                    PartCode = g.Key,
                    Qty = (int)g.Sum(x => x.Qty)
                })
                    .ToList();

                // REDUCE STOCK
                foreach (var item in groupedParts)
                {
                    await _partInventoryService.UpdateOutgoing(new PartsInventory
                    {
                        ItemCode = item.PartCode,
                        TransType = "S",
                        BatchTransQty = item.Qty,
                        VendorCode = model.Header.DealerCode,
                        CreatedBy = userName,
                        CreatedDate = DateTime.Now
                    });
                }

                var invoice = new InvoiceHeader
                {
                    InvoiceType = "Proforma Invoice",
                    ServiceType = "Counter Bill",
                    DocumentNo = header.BillNo,
                    ReferenceId = header.Id,
                    CustomerId = header.CustomerLedgerId,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    Status = "Proforma",
                    InvoiceNo = "IN-" + header.BillNo,
                    DealerCode = header.DealerCode,
                    InvoiceDetails = new List<InvoiceDetail>()
                };

                // Invoice Details
                foreach (var detail in details)
                {
                    decimal taxableAmount = detail.Qty * detail.Rate - detail.Discount;

                    decimal taxPercent = detail.Igstper > 0
                        ? detail.Igstper
                        : detail.Cgstper + detail.Sgstper;

                    var invoiceDetail = new InvoiceDetail
                    {
                        ItemId = null, // or Part Id if available
                        Description = detail.PartCode,
                        Quantity = detail.Qty,
                        Rate = detail.Rate,
                        TaxPercent = taxPercent,
                        Amount = taxableAmount
                    };

                    invoice.InvoiceDetails.Add(invoiceDetail);
                }

                // Totals
                invoice.TotalAmount = invoice.InvoiceDetails.Sum(x => x.Amount ?? 0);

                invoice.TaxAmount =
                    invoice.InvoiceDetails.Sum(x =>
                        (x.Amount ?? 0) * (x.TaxPercent ?? 0) / 100);

                invoice.NetAmount = invoice.TotalAmount + invoice.TaxAmount;

                await _context.InvoiceHeaders.AddAsync(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return header.Id;

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> UpdateCounterBillAsync(CounterBillViewModel model, string userName, int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = await _context.CounterBillHeaders
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (header == null)
                    throw new Exception("Counter Bill not found.");

                // Existing Details
                var existingDetails = await _context.CounterBillDetails
                    .Where(x => x.CounterBillId == header.Id)
                    .ToListAsync();

                // Old grouped quantities
                var oldParts = existingDetails
                    .GroupBy(x => x.PartCode)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(x => x.Qty)
                    );

                // New grouped quantities
                var newParts = model.Details
                    .GroupBy(x => x.PartCode)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(x => x.Qty)
                    );

                // Update Header
                header.DealerCode = model.Header.DealerCode;
                header.BillDate = model.Header.BillDate;
                header.BillType = model.Header.BillType;
                header.MobileNo = model.Header.MobileNo;
                header.LocCode = model.Header.LocCode;
                header.CashCreditAcc = model.Header.CashCreditAcc;
                header.PartyName = model.Header.PartyName;
                header.PartyState = model.Header.PartyState;
                header.ChassisNo = model.Header.ChassisNo;
                header.BillAmount = model.Header.BillAmount;
                header.Remarks = model.Header.Remarks;
                header.CustomerLedgerId = model.Header.CustomerLedgerId;
                header.UpdatedBy = userName;
                header.UpdatedDate = DateTime.Now;

                // Compare inventory first
                var allPartCodes = oldParts.Keys
                    .Union(newParts.Keys)
                    .Distinct()
                    .ToList();

                foreach (var partCode in allPartCodes)
                {
                    decimal oldQty = oldParts.ContainsKey(partCode)
                        ? oldParts[partCode]
                        : 0;

                    decimal newQty = newParts.ContainsKey(partCode)
                        ? newParts[partCode]
                        : 0;

                    decimal difference = newQty - oldQty;

                    if (difference == 0)
                        continue;

                    // Stock decrease
                    if (difference > 0)
                    {
                        await _partInventoryService.UpdateOutgoing(
                            new PartsInventory
                            {
                                ItemCode = partCode,
                                TransType = "S",
                                BatchTransQty = (int)difference,
                                VendorCode = model.Header.DealerCode,
                                CreatedBy = userName,
                                CreatedDate = DateTime.Now
                            });
                    }
                    // Stock restore
                    else
                    {
                        await _partInventoryService.UpdateIncoming(
                            new PartsInventory
                            {
                                ItemCode = partCode,
                                TransType = "TI",
                                BatchTransQty = (int)Math.Abs(difference),
                                VendorCode = model.Header.DealerCode,
                                CreatedBy = userName,
                                CreatedDate = DateTime.Now
                            });
                    }
                }

                // Only remove old details after inventory validation succeeds
                _context.CounterBillDetails.RemoveRange(existingDetails);

                // Add updated details
                var newDetails = model.Details.Select(x =>
                    new CounterBillDetail
                    {
                        CounterBillId = header.Id,
                        PartCode = x.PartCode,
                        SaleType = x.SaleType,
                        Qty = x.Qty,
                        Rate = x.Rate,
                        DiscType = x.DiscType,
                        Discount = x.Discount,
                        Mrp = x.Mrp,
                        Igstper = x.Igstper,
                        Igstamnt = x.Igstamnt,
                        Cgstper = x.Cgstper,
                        Cgstamnt = x.Cgstamnt,
                        Sgstper = x.Sgstper,
                        Sgstamnt = x.Sgstamnt,
                        CreatedBy = existingDetails.FirstOrDefault()?.CreatedBy ?? userName,
                        CreatedDate = existingDetails.FirstOrDefault()?.CreatedDate ?? DateTime.Now
                    }).ToList();

                await _context.CounterBillDetails.AddRangeAsync(newDetails);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return header.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CounterBillViewModel> GetCounterBillById(int id)
        {
            try
            {
                var counterBill = await _context.CounterBillHeaders
                    .Include(x => x.CounterBillDetails)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (counterBill == null)
                    throw new Exception("Counter Bill not found.");

                return new CounterBillViewModel
                {
                    Header = new CounterBillHeaderViewModel
                    {
                        Id = counterBill.Id,
                        DealerCode = counterBill.DealerCode,
                        BillNo = counterBill.BillNo,
                        BillDate = counterBill.BillDate,
                        BillType = counterBill.BillType,
                        LocName = _context.LocationMasters.Where(i => i.Loccode == counterBill.LocCode).Select(i => i.Locname).FirstOrDefault(),
                        LocCode = counterBill.LocCode,
                        MobileNo = counterBill.MobileNo,
                        CashCreditAcc = counterBill.CashCreditAcc,
                        PartyName = counterBill.PartyName,
                        PartyState = counterBill.PartyState,
                        ChassisNo = counterBill.ChassisNo,
                        BillAmount = counterBill.BillAmount,
                        Remarks = counterBill.Remarks,
                        CustomerLedgerId = counterBill.CustomerLedgerId
                    },

                    Details = counterBill.CounterBillDetails
                        .Select(x => new CounterBillDetailsViewModel
                        {
                            CounterBillId = x.CounterBillId,
                            PartCode = x.PartCode,
                            PartName = _context.ItemMasters.Where(i => i.Itemcode == x.PartCode).Select(i => i.Itemname).FirstOrDefault(),
                            SaleType = x.SaleType,
                            Qty = x.Qty,
                            Rate = x.Rate,
                            DiscType = x.DiscType,
                            Discount = x.Discount,
                            Mrp = x.Mrp,
                            Igstper = x.Igstper,
                            Igstamnt = x.Igstamnt,
                            Cgstper = x.Cgstper,
                            Cgstamnt = x.Cgstamnt,
                            Sgstper = x.Sgstper,
                            Sgstamnt = x.Sgstamnt
                        })
                        .ToList()
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<CounterBillViewModel>> GetAllCounterBills(string? dealerCode, DateTime? fromDate, DateTime? toDate, string? search, string? dealerFilter)
        {
            try
            {
                var query = _context.CounterBillHeaders
                    .Include(x => x.CounterBillDetails)
                    .Where(x => !x.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    query = query.Where(x => x.DealerCode == dealerCode);
                }
                if (!string.IsNullOrWhiteSpace(dealerFilter))
                {
                    query = query.Where(i => i.DealerCode == dealerFilter);
                }
                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.BillDate >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.BillDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    query = query.Where(x =>
                        x.BillNo.Contains(search) ||
                        x.PartyName.Contains(search) ||
                        x.MobileNo.Contains(search) ||
                        x.ChassisNo.Contains(search) ||
                        x.DealerCode.Contains(search));
                }

                var counterBills = await query
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

                return counterBills.Select(counterBill => new CounterBillViewModel
                {
                    Header = new CounterBillHeaderViewModel
                    {
                        Id = counterBill.Id,
                        DealerCode = counterBill.DealerCode,
                        BillNo = counterBill.BillNo,
                        BillDate = counterBill.BillDate,
                        BillType = counterBill.BillType,
                        LocCode = counterBill.LocCode,
                        LocName = _context.LocationMasters
                            .Where(i => i.Loccode == counterBill.LocCode)
                            .Select(i => i.Locname)
                            .FirstOrDefault(),
                        MobileNo = counterBill.MobileNo,
                        CashCreditAcc = counterBill.CashCreditAcc,
                        PartyName = counterBill.PartyName,
                        PartyState = counterBill.PartyState,
                        ChassisNo = counterBill.ChassisNo,
                        BillAmount = counterBill.BillAmount,
                        Remarks = counterBill.Remarks,
                        CustomerLedgerId = counterBill.CustomerLedgerId,
                        DealerName = _context.DealerMasters.Where(i => i.Dealercode == counterBill.DealerCode).Select(i => i.Compname).FirstOrDefault(),
                    },

                    Details = counterBill.CounterBillDetails
                        .Select(x => new CounterBillDetailsViewModel
                        {
                            CounterBillId = x.CounterBillId,
                            PartCode = x.PartCode,
                            PartName = _context.ItemMasters
                                .Where(i => i.Itemcode == x.PartCode)
                                .Select(i => i.Itemname)
                                .FirstOrDefault(),
                            SaleType = x.SaleType,
                            Qty = x.Qty,
                            Rate = x.Rate,
                            DiscType = x.DiscType,
                            Discount = x.Discount,
                            Mrp = x.Mrp,
                            Igstper = x.Igstper,
                            Igstamnt = x.Igstamnt,
                            Cgstper = x.Cgstper,
                            Cgstamnt = x.Cgstamnt,
                            Sgstper = x.Sgstper,
                            Sgstamnt = x.Sgstamnt
                        })
                        .ToList()
                }).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteCounterBill(int counterBillId, string userName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = await _context.CounterBillHeaders
                    .FirstOrDefaultAsync(x => x.Id == counterBillId && !x.IsDeleted);

                if (header == null)
                    throw new Exception("Counter Bill not found.");

                var details = await _context.CounterBillDetails
                    .Where(x => x.CounterBillId == counterBillId)
                    .ToListAsync();

                // Restore stock
                foreach (var detail in details)
                {
                    await _partInventoryService.UpdateIncoming(
                        new PartsInventory
                        {
                            ItemCode = detail.PartCode,
                            TransType = "TI",
                            BatchTransQty = (int)detail.Qty,
                            VendorCode = header.DealerCode,
                            CreatedBy = userName,
                            CreatedDate = DateTime.Now
                        });
                }

                // Soft delete header
                header.IsDeleted = true;
                header.UpdatedBy = userName;
                header.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CounterBillExcelViewModel>> GetAllCounterBillForExcel(string? dealerCode)
        {
            try
            {
                var query = _context.CounterBillHeaders
                    .Include(x => x.CounterBillDetails)
                    .Where(x => !x.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    query = query.Where(x => x.DealerCode == dealerCode);
                }

                var data = await query
                    .SelectMany(h => h.CounterBillDetails.Select(d =>
                        new CounterBillExcelViewModel
                        {
                            DealerCode = h.DealerCode,
                            DealerName =_context.DealerMasters.Where(i=>i.Dealercode ==h.DealerCode).Select(i=>i.Compname).FirstOrDefault(),
                            BillNo = h.BillNo,
                            BillDate = h.BillDate,
                            BillType = h.BillType,
                            LocCode = h.LocCode,
                            LocationName = _context.LocationMasters.Where(i => i.Loccode == h.LocCode).Select(i => i.Locname).FirstOrDefault(),
                            CashCreditAcc = h.CashCreditAcc,
                            PartyName = h.PartyName,
                            MobileNo = h.MobileNo,
                            Address = h.CustomerLedgerId != null ? _context.LedgerMasters.Where(i => i.Id == h.CustomerLedgerId).Select(i => i.Address).FirstOrDefault() : null,
                            PartyState = _context.States.Where(i => i.StateId == h.PartyState).Select(i => i.StateName).FirstOrDefault(),
                            ChassisNo = h.ChassisNo,
                            BillAmount = h.BillAmount,
                            Remarks = h.Remarks,
                            CustomerLedgerId = h.CustomerLedgerId,
                            PartDescription = _context.ItemMasters.Where(i => i.Itemcode == d.PartCode).Select(i => i.Itemdesc).FirstOrDefault(),
                            PartCode = d.PartCode,
                            SaleType = d.SaleType,
                            Qty = d.Qty,
                            Rate = d.Rate,
                            DiscType = d.DiscType,
                            Discount = d.Discount,
                            Mrp = d.Mrp,
                            Igstper = d.Igstper,
                            Igstamnt = d.Igstamnt,
                            Cgstper = d.Cgstper,
                            Cgstamnt = d.Cgstamnt,
                            Sgstper = d.Sgstper,
                            Sgstamnt = d.Sgstamnt
                        }))
                    .ToListAsync();

                return data;
            }
            catch
            {
                throw;
            }
        }

    }
}
