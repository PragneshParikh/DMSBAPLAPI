using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.HSRPRepo
{
    public class HSRPRepo : IHSRPRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IHttpContextAccessor _httpContext;
        public HSRPRepo(BapldmsvadContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        public async Task<List<VehicleSaleBillResponseViewModel>> GetAllInvoicedVehicleForHSRPOrder(string? dealerCode)
        {
            try
            {
                var result = _context.VehicleSaleBillHeaders
                    .Include(i => i.VehicleSaleBillDetails)
                    .Where(i => i.Status.ToLower() == "invoiced");

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    result = result.Where(i => i.DealerCode == dealerCode);
                }

                var data = await result.
                    Select(i => new VehicleSaleBillResponseViewModel
                    {
                        Id = i.Id,
                        LedgerId = i.LedgerId,
                        SaleDate = i.SaleDate,
                        SaleBillNo = i.SaleBillNo,
                        CustomerType = i.CustomerType,
                        Location = i.Location,
                        SaleType = i.SaleType,
                        CashAccount = i.CashAccount,
                        Financier = i.Financier,
                        BillType = i.BillType,
                        BillFrom = i.BillFrom,
                        CustomerName = i.CustomerName,
                        BillingName = i.BillingName,
                        SalesExecutive = i.SalesExecutive,
                        TempRegNo = i.TempRegNo,
                        BookingId = i.BookingId,
                        PrintType = i.PrintType,
                        RefName = i.RefName,
                        RefAddress = i.RefAddress,
                        RefEmail = i.RefEmail,
                        RefPoint = i.RefPoint,
                        RefRemarks = i.RefRemarks,
                        TotalAmount = i.TotalAmount,
                        Status = i.Status,
                        DealerCode = i.DealerCode,
                        isD2d = i.IsD2d,

                        Details = i.VehicleSaleBillDetails.Select(d => new VehicleSaleBillDetailVM
                        {
                            Id = d.Id,
                            ChassisNo = d.ChassisNo,
                            RegNo = d.RegNo,
                            ItemCode = d.ItemCode,
                            ModelName = d.ModelName,
                            Colour = d.Colour,

                        }).ToList()

                    }
                    ).ToListAsync();
                return data;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<HSRPOrderAddEditViewModel>> GetPendingHSRPListAsync(string? dealerCode,DateTime? fromDate,DateTime? toDate)
        {
            try
            {
                var result = await (from vd in _context.VehicleSaleBillDetails
                             join vh in _context.VehicleSaleBillHeaders
                             on vd.VehicleSaleBillId equals vh.Id into PendingHsrpList
                             from vh in PendingHsrpList.DefaultIfEmpty()
                             join lm in _context.LedgerMasters
                             on vh.LedgerId equals lm.Id into CustomerDetails
                             from lm in CustomerDetails.DefaultIfEmpty()
                             join inv in _context.InvoiceHeaders
                             on vh.SaleBillNo equals inv.DocumentNo into InvoiceDetails
                             from inv in InvoiceDetails.DefaultIfEmpty()
                             join ho in _context.Hsrporders
                             on vd.Id equals ho.SaleBillDetailsId into HSRPDetails
                             from ho in HSRPDetails.DefaultIfEmpty()

                             where vh.Status.ToLower() == "invoiced" && vd.Hsrpstatus == null

                             select new HSRPOrderAddEditViewModel
                             {
                                 ChassisNo = vd.ChassisNo,
                                 RegNo = vd.RegNo,
                                 ModelName = vd.ModelName,
                                 Colour = vd.Colour,
                                 CustomerName = vh.CustomerName,
                                 CustomerMobile = lm.MobileNumber,
                                 InvoiceNo = inv.DocumentNo,
                                 InvoiceDate = inv.CreatedDate,
                                 SaleBillNo = vh.SaleBillNo,
                                 SaleBillDeailsId = vd.Id,
                                 CustomerLedgerId = vh.LedgerId,
                                 //HsrpResponse =ho.Hsrpresponse,
                                 //HsrpStatus=ho.Hsrpstatus


                             }).ToListAsync();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    result = result.Where(i => i.DealerCode == dealerCode).ToList();

                }
                if (fromDate.HasValue) 
                {
                    result =result.Where(i=>i.InvoiceDate >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);

                    result = result
                        .Where(i => i.InvoiceDate != null && i.InvoiceDate <= toDateEnd)
                        .ToList();
                }

                return result.ToList();

            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<Hsrporder>> CreateBulkHSRPOrder(List<HSRPOrderCreateViwModel> order)
        //{
        //    try
        //    {
        //        var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContext.HttpContext);
        //        var orderList = new List<Hsrporder>();
        //        foreach (var item in order)
        //        {
        //            var entity = new Hsrporder
        //            {
        //                ChassisNo = item.ChassisNo,
        //                RegNo = item.RegNo,
        //                InvoiceNo = item.InvoiceNo,
        //                IsFrontPlate = item.IsFrontPlate,
        //                IsRearPlate = item.IsRearPlate,
        //                IsTlpsticker = item.IsTlpsticker,
        //                CustomerLedgerId = item.CustomerLedgerId,
        //                SaleBillDetailsId = item.SaleBillDetailsId,
        //                SupplierLedgerId = item.SupplierLedgerId,
        //                SaleBillNo = item.SaleBillNo,
        //                OrderDate = item.OrderDate,
        //                OrderNo = item.OrderNo,
        //                CreatedBy=userId,
        //                CreatedDate=DateTime.Now,

        //            };

        //            orderList.Add(entity);

        //        }
        //        await _context.AddRangeAsync(orderList);
        //        await _context.SaveChangesAsync();
        //        return orderList;


        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}


        public async Task<List<Hsrporder>> CreateBulkHSRPOrder(List<HSRPOrderCreateViwModel> orders)
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContext.HttpContext);

                // STEP 1: Convert to External API format
                var externalPayload = orders.Select(x => new HsrpExternalRequestViewModel
                {
                    VinNo = x.ChassisNo,
                    EngineNo = "",
                    VehicleRegNo = x.RegNo,
                    VehicleClass = "",
                    VehicleType = "",
                    DateOfReg = DateTime.Now.ToString("yyyy-MM-dd"),
                    FrontLaserCode = x.IsFrontPlate == true ? "YES" : "",
                    RearLaserCode = x.IsRearPlate == true ? "YES" : ""
                }).ToList();

                // STEP 2: Call External API
                //List<dynamic> apiResponse;

                //using (var client = new HttpClient())
                //{
                //    var response = await client.PostAsJsonAsync("https://test-api-url", externalPayload);

                //    if (!response.IsSuccessStatusCode)
                //        throw new Exception("External API Failed");

                //    apiResponse = await response.Content.ReadFromJsonAsync<List<dynamic>>();
                //}

                // STEP 3: Map & Save
                var orderList = new List<Hsrporder>();

                for (int i = 0; i < orders.Count; i++)
                {
                    var item = orders[i];
                    //var apiRes = apiResponse?[i];
                    var apiRes = "Success";

                    var entity = new Hsrporder
                    {
                        ChassisNo = item.ChassisNo,
                        RegNo = item.RegNo,
                        InvoiceNo = item.InvoiceNo,
                        IsFrontPlate = item.IsFrontPlate,
                        IsRearPlate = item.IsRearPlate,
                        IsTlpsticker = item.IsTlpsticker,
                        CustomerLedgerId = item.CustomerLedgerId,
                        SaleBillDetailsId = item.SaleBillDetailsId,
                        SupplierLedgerId = item.SupplierLedgerId,
                        SaleBillNo = item.SaleBillNo,
                        OrderDate = item.OrderDate,
                        OrderNo = item.OrderNo,
                        DealerCode = item.DealerCode,


                        //Hsrpstatus = apiRes?.status ?? "FAILED",
                        Hsrpstatus = "Success",
                        //Hsrpresponse = apiRes != null ? JsonSerializer.Serialize(apiRes) : "No Response",
                        Hsrpresponse = "Success",
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };

                    orderList.Add(entity);
                }

                await _context.Hsrporders.AddRangeAsync(orderList);

                //update vehicleSaleBill Details Table
                var detailIds = orderList.Where(i => i.SaleBillDetailsId.HasValue)
                    .Select(i => i.SaleBillDetailsId.Value).ToList();

                var vehicleDetails = await _context.VehicleSaleBillDetails
                    .Where(v => detailIds.Contains(v.Id))
                    .ToListAsync();

                foreach (var detail in vehicleDetails)
                {
                    var relatedOrder = orderList.FirstOrDefault(o => o.SaleBillDetailsId == detail.Id);

                    if (relatedOrder != null)
                    {
                        detail.Hsrpstatus = relatedOrder.Hsrpstatus;
                        detail.UpdatedBy = userId;
                        detail.UpdatedDate = DateTime.Now;
                    }
                }


                return orderList;
            }
            catch (Exception ex)
            {
                throw new Exception("HSRP Bulk Creation Failed: " + ex.Message);
            }
        }

        public async Task<List<Hsrporder>> UpdateBulkHSRPOrder(List<HSRPOrderCreateViwModel> orders)
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContext.HttpContext);

                //   Step 1: Get valid IDs
                var existingHSRPIDs = orders
                    .Where(i => i.id.HasValue)
                    .Select(i => i.id.Value)
                    .ToList();

                //   Step 2: Fetch existing records
                var existingHSRPOrders = await _context.Hsrporders
                    .Where(i => existingHSRPIDs.Contains(i.Id))
                    .ToListAsync();

                //   Step 3: Call API (optional - same as your logic)
                var externalPayload = orders.Select(x => new HsrpExternalRequestViewModel
                {
                    VinNo = x.ChassisNo,
                    EngineNo = "",
                    VehicleRegNo = x.RegNo,
                    VehicleClass = "",
                    VehicleType = "",
                    DateOfReg = DateTime.Now.ToString("yyyy-MM-dd"),
                    FrontLaserCode = x.IsFrontPlate == true ? "YES" : "",
                    RearLaserCode = x.IsRearPlate == true ? "YES" : ""
                }).ToList();

                //List<dynamic> apiResponse;

                //using (var client = new HttpClient())
                //{
                //    var response = await client.PostAsJsonAsync("https://test-api-url", externalPayload);

                //    if (!response.IsSuccessStatusCode)
                //        throw new Exception("External API Failed");

                //    apiResponse = await response.Content.ReadFromJsonAsync<List<dynamic>>();
                //}


                //   Step 4: Update existing records
                foreach (var existing in existingHSRPOrders)
                {
                    var item = orders.FirstOrDefault(x => x.id == existing.Id);

                    if (item == null) continue;

                    existing.ChassisNo = item.ChassisNo;
                    existing.RegNo = item.RegNo;
                    existing.InvoiceNo = item.InvoiceNo;
                    existing.IsFrontPlate = item.IsFrontPlate;
                    existing.IsRearPlate = item.IsRearPlate;
                    existing.IsTlpsticker = item.IsTlpsticker;
                    existing.CustomerLedgerId = item.CustomerLedgerId;
                    existing.SaleBillDetailsId = item.SaleBillDetailsId;
                    existing.SupplierLedgerId = item.SupplierLedgerId;
                    existing.SaleBillNo = item.SaleBillNo;
                    existing.OrderDate = item.OrderDate;
                    existing.OrderNo = item.OrderNo;

                    // API response
                    existing.Hsrpstatus = "Success";
                    existing.Hsrpresponse = "Success";

                    existing.UpdatedBy = userId;
                    existing.UpdatedDate = DateTime.Now;
                }

                //   Step 5: Update VehicleSaleBillDetails
                var detailIds = existingHSRPOrders
                    .Where(i => i.SaleBillDetailsId.HasValue)
                    .Select(i => i.SaleBillDetailsId.Value)
                    .ToList();

                var vehicleDetails = await _context.VehicleSaleBillDetails
                    .Where(v => detailIds.Contains(v.Id))
                    .ToListAsync();

                foreach (var detail in vehicleDetails)
                {
                    var relatedOrder = existingHSRPOrders
                        .FirstOrDefault(o => o.SaleBillDetailsId == detail.Id);

                    if (relatedOrder != null)
                    {
                        detail.Hsrpstatus = relatedOrder.Hsrpstatus;
                        detail.UpdatedBy = userId;
                        detail.UpdatedDate = DateTime.Now;
                    }
                }

                //   FINAL SAVE
                await _context.SaveChangesAsync();

                return existingHSRPOrders;
            }
            catch (Exception ex)
            {
                throw new Exception("HSRP Bulk Update Failed: " + ex.Message);
            }
        }
        public async Task<string> GetNextOrderNo()
        {
            string orderNo = "ORD1";
            var lastOrder = await _context.Hsrporders.
                OrderByDescending(i => i.Id).FirstOrDefaultAsync();
            if (lastOrder != null && !string.IsNullOrEmpty(lastOrder.OrderNo))
            {
                var numberPart = lastOrder.OrderNo.Replace("ORD", "");

                if (int.TryParse(numberPart, out int lastNumber))
                {
                    orderNo = "ORD" + (lastNumber + 1);
                }
            }
            return orderNo;
        }

        public async Task<List<HSRPListViewModel>> GetAllHSRPOrderAsync(string? dealerCode,DateTime? fromDate,DateTime? toDate)
        {
            try
            {
                var result = await (from o in _context.Hsrporders
                                    join vd in _context.VehicleSaleBillDetails
                                    on o.SaleBillDetailsId equals vd.Id into VehicleDetails
                                    from vd in VehicleDetails.DefaultIfEmpty()

                                    join vh in _context.VehicleSaleBillHeaders
                                    on vd.VehicleSaleBillId equals vh.Id into VehicleHeaderDetails
                                    from vh in VehicleHeaderDetails.DefaultIfEmpty()

                                    join inv in _context.InvoiceHeaders
                                    on vh.SaleBillNo equals inv.DocumentNo into VehicleInvoiceDetails
                                    from inv in VehicleInvoiceDetails.DefaultIfEmpty()

                                    join led in _context.LedgerMasters
                                    on o.CustomerLedgerId equals led.Id into VehicleCustomerDetails
                                    from led in VehicleCustomerDetails.DefaultIfEmpty()

                                    join sup in _context.LedgerMasters
                                    on o.SupplierLedgerId equals sup.Id into VehicleSupplierDetails
                                    from sup in VehicleSupplierDetails.DefaultIfEmpty()

                                    select new HSRPListViewModel
                                    {
                                        id = o.Id,
                                        ChassisNo = o.ChassisNo,
                                        OrderDate = o.OrderDate,
                                        OrderNo = o.OrderNo,
                                        SupplierLedgerId = o.SupplierLedgerId,
                                        SupplierName = sup.LedgerName,
                                        HsrpStatus = o.Hsrpstatus,
                                        HsrpResponse = o.Hsrpresponse,
                                        DealerCode = o.DealerCode,
                                        InwardStatus = o.InwardStatus,
                                        InwardResponse = o.InwardResponse,
                                        //IsFrontPlate =ho.IsFrontPlate,
                                        //IsRearPlate =ho.IsRearPlate,

                                    }
                                    ).ToListAsync();


                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    result = result.Where(i => i.DealerCode == dealerCode).ToList();
                }
                if(toDate.HasValue)
                {
                    result =result.Where(i=>i.OrderDate <= toDate).ToList();
                }
                if (fromDate.HasValue)
                {
                    result  =result.Where(i=>i.OrderDate >= fromDate).ToList();
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<HSRPOrderAddEditViewModel> GetHSRPOrderByIdAsync(int id)
        {
            try
            {
                var result = await (from o in _context.Hsrporders
                                    join vd in _context.VehicleSaleBillDetails
                                    on o.SaleBillDetailsId equals vd.Id into VehicleDetails
                                    from vd in VehicleDetails.DefaultIfEmpty()

                                    join vh in _context.VehicleSaleBillHeaders
                                    on vd.VehicleSaleBillId equals vh.Id into VehicleHeaderDetails
                                    from vh in VehicleHeaderDetails.DefaultIfEmpty()

                                    join inv in _context.InvoiceHeaders
                                    on vh.SaleBillNo equals inv.DocumentNo into VehicleInvoiceDetails
                                    from inv in VehicleInvoiceDetails.DefaultIfEmpty()

                                    join led in _context.LedgerMasters
                                    on o.CustomerLedgerId equals led.Id into VehicleCustomerDetails
                                    from led in VehicleCustomerDetails.DefaultIfEmpty()

                                    join sup in _context.LedgerMasters
                                    on o.SupplierLedgerId equals sup.Id into VehicleSupplierDetails
                                    from sup in VehicleSupplierDetails.DefaultIfEmpty()

                                    where o.Id == id

                                    select new HSRPOrderAddEditViewModel
                                    {
                                        id = o.Id,
                                        ChassisNo = o.ChassisNo,
                                        InvoiceDate = inv.CreatedDate,
                                        InvoiceNo = o.InvoiceNo,
                                        CustomerName = led.LedgerName,
                                        OrderDate = o.OrderDate,
                                        OrderNo = o.OrderNo,
                                        CustomerLedgerId = o.CustomerLedgerId,
                                        SupplierLedgerId = o.SupplierLedgerId,
                                        SupplierName = sup.LedgerName,
                                        HsrpStatus = o.Hsrpstatus,
                                        HsrpResponse = o.Hsrpresponse,
                                        IsFrontPlate = o.IsFrontPlate,
                                        IsRearPlate = o.IsRearPlate,
                                        ModelName = vd.ModelName,
                                        Colour = vd.Colour
                                    }
                                    ).FirstOrDefaultAsync();


                return result;
            }
            catch
            {
                throw;
            }
        }


        public async Task<List<HSRPInward>> GetAllHSRPInward(string? dealerCode,DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var data = await (from o in _context.Hsrporders
                                  join led in _context.LedgerMasters
                                  on o.SupplierLedgerId equals led.Id into supplierDetails
                                  from led in supplierDetails.DefaultIfEmpty()

                                  join cus in _context.LedgerMasters
                                  on o.CustomerLedgerId equals cus.Id into customerDetails
                                  from cus in customerDetails.DefaultIfEmpty()

                                  where (o.InwardStatus == null || o.InwardStatus.ToLower() == "pending") && (o.InwardResponse.ToLower() != "success")

                                  select new HSRPInward
                                  {
                                      id = o.Id,
                                      ChassisNo = o.ChassisNo,
                                      OrderNo = o.OrderNo,
                                      DealerCode = o.DealerCode,
                                      SaleBillDetailsId = o.SaleBillDetailsId,
                                      SaleBillNo = o.SaleBillNo,
                                      OrderDate = o.OrderDate,
                                      CustomerLedgerId = o.CustomerLedgerId,
                                      InvoiceNo = o.InvoiceNo,
                                      IsFrontPlate = o.IsFrontPlate,
                                      IsRearPlate = o.IsRearPlate,
                                      RegNo = o.RegNo,
                                      CustomerName = cus.LedgerName,
                                      SupplierLedgerId = o.SupplierLedgerId,
                                      SupplierName = led.LedgerName,
                                      InwardResponse = o.InwardResponse,
                                      InwardStatus = o.InwardResponse,



                                  }).ToListAsync();

                                if (!string.IsNullOrEmpty(dealerCode))
                                {
                                    data = data.Where(i => i.DealerCode == dealerCode).ToList();
                                }
                if (fromDate.HasValue)
                {
                    data= data.Where(i=>i.OrderDate >= fromDate.Value).ToList();    
                }
                 if (toDate.HasValue)
                {
                    data= data.Where(i=>i.OrderDate <= toDate.Value).ToList();    
                }
                return data;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Hsrporder>> UpdateInwardStatus(List<HSRPInwardUpdate> orders)
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContext.HttpContext);

                //   Step 1: Get valid IDs
                var existingHSRPIDs = orders
                    .Where(i => i.Id.HasValue)
                    .Select(i => i.Id.Value)
                    .ToList();

                //   Step 2: Fetch existing records
                var existingHSRPOrders = await _context.Hsrporders
                    .Where(i => existingHSRPIDs.Contains(i.Id))
                    .ToListAsync();

                //   Step 3: Call API (optional - same as your logic)
                //var externalPayload = orders.Select(x => new HsrpExternalRequestViewModel
                //{
                //    VinNo = x.ChassisNo,
                //    EngineNo = "",
                //    VehicleRegNo = x.RegNo,
                //    VehicleClass = "",
                //    VehicleType = "",
                //    DateOfReg = DateTime.Now.ToString("yyyy-MM-dd"),
                //    FrontLaserCode = x.IsFrontPlate == true ? "YES" : "",
                //    RearLaserCode = x.IsRearPlate == true ? "YES" : ""
                //}).ToList();

                //List<dynamic> apiResponse;

                //using (var client = new HttpClient())
                //{
                //    var response = await client.PostAsJsonAsync("https://test-api-url", externalPayload);

                //    if (!response.IsSuccessStatusCode)
                //        throw new Exception("External API Failed");

                //    apiResponse = await response.Content.ReadFromJsonAsync<List<dynamic>>();
                //}


                //   Step 4: Update existing records
                foreach (var existing in existingHSRPOrders)
                {
                    var item = orders.FirstOrDefault(x => x.Id == existing.Id);

                    if (item == null) continue;

                    existing.InwardStatus = "success";
                    existing.InwardResponse = "success";//To be changed with API response when live

                    existing.UpdatedBy = userId;
                    existing.UpdatedDate = DateTime.Now;
                }

                //   Step 5: Update VehicleSaleBillDetails
                var detailIds = existingHSRPOrders
                    .Where(i => i.SaleBillDetailsId.HasValue)
                    .Select(i => i.SaleBillDetailsId.Value)
                    .ToList();

                var vehicleDetails = await _context.VehicleSaleBillDetails
                    .Where(v => detailIds.Contains(v.Id))
                    .ToListAsync();

                foreach (var detail in vehicleDetails)
                {
                    var relatedOrder = existingHSRPOrders
                        .FirstOrDefault(o => o.SaleBillDetailsId == detail.Id);

                    if (relatedOrder != null)
                    {
                        detail.Hsrpstatus = relatedOrder.Hsrpstatus;
                        detail.UpdatedBy = userId;
                        detail.UpdatedDate = DateTime.Now;
                    }
                }

                //   FINAL SAVE
                await _context.SaveChangesAsync();

                return existingHSRPOrders;
            }
            catch (Exception ex)
            {
                throw new Exception("HSRP Bulk Update Failed: " + ex.Message);
            }
        }

        public async Task<List<HSRPExcelViewModel>> GetHSRPOrderForExcel(
            bool isSuperAdmin,
            string? dealerCode,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var query =
                    from hs in _context.Hsrporders

                    join vs in _context.VehicleSaleBillDetails
                        on hs.SaleBillDetailsId equals vs.Id into vsJoin
                    from vs in vsJoin.DefaultIfEmpty()

                    join vh in _context.VehicleSaleBillHeaders
                        on vs.VehicleSaleBillId equals vh.Id into vhJoin
                    from vh in vhJoin.DefaultIfEmpty()

                    join lg in _context.LedgerMasters
                        on hs.CustomerLedgerId equals lg.Id into lgJoin
                    from lg in lgJoin.DefaultIfEmpty()

                    join slg in _context.LedgerMasters
                        on hs.SupplierLedgerId equals slg.Id into slgJoin
                    from slg in slgJoin.DefaultIfEmpty()

                    join dl in _context.DealerMasters
                        on hs.DealerCode equals dl.Dealercode into dlJoin
                    from dl in dlJoin.DefaultIfEmpty()

                    select new
                    {
                        hs,
                        vh,
                        lg,
                        slg,
                        dl
                    };

               
                if (!isSuperAdmin && !string.IsNullOrEmpty(dealerCode))
                {
                    query = query.Where(x => x.hs.DealerCode == dealerCode);
                }

                
                if (fromDate.HasValue)
                {
                    query = query.Where(x =>
                        x.hs.OrderDate >= fromDate.Value);
                }

               
                if (toDate.HasValue)
                {
                    query = query.Where(x =>
                        x.hs.OrderDate <= toDate.Value);
                }

                var result = await query
                    .Select(x => new HSRPExcelViewModel
                    {
                        ChassisNo = x.hs.ChassisNo,
                        DealerName = x.dl != null ? x.dl.Compname : "",
                        DealerCode = x.hs.DealerCode,
                        DealerState = x.dl != null ? x.dl.State : "",
                        PartyName = x.lg != null ? x.lg.LedgerName : "",
                        MobileNo = x.lg != null ? x.lg.MobileNumber : "",
                        ChassisSaleDate = x.vh != null
                            ? x.vh.SaleDate.ToString()
                            : "",
                        RegNo = x.hs.RegNo,
                        SupplerName = x.slg != null
                            ? x.slg.LedgerName
                            : "",
                        OrderNo = x.hs.OrderNo,
                        OrderDate = x.hs.OrderDate.ToString(),
                        HSRPOrederStatus = x.hs.Hsrpstatus,
                        HSRPResponse = x.hs.Hsrpresponse,
                        AckInward = x.hs.InwardStatus,
                        InwardSelectStatus = x.hs.InwardStatus,
                        InwardStatusResponse = x.hs.InwardResponse
                    })
                    .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }
    }
}
