using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleStockTransferRepo
{
    public class VehicleStockTransferRepo : IVehicleStockTransferRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IHttpContextAccessor _httpContext;
        public VehicleStockTransferRepo(BapldmsvadContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        public async Task<int> CreateAsync(VehicleStockTransferCreateEditViewModel model)
        {
            var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContext.HttpContext);
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = new VehicleStockTransferHeader
                {
                    TransferNo = model.TransferNo,
                    TransferDate = model.TransferDate,
                    IssuingLocationCode = model.IssuingLocationCode,
                    IssuingStaffCode = model.IssuingStaffCode,
                    ReceivingLocationCode = model.ReceivingLocationCode,
                    ReceivingStaffCode = model.ReceivingStaffCode,
                    Remarks = model.Remarks,
                    DealerCode = model.DealerCode,
                    TransferTotal = model.TransferTotal,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };

                _context.VehicleStockTransferHeaders.Add(header);
                await _context.SaveChangesAsync();

                foreach (var item in model.VehicleStockTransferDetailsViewModel)
                {
                    var detail = new VehicleStockTransferDetail
                    {
                        TransferHeaderId = header.Id,
                        ChassisNo = item.ChassisNo,
                        ItemCode = item.ItemCode,
                        ItemRate = item.ItemRate,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };

                    _context.VehicleStockTransferDetails.Add(detail);
                }

                await _context.SaveChangesAsync();

                var chassisNosTransferred = model.VehicleStockTransferDetailsViewModel
                                            .Select(x => x.ChassisNo).ToList();
                var chassisDetailsToUpdate = await _context.ChassisDetails
                                              .Where(i => chassisNosTransferred.Contains(i.ChassisNo))
                                              .ToListAsync();
                foreach (var item in chassisDetailsToUpdate)
                {
                    item.LocationCode = model.ReceivingLocationCode;
                    item.UpdatedDate = DateTime.Now;
                    item.UpdatedBy = userId;
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

        public async Task<List<VehicleStockTransferListVewModel>> GetVehicleStockTransfer(VehicleStockTransferFilterViewModel filter)
        {
            try
            {
                var query = _context.VehicleStockTransferHeaders.AsQueryable();

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(x => x.TransferDate >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(x => x.TransferDate <= filter.ToDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.IssuingLocation))
                {
                    query = query.Where(x => x.IssuingLocationCode == filter.IssuingLocation);
                }

                if (!string.IsNullOrWhiteSpace(filter.ReceivingLocation))
                {
                    query = query.Where(x => x.ReceivingLocationCode == filter.ReceivingLocation);
                }

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                {
                    query = query.Where(x => x.DealerCode == filter.DealerCode);
                }

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.Trim().ToLower();
                    query = query.Where(x => x.TransferNo.ToLower().Contains(search) || x.IssuingLocationCode.ToLower().Contains(search) || x.ReceivingLocationCode.ToLower().Contains(search) || (x.Remarks ?? "").ToLower().Contains(search));
                }

                var result = await (
                    from vh in query

                    join loc in _context.LocationMasters
                        on vh.IssuingLocationCode equals loc.Loccode into issueLocInfo
                    from loc in issueLocInfo.DefaultIfEmpty()

                    join rloc in _context.LocationMasters
                        on vh.ReceivingLocationCode equals rloc.Loccode into recLocInfo
                    from rloc in recLocInfo.DefaultIfEmpty()

                    join isEmp in _context.EmployeeMasters
                        on vh.IssuingStaffCode equals isEmp.EmployeeCode into isEmpInfo
                    from isEmp in isEmpInfo.DefaultIfEmpty()

                    join rcEmp in _context.EmployeeMasters
                        on vh.ReceivingStaffCode equals rcEmp.EmployeeCode into rcEmpInfo
                    from rcEmp in rcEmpInfo.DefaultIfEmpty()

                    join td in _context.VehicleStockTransferDetails
                        on vh.Id equals td.TransferHeaderId into detailGroup

                    select new VehicleStockTransferListVewModel
                    {
                        Id = vh.Id,
                        TransferNo = vh.TransferNo,
                        TransferDate = vh.TransferDate,
                        IssuingLocationCode = vh.IssuingLocationCode,
                        IssuingLocationName = loc != null ? loc.Locname : "",
                        ReceivingLocationCode = vh.ReceivingLocationCode,
                        ReceivingLocationName = rloc != null ? rloc.Locname : "",
                        IssuingStaffCode = vh.IssuingStaffCode,
                        IssuingStaffName = isEmp != null ? (isEmp.FirstName + " " + isEmp.LastName) : "",
                        ReceivingStaffCode = vh.ReceivingStaffCode,
                        ReceivingStaffName = rcEmp != null ? (rcEmp.FirstName + " " + rcEmp.LastName) : "",
                        TransferTotal = vh.TransferTotal,
                        Remarks = vh.Remarks,
                        VehicleStockTransferDetailsViewModel =
                            (from d in detailGroup
                             join im in _context.ItemMasters
                             on d.ItemCode equals im.Itemcode into itemInfo
                             from im in itemInfo.DefaultIfEmpty()
                             select new VehicleStockTransferDetailsWithChassisViewModel
                             {
                                 Id = d.Id,
                                 ChassisNo = d.ChassisNo,
                                 ItemCode = d.ItemCode,
                                 ItemRate = d.ItemRate,
                                 ModelName = im != null ? im.Itemname : ""
                             }).ToList()
                    })
                    .ToListAsync();

                return result.DistinctBy(x => x.Id).ToList();
            }
            catch
            {
                throw;
            }
        }


        public async Task<VehicleStockTransferListVewModel> GetVehicleTransferById(int id)
        {
            try
            {
                var result = await (
                    from vh in _context.VehicleStockTransferHeaders
                    where vh.Id == id

                    join loc in _context.LocationMasters
                        on vh.IssuingLocationCode equals loc.Loccode into issueLocInfo
                    from loc in issueLocInfo.DefaultIfEmpty()

                    join rloc in _context.LocationMasters
                        on vh.ReceivingLocationCode equals rloc.Loccode into recLocInfo
                    from rloc in recLocInfo.DefaultIfEmpty()

                    join isEmp in _context.EmployeeMasters
                        on vh.IssuingStaffCode equals isEmp.EmployeeCode into isEmpInfo
                    from isEmp in isEmpInfo.DefaultIfEmpty()

                    join rcEmp in _context.EmployeeMasters
                        on vh.ReceivingStaffCode equals rcEmp.EmployeeCode into rcEmpInfo
                    from rcEmp in rcEmpInfo.DefaultIfEmpty()

                    join td in _context.VehicleStockTransferDetails
                        on vh.Id equals td.TransferHeaderId into detailGroup

                    select new VehicleStockTransferListVewModel
                    {
                        Id = vh.Id,
                        TransferNo = vh.TransferNo,
                        TransferDate = vh.TransferDate,
                        IssuingLocationCode = vh.IssuingLocationCode,
                        IssuingLocationName = loc != null ? loc.Locname : "",
                        ReceivingLocationCode = vh.ReceivingLocationCode,
                        ReceivingLocationName = rloc != null ? rloc.Locname : "",
                        IssuingStaffCode = vh.IssuingStaffCode,
                        IssuingStaffName = isEmp != null ? isEmp.FirstName + " " + isEmp.LastName : "",
                        ReceivingStaffCode = vh.ReceivingStaffCode,
                        ReceivingStaffName = rcEmp != null ? rcEmp.FirstName + " " + rcEmp.LastName : "",
                        TransferTotal = vh.TransferTotal,
                        Remarks = vh.Remarks,
                        VehicleStockTransferDetailsViewModel =
                        (
                            from d in detailGroup
                            join cd in _context.ChassisDetails
                                on d.ChassisNo equals cd.ChassisNo into chassisInfo
                            from cd in chassisInfo.DefaultIfEmpty()

                            join im in _context.ItemMasters
                                on d.ItemCode equals im.Itemcode into itemInfo
                            from im in itemInfo.DefaultIfEmpty()

                            join clr in _context.ColorMasters
                                on im.Colorcode equals clr.Colorcode into colourInfo
                            from clr in colourInfo.DefaultIfEmpty()

                            join vi in _context.VehicleInwards
                                on d.ChassisNo equals vi.ChasisNo into inwardInfo
                            from vi in inwardInfo.DefaultIfEmpty()

                            select new VehicleStockTransferDetailsWithChassisViewModel
                            {
                                Id = d.Id,
                                ChassisNo = d.ChassisNo,
                                ItemCode = d.ItemCode,
                                ItemRate = d.ItemRate,
                                ModelName = im != null ? im.Itemname : "",
                                Colour = clr != null ? clr.Colorname : "",
                                MfgYear = vi != null ? vi.MfgYear : null,
                                KeyNo = vi != null ? vi.KeyNo : "",
                                BatteryMake = vi != null ? vi.BatteryMake : "",
                                BatteryCapacity = vi != null ? vi.BatteryCapacity : "",
                                BatteryNo = vi != null ? vi.BatteryNo : "",
                                Charger = vi != null ? vi.ChargerNo : "",
                                Convertor = vi != null ? vi.Converter : "",
                                Controller = vi != null ? vi.ControllerNo : "",
                                FameII = vi != null ? vi.Fame2Discount : null,
                                Rate = d.ItemRate
                            }

                        ).ToList()
                    }
                ).FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<VehicleStockExcelViewModel>> GetExcelReportData()
        //{
        //    try
        //    {
        //        var result = await (
        //            from d in _context.VehicleStockTransferDetails

        //            join h in _context.VehicleStockTransferHeaders
        //                on d.TransferHeaderId equals h.Id

        //            join issueLoc in _context.LocationMasters
        //                on h.IssuingLocationCode equals issueLoc.Loccode into issueLocInfo
        //            from issueLoc in issueLocInfo.DefaultIfEmpty()

        //            join receiveLoc in _context.LocationMasters
        //                on h.ReceivingLocationCode equals receiveLoc.Loccode into receiveLocInfo
        //            from receiveLoc in receiveLocInfo.DefaultIfEmpty()

        //            join issueEmp in _context.EmployeeMasters
        //                on h.IssuingStaffCode equals issueEmp.EmployeeCode into issueEmpInfo
        //            from issueEmp in issueEmpInfo.DefaultIfEmpty()

        //            join receiveEmp in _context.EmployeeMasters
        //                on h.ReceivingStaffCode equals receiveEmp.EmployeeCode into receiveEmpInfo
        //            from receiveEmp in receiveEmpInfo.DefaultIfEmpty()

        //            join item in _context.ItemMasters
        //                on d.ItemCode equals item.Itemcode into itemInfo
        //            from item in itemInfo.DefaultIfEmpty()

        //            join clr in _context.ColorMasters
        //            on item.Colorcode equals clr.Colorcode into clrInfo
        //            from clr in clrInfo.DefaultIfEmpty()

        //            join vi in _context.VehicleInwards
        //            on d.ChassisNo  equals vi.ChasisNo into VehicleInwardsInfo
        //            from vi in VehicleInwardsInfo.DefaultIfEmpty()

        //            select new VehicleStockExcelViewModel
        //            {
        //                id = h.Id,
        //                TransferNo = h.TransferNo,
        //                TransferDate = h.TransferDate,
        //                IssuingLocation = issueLoc != null ? issueLoc.Locname : string.Empty,
        //                ReceivingLocation = receiveLoc != null ? receiveLoc.Locname : string.Empty,
        //                IssuingStaff = issueEmp != null ? issueEmp.FirstName + " " + issueEmp.LastName : string.Empty,
        //                ReceivingStaff = receiveEmp != null ? receiveEmp.FirstName + " " + receiveEmp.LastName : string.Empty,
        //                ChassisNo = d.ChassisNo,
        //                ItemCode = d.ItemCode,
        //                ModelName = item != null ? item.Itemname : string.Empty,
        //                Colour = clr.Colorname,
        //                MfgYear = vi.MfgYear,
        //                KeyNo = vi.KeyNo,
        //                BatteryMake = vi.BatteryMake,
        //                BatteryNo = vi.BatteryNo,
        //                BatteryCapacity = vi.BatteryCapacity,
        //                Charger = vi.ChargerNo,
        //                Controller = vi.ControllerNo
        //            }
        //        ).ToListAsync();

        //        return result;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<List<VehicleStockExcelViewModel>> GetExcelReportData(DateTime? dateFrom, DateTime? dateTo, string? issuingLocation, string? receivingLocation, string? search)
        {
            try
            {
                var query = from d in _context.VehicleStockTransferDetails

                            join h in _context.VehicleStockTransferHeaders
                                on d.TransferHeaderId equals h.Id

                            join issueLoc in _context.LocationMasters
                                on h.IssuingLocationCode equals issueLoc.Loccode into issueLocInfo
                            from issueLoc in issueLocInfo.DefaultIfEmpty()

                            join receiveLoc in _context.LocationMasters
                                on h.ReceivingLocationCode equals receiveLoc.Loccode into receiveLocInfo
                            from receiveLoc in receiveLocInfo.DefaultIfEmpty()

                            join issueEmp in _context.EmployeeMasters
                                on h.IssuingStaffCode equals issueEmp.EmployeeCode into issueEmpInfo
                            from issueEmp in issueEmpInfo.DefaultIfEmpty()

                            join receiveEmp in _context.EmployeeMasters
                                on h.ReceivingStaffCode equals receiveEmp.EmployeeCode into receiveEmpInfo
                            from receiveEmp in receiveEmpInfo.DefaultIfEmpty()

                            join item in _context.ItemMasters
                                on d.ItemCode equals item.Itemcode into itemInfo
                            from item in itemInfo.DefaultIfEmpty()

                            join clr in _context.ColorMasters
                                on item.Colorcode equals clr.Colorcode into clrInfo
                            from clr in clrInfo.DefaultIfEmpty()

                            join vi in _context.VehicleInwards
                                on d.ChassisNo equals vi.ChasisNo into vehicleInwardsInfo
                            from vi in vehicleInwardsInfo.DefaultIfEmpty()

                            select new VehicleStockExcelViewModel
                            {
                                id = h.Id,
                                TransferNo = h.TransferNo,
                                TransferDate = h.TransferDate,
                                IssuingLocation = issueLoc != null ? issueLoc.Locname : string.Empty,
                                ReceivingLocation = receiveLoc != null ? receiveLoc.Locname : string.Empty,
                                IssuingStaff = issueEmp != null ? issueEmp.FirstName + " " + issueEmp.LastName : string.Empty,
                                ReceivingStaff = receiveEmp != null ? receiveEmp.FirstName + " " + receiveEmp.LastName : string.Empty,
                                ChassisNo = d.ChassisNo,
                                ItemCode = d.ItemCode,
                                ModelName = item != null ? item.Itemname : string.Empty,
                                Colour = clr != null ? clr.Colorname : string.Empty,
                                MfgYear = vi != null ? vi.MfgYear : null,
                                KeyNo = vi != null ? vi.KeyNo : string.Empty,
                                BatteryMake = vi != null ? vi.BatteryMake : string.Empty,
                                BatteryNo = vi != null ? vi.BatteryNo : string.Empty,
                                BatteryCapacity = vi != null ? vi.BatteryCapacity : string.Empty,
                                Charger = vi != null ? vi.ChargerNo : string.Empty,
                                Controller = vi != null ? vi.ControllerNo : string.Empty
                            };

                if (dateFrom.HasValue)
                {
                    query = query.Where(x => x.TransferDate.Date >= dateFrom.Value.Date);
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(x => x.TransferDate.Date <= dateTo.Value.Date);
                }

                if (!string.IsNullOrWhiteSpace(issuingLocation))
                {
                    query = query.Where(x => x.IssuingLocation.Contains(issuingLocation));
                }

                if (!string.IsNullOrWhiteSpace(receivingLocation))
                {
                    query = query.Where(x => x.ReceivingLocation.Contains(receivingLocation));
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();

                    query = query.Where(x =>
                        (x.TransferNo ?? "").ToLower().Contains(search) ||
                        (x.ChassisNo ?? "").ToLower().Contains(search) ||
                        (x.ModelName ?? "").ToLower().Contains(search) ||
                        (x.ItemCode ?? "").ToLower().Contains(search) ||
                        (x.IssuingLocation ?? "").ToLower().Contains(search) ||
                        (x.ReceivingLocation ?? "").ToLower().Contains(search) ||
                        (x.IssuingStaff ?? "").ToLower().Contains(search) ||
                        (x.ReceivingStaff ?? "").ToLower().Contains(search));
                }

                var result = await query.ToListAsync();

                return result.GroupBy(x => x.id).Select(g => g.First()).ToList();
            }
            catch
            {
                throw;
            }
        }
    }
}
