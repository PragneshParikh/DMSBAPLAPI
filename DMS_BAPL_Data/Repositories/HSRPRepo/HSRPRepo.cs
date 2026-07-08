using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Vml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.HSRPRepo
{
    public class HSRPRepo : IHSRPRepo
    {
        private readonly string username = "bgauss-dev-user";
        private readonly string password = "cat2024@bgauss";
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
                return data.OrderByDescending(i=>i.SaleDate).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<HSRPOrderAddEditViewModel>> GetPendingHSRPListAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
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
                                    join vbd in _context.ChassisBatteryDetails
                                    on vd.ChassisNo equals vbd.ChassisNo into ChassisBatteryDetails
                                    from vbd in ChassisBatteryDetails.DefaultIfEmpty()
                                    where vh.Status.ToLower() == "invoiced" && vd.Hsrpstatus == null

                                    select new HSRPOrderAddEditViewModel
                                    {
                                        ChassisNo = vd.ChassisNo,
                                        RegNo = vd.RegNo,
                                        RegDate = vd.InsStartDate,
                                        ModelName = vd.ModelName,
                                        Colour = vd.Colour,
                                        CustomerName = vh.CustomerName,
                                        CustomerMobile = lm.MobileNumber,
                                        InvoiceNo = inv.DocumentNo,
                                        InvoiceDate = inv.CreatedDate,
                                        SaleBillNo = vh.SaleBillNo,
                                        SaleBillDeailsId = vd.Id,
                                        CustomerLedgerId = vh.LedgerId,
                                        DealerCode = vh.DealerCode,
                                        HsrpResponse = ho.Hsrpresponse,
                                        HsrpStatus = ho.Hsrpstatus


                                    }).ToListAsync();

                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    result = result.Where(i => i.DealerCode == dealerCode).ToList();

                }
                if (fromDate.HasValue)
                {
                    result = result.Where(i => i.InvoiceDate >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);

                    result = result
                        .Where(i => i.InvoiceDate != null && i.InvoiceDate <= toDateEnd)
                        .ToList();
                }

                return result.OrderByDescending(i=>i.OrderDate).ToList();

            }
            catch
            {
                throw;
            }
        }



        public async Task<List<Hsrporder>> CreateBulkHSRPOrder(List<HSRPOrderCreateViwModel> orders, string accessToken)
        {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContext.HttpContext);

            var dbOrders = new List<Hsrporder>();

            foreach (var item in orders)
            {
                var existing = await _context.Hsrporders.FirstOrDefaultAsync(x => x.ChassisNo == item.ChassisNo);

                if (existing == null)
                {
                    existing = new Hsrporder
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
                        Hsrpstatus = null,
                        Hsrpresponse = null,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };

                    _context.Hsrporders.Add(existing);
                }
                else
                {
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
                    existing.DealerCode = item.DealerCode;
                    existing.UpdatedBy = userId;
                    existing.UpdatedDate = DateTime.Now;
                    existing.Hsrpresponse = null;
                }

                dbOrders.Add(existing);
            }

            // Save first to generate Id
            await _context.SaveChangesAsync();

            try
            {
                // Build payload using generated Hsrporder.Id
                var apiRequests = await BuildHSRPPayloadAsync(dbOrders);

                var apiResponse = await SendOrderPlacementAsync(apiRequests, accessToken);

                var responses = JsonSerializer.Deserialize<List<HsrpApiResponse>>(
                    apiResponse,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<HsrpApiResponse>();

                foreach (var order in dbOrders)
                {
                    var responseItem = responses.FirstOrDefault(x => x.Value != null && x.Value.ChassisNumber == order.ChassisNo);

                    order.Hsrpstatus = responseItem?.STATUS == "1" ? "Success" : "Failed";

                    order.Hsrpresponse = responseItem?.MESSAGE ?? "No response received";
                    var isSuccess = responseItem?.STATUS == "1";
                    if (isSuccess)
                    {
                        var vehicle = await _context.VehicleSaleBillDetails.FirstOrDefaultAsync(v => v.ChassisNo == order.ChassisNo);

                        if (vehicle != null)
                        {
                            vehicle.Hsrpstatus = "Success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (var order in dbOrders)
                {
                    order.Hsrpstatus = "Failed";
                    order.Hsrpresponse = ex.Message;
                }
            }

            await _context.SaveChangesAsync();

            return dbOrders;
        }

        private async Task<List<HsrpExternalRequestViewModel>> BuildHSRPPayloadAsync(List<Hsrporder> orders)
        {
            var payload = new List<HsrpExternalRequestViewModel>();

            foreach (var item in orders)
            {
                var vehicle = await _context.ChassisBatteryDetails
                    .Where(x => x.ChassisNo == item.ChassisNo && x.MotorOrderNo == 1)
                    .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                    .Select(x => new { x.Id, x.MotorNo }).FirstOrDefaultAsync();
                var customerName = await _context.LedgerMasters
            .Where(x => x.Id == item.CustomerLedgerId)
            .Select(x => x.LedgerName)
            .FirstOrDefaultAsync();

                payload.Add(new HsrpExternalRequestViewModel
                {
                    DealerCode = item.DealerCode,
                    vendorid = 14,
                    OrderNumber = item.Id.ToString(),
                    HSRPOrderType = "New",
                    OrderDate = item.OrderDate.ToString("yyyy-MM-dd"),
                    RegDate = item.OrderDate.ToString("yyyy-MM-dd"),
                    ChassisNumber = item.ChassisNo,
                    EngineNo = vehicle.MotorNo,
                    RegistrationNo = item.RegNo,
                    CustomerName = customerName,
                    VehicleClass = "2 WHEELER BOV-L2",
                    VehicleType = "Private",
                    Fuel = "PURE EV",
                    FrontPlate = item.IsFrontPlate == true ? "1" : "0",
                    RearPlate = item.IsRearPlate == true ? "1" : "0",
                    TLP = item.IsTlpsticker == true ? "1" : "0"
                });
            }

            return payload;
        }
        private async Task<string> SendOrderPlacementAsync(List<HsrpExternalRequestViewModel> requests, string accessToken)
        {
            using var httpClient = new HttpClient();
            var json = JsonSerializer.Serialize(requests);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://devbgaussapi.rosmertahsrp.com/api/pos/OrderPlacement");
            httpRequest.Headers.Add("API-Key", $"{accessToken}");
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            return responseBody;
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

        public async Task<List<HSRPListViewModel>> GetAllHSRPOrderAsync(string? dealerCode, DateTime? fromDate, DateTime? toDate)
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
                                        RegNo = o.RegNo,
                                        //IsFrontPlate =ho.IsFrontPlate,
                                        //IsRearPlate =ho.IsRearPlate,

                                    }
                                    ).ToListAsync();


                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    result = result.Where(i => i.DealerCode == dealerCode).ToList();
                }
                if (toDate.HasValue)
                {
                    result = result.Where(i => i.OrderDate <= toDate).ToList();
                }
                if (fromDate.HasValue)
                {
                    result = result.Where(i => i.OrderDate >= fromDate).ToList();
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
                                        RegNo =o.RegNo,
                                        InvoiceDate = inv.CreatedDate,
                                        InvoiceNo = o.InvoiceNo,
                                        CustomerName = led.LedgerName,
                                        CustomerMobile=led.MobileNumber,
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

        public async Task<List<HSRPInward>> GetAllHSRPInward(string? dealerCode, DateTime? fromDate, DateTime? toDate)
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

                                  where (o.InwardStatus == null || o.InwardStatus.ToLower() == "pending" || o.InwardStatus.ToLower() == "") && (o.InwardResponse.ToLower() != "success")

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
                                      CustomerNumber = cus.MobileNumber,
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
                    data = data.Where(i => i.OrderDate >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    data = data.Where(i => i.OrderDate <= toDate.Value).ToList();
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

        public async Task<List<HSRPExcelViewModel>> GetHSRPOrderForExcel(bool isSuperAdmin, string? dealerCode, DateTime? fromDate, DateTime? toDate)
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


        public async Task<string> GetHSRPLoginTokenAsync()
        {
            using var httpClient = new HttpClient();

            var payload = new
            {
                username = "bgauss-dev-user",
                password = "cat2024@bgauss"
            };

            var response = await httpClient.PostAsJsonAsync(
                "https://devbgaussapi.rosmertahsrp.com/api/pos/getApiKey",
                payload);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseContent);
            }

            var tokenResponse =
                JsonSerializer.Deserialize<HSRPLoginResponseViewModel>(
                    responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (tokenResponse?.Value?.AccessToken == null)
            {
                throw new Exception("Access token not found.");
            }

            return tokenResponse.Value.AccessToken;
        }

        public async Task<bool> ReceiveDispatchAsync(List<HSRPDispatchItem> request)
        {
            var orderIds = request.Select(x => x.OrderId).Distinct().ToList();

            var orders = await _context.Hsrporders
                .Where(x => orderIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            var missingOrderIds = new List<int>();

            foreach (var item in request)
            {
                if (!orders.TryGetValue(item.OrderId, out var order))
                {
                    missingOrderIds.Add(item.OrderId);
                    continue;
                }

                order.DispatchNumber = item.DispatchNumber;

                order.DispatchDate = DateTime.ParseExact(
                    item.DispatchDate,
                    "dd-MM-yyyy",
                    CultureInfo.InvariantCulture);

                order.FrontLasercode = item.FrontLaserCode;
                order.Rearlasercode = item.RearLaserCode;

                order.DispatchStatus = "Dispatched";
                order.DispatchResponse = "Dispatch Received";
            }

            await _context.SaveChangesAsync();

            if (missingOrderIds.Any())
            {
                throw new Exception(
                    $"OrderId(s) not found: {string.Join(", ", missingOrderIds.Distinct())}");
            }

            return true;
        }
        public async Task<List<Hsrporder>> UpdateInwardStatus(List<HSRPInwardUpdate> orders, string accessToken)
        {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContext.HttpContext);

            var ids = orders.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();

            var dbOrders = await _context.Hsrporders.Where(x => ids.Contains(x.Id)).ToListAsync();

            var inwardRequests = dbOrders.Select(x => new HSRPInwardRequestViewModel
            {
                //Ver = "1.1",
                VendorId = 14,
                DealerCode = x.DealerCode,
                DCNumber = x.OrderNo,
                RegistrationNo = x.RegNo,
                ChassisNumber = x.ChassisNo,
                FrontPlate = x.IsFrontPlate == true ? 1 : 0,
                RearPlate = x.IsRearPlate == true ? 1 : 0,
                ReceivingDate = DateTime.Now.ToString("yyyy-MM-dd"),
                ReceivingMessage = "RECEIVED",
                OrderID = x.Id
            })
                .ToList();

            try
            {
                var responseJson = await SendInwardAsync(inwardRequests, accessToken);

                var apiResponse =
                    JsonSerializer.Deserialize<List<HSRPInwardResponseViewModel>>(
                        responseJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                var result = apiResponse?.FirstOrDefault();

                foreach (var order in dbOrders)
                {
                    order.InwardStatus = result?.STATUS == "1" ? "Success" : "Failed";
                    order.InwardResponse = result?.MESSAGE;
                    order.UpdatedBy = userId;
                    order.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return dbOrders;
            }
            catch (Exception ex)
            {
                foreach (var order in dbOrders)
                {
                    order.InwardStatus = "Failed";
                    order.InwardResponse = ex.Message;
                    order.UpdatedBy = userId;
                    order.UpdatedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                throw;
            }
        }


        public async Task<HSRPFitmentResponse> ReceiveFitmentAsync(HSRPFitmentRequestData request)
        {
            // Validate Order Number
            if (string.IsNullOrWhiteSpace(request.OrderNumber))
            {
                return new HSRPFitmentResponse
                {
                    Valid = false,
                    Description = "Order Number cannot be null or empty.",
                    Value = new List<HSRPFitmentResponseValue>
            {
                new HSRPFitmentResponseValue
                {
                    Fitment_Idno = "",
                    Msg = "Order Number cannot be null or empty.",
                    StatusCode = "400",
                    ResponseStatus = "false"
                }
            }
                };
            }

            // Convert Order Number to Id
            if (!int.TryParse(request.OrderNumber, out int orderId))
            {
                return new HSRPFitmentResponse
                {
                    Valid = false,
                    Description = $"Invalid Order Number: {request.OrderNumber}",
                    Value = new List<HSRPFitmentResponseValue>
            {
                new HSRPFitmentResponseValue
                {
                    Fitment_Idno = "",
                    Msg = $"Invalid Order Number: {request.OrderNumber}",
                    StatusCode = "400",
                    ResponseStatus = "false"
                }
            }
                };
            }

            var order = await _context.Hsrporders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
            {
                return new HSRPFitmentResponse
                {
                    Valid = false,
                    Description = $"Order does not exist. Order No: {request.OrderNumber}",
                    Value = new List<HSRPFitmentResponseValue>
            {
                new HSRPFitmentResponseValue
                {
                    Fitment_Idno = "",
                    Msg = $"Order does not exist. Order No: {request.OrderNumber}",
                    StatusCode = "400",
                    ResponseStatus = "false"
                }
            }
                };
            }

            if (order.RegNo != request.RegNumber)
            {
                return new HSRPFitmentResponse
                {
                    Valid = false,
                    Description = $"Reg No  {request.RegNumber} does not match Order No: {request.OrderNumber}",
                    Value = new List<HSRPFitmentResponseValue>
            {
                new HSRPFitmentResponseValue
                {
                    Fitment_Idno = "",
                    Msg =  $"Reg No  {request.RegNumber} does not match Order No: {request.OrderNumber}",
                    StatusCode = "400",
                    ResponseStatus = "false"
                }
            }
                };

            }

            // Optional validations
            if (string.IsNullOrWhiteSpace(request.RegNumber))
            {
                return new HSRPFitmentResponse
                {
                    Valid = false,
                    Description = "Registration Number cannot be null or empty.",
                    Value = new List<HSRPFitmentResponseValue>
            {
                new HSRPFitmentResponseValue
                {
                    Fitment_Idno = "",
                    Msg = "Registration Number cannot be null or empty.",
                    StatusCode = "400",
                    ResponseStatus = "false"
                }
            }
                };
            }
            // Check if fitment details already exist with the same values
            if (order.FitmentStatus == "Completed" &&
                order.FitmentDate.HasValue &&
                order.FitmentDate.Value.ToString("dd-MM-yyyy") == request.FitmentDate &&
                order.FitmentResponse == request.VahanResponse)
            {
                return new HSRPFitmentResponse
                {
                    Valid = true,
                    Description = $"Fitment detail for {request.RegNumber} already exists on order no.{order.Id}.",
                    Value = new List<HSRPFitmentResponseValue>
        {
            new HSRPFitmentResponseValue
            {
                Fitment_Idno = order.Id.ToString(),
                Msg = $"Fitment detail for {request.RegNumber} already exists on order no.{order.Id}.",
                StatusCode = "200",
                ResponseStatus = "true"
            }
        }
                };
            }
            // Update Fitment Details
            order.FitmentDate = DateTime.ParseExact(request.FitmentDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            order.FitmentStatus = "Completed";
            order.FitmentResponse = request.VahanResponse;

            await _context.SaveChangesAsync();

            return new HSRPFitmentResponse
            {
                Valid = true,
                Description = $"Fitment detail received successfully for {request.RegNumber}.",
                Value = new List<HSRPFitmentResponseValue>
        {
            new HSRPFitmentResponseValue
            {
                Fitment_Idno = order.Id.ToString(),
                Msg = $"Fitment detail received successfully for {request.RegNumber}.",
                StatusCode = "200",
                ResponseStatus = "true"
            }
        }
            };
        }
        private async Task<string> SendInwardAsync(List<HSRPInwardRequestViewModel> requests, string accessToken)
        {
            using var httpClient = new HttpClient();
            var json = JsonSerializer.Serialize(requests);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://devbgaussapi.rosmertahsrp.com/api/pos/hsrpinward");

            httpRequest.Headers.Add("API-Key", $"{accessToken}");

            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(httpRequest);

            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseBody);

            return responseBody;
        }
    }
}

