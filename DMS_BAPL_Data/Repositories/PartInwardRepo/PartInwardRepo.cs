using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Utils.ViewModels;
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
                                     where pi.InvoiceNo == invoiceNo && pi.IsAccepted == false
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
                PrefixNo = prefixNo,
                InvoiceDate = partInwards.First().InvoiceDate,
                LocationCode = partInwards.First().LocCode,
                InvoiceNo = partInwards.First().InvoiceNo,
                PartInwards = partInwards
            };
        }

    }
}
