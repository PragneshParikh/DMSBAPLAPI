using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ReportRepo
{
    public class ReportRepo : IReportRepo
    {
        private readonly BapldmsvadContext _context;

        public ReportRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        // ═════════════════════════════════════════════════════════════════════
        // STOCK REPORT  (unchanged from StockReportRepo)
        // ═════════════════════════════════════════════════════════════════════

        public async Task<List<StockReportViewModel>> GetDealerWiseStockReportAsync(string? dealerCode = null)
        {
            try
            {
                var vehicleInwards = await _context.VehicleInwards
                 .AsNoTracking()
                 .Where(vi => vi.DealerCode != null && (string.IsNullOrEmpty(dealerCode) || vi.DealerCode == dealerCode)) // ✅ filter here
                 .ToListAsync();

                var dealers = await _context.DealerMasters
                    .AsNoTracking()
                    .ToListAsync();

                var items = await _context.ItemMasters
                    .AsNoTracking()
                    .ToListAsync();

                var colors = await _context.ColorMasters
                    .AsNoTracking()
                    .ToListAsync();

                var result = (
                    from vi in vehicleInwards
                    join dm in dealers
                        on vi.DealerCode equals dm.Dealercode into dealerGroup
                    from dm in dealerGroup.DefaultIfEmpty()
                    join im in items
                        on vi.ItemCode equals im.Itemcode into itemGroup
                    from im in itemGroup.DefaultIfEmpty()
                    join cm in colors
                        on vi.ColrCode equals cm.Colorcode into colorGroup
                    from cm in colorGroup.DefaultIfEmpty()
                    group new { vi, dm, im, cm }
                        by new
                        {
                            DealerCode = vi.DealerCode ?? "Unknown",
                            DealerName = dm != null ? dm.Compname : (vi.DealerCode ?? "Unknown"),
                            Model = im != null ? (im.Oemmodelname ?? im.Itemcode ?? "Unknown")
                                                    : (vi.ItemCode ?? "Unknown"),
                            Colour = cm != null ? cm.Colorname
                                                    : (vi.ColrCode ?? "Unknown")
                        }
                    into g
                    orderby g.Key.DealerName, g.Key.Model, g.Key.Colour
                    select new StockReportViewModel
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        Model = g.Key.Model,
                        Colour = g.Key.Colour,
                        TotalQty = g.Count()
                    }
                ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetDealerWiseStockReportAsync: " + ex.Message, ex);
            }
        }

        public async Task<List<StockReportViewModel>> GetColourWiseStockReportAsync()
        {
            try
            {
                var vehicleInwards = await _context.VehicleInwards
                    .AsNoTracking()
                    .Where(vi => vi.DealerCode != null)
                    .ToListAsync();

                var dealers = await _context.DealerMasters
                    .AsNoTracking()
                    .ToListAsync();

                var items = await _context.ItemMasters
                    .AsNoTracking()
                    .ToListAsync();

                var colors = await _context.ColorMasters
                    .AsNoTracking()
                    .ToListAsync();

                var result = (
                    from vi in vehicleInwards
                    join dm in dealers
                        on vi.DealerCode equals dm.Dealercode into dealerGroup
                    from dm in dealerGroup.DefaultIfEmpty()
                    join im in items
                        on vi.ItemCode equals im.Itemcode into itemGroup
                    from im in itemGroup.DefaultIfEmpty()
                    join cm in colors
                        on vi.ColrCode equals cm.Colorcode into colorGroup
                    from cm in colorGroup.DefaultIfEmpty()
                    group new { vi, dm, im, cm }
                        by new
                        {
                            DealerCode = vi.DealerCode ?? "Unknown",
                            DealerName = dm != null ? dm.Compname : (vi.DealerCode ?? "Unknown"),
                            Model = im != null ? (im.Oemmodelname ?? im.Itemcode ?? "Unknown")
                                                    : (vi.ItemCode ?? "Unknown"),
                            Colour = cm != null ? cm.Colorname
                                                    : (vi.ColrCode ?? "Unknown")
                        }
                    into g
                    orderby g.Key.Colour, g.Key.DealerName, g.Key.Model
                    select new StockReportViewModel
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        Model = g.Key.Model,
                        Colour = g.Key.Colour,
                        TotalQty = g.Count()
                    }
                ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetColourWiseStockReportAsync: " + ex.Message, ex);
            }
        }


        // ═════════════════════════════════════════════════════════════════════
        // JOB REPORT  (unchanged from JobReportRepo)
        // ═════════════════════════════════════════════════════════════════════

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportAsync(
            JobReportFilterModel filter)
        {
            try
            {
                var baseQuery = from jh in _context.JobCardHeaders
                                join jc in _context.JobCardCustomers
                                    on jh.Id equals jc.JobCardHeaderId
                                join jt in _context.JobTypes
                                    on jh.Jobtype equals jt.Id
                                join sh in _context.ServiceHeads
                                    on jh.Servicehead equals sh.Id
                                join st in _context.ServiceTypes
                                    on jh.Servicetype equals st.Id
                                join inv in _context.InvoiceHeaders
                                    on jh.Id equals inv.ReferenceId into invGroup
                                from invoice in invGroup.DefaultIfEmpty()
                                select new { jh, jc, jt, sh, st, invoice };

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    baseQuery = baseQuery.Where(x => x.jh.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                    baseQuery = baseQuery.Where(x =>
                        (x.invoice != null ? x.invoice.CreatedDate : x.jh.CreatedDate) >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    baseQuery = baseQuery.Where(x =>
                        (x.invoice != null ? x.invoice.CreatedDate : x.jh.CreatedDate) <= filter.ToDate.Value.Date.AddDays(1));

                if (!string.IsNullOrWhiteSpace(filter.ServiceLocation))
                    baseQuery = baseQuery.Where(x => x.jh.Serviceloc == filter.ServiceLocation);

                if (filter.JobNo.HasValue)
                    baseQuery = baseQuery.Where(x => x.jh.JobNo == filter.JobNo.Value);

                if (!string.IsNullOrWhiteSpace(filter.PartyName))
                    baseQuery = baseQuery.Where(x => x.jc.CustomerName.Contains(filter.PartyName));

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                    baseQuery = baseQuery.Where(x => x.jh.Chassisno.Contains(filter.ChassisNo));

                if (!string.IsNullOrWhiteSpace(filter.RegNo))
                    baseQuery = baseQuery.Where(x => x.jc.RegisterNo.Contains(filter.RegNo));

                var totalRecords = await baseQuery.CountAsync();

                var pagedRows = await baseQuery
                    .OrderByDescending(x => x.invoice != null ? x.invoice.CreatedDate : x.jh.CreatedDate)
                    .ThenByDescending(x => x.jh.JobNo)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var invoiceIds = pagedRows
                    .Where(r => r.invoice != null)
                    .Select(r => r.invoice!.Id)
                    .Distinct()
                    .ToList();

                var invoiceDetails = await _context.InvoiceDetails
                    .Where(d => invoiceIds.Contains(d.InvoiceId))
                    .ToListAsync();

                int srNo = (filter.PageIndex - 1) * filter.PageSize + 1;
                var data = pagedRows.Select(r =>
                {
                    var details = r.invoice != null
                        ? invoiceDetails.Where(d => d.InvoiceId == r.invoice.Id).ToList()
                        : new List<InvoiceDetail>();

                    var sparesAmount = details.Sum(d => (d.Quantity ?? 0) * (d.Rate ?? 0));
                    var taxAmount = r.invoice?.TaxAmount ?? 0;
                    var totalAmount = r.invoice?.TotalAmount ?? 0;

                    return new JobReportViewModel
                    {
                        SrNo = srNo++,
                        InvoiceNo = r.invoice != null ? Convert.ToInt32(r.invoice.DocumentNo) : 0,
                        InvoiceDate = r.invoice != null ? r.invoice.CreatedDate : r.jh.CreatedDate,
                        JobNo = r.jh.JobNo ?? 0,
                        PartyName = r.jc.CustomerName,
                        PartyMobileNo = r.jc.CustomerMobile,
                        RegNo = r.jc.RegisterNo,
                        MechanicName = r.jh.Technician,
                        InvoiceType = r.invoice?.InvoiceType ?? r.jt.JobTypeName,
                        InvoiceMode = r.invoice?.ServiceType ?? r.sh.ServiceHeadName,
                        JobType = r.jt.JobTypeName,
                        ServiceHead = r.sh.ServiceHeadName,
                        ServiceType = r.st.ServiceTypeName,
                        ChassisNo = r.jh.Chassisno,
                        DealerCode = r.jh.DealerCode,
                        ServiceLocation = r.jh.Serviceloc,
                        SparesAmount = sparesAmount,
                        AcsrAmount = 0,
                        OilAmount = 0,
                        LabourAmount = 0,
                        OutsideWorkAmount = 0,
                        TaxableAmount = totalAmount,
                        SGSTAmount = taxAmount / 2,
                        CGSTAmount = taxAmount / 2,
                        JobInDate = r.jh.JobinDate.HasValue
                            ? r.jh.JobinDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        EstimatedDeliveryDate = r.jh.EstdelDate.HasValue
                            ? r.jh.EstdelDate.Value.ToDateTime(TimeOnly.MinValue) : null
                    };
                }).ToList();

                return new JobReportPagedResponse<JobReportViewModel>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    TotalSpares = data.Sum(x => x.SparesAmount),
                    TotalAcsr = data.Sum(x => x.AcsrAmount),
                    TotalOil = data.Sum(x => x.OilAmount),
                    TotalLabour = data.Sum(x => x.LabourAmount),
                    TotalOutsideWork = data.Sum(x => x.OutsideWorkAmount),
                    TotalTaxable = data.Sum(x => x.TaxableAmount),
                    TotalSGST = data.Sum(x => x.SGSTAmount),
                    TotalCGST = data.Sum(x => x.CGSTAmount),
                    GrandTotal = data.Sum(x => x.TaxableAmount + x.SGSTAmount + x.CGSTAmount)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching job report: " + ex.Message, ex);
            }
        }

        public async Task<List<DealerWiseJobReportSummary>> GetDealerWiseJobReportAsync(
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var query = BuildJobReportQuery();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                    query = query.Where(x => x.DealerCode == dealerCode);

                if (fromDate.HasValue)
                    query = query.Where(x => x.InvoiceDate >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(x => x.InvoiceDate <= toDate.Value.Date.AddDays(1));

                var data = await query.ToListAsync();

                return data
                    .GroupBy(x => x.DealerCode)
                    .Select(g => new DealerWiseJobReportSummary
                    {
                        DealerCode = g.Key,
                        DealerName = g.First().PartyName,
                        TotalJobs = g.Count(),
                        TotalSpares = g.Sum(x => x.SparesAmount),
                        TotalLabour = g.Sum(x => x.LabourAmount),
                        TotalTaxable = g.Sum(x => x.TaxableAmount),
                        TotalSGST = g.Sum(x => x.SGSTAmount),
                        TotalCGST = g.Sum(x => x.CGSTAmount),
                        GrandTotal = g.Sum(x => x.TaxableAmount + x.SGSTAmount + x.CGSTAmount),
                        JobDetails = g.ToList()
                    })
                    .OrderByDescending(x => x.GrandTotal)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching dealer wise job report", ex);
            }
        }

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetJobReportByDealerAsync(
            string dealerCode,
            int pageIndex,
            int pageSize,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var filter = new JobReportFilterModel
            {
                DealerCode = dealerCode,
                PageIndex = pageIndex,
                PageSize = pageSize,
                FromDate = fromDate,
                ToDate = toDate
            };

            return await GetJobReportAsync(filter);
        }

        public async Task<JobReportPagedResponse<JobReportViewModel>> GetFilteredJobReportAsync(
            JobReportFilterModel filter)
        {
            return await GetJobReportAsync(filter);
        }

        public async Task<List<JobReportViewModel>> GetJobReportForExportAsync(
            string dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var query = BuildJobReportQuery();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                    query = query.Where(x => x.DealerCode == dealerCode);

                if (fromDate.HasValue)
                    query = query.Where(x => x.InvoiceDate >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(x => x.InvoiceDate <= toDate.Value.Date.AddDays(1));

                return await query
                    .OrderByDescending(x => x.InvoiceDate)
                    .ThenByDescending(x => x.JobNo)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error exporting report", ex);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // VEHICLE SALE REPORT  (unchanged from VehicleSaleReportRepo)
        // ═════════════════════════════════════════════════════════════════════

        public async Task<List<VehicleSaleReportViewModel>> GetVehicleSaleReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode)
        {
            var query =
                from h in _context.VehicleSaleBillHeaders

                join d in _context.VehicleSaleBillDetails
                    on h.Id equals d.VehicleSaleBillId

                join vi in _context.VehicleInwards
                    on d.ChassisNo equals vi.ChasisNo into viJoin
                from vi in viJoin.DefaultIfEmpty()

                join im in _context.ItemMasters
                    on d.ItemCode equals im.Itemcode into imJoin
                from im in imJoin.DefaultIfEmpty()

                join dm in _context.DealerMasters
                    on h.DealerCode equals dm.Dealercode into dmJoin
                from dm in dmJoin.DefaultIfEmpty()

                join lm in _context.LedgerMasters
                    on h.LedgerId equals lm.Id into lmJoin
                from lm in lmJoin.DefaultIfEmpty()

                join city in _context.Cities
                    on lm.City equals city.CityId into cityJoin
                from city in cityJoin.DefaultIfEmpty()

                join state in _context.States
                    on lm.State equals state.StateId into stateJoin
                from state in stateJoin.DefaultIfEmpty()

                join clr in _context.ColorMasters
                    on vi.ColrCode equals clr.Colorcode into clrJoin
                from clr in clrJoin.DefaultIfEmpty()

                join fnr in _context.LedgerMasters
                 on h.Financier equals fnr.Id into fnrJoin
                 from fnr in fnrJoin.DefaultIfEmpty()

                where
                    (!fromDate.HasValue || h.SaleDate.Date >= fromDate.Value.Date)
                    && (!toDate.HasValue || h.SaleDate.Date <= toDate.Value.Date)
                    && (string.IsNullOrEmpty(dealerCode) || h.DealerCode == dealerCode)

                select new VehicleSaleReportViewModel
                {
                    SrNo = d.Id,
                    ModelCode = im.Itemcode,
                    ModelDescription = im.Itemname,
                    OemModelName = im.Oemmodelname,
                    VehicleGroup = im.Itemdesc,
                    ColorCode = clr.Colorname,
                    ChasisNo = d.ChassisNo,
                    RegNo = d.RegNo,
                    DealerCode = dm.Dealercode,
                    DealerName = dm.Compname,
                    DealerCity = dm.City,
                    DealerState = dm.State,
                    Location = h.Location,
                    LocCode = vi.LocCode,
                    LocCity = dm.City,
                    Name = h.CustomerName,
                    Address1 = lm.Address,
                    Address2 = "",
                    CustomerState = state.StateName,
                    CustomerCity = city.CityName,
                    Pin = lm.Pin,
                    Email = lm.EMail,
                    MobileNo = lm.MobileNumber,
                    Type = h.CustomerType,
                    BookingId = h.BookingId,
                    DispatchDate = vi.InvoiceDate.HasValue
                        ? vi.InvoiceDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null,
                    SaleDate = h.SaleDate,
                    InvoiceNo = h.SaleBillNo,
                    BillType = h.BillType,
                    FinanceBy = fnr.LedgerName,
                    FinancerCode = "",
                    FinancerCategory = "",
                    ExecutiveName = h.SalesExecutive,
                    ProspectName = h.RefName,
                    ProspectMobNo = "",
                    MotorNumber = vi.MotorNo,
                    BatteryNo = vi.BatteryNo,
                    BatteryNo2 = vi.BatteryNo2,
                    BatteryNo3 = vi.BatteryNo3,
                    BatteryNo4 = vi.BatteryNo4,
                    BatteryNo5 = vi.BatteryNo5,
                    BatteryNo6 = vi.BatteryNo6,
                    BatteryCapacity = vi.BatteryCapacity,
                    SubsidyAmount = im.Fame2amount,
                    FameIIRequired = im.Fame2amount > 0,
                    TotalAmount = h.TotalAmount,
                    BillDate = h.CreatedDate
                };

            return await query
                .OrderByDescending(x => x.BillDate)
                .ToListAsync();
        }

        public async Task<PagedResponse<CurrentStockReportViewModel>> GetCurrentStockReportAsync(CurrentStockFilterModel filter)
        {
            try
            {
                var query =
                    from vi in _context.VehicleInwards.AsNoTracking()

                    join dm in _context.DealerMasters.AsNoTracking()
                        on vi.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    join im in _context.ItemMasters.AsNoTracking()
                        on vi.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join cm in _context.ColorMasters.AsNoTracking()
                        on vi.ColrCode equals cm.Colorcode into cmJoin
                    from cm in cmJoin.DefaultIfEmpty()

                    join vsd in _context.VehicleSaleBillDetails.AsNoTracking()
                        on vi.ChasisNo equals vsd.ChassisNo into saleJoin
                    from vsd in saleJoin.DefaultIfEmpty()

                    select new
                    {
                        Vehicle = vi,
                        Dealer = dm,
                        Item = im,
                        Color = cm,
                        Sale = vsd
                    };

                // Filters

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                {
                    query = query.Where(x =>
                        x.Vehicle.DealerCode == filter.DealerCode);
                }

                if (!string.IsNullOrWhiteSpace(filter.ModelCode))
                {
                    query = query.Where(x =>
                        x.Vehicle.ItemCode == filter.ModelCode);
                }

                if (!string.IsNullOrWhiteSpace(filter.ColorCode))
                {
                    query = query.Where(x =>
                        x.Vehicle.ColrCode == filter.ColorCode);
                }

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                {
                    query = query.Where(x =>
                        x.Vehicle.ChasisNo != null &&
                        x.Vehicle.ChasisNo.Contains(filter.ChassisNo));
                }

                if (!string.IsNullOrWhiteSpace(filter.StockStatus))
                {
                    if (filter.StockStatus == "Billed")
                    {
                        query = query.Where(x => x.Sale != null);
                    }
                    else if (filter.StockStatus == "In Stock")
                    {
                        query = query.Where(x => x.Sale == null);
                    }
                }

                if (filter.IsBilled.HasValue)
                {
                    query = query.Where(x =>
                        (x.Sale != null) == filter.IsBilled.Value);
                }

                var rawData = await query.ToListAsync();

                var result = rawData.Select((x, index) =>
                {
                    DateTime? invoiceDate = null;

                    if (x.Vehicle.InvoiceDate.HasValue)
                    {
                        invoiceDate =
                            x.Vehicle.InvoiceDate.Value
                                .ToDateTime(TimeOnly.MinValue);
                    }

                    return new CurrentStockReportViewModel
                    {
                        SrNo = index + 1,

                        DealerCode = x.Vehicle.DealerCode,

                        DealerName = x.Dealer != null
                            ? x.Dealer.Compname
                            : "",

                        ModelCode = x.Vehicle.ItemCode,

                        ModelName = x.Item != null
                            ? x.Item.Itemname
                            : "",

                        OEMModelName = x.Item != null
                            ? x.Item.Oemmodelname
                            : "",

                        ColorCode = x.Vehicle.ColrCode,

                        ColorName = x.Color != null
                            ? x.Color.Colorname
                            : "",

                        ChassisNo = x.Vehicle.ChasisNo,

                        MotorNo = x.Vehicle.MotorNo,

                        BatteryNo = x.Vehicle.BatteryNo,
                        BatteryNo2 = x.Vehicle.BatteryNo2,
                        BatteryNo3 = x.Vehicle.BatteryNo3,
                        BatteryNo4 = x.Vehicle.BatteryNo4,
                        BatteryNo5 = x.Vehicle.BatteryNo5,
                        BatteryNo6 = x.Vehicle.BatteryNo6,

                        ChargerNo = x.Vehicle.ChargerNo,

                        ControllerNo = x.Vehicle.ControllerNo,

                        RegisterNo = x.Sale != null
                            ? x.Sale.RegNo
                            : "",

                        InvoiceNo = x.Vehicle.InvoiceNo,

                        DispatchDate = invoiceDate,

                        ReceiveDate = invoiceDate,

                        VehicleStatus = "Available",

                        StockStatus = x.Sale != null
                            ? "Billed"
                            : "In Stock",

                        Location = x.Vehicle.LocCode,

                        CurrentLocation = x.Vehicle.LocCode,

                        PurchaseRate = 0,

                        EstimatedSaleRate = 0,

                        IsBilled = x.Sale != null,

                        DaysInStock = invoiceDate.HasValue
                            ? (DateTime.Now - invoiceDate.Value).Days
                            : 0
                    };
                });

                // Date Filters after memory conversion

                if (filter.FromDate.HasValue)
                {
                    result = result.Where(x =>
                        x.ReceiveDate.HasValue &&
                        x.ReceiveDate.Value.Date >=
                        filter.FromDate.Value.Date);
                }

                if (filter.ToDate.HasValue)
                {
                    result = result.Where(x =>
                        x.ReceiveDate.HasValue &&
                        x.ReceiveDate.Value.Date <=
                        filter.ToDate.Value.Date);
                }

                var totalRecords = result.Count();

                var data = result
                    .OrderByDescending(x => x.ReceiveDate)
                    .ThenBy(x => x.DealerName)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                return new PagedResponse<CurrentStockReportViewModel>
                {
                    Data = data,
                    TotalRecords = totalRecords
                };
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error fetching current stock report: "
                    + ex.Message,
                    ex);
            }
        }

        public async Task<PagedResponse<POTrackingReportViewModel>> GetPOTrackingReportAsync(POTrackingFilterModel filter)
        {
            try
            {
                var query =
                    from po in _context.PurchaseOrders

                    join dealer in _context.DealerMasters
                        on po.CustomerCode equals dealer.Dealercode
                        into dealerGroup

                    from dealer in dealerGroup.DefaultIfEmpty()

                    join loc in _context.LocationMasters
                        on po.LocCode equals loc.Loccode
                        into locationGroup

                    from loc in locationGroup.DefaultIfEmpty()

                    select new
                    {
                        po,
                        dealer,
                        loc
                    };

                // =====================================================
                // FILTERS
                // =====================================================

                if (!string.IsNullOrWhiteSpace(
                    filter.DealerCode))
                {
                    query = query.Where(x =>
                        x.po.CustomerCode ==
                        filter.DealerCode);
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(x =>
                        x.po.PurchaseDate >=
                        filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(x =>
                        x.po.PurchaseDate <=
                        filter.ToDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(
                    filter.POType))
                {
                    query = query.Where(x =>
                        x.po.OrderType ==
                        filter.POType);
                }

                if (!string.IsNullOrWhiteSpace(filter.POStatus))
                {
                    bool isActive = filter.POStatus == "Active";
                    query = query.Where(x => x.po.Status == isActive);
                }

                // =====================================================
                // TOTAL RECORDS
                // =====================================================

                var totalRecords =
                    await query.CountAsync();

                // =====================================================
                // PAGINATION
                // =====================================================

                var rawData =
                    await query
                    .OrderByDescending(x =>
                        x.po.PurchaseDate)
                    .Skip(
                        (filter.PageIndex - 1)
                        * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                int srNo =
                    ((filter.PageIndex - 1)
                    * filter.PageSize) + 1;

                // =====================================================
                // FINAL RESULT
                // =====================================================

                var result =
                    rawData.Select(x =>
                    {
                        // =============================================
                        // PURCHASE ORDER DETAILS
                        // =============================================

                        var poDetails =
                            _context.PurchaseOrderDetails
                            .Where(d =>
                                d.Ponumber ==
                                x.po.Ponumber)
                            .ToList();

                        // =============================================
                        // QTY CALCULATIONS
                        // =============================================

                        decimal poQty =
                            poDetails.Sum(d =>
                                d.Qty ?? 0);

                        // =============================================
                        // PRICE CALCULATIONS
                        // =============================================

                        decimal poPrice =
                            poDetails.Sum(d =>
                                d.LineAmount ?? 0);

                        // =============================================
                        // TEMP VALUES
                        // =============================================

                        decimal billedQty = 0;

                        decimal billedPrice = 0;

                        decimal pendingQty =
                            poQty - billedQty;

                        decimal pendingPrice =
                            poPrice - billedPrice;

                        // =============================================
                        // RETURN VIEWMODEL
                        // =============================================

                        return new
                            POTrackingReportViewModel
                        {
                            // =========================================
                            // BASIC DETAILS
                            // =========================================

                            SrNo = srNo++,

                            DealerName =
                                x.dealer != null
                                ? x.dealer.Compname
                                : "",

                            DealerCode =
                                x.po.CustomerCode,

                            LocationName =
                                x.loc != null
                                ? x.loc.Locname
                                : "",

                            // =========================================
                            // ORDER DETAILS
                            // =========================================

                            OrderNumber =
                                x.po.Ponumber,

                            OrderDate =
                                x.po.PurchaseDate,

                            SubmitToERPDate =
                                x.po.UpdatedDate,

                            POType =
                                x.po.OrderType,

                            // =========================================
                            // QUANTITY DETAILS
                            // =========================================

                            POQty =
                                poQty,

                            BilledQty =
                                billedQty,

                            PendingQty =
                                pendingQty,

                            Archived = 0,

                            // =========================================
                            // PRICE DETAILS
                            // =========================================

                            POPrice =
                                poPrice,

                            BilledPrice =
                                billedPrice,

                            PendingPOPrice =
                                pendingPrice,

                            ArchivedPriceExclGST = 0,

                            // =========================================
                            // STATUS DETAILS
                            // =========================================

                            POStatus =
                                x.po.Status
                                ? "Active"
                                : "Inactive",

                            UniqueId =
                                x.po.Id.ToString(),

                            DealerPONo =
                                x.po.Ponumber,

                            // =========================================
                            // PAYMENT DETAILS
                            // =========================================

                            WalletDebit = 0,

                            PGDebit = 0,

                            PGStatus = "",

                            PaymentLink = "",

                            PaymentType = "",

                            TempPONo =
                                x.po.Ponumber,

                            MerchantOrderNo = "",

                            MerchantOrderStatus = ""
                        };
                    })
                    .ToList();

                // =====================================================
                // RESPONSE
                // =====================================================

                return new
                    PagedResponse<
                        POTrackingReportViewModel>
                {
                    Data = result,

                    TotalRecords =
                        totalRecords
                };
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error fetching PO Tracking Report",
                    ex);
            }
        }

        public async Task<List<string>> GetPOTypeDropdownAsync()
        {
            return await _context.PurchaseOrders
                .Where(x => x.OrderType != null && x.OrderType != "")
                .Select(x => x.OrderType!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<List<string>> GetPOStatusDropdownAsync()
        {

            // PO Status is derived from bool Status field
            // Active / Inactive — return fixed meaningful labels
            return await Task.FromResult(new List<string>
            {
                "Active",
                "Inactive"
            });
        }



        // ═════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ═════════════════════════════════════════════════════════════════════

        private IQueryable<JobReportViewModel> BuildJobReportQuery()
        {
            var query =
                from jh in _context.JobCardHeaders
                join jc in _context.JobCardCustomers
                    on jh.Id equals jc.JobCardHeaderId
                join jt in _context.JobTypes
                    on jh.Jobtype equals jt.Id
                join sh in _context.ServiceHeads
                    on jh.Servicehead equals sh.Id
                join st in _context.ServiceTypes
                    on jh.Servicetype equals st.Id
                join inv in _context.InvoiceHeaders
                    on jh.Id equals inv.ReferenceId into invGroup
                from invoice in invGroup.DefaultIfEmpty()
                select new { jh, jc, jt, sh, st, invoice };

            var result = query
                .AsEnumerable()
                .Select(x => new JobReportViewModel
                {
                    SrNo = x.jh.Id,
                    InvoiceNo = x.invoice != null ? Convert.ToInt32(x.invoice.DocumentNo) : 0,
                    InvoiceDate = x.invoice != null ? x.invoice.CreatedDate : x.jh.CreatedDate,
                    JobNo = x.jh.JobNo ?? 0,
                    PartyName = x.jc.CustomerName,
                    PartyMobileNo = x.jc.CustomerMobile,
                    RegNo = x.jc.RegisterNo,
                    MechanicName = x.jh.Technician,
                    InvoiceType = x.invoice != null ? x.invoice.InvoiceType : x.jt.JobTypeName,
                    InvoiceMode = x.invoice != null ? x.invoice.ServiceType : x.sh.ServiceHeadName,
                    JobType = x.jt.JobTypeName,
                    ServiceHead = x.sh.ServiceHeadName,
                    ServiceType = x.st.ServiceTypeName,
                    ChassisNo = x.jh.Chassisno,
                    DealerCode = x.jh.DealerCode,
                    ServiceLocation = x.jh.Serviceloc,
                    SparesAmount = x.invoice != null
                        ? _context.InvoiceDetails
                            .Where(d => d.InvoiceId == x.invoice.Id)
                            .Sum(d => ((d.Quantity ?? 0) * (d.Rate ?? 0)))
                        : 0,
                    AcsrAmount = 0,
                    OilAmount = 0,
                    LabourAmount = 0,
                    OutsideWorkAmount = 0,
                    TaxableAmount = x.invoice != null ? x.invoice.TotalAmount ?? 0 : 0,
                    SGSTAmount = x.invoice != null ? (x.invoice.TaxAmount ?? 0) / 2 : 0,
                    CGSTAmount = x.invoice != null ? (x.invoice.TaxAmount ?? 0) / 2 : 0,
                    JobInDate = x.jh.JobinDate.HasValue
                        ? x.jh.JobinDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    EstimatedDeliveryDate = x.jh.EstdelDate.HasValue
                        ? x.jh.EstdelDate.Value.ToDateTime(TimeOnly.MinValue) : null
                });

            return result.AsQueryable();
        }

        public async Task<List<PartsDispatchReportViewModel>> GetPartsDispatchReportAsync( DateTime? fromDate, DateTime? toDate, string? dealerCode)
        {
            try
            {
                var query = await (
                    from vd in _context.VehicleInwards.AsNoTracking()

                    join dm in _context.DealerMasters.AsNoTracking()
                        on vd.DealerCode equals dm.Dealercode into dealerGroup
                    from dm in dealerGroup.DefaultIfEmpty()

                    join im in _context.ItemMasters.AsNoTracking()
                        on vd.ItemCode equals im.Itemcode into itemGroup
                    from im in itemGroup.DefaultIfEmpty()

                    join jc in _context.JobCardCustomers.AsNoTracking()
                        on vd.ChasisNo equals jc.ChassisNo into customerGroup
                    from jc in customerGroup.DefaultIfEmpty()

                    join jh in _context.JobCardHeaders.AsNoTracking()
                        on jc.JobCardHeaderId equals jh.Id into jobGroup
                    from jh in jobGroup.DefaultIfEmpty()

                    select new PartsDispatchReportViewModel
                    {
                        DealerName = dm != null
                            ? dm.Compname
                            : null,

                        DealerCode = dm != null
                            ? dm.Dealercode
                            : null,

                        CustomerName = jc != null
                            ? jc.CustomerName
                            : null,

                        MobileNo = jc != null
                            ? jc.CustomerMobile
                            : null,

                        City = dm != null
                            ? dm.City
                            : null,

                        State = dm != null
                            ? dm.State
                            : null,

                        VehicleModel = im != null
                            ? im.Itemname
                            : null,

                        VehicleVIN = vd.ChasisNo,

                        BatteryMasterName = vd.BatteryMake,

                        DateOfSale = jc != null
                            ? jc.SaleDate
                            : null,

                        PartName = im != null
                            ? im.Itemname
                            : null,

                        DeviceGroup = im != null
                            ? im.Itemtype.ToString()
                            : null,

                        DeviceType = im != null
                            ? im.Vehtype.ToString()
                            : null,

                        ItemDescription = im != null
                            ? im.Itemdesc
                            : null,

                        VehicleStandardWarrantyMonths = 36,

                        VehicleStandardWarrantyODOReading = 30000,

                        VehicleExtendedWarrantyMonths = 12,

                        VehicleExtendedWarrantyODOReading = 10000,

                        StandardWarrantyExpiryDate =
                            jc != null &&
                            jc.SaleDate.HasValue
                                ? jc.SaleDate.Value.AddMonths(36)
                                : null,

                        ExtendedWarrantyExpiryDate =
                            jc != null &&
                            jc.SaleDate.HasValue
                                ? jc.SaleDate.Value.AddMonths(48)
                                : null,

                        LastODOReadingDate =
                            jh != null
                                ? jh.CreatedDate
                                : null,

                        ODOReadingLastDate =
                            jh != null
                                ? (jh.Vehiclekms ?? 0)
                                : 0,

                        CurrentWarrantyStatusDate =
                            jc != null &&
                            jc.SaleDate.HasValue &&
                            jc.SaleDate.Value.AddMonths(36)
                                >= DateTime.Now
                                    ? "In Warranty"
                                    : "Expired",

                        CurrentWarrantyStatusODO =
                            jh != null &&
                            (jh.Vehiclekms ?? 0) <= 30000
                                ? "In Warranty"
                                : "Expired",

                        FinalWarrantyStatus =
                            jc != null &&
                            jc.SaleDate.HasValue &&
                            jc.SaleDate.Value.AddMonths(36)
                                >= DateTime.Now
                            &&
                            jh != null &&
                            (jh.Vehiclekms ?? 0) <= 30000
                                ? "Active"
                                : "Expired"
                    }).ToListAsync();

                // Dealer Filter
                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    query = query
                        .Where(x => x.DealerCode == dealerCode)
                        .ToList();
                }

                // From Date
                if (fromDate.HasValue)
                {
                    query = query
                        .Where(x =>
                            x.DateOfSale.HasValue &&
                            x.DateOfSale.Value.Date >=
                            fromDate.Value.Date)
                        .ToList();
                }

                // To Date
                if (toDate.HasValue)
                {
                    query = query
                        .Where(x =>
                            x.DateOfSale.HasValue &&
                            x.DateOfSale.Value.Date <=
                            toDate.Value.Date)
                        .ToList();
                }

                int srNo = 1;

                query = query
                    .OrderByDescending(x => x.DateOfSale)
                    .ToList();

                query.ForEach(x =>
                {
                    x.SrNo = srNo++;
                });

                return query;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while fetching Parts Dispatch Report",
                    ex);
            }
        }

        public async Task<List<object>> GetDealerListAsync()
        {
            try
            {
                return await _context.DealerMasters
                    .AsNoTracking()
                    .Select(x => new
                    {
                        dealerCode = x.Dealercode,
                        dealerName = x.Compname
                    })
                    .OrderBy(x => x.dealerName)
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while fetching dealer list",
                    ex);
            }
        }

        public async Task<List<object>> GetModelListAsync()
        {
            try
            {
                var data = await (

                    from vi in _context.VehicleInwards

                    join im in _context.ItemMasters
                        on vi.ItemCode equals im.Itemcode

                    where
                        !string.IsNullOrEmpty(im.Itemname)

                        // REMOVE ASSEMBLY / PART ITEMS

                        && !im.Itemname.Contains("BRAKE")
                        && !im.Itemname.Contains("LEVER")
                        && !im.Itemname.Contains("BUZZER")
                        && !im.Itemname.Contains("CONTROLLER")
                        && !im.Itemname.Contains("HARNESS")
                        && !im.Itemname.Contains("CABLE")
                        && !im.Itemname.Contains("SWITCH")
                        && !im.Itemname.Contains("FOOTREST")
                        && !im.Itemname.Contains("DRUM")
                        && !im.Itemname.Contains("TOOL")
                        && !im.Itemname.Contains("PART")
                        && !im.Itemname.Contains("KIT")

                    select new
                    {
                        modelCode = im.Itemcode,
                        modelName = im.Itemname
                    }

                )
                .Distinct()
                .OrderBy(x => x.modelName)
                .ToListAsync();

                return data
                    .Cast<object>()
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while fetching model list",
                    ex);
            }
        }

        public async Task<List<object>> GetModelListByDealerAsync(string dealerCode)
        {
            try
            {
                var data = await (

                    from vi in _context.VehicleInwards

                    join im in _context.ItemMasters
                        on vi.ItemCode equals im.Itemcode

                    where vi.DealerCode == dealerCode

                    select new
                    {
                        modelCode = im.Itemcode,
                        modelName = im.Itemname
                    }

                )
                .Distinct()
                .OrderBy(x => x.modelName)
                .ToListAsync();

                return data
                    .Cast<object>()
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error fetching model list",
                    ex);
            }
        }
        public async Task<List<string>> GetChassisListAsync()
        {
            try
            {
                return await _context.VehicleInwards
                    .AsNoTracking()
                    .Where(x => x.ChasisNo != null)
                    .Select(x => x.ChasisNo!)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while fetching chassis list",
                    ex);
            }
        }

        public async Task<List<PartDispatchKitReportViewModel>> GetPartDispatchKitReportAsync(DateTime? fromDate, DateTime? toDate, string? dealerCode)
        {
            try
            {
                var query =

                    from po in _context.PurchaseOrders

                    join dealer in _context.DealerMasters
                        on po.CustomerCode equals dealer.Dealercode
                        into dealerGroup

                    from dealer in dealerGroup.DefaultIfEmpty()

                    where po.Id > 0

                    select new PartDispatchKitReportViewModel
                    {
                        // =====================================
                        // PO DETAILS
                        // =====================================

                        PONumber = po.Ponumber,

                        PODate = po.PurchaseDate,

                        SubmitToERPDate = po.UpdatedDate,

                        POType =
                                (po.TransactionType ?? "") +
                                "-" +
                                (po.OrderType ?? ""),

                        // =====================================
                        // DEALER DETAILS
                        // =====================================

                        CompanyName = dealer != null
                            ? dealer.Compname
                            : "",

                        MobileNo = dealer != null
                            ? dealer.Mobile
                            : "",

                        DealerCode = dealer != null
                            ? dealer.Dealercode
                            : "",

                        DealerCity = dealer != null
                            ? dealer.City
                            : "",

                        DealerState = dealer != null
                            ? dealer.State
                            : "",

                        // =====================================
                        // LOCATION
                        // =====================================

                        LocationCode = po.LocCode,

                        LocationName = dealer != null
                            ? dealer.Compname
                            : "",

                        LocationCity = dealer != null
                            ? dealer.City
                            : ""
                    };

                // =========================================
                // DEALER FILTER
                // =========================================

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    query = query.Where(x =>
                        x.DealerCode == dealerCode);
                }


                // =========================================
                // FROM DATE
                // =========================================

                if (fromDate.HasValue)
                {
                    query = query.Where(x =>
                        x.PODate.HasValue &&
                        x.PODate.Value.Date >=
                        fromDate.Value.Date);
                }

                // =========================================
                // TO DATE
                // =========================================

                if (toDate.HasValue)
                {
                    query = query.Where(x =>
                        x.PODate.HasValue &&
                        x.PODate.Value.Date <=
                        toDate.Value.Date);
                }

                var result = await query
                    .OrderByDescending(x => x.PODate)
                    .ToListAsync();

                // =========================================
                // SR NO
                // =========================================

                for (int i = 0; i < result.Count; i++)
                {
                    result[i].SrNo = i + 1;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error fetching Part Dispatch Kit Report",
                    ex
                );
            }
        }

        public async Task<List<string>> GetPartDispatchKitPOTypeDropdownAsync()
        {
            try
            {
                return await _context.PurchaseOrders

                    .Where(x =>
                        x.TransactionType != null &&
                        x.OrderType != null)

                    .Select(x =>
                        x.TransactionType + "-" + x.OrderType)

                    .Distinct()

                    .OrderBy(x => x)

                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        //FORM22 REPORT FOR VEHICLE SALE BILL
        public async Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo)
        {
            try
            {
                var data = await (
                     from vi in _context.VehicleInwards
                     join im in _context.ItemMasters
                     on vi.ItemCode equals im.Itemcode

                     join f2 in _context.Form22Masters
                     on (im.Oemmodelname ?? "").Trim().ToLower()
                     equals (f2.OemModelName ?? "").Trim().ToLower()
                     into formGroup

                     from f2 in formGroup.DefaultIfEmpty()

                     where vi.ChasisNo == chassisNo

                     select new Form22SlipViewModel
                     {
                         ChassisNo = chassisNo,
                         TypeApprovalCertNo = f2.ApprovalCertificateNo,
                         BrandName = im.Itemname,
                         MotorNo = vi.MotorNo,
                         Emission = "",
                         SoundLevelHorn = f2.SoundLevelHorn,
                         NoiseLevel = f2.PassbyNoiseLevel

                     }
                     ).FirstOrDefaultAsync();
                return data;
            }
            catch
            {
                throw;
            }
        }

        public async Task<VehicleSaleBillReportResponse> GetVehicleSaleBillReportAsync(VehicleSaleBillReportFilterModel filter)
        {
            try
            {
                var query =
                    from vd in _context.VehicleSaleBillDetails

                    join vh in _context.VehicleSaleBillHeaders
                        on vd.VehicleSaleBillId equals vh.Id

                    join vi in _context.VehicleInwards
                        on vd.ChassisNo equals vi.ChasisNo into viJoin
                    from vi in viJoin.DefaultIfEmpty()

                    join im in _context.ItemMasters
                        on vd.ItemCode equals im.Itemcode into imJoin
                    from im in imJoin.DefaultIfEmpty()

                    join clr in _context.ColorMasters
                        on vi.ColrCode equals clr.Colorcode into clrJoin
                    from clr in clrJoin.DefaultIfEmpty()

                    join dm in _context.DealerMasters
                        on vh.DealerCode equals dm.Dealercode into dmJoin
                    from dm in dmJoin.DefaultIfEmpty()

                    join cust in _context.LedgerMasters
                        on vh.LedgerId equals cust.Id into custJoin
                    from cust in custJoin.DefaultIfEmpty()

                    join fin in _context.LedgerMasters
                        on vh.Financier equals fin.Id into finJoin
                    from fin in finJoin.DefaultIfEmpty()

                    join city in _context.Cities
                        on cust.City equals city.CityId into cityJoin
                    from city in cityJoin.DefaultIfEmpty()

                    join state in _context.States
                        on cust.State equals state.StateId into stateJoin
                    from state in stateJoin.DefaultIfEmpty()

                    join inv in _context.InvoiceHeaders
                        on vd.VehicleSaleBillId equals inv.ReferenceId into invJoin
                    from inv in invJoin.DefaultIfEmpty()

                    select new VehicleSaleBillReportViewModel
                    {
                        SaleBillId = vh.Id,
                        SaleBillNo = vh.SaleBillNo,
                        SaleDate = vh.SaleDate,
                        Status = vh.Status,
                        Location = vh.Location,
                        DealerCode = vh.DealerCode,
                        DealerName = dm.Compname,
                        CustomerName = vh.CustomerName ?? cust.LedgerName,
                        BillingName = vh.BillingName,
                        CustomerType = vh.CustomerType,
                        SaleType = vh.SaleType,
                        BillType = vh.BillType,
                        Financier = fin.LedgerName,
                        SalesExecutive = vh.SalesExecutive,
                        CustomerMobile = cust.MobileNumber,
                        CustomerCity = city.CityName,
                        CustomerState = state.StateName,
                        InvoiceNo = inv.InvoiceNo,

                        ChassisNo = vd.ChassisNo,
                        MotorNo = vi.MotorNo,
                        ItemCode = vd.ItemCode,
                        ModelName = im.Itemname ?? vd.ModelName,
                        OemModelName = im.Oemmodelname,
                        Colour = clr.Colorname ?? vd.Colour,
                        Hsn = im.Hsncode,
                        MfgYear = vd.MfgYear ?? vi.MfgYear,
                        RegNo = vd.RegNo,
                        InsNo = vd.InsNo,

                        ItemRate = vd.ItemRate,
                        PreGstDiscount = vd.PreGstDiscount ?? 0,
                        TaxableAmount = vd.ItemRate - (vd.PreGstDiscount ?? 0),
                        SgstPer = vd.Sgstper ?? 0,
                        SgstAmount = vd.Sgstamnt ?? 0,
                        CgstPer = vd.Cgstper ?? 0,
                        CgstAmount = vd.Cgstamnt ?? 0,
                        IgstPer = vd.Igstper ?? 0,
                        IgstAmount = vd.Igstamnt ?? 0,
                        FameIIDiscount = vd.FameIi ?? 0,
                        RegAmount = vd.RegAmount ?? 0,
                        InsuranceAmount = vd.InsuranceAmount ?? 0,
                        PostGstDiscount = vd.PostGstDisc ?? 0,
                        FinalAmount = vd.FinalAmount,

                        Battery = vd.Battery,
                        ChargerNo = vd.ChargerNo,
                        ControllerNo = vd.ControllerNo,
                        Vcu = vd.Vcu
                    };

                // ── DB-side filters (map to direct columns) ──
                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    query = query.Where(x => x.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                    query = query.Where(x => x.SaleDate.Date >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    query = query.Where(x => x.SaleDate.Date <= filter.ToDate.Value.Date);

                if (!string.IsNullOrWhiteSpace(filter.SaleType))
                    query = query.Where(x => x.SaleType == filter.SaleType);

                if (!string.IsNullOrWhiteSpace(filter.CustomerType))
                    query = query.Where(x => x.CustomerType == filter.CustomerType);

                if (filter.BillType.HasValue)
                    query = query.Where(x => x.BillType == filter.BillType.Value);

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    query = query.Where(x => x.Status == filter.Status);

                if (!string.IsNullOrWhiteSpace(filter.SaleBillNo))
                    query = query.Where(x => x.SaleBillNo != null && x.SaleBillNo.Contains(filter.SaleBillNo));

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                    query = query.Where(x => x.ChassisNo != null && x.ChassisNo.Contains(filter.ChassisNo));

                var rows = await query.ToListAsync();

                // ── Free-text search (in memory) ──
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var s = filter.Search.Trim().ToLower();
                    rows = rows.Where(x =>
                        (x.SaleBillNo ?? "").ToLower().Contains(s) ||
                        (x.CustomerName ?? "").ToLower().Contains(s) ||
                        (x.BillingName ?? "").ToLower().Contains(s) ||
                        (x.ChassisNo ?? "").ToLower().Contains(s) ||
                        (x.ModelName ?? "").ToLower().Contains(s) ||
                        (x.RegNo ?? "").ToLower().Contains(s)
                    ).ToList();
                }

                rows = rows
                    .OrderByDescending(x => x.SaleDate)
                    .ThenByDescending(x => x.SaleBillNo)
                    .ToList();

                var response = new VehicleSaleBillReportResponse
                {
                    TotalRecords = rows.Count,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,

                    TotalItemRate = rows.Sum(x => x.ItemRate),
                    TotalTaxable = rows.Sum(x => x.TaxableAmount),
                    TotalSgst = rows.Sum(x => x.SgstAmount),
                    TotalCgst = rows.Sum(x => x.CgstAmount),
                    TotalIgst = rows.Sum(x => x.IgstAmount),
                    TotalFameII = rows.Sum(x => x.FameIIDiscount),
                    TotalRegistration = rows.Sum(x => x.RegAmount),
                    TotalInsurance = rows.Sum(x => x.InsuranceAmount),
                    GrandTotal = rows.Sum(x => x.FinalAmount)
                };

                var paged = rows
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                int srNo = ((filter.PageIndex - 1) * filter.PageSize) + 1;
                foreach (var r in paged)
                    r.SrNo = srNo++;

                response.Data = paged;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching vehicle sale bill report: " + ex.Message, ex);
            }
        }
    }

}