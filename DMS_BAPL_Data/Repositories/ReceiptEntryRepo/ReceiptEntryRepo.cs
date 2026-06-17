using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ReceiptEntryRepo
{
    public class ReceiptEntryRepo : IReceiptEntryRepo
    {
        private readonly BapldmsvadContext _bapldmsvadContext;

        public ReceiptEntryRepo(BapldmsvadContext bapldmsvadContext)
        {
            _bapldmsvadContext = bapldmsvadContext;
        }

        public async Task<string?> GetLastReceiptNoAsync()
        {
            return await _bapldmsvadContext.ReceiptEntries.OrderByDescending(x => x.CreatedDate)
                .Select(x => x.ReceiptNo).FirstOrDefaultAsync();
        }

        public async Task<ReceiptEntry> AddReceiptEntryAsync(ReceiptEntryViewModel receiptEntry, string userId, string dealerCode)
        {
            try
            {
                var newReceiptEntry = new ReceiptEntry
                {
                    Location = receiptEntry.Location,
                    ReceiptNo = receiptEntry.ReceiptNo,
                    ReceiptDate = DateOnly.FromDateTime(DateTime.Now),
                    SaleType = receiptEntry.SaleType,
                    BookingId = receiptEntry.BookingId,
                    PartyName = receiptEntry.PartyName,
                    Financier = receiptEntry.Financier,
                    ProductCode = receiptEntry.ProductCode,
                    SalesExecutive = receiptEntry.SalesExecutive,
                    MobileNo = receiptEntry.MobileNo,
                    //ReceiptType = receiptEntry.ReceiptType,
                    RefNo = receiptEntry.RefNo,
                    Narration = receiptEntry.Narration,
                    BusinessType = receiptEntry.BusinessType,
                    TotalAmount = receiptEntry.TotalAmount,
                    DealerCode = dealerCode,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                await _bapldmsvadContext.ReceiptEntries.AddAsync(newReceiptEntry);
                await _bapldmsvadContext.SaveChangesAsync();

                if (receiptEntry.ReceiptEntryDetail != null &&
            receiptEntry.ReceiptEntryDetail.Any())
                {
                    var details = receiptEntry.ReceiptEntryDetail
                        .Select(x => new ReceiptEntryDetail
                        {
                            ReceiptId = newReceiptEntry.Id,
                            LineItemNo = x.LineItemNo,
                            ReceiptType = x.ReceiptType,
                            LineDate = DateTime.Now,
                            Amount = x.Amount,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        })
                        .ToList();

                    await _bapldmsvadContext.ReceiptEntryDetails.AddRangeAsync(details);
                    await _bapldmsvadContext.SaveChangesAsync();
                }

                return newReceiptEntry;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ReceiptEntryEditViewModel>> GetReceiptEntryListAsync(ReceiptFilterViewModel filter)
        {
            try
            {

                var query =
     from r in _bapldmsvadContext.ReceiptEntries.AsNoTracking()

     join i in _bapldmsvadContext.ItemMasters
         on r.ProductCode equals i.Itemcode into itemGroup
     from i in itemGroup.DefaultIfEmpty()

     select new ReceiptEntryEditViewModel
     {
         Id = r.Id,
         Location = r.Location,
         ReceiptNo = r.ReceiptNo,
         ReceiptDate = r.ReceiptDate,
         SaleType = r.SaleType,
         BookingId = r.BookingId,
         PartyName = r.PartyName,
         MobileNo = r.MobileNo,
         Financier = r.Financier,
         BusinessType = r.BusinessType,
         DealerCode = r.DealerCode,

         ProductCode = r.ProductCode,
         ProductName = i.Itemname,

         SalesExecutive = r.SalesExecutive,
         RefNo = r.RefNo,
         Narration = r.Narration,
         TotalAmount = r.TotalAmount,

         CreatedBy = r.CreatedBy,
         CreatedDate = r.CreatedDate,
         UpdatedBy = r.UpdatedBy,
         UpdatedDate = r.UpdatedDate,

         ReceiptEntryDetail = r.ReceiptEntryDetails
             .OrderBy(x => x.LineItemNo)
             .Select(x => new ReceiptEntryDetailViewModel
             {
                 LineItemNo = x.LineItemNo,
                 ReceiptType = x.ReceiptType,
                 Amount = x.Amount
             })
             .ToList()
     };

                // APPLY FILTERS
                if (filter != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                        query = query.Where(x => x.DealerCode == filter.DealerCode);
                    if (filter.FromDate.HasValue)
                        query = query.Where(x => x.ReceiptDate >= filter.FromDate.Value);

                    if (filter.ToDate.HasValue)
                        query = query.Where(x => x.ReceiptDate <= filter.ToDate.Value);

                    if (!string.IsNullOrWhiteSpace(filter.ReceiptNo))
                        query = query.Where(x => x.ReceiptNo.Contains(filter.ReceiptNo));

                    if (!string.IsNullOrWhiteSpace(filter.PartyName))
                        query = query.Where(x => x.PartyName.Contains(filter.PartyName));

                    if (!string.IsNullOrWhiteSpace(filter.MobileNo))
                        query = query.Where(x => x.MobileNo.Contains(filter.MobileNo));

                    if (!string.IsNullOrWhiteSpace(filter.BookingId))
                        query = query.Where(x => x.BookingId.Contains(filter.BookingId));

                    if (!string.IsNullOrWhiteSpace(filter.Location))
                        query = query.Where(x => x.Location.Contains(filter.Location));

                    if (!string.IsNullOrWhiteSpace(filter.ItemCode))
                        query = query.Where(x => x.ProductCode.Contains(filter.ItemCode));


                    if (!string.IsNullOrWhiteSpace(filter.SaleType))
                    {
                        var saleType = filter.SaleType.Trim().ToLower();

                        if (saleType == "cash")
                            query = query.Where(x => string.IsNullOrWhiteSpace(x.Financier));

                        else if (saleType == "credit")
                            query = query.Where(x => !string.IsNullOrWhiteSpace(x.Financier));
                    }
                }

                return await query
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<LedgerMaster>> GetLedgerMasterDetailsByTypeAsync(string ledgerType)
        {
            try
            {
                if (ledgerType.ToLower() == "party")
                {
                    return await _bapldmsvadContext.LedgerMasters.Where(x => x.LedgerType.ToLower() == "party" ||
                    x.LedgerType.ToLower() == "institutional").ToListAsync();
                }
                return await _bapldmsvadContext.LedgerMasters
            .Where(x => x.LedgerType != null &&
                        x.LedgerType.ToLower() == ledgerType.ToLower())
            .ToListAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<ReceiptEntryEditViewModel?> GetReceiptByIdAsync(int id)
        {
            try
            {
                var result = await (
                    from r in _bapldmsvadContext.ReceiptEntries

                    join i in _bapldmsvadContext.ItemMasters
                        on r.ProductCode equals i.Itemcode into itemGroup
                    from i in itemGroup.DefaultIfEmpty()

                    join c in _bapldmsvadContext.ColorMasters
                        on i.Colorcode equals c.Colorcode into colorGroup
                    from c in colorGroup.DefaultIfEmpty()

                    where r.Id == id

                    select new ReceiptEntryEditViewModel
                    {
                        Id = r.Id,
                        Location = r.Location,
                        ReceiptNo = r.ReceiptNo,
                        MobileNo = r.MobileNo,
                        ReceiptDate = r.ReceiptDate,
                        SaleType = r.SaleType,
                        BookingId = r.BookingId,
                        PartyName = r.PartyName,
                        Financier = r.Financier,
                        ProductCode = r.ProductCode,
                        BusinessType = r.BusinessType,

                        ProductName = i != null ? i.Itemname : null,
                        ProductDescription = i != null ? i.Itemdesc : null,
                        ProductColor = c != null ? c.Colorname : null,

                        SalesExecutive = r.SalesExecutive,
                        RefNo = r.RefNo,
                        Narration = r.Narration,
                        TotalAmount = r.TotalAmount,

                        CreatedBy = r.CreatedBy,
                        CreatedDate = r.CreatedDate,
                        UpdatedBy = r.UpdatedBy,
                        UpdatedDate = r.UpdatedDate,

                        ReceiptEntryDetail = r.ReceiptEntryDetails
                            .OrderBy(x => x.LineItemNo)
                            .Select(x => new ReceiptEntryDetailViewModel
                            {
                                LineItemNo = x.LineItemNo,
                                ReceiptType = x.ReceiptType,
                                Amount = x.Amount,
                                LineDate = x.LineDate,
                            })
                            .ToList()
                    }
                ).FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }


        //    public async Task<ReceiptEntry?> UpdateReceiptEntryAsync(
        //int id,
        //ReceiptEntryViewModel receiptEntry,
        //string userId)
        //    {
        //        try
        //        {
        //            var existingReceipt = await _bapldmsvadContext.ReceiptEntries
        //                .FirstOrDefaultAsync(x => x.Id == id);

        //            if (existingReceipt == null)
        //                return null;

        //            // Header Update
        //            existingReceipt.Location = receiptEntry.Location;
        //            existingReceipt.SaleType = receiptEntry.SaleType;
        //            existingReceipt.BookingId = receiptEntry.BookingId;
        //            existingReceipt.PartyName = receiptEntry.PartyName;
        //            existingReceipt.Financier = receiptEntry.Financier;
        //            existingReceipt.ProductCode = receiptEntry.ProductCode;
        //            existingReceipt.SalesExecutive = receiptEntry.SalesExecutive;
        //            existingReceipt.MobileNo = receiptEntry.MobileNo;
        //            existingReceipt.RefNo = receiptEntry.RefNo;
        //            existingReceipt.Narration = receiptEntry.Narration;
        //            existingReceipt.TotalAmount = receiptEntry.TotalAmount;
        //            existingReceipt.BusinessType = receiptEntry.BusinessType;

        //            existingReceipt.ReceiptDate = receiptEntry.BillDate;

        //            existingReceipt.UpdatedBy = userId;
        //            existingReceipt.UpdatedDate = DateTime.Now;

        //            // Remove old detail rows
        //            var existingDetails = await _bapldmsvadContext.ReceiptEntryDetails
        //                .Where(x => x.ReceiptId == id)
        //                .ToListAsync();

        //            if (existingDetails.Any())
        //            {
        //                _bapldmsvadContext.ReceiptEntryDetails.RemoveRange(existingDetails);
        //            }

        //            // Add new detail rows
        //            if (receiptEntry.ReceiptEntryDetail != null)
        //            {
        //                var details = receiptEntry.ReceiptEntryDetail
        //                    .Select(x => new ReceiptEntryDetail
        //                    {
        //                        ReceiptId = id,
        //                        LineItemNo = x.LineItemNo,
        //                        ReceiptType = x.ReceiptType,
        //                        Amount = x.Amount,
        //                        LineDate = DateTime.Now,
        //                        CreatedBy = userId,
        //                        CreatedDate = DateTime.Now
        //                    })
        //                    .ToList();

        //                await _bapldmsvadContext.ReceiptEntryDetails.AddRangeAsync(details);
        //            }

        //            await _bapldmsvadContext.SaveChangesAsync();

        //            return existingReceipt;
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //    }

        public async Task<ReceiptEntry?> UpdateReceiptEntryAsync(
    int id,
    ReceiptEntryViewModel receiptEntry,
    string userId)
        {
            try
            {
                var existingReceipt = await _bapldmsvadContext.ReceiptEntries
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existingReceipt == null)
                    return null;

                // =========================
                // HEADER UPDATE
                // =========================
                existingReceipt.Location = receiptEntry.Location;
                existingReceipt.SaleType = receiptEntry.SaleType;
                existingReceipt.BookingId = receiptEntry.BookingId;
                existingReceipt.PartyName = receiptEntry.PartyName;
                existingReceipt.Financier = receiptEntry.Financier;
                existingReceipt.ProductCode = receiptEntry.ProductCode;
                existingReceipt.SalesExecutive = receiptEntry.SalesExecutive;
                existingReceipt.MobileNo = receiptEntry.MobileNo;
                existingReceipt.RefNo = receiptEntry.RefNo;
                existingReceipt.Narration = receiptEntry.Narration;
                existingReceipt.TotalAmount = receiptEntry.TotalAmount;
                existingReceipt.BusinessType = receiptEntry.BusinessType;
                existingReceipt.ReceiptDate = receiptEntry.BillDate;

                existingReceipt.UpdatedBy = userId;
                existingReceipt.UpdatedDate = DateTime.Now;

                // =========================
                // EXISTING DETAILS
                // =========================
                var existingDetails = await _bapldmsvadContext.ReceiptEntryDetails
                    .Where(x => x.ReceiptId == id)
                    .ToListAsync();

                var incomingDetails = receiptEntry.ReceiptEntryDetail?
                    .ToList() ?? new List<ReceiptEntryDetailViewModel>();

                // =========================
                // DELETE REMOVED ROWS
                // =========================
                var incomingLineNos = incomingDetails
                    .Where(x => x.LineItemNo > 0)
                    .Select(x => x.LineItemNo)
                    .ToList();

                var toDelete = existingDetails
                    .Where(x => !incomingLineNos.Contains(x.LineItemNo))
                    .ToList();

                if (toDelete.Any())
                {
                    _bapldmsvadContext.ReceiptEntryDetails.RemoveRange(toDelete);
                }

                // =========================
                // INSERT / UPDATE
                // =========================
                foreach (var item in incomingDetails)
                {
                    var existing = existingDetails
                        .FirstOrDefault(x => x.LineItemNo == item.LineItemNo);

                    if (existing != null)
                    {
                        // UPDATE EXISTING ROW
                        existing.ReceiptType = item.ReceiptType;
                        existing.Amount = item.Amount;
                        existing.LineDate = item.LineDate;

                        existing.UpdatedBy = userId;
                        existing.UpdatedDate = DateTime.Now;
                    }
                    else
                    {
                        // INSERT NEW ROW
                        var newDetail = new ReceiptEntryDetail
                        {
                            ReceiptId = id,
                            LineItemNo = item.LineItemNo,
                            ReceiptType = item.ReceiptType,
                            Amount = item.Amount,
                            LineDate = item.LineDate,

                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        };

                        _bapldmsvadContext.ReceiptEntryDetails.Add(newDetail);
                    }
                }

                // =========================
                // SAVE
                // =========================
                await _bapldmsvadContext.SaveChangesAsync();

                return existingReceipt;
            }
            catch
            {
                throw;
            }
        }
        //public async Task<bool> CheckReceiptExist(string? mobileNo, string? bookingId, string? recType, string? dealerCode)
        //{
        //    try { 
        //    if (string.IsNullOrWhiteSpace(mobileNo) && string.IsNullOrWhiteSpace(bookingId))
        //        throw new Exception("Please provide Mobile No or Booking Id");

        //    var query = _bapldmsvadContext.ReceiptEntries.AsQueryable();

        //    if (!string.IsNullOrWhiteSpace(mobileNo))
        //    {
        //        var normalizedMobile = mobileNo.Trim();

        //        return await query.AnyAsync(x =>
        //            x.MobileNo != null &&
        //            x.MobileNo.Trim() == normalizedMobile && x.DealerCode == dealerCode && x.ReceiptType == recType
        //        );
        //    }

        //    if (!string.IsNullOrWhiteSpace(bookingId))
        //    {
        //        return await query.AnyAsync(x =>
        //            x.BookingId != null &&
        //            x.BookingId == bookingId && x.DealerCode == dealerCode && x.ReceiptType == recType
        //        );
        //    }

        //    return false;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public async Task<bool> CheckReceiptExist(string? mobileNo, string? bookingId, string? recType, string? dealerCode)
        {
            if (string.IsNullOrWhiteSpace(mobileNo) && string.IsNullOrWhiteSpace(bookingId))
            {
                throw new Exception("Please provide Mobile No or Booking Id");
            }

            var query = _bapldmsvadContext.ReceiptEntries.Where(x => x.DealerCode == dealerCode);

            if (!string.IsNullOrWhiteSpace(mobileNo))
            {
                var normalizedMobile = mobileNo.Trim();
                query = query.Where(x => x.MobileNo != null && x.MobileNo.Trim() == normalizedMobile);
            }
            else
            {
                query = query.Where(x => x.BookingId != null && x.BookingId == bookingId);
            }

            if (string.Equals(recType, "Receipt", StringComparison.OrdinalIgnoreCase))
            {
                return await query.AnyAsync(x => x.SaleType == "Receipt" || x.SaleType == "Against Lead");
            }

            if (string.Equals(recType, "Against Lead", StringComparison.OrdinalIgnoreCase))
            {
                return await query.AnyAsync(x => x.SaleType == "Against Lead");
            }

            return false;
        }

        public async Task<List<ReceiptEntryEditViewModel>> GetReceiptEntryListAsyncWithSearch(string? dealerCode, string? search, DateOnly? fromDate, DateOnly? toDate)
        {
            try
            {
                var query = from r in _bapldmsvadContext.ReceiptEntries.AsNoTracking()
                            join i in _bapldmsvadContext.ItemMasters
                            on r.ProductCode equals i.Itemcode into itemGroup
                            from i in itemGroup.DefaultIfEmpty()

                            join c in _bapldmsvadContext.ColorMasters
                            on i.Colorcode equals c.Colorcode into colorGroup
                            from c in colorGroup.DefaultIfEmpty()

                            select new ReceiptEntryEditViewModel
                            {
                                Id = r.Id,
                                Location = r.Location,
                                ReceiptNo = r.ReceiptNo,
                                MobileNo = r.MobileNo,
                                ReceiptDate = r.ReceiptDate,
                                SaleType = r.SaleType,
                                BookingId = r.BookingId,
                                PartyName = r.PartyName,
                                Financier = r.Financier,
                                ProductCode = r.ProductCode,
                                BusinessType = r.BusinessType,
                                DealerCode = r.DealerCode,
                                ProductName = i.Itemname,
                                ProductDescription = i.Itemdesc,
                                ProductColor = c.Colorname,
                                SalesExecutive = r.SalesExecutive,
                                RefNo = r.RefNo,
                                Narration = r.Narration,
                                TotalAmount = r.TotalAmount,
                                CreatedBy = r.CreatedBy,
                                CreatedDate = r.CreatedDate,
                                UpdatedBy = r.UpdatedBy,
                                UpdatedDate = r.UpdatedDate,
                                ReceiptEntryDetail = r.ReceiptEntryDetails

                                .OrderBy(x => x.LineItemNo).Select(x => new ReceiptEntryDetailViewModel
                                {
                                    LineItemNo = x.LineItemNo,
                                    ReceiptType = x.ReceiptType,
                                    Amount = x.Amount
                                }).ToList()
                            };

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.ReceiptDate <= toDate.Value);
                }
                if (!string.IsNullOrWhiteSpace(dealerCode))
                {

                    query = query.Where(x => x.DealerCode == dealerCode);
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();

                    query = query.Where(x =>
                        (x.ReceiptNo != null && x.ReceiptNo.ToLower().Contains(search)) ||
                        (x.PartyName != null && x.PartyName.ToLower().Contains(search)) ||
                        (x.MobileNo != null && x.MobileNo.ToLower().Contains(search)) ||
                        (x.BookingId != null && x.BookingId.ToLower().Contains(search)) ||
                        (x.Location != null && x.Location.ToLower().Contains(search)) ||
                        (x.Financier != null && x.Financier.ToLower().Contains(search)) ||
                        (x.ProductCode != null && x.ProductCode.ToLower().Contains(search)) ||
                        (x.ProductName != null && x.ProductName.ToLower().Contains(search)) ||
                        (x.ProductDescription != null && x.ProductDescription.ToLower().Contains(search)) ||
                        (x.ProductColor != null && x.ProductColor.ToLower().Contains(search)) ||
                        (x.SalesExecutive != null && x.SalesExecutive.ToLower().Contains(search)) ||
                        (x.RefNo != null && x.RefNo.ToLower().Contains(search)) ||
                        (x.Narration != null && x.Narration.ToLower().Contains(search))
                    );

                    if (int.TryParse(search, out int number))
                    {
                        query = query.Where(x =>
                            x.ReceiptDate.Day == number ||
                            x.ReceiptDate.Month == number ||
                            x.ReceiptDate.Year == number
                        );
                    }
                    if (search == "cash")
                        query = query.Where(x => string.IsNullOrWhiteSpace(x.Financier));

                    if (search == "credit")
                        query = query.Where(x => !string.IsNullOrWhiteSpace(x.Financier));
                }

                return await query
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }


    }
}
