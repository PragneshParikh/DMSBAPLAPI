using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.InkML;
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

        public async Task<ReceiptEntry> AddReceiptEntryAsync(ReceiptEntryViewModel receiptEntry, string userId)
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
                    ReceiptType = receiptEntry.ReceiptType,
                    RefNo = receiptEntry.RefNo,
                    Narration = receiptEntry.Narration,
                    BusinessType = receiptEntry.BusinessType,
                    TotalAmount = receiptEntry.TotalAmount,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                await _bapldmsvadContext.ReceiptEntries.AddAsync(newReceiptEntry);
                await _bapldmsvadContext.SaveChangesAsync();

                return newReceiptEntry;
            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<ReceiptEntry>> GetReceiptEntryListAsync()
        //{
        //    try
        //    {
        //        var receiptList = await _bapldmsvadContext.ReceiptEntries.ToListAsync();
        //        return receiptList;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public async Task<List<ReceiptEntry>> GetReceiptEntryListAsync(ReceiptFilterViewModel filter)
        {
            try
            {
                var query = _bapldmsvadContext.ReceiptEntries.AsQueryable();

                // Date filters
                if (filter.FromDate.HasValue)
                    query = query.Where(x => x.ReceiptDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(x => x.ReceiptDate <= filter.ToDate.Value);

                // String filters
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

                // SaleType filter
                if (!string.IsNullOrWhiteSpace(filter.SaleType))
                {
                    var saleType = filter.SaleType.Trim().ToLower();

                    if (saleType == "cash")
                    {
                        query = query.Where(x => string.IsNullOrWhiteSpace(x.Financier));
                    }
                    else if (saleType == "credit")
                    {
                        query = query.Where(x => !string.IsNullOrWhiteSpace(x.Financier));
                    }
                }

                return await query.ToListAsync();
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

                        // ✅ FROM ITEM MASTER
                        ProductName = i != null ? i.Itemname : null,
                        ProductDescription = i != null ? i.Itemdesc : null,

                        // ✅ FROM COLOR MASTER
                        ProductColor = c != null ? c.Colorname : null,

                        SalesExecutive = r.SalesExecutive,
                        ReceiptType = r.ReceiptType,
                        RefNo = r.RefNo,
                        Narration = r.Narration,
                        TotalAmount = r.TotalAmount,
                        CreatedBy = r.CreatedBy,
                        CreatedDate = r.CreatedDate,
                        UpdatedBy = r.UpdatedBy,
                        UpdatedDate = r.UpdatedDate
                    }
                ).FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }


        public async Task<ReceiptEntry?> UpdateReceiptEntryAsync(int id, ReceiptEntryViewModel receiptEntry, string userId)
        {
            try
            {
                var existingReceipt = await _bapldmsvadContext.ReceiptEntries
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existingReceipt == null)
                    return null;

                // 🔥 Update fields
                existingReceipt.Location = receiptEntry.Location;
                existingReceipt.SaleType = receiptEntry.SaleType;
                existingReceipt.BookingId = receiptEntry.BookingId;
                existingReceipt.PartyName = receiptEntry.PartyName;
                existingReceipt.Financier = receiptEntry.Financier;
                existingReceipt.ProductCode = receiptEntry.ProductCode;
                existingReceipt.SalesExecutive = receiptEntry.SalesExecutive;
                existingReceipt.MobileNo = receiptEntry.MobileNo;
                existingReceipt.ReceiptType = receiptEntry.ReceiptType;
                existingReceipt.RefNo = receiptEntry.RefNo;
                existingReceipt.Narration = receiptEntry.Narration;
                existingReceipt.TotalAmount = receiptEntry.TotalAmount;
                existingReceipt.BusinessType = receiptEntry.BusinessType;

                // Optional: update date if needed
                existingReceipt.ReceiptDate = DateOnly.FromDateTime(DateTime.Now);

                // Audit
                existingReceipt.UpdatedBy = userId;
                existingReceipt.UpdatedDate = DateTime.Now;

                await _bapldmsvadContext.SaveChangesAsync();

                return existingReceipt;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> CheckReceiptExist(string? mobileNo, string? bookingId)
        {
            try { 
            if (string.IsNullOrWhiteSpace(mobileNo) && string.IsNullOrWhiteSpace(bookingId))
                throw new Exception("Please provide Mobile No or Booking Id");

            var query = _bapldmsvadContext.ReceiptEntries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(mobileNo))
            {
                var normalizedMobile = mobileNo.Trim();

                return await query.AnyAsync(x =>
                    x.MobileNo != null &&
                    x.MobileNo.Trim() == normalizedMobile
                );
            }

            if (!string.IsNullOrWhiteSpace(bookingId))
            {
                return await query.AnyAsync(x =>
                    x.BookingId != null &&
                    x.BookingId == bookingId
                );
            }

            return false;
            }
            catch
            {
                throw;
            }
        }
    }
}
