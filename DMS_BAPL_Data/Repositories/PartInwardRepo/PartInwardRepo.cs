using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PartInwardRepo
{
    public partial class PartInwardRepo : IPartInwardRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IPartInventoryService _partInventoryService;
        public PartInwardRepo(BapldmsvadContext context, IPartInventoryService partInventoryService)
        {
            _context = context;
            _partInventoryService = partInventoryService;
        }

        async Task<IEnumerable<PartsInward>> IPartInwardRepo.Get()
        {
            return await Task.FromResult(_context.PartsInwards.ToList());
        }

        async Task<IEnumerable<PartsInward>> IPartInwardRepo.GetPartInwardByDealerAsync(string dealerCode)
        {
            return await Task.FromResult(_context.PartsInwards.Where(p => p.DealerCode == dealerCode && p.IsAccepted == false).ToList());
        }

        async Task<bool> IPartInwardRepo.UpdateByInvoice(PartsInwardDetailsViewModel partsInwardDetailsViewModel)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var partsListByInvoice = await _context.PartsInwards
                    .Where(x => x.InvoiceNo == partsInwardDetailsViewModel.InvoiceNo)
                    .AsNoTracking()
                    .ToListAsync();

                if (!partsListByInvoice.Any())
                    return false;

                var inventoryList = partsListByInvoice
                    .GroupBy(x => x.PartNo)
                    .Select(g => new PartsInventory
                    {
                        TransId = Guid.NewGuid().ToString(),
                        ItemCode = g.Key,

                        VoucherNo = null!,
                        TransType = "P",
                        BatchNo = "Batch 1",

                        // total qty by part
                        BatchTransQty = g.Sum(x => x.ItemQty),

                        BatchOpeningQty = 0,
                        BatchClosingQty = 0,

                        TransDate = DateOnly.FromDateTime(DateTime.Now),

                        DealerLocation = g.First().LocCode,
                        VendorCode = g.First().DealerCode,

                        // optional calculations
                        TotalRate = g.Sum(x => x.ItemRate * x.ItemQty),
                        PurchaseRate = g.First().ItemRate,

                        Potype = "B2C",
                        PostTransaction = 0,

                        CreatedBy = partsInwardDetailsViewModel.UpdatedBy,
                        CreatedDate = partsInwardDetailsViewModel.UpdatedDate
                    })
                    .ToList();

                foreach (var vehicle in inventoryList)
                {
                    await _partInventoryService.UpdateIncoming(vehicle);
                }

                var affectedRows = await _context.PartsInwards
                        .Where(x => x.InvoiceNo == partsInwardDetailsViewModel.InvoiceNo)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.IsAccepted, true)
                            .SetProperty(x => x.ReceiptDate, partsInwardDetailsViewModel.ReceiptDate)
                            .SetProperty(x => x.PrefixNo, partsInwardDetailsViewModel.PrefixNo)
                            .SetProperty(x => x.DocumentNo, partsInwardDetailsViewModel.DocumentNo)
                            .SetProperty(x => x.PartyName, partsInwardDetailsViewModel.PartyCode)
                            .SetProperty(x => x.SourceType, partsInwardDetailsViewModel.SourceType)
                            .SetProperty(x => x.UpdatedBy, partsInwardDetailsViewModel.UpdatedBy)
                            .SetProperty(x => x.UpdatedDate, partsInwardDetailsViewModel.UpdatedDate)
                        );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return affectedRows > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        async Task<object> IPartInwardRepo.PartsInward(PartsInwardViewModel partsInwardViewModel)
        {
            var exist = await _context.PartsInwards
                .FirstOrDefaultAsync(x =>
                    x.InvoiceNo == partsInwardViewModel.invoice_no &&
                    x.PartNo == partsInwardViewModel.part_no);

            if (exist != null)
            {
                return new
                {
                    Success = false,
                    Message = "Part No and Invoice No already exist. Duplicate entry."
                };
            }

            var entity = new PartsInward
            {
                DealerCode = partsInwardViewModel.dealer_code,
                LocCode = partsInwardViewModel.loc_code,
                InvoiceNo = partsInwardViewModel.invoice_no,
                PartNo = partsInwardViewModel.part_no,
                ItemQty = partsInwardViewModel.item_qty,
                ItemRate = partsInwardViewModel.item_rate,
                IsAccepted = false,
                InvoiceDate = DateTime.ParseExact(partsInwardViewModel.invoice_date, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                ItemIdno = partsInwardViewModel.item_idno,
                ItemHsncode = partsInwardViewModel.item_hsncode,
                ItemMrp = partsInwardViewModel.item_mrp,
                Sgst = partsInwardViewModel.sgst,
                Cgst = partsInwardViewModel.cgst,
                Igst = partsInwardViewModel.igst,
                ItemDisc = partsInwardViewModel.item_disc,
                DiscountType = partsInwardViewModel.discount_type
            };

            await _context.PartsInwards.AddAsync(entity);

            var result = await _context.SaveChangesAsync();

            return new
            {
                Success = result > 0,
                Message = result > 0 ? "Part inward saved successfully." : "Failed to save part inward."
            };
        }

        async Task<IEnumerable<PartsInward>> IPartInwardRepo.GetPendingPartInwardDetailByLocation(string locationCode)
        {
            return await _context.PartsInwards
                .AsNoTracking()
                .Where(x => x.LocCode == locationCode && x.IsAccepted == false)
                .ToListAsync();
        }

        async Task<object> IPartInwardRepo.GetInwardPartDetailsByInvoiceNo(string invoiceNo)
        {
            var partInwards = await (from pi in _context.PartsInwards
                                     join im in _context.ItemMasters
                                        on pi.PartNo equals im.Itemcode
                                     where pi.InvoiceNo == invoiceNo
                                     //&& pi.IsAccepted == false
                                     select new
                                     {
                                         pi.InvoiceDate,
                                         pi.InvoiceNo,
                                         pi.PartNo,
                                         pi.ItemHsncode,
                                         pi.ItemRate,
                                         pi.ItemMrp,
                                         pi.ItemQty,
                                         pi.Sgst,
                                         pi.Cgst,
                                         pi.Igst,
                                         pi.ItemDisc,
                                         pi.DiscountType,
                                         pi.LocCode,
                                         pi.DealerCode,
                                         pi.IsAccepted,
                                         pi.DocumentNo,
                                         pi.ReceiptDate,

                                         im.Itemtype,
                                         im.Itemname,
                                         im.Itemdesc,
                                     })
                                     .ToListAsync();

            if (!partInwards.Any())
                return null;

            var dealerCode = partInwards.First().DealerCode;

            var sequence = await _context.NumberSequences
                .FirstOrDefaultAsync(x =>
                    x.DealerCode == dealerCode &&
                    x.SequenceName == "part_inward");

            if (sequence == null)
                return null;

            int digitCount = sequence.SequenceCode.Count(c => c == '#');
            string formattedNo = sequence.NextNo.ToString().PadLeft(digitCount, '0');
            string prefixNo = sequence.SequenceCode.Replace(new string('#', digitCount), formattedNo);

            return new
            {
                IsAccepted = partInwards.First().IsAccepted,
                DocumentNo = partInwards.First().DocumentNo,
                PrefixNo = prefixNo,
                InvoiceDate = partInwards.First().InvoiceDate,
                LocationCode = partInwards.First().LocCode,
                InvoiceNo = partInwards.First().InvoiceNo,
                PartInwards = partInwards
            };
        }

        async Task<object> IPartInwardRepo.GetPartsInwardDetailsByDealer(int pageIndex, int pageSize, DateTime fromDate, DateTime toDate, string? dealerCode)
        {
            var query = from p in _context.PartsInwards
                        join l in _context.LedgerMasters
                            on p.PartyName equals l.LedgerCode into ledgerGroup
                        from l in ledgerGroup.DefaultIfEmpty()

                        join lm in _context.LocationMasters
                            on p.LocCode equals lm.Loccode into locationGroup
                        from lm in locationGroup.DefaultIfEmpty()
                        select new
                        {
                            Part = p,
                            PartyName = l.LedgerName,
                            lm.Locname
                        };

            if (!string.IsNullOrWhiteSpace(dealerCode))
            {
                query = query.Where(x => x.Part.DealerCode == dealerCode);
            }

            query = query.Where(x => x.Part.InvoiceDate >= fromDate.Date && x.Part.InvoiceDate <= toDate.Date);

            var result = await query
                .GroupBy(x => new
                {
                    x.Part.InvoiceNo,
                    x.Part.InvoiceDate,
                    x.Part.DealerCode,
                    x.PartyName,
                })
                .Select(g => new
                {
                    g.Key.InvoiceNo,
                    g.Key.InvoiceDate,
                    g.Key.DealerCode,
                    g.Key.PartyName,

                    LocationCode = g.First().Part.LocCode,
                    LocationName = g.First().Locname,

                    TotalQty = g.Sum(x => x.Part.ItemQty),
                    TotalAmount = g.Sum(x => x.Part.ItemQty * x.Part.ItemRate),

                    TotalItems = g.Count(),

                    CreatedDate = g.Max(x => x.Part.CreatedDate),
                    CreatedBy = g.Max(x => x.Part.CreatedBy),

                    IsAccepted = g.All(x => x.Part.IsAccepted.Value == true) ? "Received" : "Pending",

                    GST = g.First().Part.Igst == 0m
                        ? g.First().Part.Cgst + g.First().Part.Sgst
                        : g.First().Part.Igst
                })
                .OrderBy(x => x.InvoiceDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = result.Select((x, index) => new
            {
                SrNo = ((pageIndex - 1) * pageSize) + index + 1,

                x.InvoiceNo,
                x.InvoiceDate,
                x.DealerCode,
                x.PartyName,
                x.LocationCode,
                x.LocationName,

                x.TotalItems,
                x.TotalQty,
                x.TotalAmount,

                x.IsAccepted,
                x.CreatedDate,
                x.CreatedBy,

                x.GST
            });

            return data;
        }

        async Task<IEnumerable<object>> IPartInwardRepo.GetPartInwardExcelByDealer(DateTime fromDate, DateTime toDate, string? dealerCode)
        {
            var query = from p in _context.PartsInwards
                        join l in _context.LedgerMasters
                            on p.PartyName equals l.LedgerCode into ledgerGroup
                        from l in ledgerGroup.DefaultIfEmpty()

                        join lm in _context.LocationMasters
                            on p.LocCode equals lm.Loccode into locationGroup
                        from lm in locationGroup.DefaultIfEmpty()
                        select new
                        {
                            Part = p,
                            PartyName = l.LedgerName,
                            lm.Locname
                        };

            if (!string.IsNullOrWhiteSpace(dealerCode))
            {
                query = query.Where(x => x.Part.DealerCode == dealerCode);
            }

            query = query.Where(x => x.Part.InvoiceDate >= fromDate.Date && x.Part.InvoiceDate <= toDate.Date);

            var result = await query
                .GroupBy(x => new
                {
                    x.Part.InvoiceNo,
                    x.Part.InvoiceDate,
                    x.Part.DealerCode,
                    x.PartyName,
                })
                .Select(g => new
                {
                    g.Key.InvoiceNo,
                    g.Key.InvoiceDate,
                    g.Key.DealerCode,
                    g.Key.PartyName,

                    LocationCode = g.First().Part.LocCode,
                    LocationName = g.First().Locname,

                    TotalQty = g.Sum(x => x.Part.ItemQty),
                    TotalAmount = g.Sum(x => x.Part.ItemQty * x.Part.ItemRate),

                    TotalItems = g.Count(),

                    CreatedDate = g.Max(x => x.Part.CreatedDate),
                    CreatedBy = g.Max(x => x.Part.CreatedBy),

                    IsAccepted = g.All(x => x.Part.IsAccepted.Value == true) ? "Received" : "Pending",

                    GST = g.First().Part.Igst == 0m
                        ? g.First().Part.Cgst + g.First().Part.Sgst
                        : g.First().Part.Igst
                })
                .OrderBy(x => x.InvoiceDate)
                .ToListAsync();

            var data = result.Select((x, index) => new
            {
                x.InvoiceNo,
                x.InvoiceDate,
                x.DealerCode,
                x.PartyName,
                x.LocationCode,
                x.LocationName,

                x.TotalItems,
                x.TotalQty,
                x.TotalAmount,

                x.IsAccepted,
                x.CreatedDate,
                x.CreatedBy,

                x.GST
            });

            return data;
        }
    }
}
