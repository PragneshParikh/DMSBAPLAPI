using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.EbwInvoiceRepo
{
    public class EbwInvoiceRepo : IEbwInvoiceRepo
    {
        private readonly BapldmsvadContext _context;
        public EbwInvoiceRepo(BapldmsvadContext context) { _context = context; }

        public async Task<int> SaveAsync(EbwInvoiceSaveViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                EbwInvoiceHeader header;

                // Fetch the scheme's Duration/DurationType (both int) once, used in both branches below
                DateTime? warrantyEndDate = null;
                if (model.SchemeId.HasValue && model.ChassisSaleDate.HasValue)
                {
                    var scheme = await _context.ExtendedBatteryWarranties
                        .FirstOrDefaultAsync(x => x.Id == model.SchemeId.Value);

                    if (scheme != null)
                    {
                        warrantyEndDate = CalculateWarrantyEndDate(
                            model.ChassisSaleDate.Value,
                            scheme.Duration,       // int
                            scheme.DurationType    // int
                        );
                    }
                }

                if (model.Id > 0)
                {
                    // ===== UPDATE =====
                    header = await _context.EbwInvoiceHeaders
                        .Include(x => x.EbwInvoiceDetails)
                        .FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (header == null)
                        throw new Exception("Invoice not found for update.");

                    header.InvoiceDate = model.InvoiceDate;
                    header.PrefixNo = model.PrefixNo;
                    header.BillNo = model.BillNo ?? header.BillNo;
                    header.LocationCode = model.LocationCode;
                    header.BillType = model.BillType;
                    header.CashAccountId = model.CashAccountId;
                    header.SchemeId = model.SchemeId;
                    header.SchemeName = model.SchemeName;
                    header.ChassisNo = model.ChassisNo;
                    header.SoldByDealerCode = model.SoldByDealerCode;
                    header.ChassisSaleDate = model.ChassisSaleDate;
                    header.ValidityExpiryDate = model.ValidityExpiryDate;
                    header.WarrantyEndDate = warrantyEndDate;   // ADDED
                    header.PartyName = model.PartyName;
                    header.PartyMobile = model.PartyMobile;
                    header.PartyAddress = model.PartyAddress;
                    header.PartyCity = model.PartyCity;
                    header.PartyPincode = model.PartyPincode;
                    header.PartyState = model.PartyState;
                    header.DealerState = model.DealerState;
                    header.IsInterstate = model.IsInterstate;
                    header.SerialNo = model.SerialNo;
                    header.ItemCode = model.ItemCode;
                    header.PartsAmount = model.PartsAmount;
                    header.NetAmount = model.NetAmount;
                    header.Remarks = model.Remarks;
                    header.UpdatedBy = userId;
                    header.UpdatedDate = DateTime.UtcNow;

                    _context.EbwInvoiceDetails.RemoveRange(header.EbwInvoiceDetails);
                    await _context.SaveChangesAsync();

                    var updatedDetails = model.Items.Select(x => new EbwInvoiceDetail
                    {
                        EbwInvoiceHeaderId = header.Id,
                        ItemCode = x.ItemCode,
                        ItemName = x.ItemName,
                        Description = x.Description,
                        HsnCode = x.HsnCode,
                        Qty = x.Qty,
                        ItemMrp = x.ItemMrp,
                        BaseItemRate = x.BaseItemRate,
                        ItemRate = x.ItemRate,
                        Discount = x.Discount,
                        DiscountType = x.DiscountType,
                        IgstPer = x.IgstPer,
                        IgstAmount = x.IgstAmount,
                        CgstPer = x.CgstPer,
                        CgstAmount = x.CgstAmount,
                        SgstPer = x.SgstPer,
                        SgstAmount = x.SgstAmount,
                        Amount = x.Amount,
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow
                    }).ToList();

                    await _context.EbwInvoiceDetails.AddRangeAsync(updatedDetails);
                }
                else
                {
                    // ===== INSERT =====
                    header = new EbwInvoiceHeader
                    {
                        DealerCode = model.DealerCode,
                        InvoiceDate = model.InvoiceDate,
                        PrefixNo = model.PrefixNo,
                        BillNo = model.BillNo,
                        LocationCode = model.LocationCode,
                        BillType = model.BillType,
                        CashAccountId = model.CashAccountId,
                        SchemeId = model.SchemeId,
                        SchemeName = model.SchemeName,
                        ChassisNo = model.ChassisNo,
                        SoldByDealerCode = model.SoldByDealerCode,
                        ChassisSaleDate = model.ChassisSaleDate,
                        ValidityExpiryDate = model.ValidityExpiryDate,
                        WarrantyEndDate = warrantyEndDate,   // ADDED
                        PartyName = model.PartyName,
                        PartyMobile = model.PartyMobile,
                        PartyAddress = model.PartyAddress,
                        PartyCity = model.PartyCity,
                        PartyPincode = model.PartyPincode,
                        PartyState = model.PartyState,
                        DealerState = model.DealerState,
                        IsInterstate = model.IsInterstate,
                        SerialNo = model.SerialNo,
                        ItemCode = model.ItemCode,
                        PartsAmount = model.PartsAmount,
                        NetAmount = model.NetAmount,
                        Remarks = model.Remarks,
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _context.EbwInvoiceHeaders.AddAsync(header);
                    await _context.SaveChangesAsync();

                    var details = model.Items.Select(x => new EbwInvoiceDetail
                    {
                        EbwInvoiceHeaderId = header.Id,
                        ItemCode = x.ItemCode,
                        ItemName = x.ItemName,
                        Description = x.Description,
                        HsnCode = x.HsnCode,
                        Qty = x.Qty,
                        ItemMrp = x.ItemMrp,
                        BaseItemRate = x.BaseItemRate,
                        ItemRate = x.ItemRate,
                        Discount = x.Discount,
                        DiscountType = x.DiscountType,
                        IgstPer = x.IgstPer,
                        IgstAmount = x.IgstAmount,
                        CgstPer = x.CgstPer,
                        CgstAmount = x.CgstAmount,
                        SgstPer = x.SgstPer,
                        SgstAmount = x.SgstAmount,
                        Amount = x.Amount,
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow
                    }).ToList();

                    await _context.EbwInvoiceDetails.AddRangeAsync(details);
                }

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

        /// <summary>
        /// DurationType: 1 = Month, 2 = Year
        /// </summary>
        private static DateTime CalculateWarrantyEndDate(DateTime saleDate, int duration, int durationType)
        {
            return durationType switch
            {
                1 => saleDate.AddMonths(duration),
                2 => saleDate.AddYears(duration),
                _ => saleDate.AddYears(duration)
            };
        }

        public async Task<List<object>> GetAllAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.EbwInvoiceHeaders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dealerCode))
                query = query.Where(x => x.DealerCode == dealerCode);

            if (fromDate.HasValue)
                query = query.Where(x => x.InvoiceDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.InvoiceDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var result = await query
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.BillNo,
                    x.InvoiceDate,
                    x.PartyName,
                    x.ChassisNo,
                    x.LocationCode,
                    LocationName = _context.LocationMasters
                        .Where(l => l.Loccode == x.LocationCode)
                        .Select(l => l.Locname)
                        .FirstOrDefault(),
                    x.BillType,
                    CashAccName = x.CashAccountId == 130 ? "Bank Transfer"
                                : x.CashAccountId == 85 ? "Cash"
                                : x.CashAccountId == 86 ? "Cheque"
                                : x.CashAccountId == 84 ? "UPI Payment" : "",
                    x.NetAmount
                })
                .ToListAsync();

            return result.Cast<object>().ToList();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var header = await _context.EbwInvoiceHeaders
                .Include(x => x.EbwInvoiceDetails)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (header == null)
                return false;

            _context.EbwInvoiceDetails.RemoveRange(header.EbwInvoiceDetails);
            _context.EbwInvoiceHeaders.Remove(header);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object?> GetDealerInfoAsync(string dealerCode)
        {
            return await _context.DealerMasters
                .Where(x => x.Dealercode == dealerCode)
                .Select(x => new
                {
                    x.Compname,
                    x.Adress1,
                    x.Adress2,
                    x.City,
                    x.State,
                    x.Pin,
                    x.PhoneOff,
                    x.Mobile,
                    x.CompgstinNo,
                    x.Pan
                })
                .FirstOrDefaultAsync();
        }

        public async Task<object?> GetByIdAsync(int id)
        {
            var header = await _context.EbwInvoiceHeaders
                .Include(x => x.EbwInvoiceDetails)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (header == null)
                return null;

            var batteryNumber = await _context.ChassisBatteryDetails
                .Where(x => x.ChassisNo == header.ChassisNo)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.BatteryNo)
                .FirstOrDefaultAsync();

            return new
            {
                header.Id,
                header.DealerCode,
                header.InvoiceDate,
                header.PrefixNo,
                header.BillNo,
                header.LocationCode,
                header.BillType,
                header.CashAccountId,
                header.SchemeId,
                header.SchemeName,
                header.ChassisNo,
                header.SoldByDealerCode,
                header.ChassisSaleDate,
                header.ValidityExpiryDate,
                header.PartyName,
                header.PartyMobile,
                header.PartyAddress,
                header.PartyCity,
                header.PartyPincode,
                header.PartyState,
                header.DealerState,
                header.IsInterstate,
                header.SerialNo,
                header.WarrantyEndDate,
                header.ItemCode,
                header.PartsAmount,
                header.NetAmount,
                header.Remarks,
                BatteryNumber = batteryNumber,   // ADDED
                EbwInvoiceDetails = header.EbwInvoiceDetails.Select(d => new
                {
                    d.ItemCode,
                    d.ItemName,
                    d.Description,
                    d.HsnCode,
                    d.Qty,
                    d.ItemMrp,
                    d.BaseItemRate,
                    d.ItemRate,
                    d.Discount,
                    d.DiscountType,
                    d.IgstPer,
                    d.IgstAmount,
                    d.CgstPer,
                    d.CgstAmount,
                    d.SgstPer,
                    d.SgstAmount,
                    d.Amount
                })
            };
        }

        // EbwInvoiceRepo.cs — add
        public async Task<List<object>> GetReportDataAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.EbwInvoiceHeaders
                .Include(x => x.EbwInvoiceDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dealerCode))
                query = query.Where(x => x.DealerCode == dealerCode);

            if (fromDate.HasValue)
                query = query.Where(x => x.InvoiceDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.InvoiceDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var headers = await query.OrderByDescending(x => x.Id).ToListAsync();

            var rows = new List<object>();

            foreach (var h in headers)
            {
                var locationName = await _context.LocationMasters
                    .Where(l => l.Loccode == h.LocationCode)
                    .Select(l => l.Locname)
                    .FirstOrDefaultAsync();

                var modelName = await (from s in _context.ExtendedBatteryWarranties
                                       join m in _context.OemmodelMasters on s.OemmodelId equals m.Id
                                       where s.Id == h.SchemeId
                                       select m.ModelName)
                                       .FirstOrDefaultAsync();

                foreach (var d in h.EbwInvoiceDetails)
                {
                    rows.Add(new
                    {
                        HeaderId = h.Id,
                        DealerCode = h.DealerCode,
                        LocationCode = h.LocationCode,
                        LocationName = locationName,
                        InvoiceNo = h.BillNo,
                        InvoiceDate = h.InvoiceDate,
                        ReceivedDate = h.InvoiceDate,          // no separate "received" date exists — using InvoiceDate
                        ModelName = modelName ?? "—",
                        ChassisNo = h.ChassisNo,
                        PartNo = d.ItemCode,
                        ItemDesc = d.Description,
                        SerialNo = h.SerialNo,
                        WarrantyEndDate = h.WarrantyEndDate,
                        RBillNo = h.BillNo,                     // same invoice — no separate repair bill link exists
                        RBillDate = h.InvoiceDate,
                        PartyName = h.PartyName,
                        PartyState = h.PartyState
                    });
                }
            }

            return rows;
        }

        public async Task<object?> GetLatestByChassisNoAsync(string chassisNo)
        {
            var header = await _context.EbwInvoiceHeaders
                .Where(x => x.ChassisNo == chassisNo)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (header == null)
                return null;

            var batteryNumber = await _context.ChassisBatteryDetails
                .Where(x => x.ChassisNo == chassisNo)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.BatteryNo)
                .FirstOrDefaultAsync();

            return new
            {
                header.Id,
                header.BillNo,
                header.SchemeName,
                header.ChassisNo,
                header.ChassisSaleDate,
                header.WarrantyEndDate,
                BatteryNumber = batteryNumber
            };
        }

        public async Task<(string PrefixNo, int NextNo)> GetNextPrefixNoAsync(string dealerCode)
        {
            var maxBillNo = await _context.EbwInvoiceHeaders
                .Where(x => x.DealerCode == dealerCode)
                .Select(x => (int?)x.BillNo)
                .MaxAsync();

            int nextNo = (maxBillNo ?? 0) + 1;

            // Prefix stays blank/free-text — user types it manually per your UI (matches
            // your screenshot where Prefix box is plain editable text, not auto-generated)
            return ("", nextNo);
        }
    }
}