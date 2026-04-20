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

        //public async Task<List<ReceiptEntry>> GetAllReceiptEntryListAsync()
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

        //public async Task<List<ReceiptEntry>> GetReceiptEntryListAsync(ReceiptFilterViewModel filter)
        //{
        //    try
        //    {
        //        var query = _bapldmsvadContext.ReceiptEntries.AsQueryable();

        //        if (filter != null)
        //        {
        //            // Date filters
        //            if (filter.FromDate.HasValue)
        //                query = query.Where(x => x.ReceiptDate >= filter.FromDate.Value);

        //            if (filter.ToDate.HasValue)
        //                query = query.Where(x => x.ReceiptDate <= filter.ToDate.Value);

        //            // String filters
        //            if (!string.IsNullOrWhiteSpace(filter.ReceiptNo))
        //                query = query.Where(x => x.ReceiptNo.Contains(filter.ReceiptNo));

        //            if (!string.IsNullOrWhiteSpace(filter.PartyName))
        //                query = query.Where(x => x.PartyName.Contains(filter.PartyName));

        //            if (!string.IsNullOrWhiteSpace(filter.MobileNo))
        //                query = query.Where(x => x.MobileNo.Contains(filter.MobileNo));

        //            if (!string.IsNullOrWhiteSpace(filter.BookingId))
        //                query = query.Where(x => x.BookingId.Contains(filter.BookingId));

        //            if (!string.IsNullOrWhiteSpace(filter.Location))
        //                query = query.Where(x => x.Location.Contains(filter.Location));
        //            if(!string.IsNullOrWhiteSpace(filter.ItemCode))
        //                query= query.Where(x=>x.ProductCode.Contains(filter.ItemCode));

        //            // SaleType filter
        //            if (!string.IsNullOrWhiteSpace(filter.SaleType))
        //            {
        //                var saleType = filter.SaleType.Trim().ToLower();

        //                if (saleType == "cash")
        //                {
        //                    query = query.Where(x => string.IsNullOrWhiteSpace(x.Financier));
        //                }
        //                else if (saleType == "credit")
        //                {
        //                    query = query.Where(x => !string.IsNullOrWhiteSpace(x.Financier));
        //                }
        //            }
        //        }

        //        return await query.OrderByDescending(i => i.CreatedDate).ToListAsync();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}


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

                        ProductCode = r.ProductCode,

                        // ✅ ADD THIS
                        ProductName = i.Itemname,

                        SalesExecutive = r.SalesExecutive,
                        ReceiptType = r.ReceiptType,
                        RefNo = r.RefNo,
                        Narration = r.Narration,
                        TotalAmount = r.TotalAmount,
                        CreatedBy = r.CreatedBy,
                        CreatedDate = r.CreatedDate,
                        UpdatedBy = r.UpdatedBy,
                        UpdatedDate = r.UpdatedDate
                    };

                // ✅ APPLY FILTERS
                if (filter != null)
                {
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

                    //// ✅ OPTIONAL: filter by product name also
                    //if (!string.IsNullOrWhiteSpace(filter.ItemCode))
                    //{
                    //    var itemSearch = filter.ItemCode.ToLower();

                    //    query = query.Where(x =>
                    //        x.ProductName != null &&
                    //        x.ProductName.ToLower().Contains(itemSearch)
                    //    );
                    //}

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

        public async Task<List<ReceiptEntryEditViewModel>> GetReceiptEntryListAsyncWithSearch(string? search,DateOnly? fromDate,DateOnly? toDate)
        {
            try
            {
                var query =
                    from r in _bapldmsvadContext.ReceiptEntries.AsNoTracking()

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

                        // ✅ Product Details
                        ProductName = i.Itemname,
                        ProductDescription = i.Itemdesc,
                        ProductColor = c.Colorname,

                        SalesExecutive = r.SalesExecutive,
                        ReceiptType = r.ReceiptType,
                        RefNo = r.RefNo,
                        Narration = r.Narration,
                        TotalAmount = r.TotalAmount,
                        CreatedBy = r.CreatedBy,
                        CreatedDate = r.CreatedDate,
                        UpdatedBy = r.UpdatedBy,
                        UpdatedDate = r.UpdatedDate
                    };
                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.ReceiptDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.ReceiptDate <= toDate.Value);
                }
                // APPLY SEARCH
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();

                    query = query.Where(x =>
                        (x.ReceiptNo != null && x.ReceiptNo.ToLower().Contains(search)) ||
                        (x.PartyName != null && x.PartyName.ToLower().Contains(search)) ||
                        (x.MobileNo != null && x.MobileNo.ToLower().Contains(search)) ||
                        (x.BookingId != null && x.BookingId.ToLower().Contains(search)) ||
                        (x.Location != null && x.Location.ToLower().Contains(search)) ||
                        (x.Financier != null && x.Financier.ToLower().Contains(search))||

                        // ✅ PRODUCT SEARCH INCLUDED
                        (x.ProductCode != null && x.ProductCode.ToLower().Contains(search)) ||
                        (x.ProductName != null && x.ProductName.ToLower().Contains(search)) ||
                        (x.ProductDescription != null && x.ProductDescription.ToLower().Contains(search)) ||
                        (x.ProductColor != null && x.ProductColor.ToLower().Contains(search)) ||

                        (x.SalesExecutive != null && x.SalesExecutive.ToLower().Contains(search)) ||
                        (x.ReceiptType != null && x.ReceiptType.ToLower().Contains(search)) ||
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
                    //Smart filters
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
