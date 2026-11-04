using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.FreeServiceClaimRepo
{
    public partial class FreeServiceClaimRepo : IFreeServiceClaimRepo
    {
        private readonly BapldmsvadContext _context;

        public FreeServiceClaimRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<FreeServiceClaimHeader>> IFreeServiceClaimRepo.Get()
        {
            return await _context.FreeServiceClaimHeaders
                .AsNoTracking()
                .ToListAsync();
        }
        async Task<int> IFreeServiceClaimRepo.Insert(FreeServiceClaimHeader freeServiceClaimHeader)
        {
            await _context.FreeServiceClaimHeaders.AddAsync(freeServiceClaimHeader);
            await _context.SaveChangesAsync();

            return freeServiceClaimHeader.Id;
        }
        //async Task<IEnumerable<PendingApprovalJobCardViewModel>> IFreeServiceClaimRepo.GetPendingApprovalJobCard(string? dealerCode)
        //{
        //    return await (
        //        from jc in _context.JobCardHeaders

        //        join rb in _context.RepairBillHeaders
        //            on jc.Id equals rb.JobId

        //        join sh in _context.ServiceHeads
        //            on jc.Servicehead equals sh.Id

        //        join st in _context.ServiceTypes
        //            on jc.Servicetype equals st.Id

        //        // Chassis Details (Left Join)
        //        join cd in _context.ChassisDetails
        //            on jc.Chassisno equals cd.ChassisNo into chassisGroup
        //        from cd in chassisGroup.DefaultIfEmpty()

        //            // Battery Details (Left Join)
        //        join cbd in _context.ChassisBatteryDetails
        //            on jc.Chassisno equals cbd.ChassisNo into batteryGroup
        //        from cbd in batteryGroup.DefaultIfEmpty()

        //            // Vehicle Sale Bill Detail (Left Join)
        //        join vsd in _context.VehicleSaleBillDetails
        //            on jc.Chassisno equals vsd.ChassisNo into saleBillGroup
        //        from vsd in saleBillGroup.DefaultIfEmpty()

        //            // Vehicle Sale Bill Header (Left Join)
        //        join vsh in _context.VehicleSaleBillHeaders
        //            on vsd.VehicleSaleBillId equals vsh.Id into saleBillHeaderGroup
        //        from vsh in saleBillHeaderGroup.DefaultIfEmpty()

        //            // Ledger Master (Left Join)
        //        join lm in _context.LedgerMasters
        //            on vsh.LedgerId equals lm.Id into ledgerGroup
        //        from lm in ledgerGroup.DefaultIfEmpty()

        //            // Item Master (Left Join)
        //        join im in _context.ItemMasters
        //            on vsd.ItemCode equals im.Itemcode into itemGroup
        //        from im in itemGroup.DefaultIfEmpty()

        //            // OEM Model (Left Join)
        //        join omm in _context.OemmodelMasters
        //            on im.Oemmodelname equals omm.ModelName into modelGroup
        //        from omm in modelGroup.DefaultIfEmpty()

        //            //    // Free Service Rate (Left Join)
        //            //join fsr in _context.FreeServiceRates
        //            //    on omm.Id equals fsr.OemmodelId into fsrGroup
        //            //from fsr in fsrGroup.DefaultIfEmpty()


        //        from fsr in _context.FreeServiceRates
        //            .Where(r => r.OemmodelId == omm.Id
        //                     && r.ServiceId == (st.ServiceTypeName.Contains("4th") ? 2 : 1)
        //                     && r.EffectiveDate <= DateTime.Today)
        //            .OrderByDescending(r => r.EffectiveDate)
        //            .ThenByDescending(r => r.Id)
        //            .Take(1)
        //            .DefaultIfEmpty()

        //            // Dealer Ledger Master
        //        join dlm in _context.LedgerMasters
        //            on jc.DealerCode equals dlm.LedgerCode into dealerLedgerGroup
        //        from dlm in dealerLedgerGroup.DefaultIfEmpty()

        //            // Dealer City
        //        join city in _context.Cities
        //            on dlm.City equals city.CityId into cityGroup
        //        from city in cityGroup.DefaultIfEmpty()

        //        where (string.IsNullOrEmpty(dealerCode) || jc.DealerCode == dealerCode)
        //              && rb.IsDelete != true
        //              && jc.Servicehead == 3
        //              && (jc.Servicetype == 3 || jc.Servicetype == 6)
        //              && !_context.FreeServiceClaimDetails
        //              .Any(fscd => fscd.JobId == jc.Id)

        //        select new PendingApprovalJobCardViewModel
        //        {
        //            JobCardId = jc.Id,

        //            JobNo = jc.JobNo,
        //            JobDate = jc.JobinDate,

        //            RepairBillNo = rb.BillNo,
        //            RepairBillDate = rb.CreatedDate,

        //            RegistrationNo = cd != null ? cd.RegNo : null,
        //            ChassisNo = jc.Chassisno,

        //            MotorNo = cbd != null ? cbd.BatteryNo : null,

        //            VehicleKMs = jc.Vehiclekms,

        //            PartyName = lm != null ? lm.LedgerName : null,
        //            MobileNo = lm != null ? lm.MobileNumber : null,

        //            SaleDate = vsh.SaleDate,

        //            Days = vsh != null
        //                ? EF.Functions.DateDiffDay(vsh.SaleDate, DateTime.Today)
        //                : (int?)null,

        //            Supervisor = jc.Supervisor,
        //            Technician = jc.Technician,

        //            ModelName = vsd != null ? vsd.ModelName : null,

        //            CouponNo = jc.Couponno,

        //            ServiceHead = sh.ServiceHeadName,
        //            ServiceType = st.ServiceTypeName,

        //            DealerCode = jc.DealerCode,

        //            Rate = city != null && city.IsMetro == true
        //            ? fsr.MetroRate
        //            : fsr.NonMetroRate,

        //            GstAmount = city != null && city.IsMetro == true
        //            ? (fsr.MetroRate * fsr.MetroGst / 100)
        //            : (fsr.NonMetroRate * fsr.NonMetroGst / 100),

        //            IsApproved = null
        //        })
        //        .AsNoTracking()
        //        .ToListAsync();
        //}

        async Task<IEnumerable<PendingApprovalJobCardViewModel>> IFreeServiceClaimRepo.GetPendingApprovalJobCard(string? dealerCode)
        {
            return await (
                from jc in _context.JobCardHeaders

                join rb in _context.RepairBillHeaders
                    on jc.Id equals rb.JobId

                join sh in _context.ServiceHeads
                    on jc.Servicehead equals sh.Id

                join st in _context.ServiceTypes
                    on jc.Servicetype equals st.Id

                // Chassis Details (Left Join)
                join cd in _context.ChassisDetails
                    on jc.Chassisno equals cd.ChassisNo into chassisGroup
                from cd in chassisGroup.DefaultIfEmpty()

                    // Battery Details (Left Join)
                join cbd in _context.ChassisBatteryDetails
                    on jc.Chassisno equals cbd.ChassisNo into batteryGroup
                from cbd in batteryGroup.DefaultIfEmpty()

                    // Vehicle Sale Bill Detail (Left Join)
                join vsd in _context.VehicleSaleBillDetails
                    on jc.Chassisno equals vsd.ChassisNo into saleBillGroup
                from vsd in saleBillGroup.DefaultIfEmpty()

                    // Vehicle Sale Bill Header (Left Join)
                join vsh in _context.VehicleSaleBillHeaders
                    on vsd.VehicleSaleBillId equals vsh.Id into saleBillHeaderGroup
                from vsh in saleBillHeaderGroup.DefaultIfEmpty()

                    // Ledger Master (Left Join)
                join lm in _context.LedgerMasters
                    on vsh.LedgerId equals lm.Id into ledgerGroup
                from lm in ledgerGroup.DefaultIfEmpty()

                    // Item Master (Left Join)
                join im in _context.ItemMasters
                    on vsd.ItemCode equals im.Itemcode into itemGroup
                from im in itemGroup.DefaultIfEmpty()

                    // OEM Model (Left Join)
                join omm in _context.OemmodelMasters
                    on im.Oemmodelname equals omm.ModelName into modelGroup
                from omm in modelGroup.DefaultIfEmpty()

                from fsr in _context.FreeServiceRates
                    .Where(r => r.OemmodelId == omm.Id
                             && r.ServiceId == (st.ServiceTypeName.Contains("4th") ? 2 : 1)
                             && r.EffectiveDate <= DateTime.Today)
                    .OrderByDescending(r => r.EffectiveDate)
                    .ThenByDescending(r => r.Id)
                    .Take(1)
                    .DefaultIfEmpty()

                    // Dealer Ledger Master
                join dlm in _context.LedgerMasters
                    on jc.DealerCode equals dlm.LedgerCode into dealerLedgerGroup
                from dlm in dealerLedgerGroup.DefaultIfEmpty()

                    // Dealer City
                join city in _context.Cities
                    on dlm.City equals city.CityId into cityGroup
                from city in cityGroup.DefaultIfEmpty()

                   
                from fscd in _context.FreeServiceClaimDetails
                    .Where(x => x.JobId == jc.Id)
                    .OrderByDescending(x => x.Id)
                    .Take(1)
                    .DefaultIfEmpty()

                    //where (string.IsNullOrEmpty(dealerCode) || jc.DealerCode == dealerCode)
                    //      && rb.IsDelete != true
                    //      && jc.Servicehead == 3
                    //      && (jc.Servicetype == 3 || jc.Servicetype == 6)
                where (string.IsNullOrEmpty(dealerCode) || jc.DealerCode == dealerCode)
                          && rb.IsDelete != true
                          && jc.Servicehead == 3
                          && (jc.Servicetype == 3 || jc.Servicetype == 6)
                         orderby jc.Id descending

                select new PendingApprovalJobCardViewModel
                {
                    JobCardId = jc.Id,
                    JobNo = jc.JobNo,
                    JobDate = jc.JobinDate,
                    RepairBillNo = rb.BillNo,
                    RepairBillDate = rb.CreatedDate,
                    RegistrationNo = cd != null ? cd.RegNo : null,
                    ChassisNo = jc.Chassisno,
                    MotorNo = cbd != null ? cbd.BatteryNo : null,
                    VehicleKMs = jc.Vehiclekms,
                    PartyName = lm != null ? lm.LedgerName : null,
                    MobileNo = lm != null ? lm.MobileNumber : null,
                    SaleDate = vsh.SaleDate,
                    Days = vsh != null
                        ? EF.Functions.DateDiffDay(vsh.SaleDate, DateTime.Today)
                        : (int?)null,
                    Supervisor = jc.Supervisor,
                    Technician = jc.Technician,
                    ModelName = vsd != null ? vsd.ModelName : null,
                    CouponNo = jc.Couponno,
                    ServiceHead = sh.ServiceHeadName,
                    ServiceType = st.ServiceTypeName,
                    DealerCode = jc.DealerCode,
                    LocationCode = jc.Serviceloc,
                    Rate = city != null && city.IsMetro == true
                    ? fsr.MetroRate
                    : fsr.NonMetroRate,
                    GstAmount = city != null && city.IsMetro == true
                    ? (fsr.MetroRate * fsr.MetroGst / 100)
                    : (fsr.NonMetroRate * fsr.NonMetroGst / 100),
                    //IsApproved = null
                    IsClaimed = fscd != null,
                    IsApproved = fscd != null ? fscd.IsApproved : (bool?)null,
                    RejectReason = fscd != null ? fscd.RejectReason : null

                })
                .AsNoTracking()
                .ToListAsync();
        }
        //async Task<PagedResponse<FreeServiceClaimHeaderViewModel>> IFreeServiceClaimRepo.GetWarrantyClaimByDealerCode(string dealerCode, int pageSize, int pageIndex)
        //{
        //    try
        //    {
        //        var query = _context.FreeServiceClaimHeaders
        //                .Where(x => x.DealerCode == dealerCode)
        //                .AsNoTracking();

        //        int totalRecords = await query.CountAsync();

        //        var items = await query
        //            .AsNoTracking()
        //            .OrderByDescending(c => c.CreatedDate)
        //            .Skip((pageIndex - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToListAsync();

        //        int startSrNo = ((pageIndex - 1) * pageSize) + 1;

        //        var viewModelItems = items.Select((item, index) => new FreeServiceClaimHeaderViewModel
        //        {
        //            srNo = startSrNo + index,
        //            Id = item.Id,
        //            ClaimPrefix = item.ClaimPrefix,
        //            ClaimNo = item.ClaimNo,
        //            ClaimDate = item.ClaimDate,
        //            LocationCode = item.LocationCode,
        //        }).ToList();

        //        return new PagedResponse<FreeServiceClaimHeaderViewModel>
        //        {
        //            Data = viewModelItems,
        //            TotalRecords = totalRecords
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        async Task<PagedResponse<FreeServiceClaimHeaderViewModel>> IFreeServiceClaimRepo.GetWarrantyClaimByDealerCode(string dealerCode, int pageSize, int pageIndex)
        {
            try
            {
                var query = _context.FreeServiceClaimHeaders
                .Where(x => string.IsNullOrEmpty(dealerCode) || x.DealerCode == dealerCode)
                .AsNoTracking();

                int totalRecords = await query.CountAsync();

                var items = await query
                    .OrderByDescending(c => c.CreatedDate)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var headerIds = items.Select(i => i.Id).ToList();

                var counts = await _context.FreeServiceClaimDetails
                    .Where(d => headerIds.Contains(d.HeaderClaimId))
                    .GroupBy(d => d.HeaderClaimId)
                    .Select(g => new
                    {
                        HeaderId = g.Key,
                        Total = g.Count(),
                        Approved = g.Count(d => d.IsApproved == true),
                        Rejected = g.Count(d => d.IsApproved == false),
                        Pending = g.Count(d => d.IsApproved == null)
                    })
                    .ToDictionaryAsync(x => x.HeaderId);

                int startSrNo = ((pageIndex - 1) * pageSize) + 1;

                var viewModelItems = items.Select((item, index) =>
                {
                    counts.TryGetValue(item.Id, out var c);

                    string status =
                        c == null || c.Pending == c.Total ? "Pending"
                        : c.Pending > 0 ? "Partially Processed"
                        : c.Rejected == 0 ? "Approved"
                        : c.Approved == 0 ? "Rejected"
                        : "Approved/Rejected";

                    return new FreeServiceClaimHeaderViewModel
                    {
                        srNo = startSrNo + index,
                        Id = item.Id,
                        ClaimPrefix = item.ClaimPrefix,
                        ClaimNo = item.ClaimNo,
                        ClaimDate = item.ClaimDate,
                        LocationCode = item.LocationCode,
                        TotalCount = c?.Total ?? 0,
                        ApprovedCount = c?.Approved ?? 0,
                        RejectedCount = c?.Rejected ?? 0,
                        PendingCount = c?.Pending ?? 0,
                        Status = status
                    };
                }).ToList();

                return new PagedResponse<FreeServiceClaimHeaderViewModel>
                {
                    Data = viewModelItems,
                    TotalRecords = totalRecords
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        async Task<object?> IFreeServiceClaimRepo.GetClaimById(int id)
        {
            var header = await _context.FreeServiceClaimHeaders
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    Id = x.Id,
                    ClaimPrefix = x.ClaimPrefix,
                    ClaimNo = x.ClaimNo,
                    ClaimDate = x.ClaimDate,
                    DealerCode = x.DealerCode,
                    LocationCode = x.LocationCode,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedDate = x.UpdatedDate
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (header == null)
                return null;

            var itemDetails = await (
                from fscd in _context.FreeServiceClaimDetails

                join jc in _context.JobCardHeaders
                    on fscd.JobId equals jc.Id

                join rb in _context.RepairBillHeaders
                    on jc.Id equals rb.JobId

                join sh in _context.ServiceHeads
                    on jc.Servicehead equals sh.Id

                join st in _context.ServiceTypes
                    on jc.Servicetype equals st.Id

                join cd in _context.ChassisDetails
                    on jc.Chassisno equals cd.ChassisNo into chassisGroup
                from cd in chassisGroup.DefaultIfEmpty()

                join cbd in _context.ChassisBatteryDetails
                    on jc.Chassisno equals cbd.ChassisNo into batteryGroup
                from cbd in batteryGroup.DefaultIfEmpty()

                join vsd in _context.VehicleSaleBillDetails
                    on jc.Chassisno equals vsd.ChassisNo into saleBillGroup
                from vsd in saleBillGroup.DefaultIfEmpty()

                join vsh in _context.VehicleSaleBillHeaders
                    on vsd.VehicleSaleBillId equals vsh.Id into saleBillHeaderGroup
                from vsh in saleBillHeaderGroup.DefaultIfEmpty()

                join lm in _context.LedgerMasters
                    on vsh.LedgerId equals lm.Id into ledgerGroup
                from lm in ledgerGroup.DefaultIfEmpty()

                join im in _context.ItemMasters
                    on vsd.ItemCode equals im.Itemcode into itemGroup
                from im in itemGroup.DefaultIfEmpty()

                join omm in _context.OemmodelMasters
                    on im.Oemmodelname equals omm.ModelName into modelGroup
                from omm in modelGroup.DefaultIfEmpty()

                    //join fsr in _context.FreeServiceRates
                    //    on omm.Id equals fsr.OemmodelId into fsrGroup
                    //from fsr in fsrGroup.DefaultIfEmpty()
                    // Free Service Rate: model + service (1st = 1, 4th = 2) sathi latest 1 row
                from fsr in _context.FreeServiceRates
                    .Where(r => r.OemmodelId == omm.Id
                             && r.ServiceId == (st.ServiceTypeName.Contains("4th") ? 2 : 1)
                             && r.EffectiveDate <= DateTime.Today)
                    .OrderByDescending(r => r.EffectiveDate)
                    .ThenByDescending(r => r.Id)
                    .Take(1)
                    .DefaultIfEmpty()

                join dlm in _context.LedgerMasters
                    on jc.DealerCode equals dlm.LedgerCode into dealerLedgerGroup
                from dlm in dealerLedgerGroup.DefaultIfEmpty()

                join city in _context.Cities
                    on dlm.City equals city.CityId into cityGroup
                from city in cityGroup.DefaultIfEmpty()

                where fscd.HeaderClaimId == id
                       && rb.IsDelete != true

                select new
                {
                    id = fscd.Id,
                    headerClaimId = fscd.HeaderClaimId,

                    JobCardId = jc.Id,
                    JobNo = jc.JobNo,
                    JobDate = jc.JobinDate,

                    RepairBillNo = rb.BillNo,
                    RepairBillDate = rb.CreatedDate,

                    RegistrationNo = cd != null ? cd.RegNo : null,
                    ChassisNo = jc.Chassisno,

                    MotorNo = cbd != null ? cbd.BatteryNo : null,

                    VehicleKMs = jc.Vehiclekms,

                    PartyName = lm != null ? lm.LedgerName : null,
                    MobileNo = lm != null ? lm.MobileNumber : null,

                    SaleDate = vsh != null ? vsh.SaleDate : DateTime.Now,

                    Days = vsh != null
                    ? EF.Functions.DateDiffDay(vsh.SaleDate, DateTime.Today)
                    : 0,

                    Supervisor = jc.Supervisor,
                    Technician = jc.Technician,

                    ModelName = vsd != null ? vsd.ModelName : null,

                    CouponNo = jc.Couponno,

                    ServiceHead = sh.ServiceHeadName,
                    ServiceType = st.ServiceTypeName,

                    DealerCode = jc.DealerCode,

                    Rate = city != null && city.IsMetro == true
                        ? fsr != null ? fsr.MetroRate : 0
                        : fsr != null ? fsr.NonMetroRate : 0,

                    GstAmount = city != null && city.IsMetro == true
                        ? fsr != null ? (fsr.MetroRate * fsr.MetroGst / 100) : 0
                        : fsr != null ? (fsr.NonMetroRate * fsr.NonMetroGst / 100) : 0,

                    IsApproved = fscd.IsApproved,
                    RejectReason = fscd.RejectReason,
                    ApprovedRejectBy = fscd.ApprovedRejectBy,
                    ApproveRejectDate = fscd.ApprovedRejectDate,

                    CreatedBy = fscd.CreatedBy,
                    CreatedDate = fscd.CreatedDate
                })
                .AsNoTracking()
                .ToListAsync();

            dynamic result = new ExpandoObject();

            result.Id = header.Id;
            result.ClaimPrefix = header.ClaimPrefix;
            result.ClaimNo = header.ClaimNo;
            result.ClaimDate = header.ClaimDate;
            result.DealerCode = header.DealerCode;
            result.LocationCode = header.LocationCode;
            result.CreatedBy = header.CreatedBy;
            result.CreatedDate = header.CreatedDate;
            result.UpdatedBy = header.UpdatedBy;
            result.UpdatedDate = header.UpdatedDate;
            result.ItemDetails = itemDetails;

            return result;
        }

        //async Task<IEnumerable<object>> IFreeServiceClaimRepo.GetPendingApprovalClaims(string? dealerCode)
        //{
        //    return await (
        //        from fscd in _context.FreeServiceClaimDetails

        //        join fsch in _context.FreeServiceClaimHeaders
        //            on fscd.HeaderClaimId equals fsch.Id

        //        join jc in _context.JobCardHeaders
        //            on fscd.JobId equals jc.Id

        //        join rb in _context.RepairBillHeaders
        //            on jc.Id equals rb.JobId

        //        join sh in _context.ServiceHeads
        //            on jc.Servicehead equals sh.Id

        //        join st in _context.ServiceTypes
        //            on jc.Servicetype equals st.Id

        //        join cd in _context.ChassisDetails
        //            on jc.Chassisno equals cd.ChassisNo into chassisGroup
        //        from cd in chassisGroup.DefaultIfEmpty()

        //        join cbd in _context.ChassisBatteryDetails
        //            on jc.Chassisno equals cbd.ChassisNo into batteryGroup
        //        from cbd in batteryGroup.DefaultIfEmpty()

        //        join vsd in _context.VehicleSaleBillDetails
        //            on jc.Chassisno equals vsd.ChassisNo into saleBillGroup
        //        from vsd in saleBillGroup.DefaultIfEmpty()

        //        join vsh in _context.VehicleSaleBillHeaders
        //            on vsd.VehicleSaleBillId equals vsh.Id into saleBillHeaderGroup
        //        from vsh in saleBillHeaderGroup.DefaultIfEmpty()

        //        join lm in _context.LedgerMasters
        //            on vsh.LedgerId equals lm.Id into ledgerGroup
        //        from lm in ledgerGroup.DefaultIfEmpty()

        //        join im in _context.ItemMasters
        //            on vsd.ItemCode equals im.Itemcode into itemGroup
        //        from im in itemGroup.DefaultIfEmpty()

        //        join omm in _context.OemmodelMasters
        //            on im.Oemmodelname equals omm.ModelName into modelGroup
        //        from omm in modelGroup.DefaultIfEmpty()

        //        from fsr in _context.FreeServiceRates
        //            .Where(r => r.OemmodelId == omm.Id
        //                     && r.ServiceId == (st.ServiceTypeName.Contains("4th") ? 2 : 1)
        //                     && r.EffectiveDate <= DateTime.Today)
        //            .OrderByDescending(r => r.EffectiveDate)
        //            .ThenByDescending(r => r.Id)
        //            .Take(1)
        //            .DefaultIfEmpty()

        //        join dlm in _context.LedgerMasters
        //            on jc.DealerCode equals dlm.LedgerCode into dealerLedgerGroup
        //        from dlm in dealerLedgerGroup.DefaultIfEmpty()

        //        join city in _context.Cities
        //            on dlm.City equals city.CityId into cityGroup
        //        from city in cityGroup.DefaultIfEmpty()

        //        where fscd.IsApproved == null
        //              && (string.IsNullOrEmpty(dealerCode) || fsch.DealerCode == dealerCode)

        //        orderby fsch.CreatedDate descending

        //        select new
        //        {
        //            Id = fscd.Id,
        //            HeaderClaimId = fscd.HeaderClaimId,

        //            ClaimPrefix = fsch.ClaimPrefix,
        //            ClaimNo = fsch.ClaimNo,
        //            ClaimDate = fsch.ClaimDate,
        //            LocationCode = fsch.LocationCode,
        //            DealerCode = fsch.DealerCode,

        //            JobCardId = jc.Id,
        //            JobNo = jc.JobNo,
        //            JobDate = jc.JobinDate,

        //            RepairBillNo = rb.BillNo,
        //            RepairBillDate = rb.CreatedDate,

        //            RegistrationNo = cd != null ? cd.RegNo : null,
        //            ChassisNo = jc.Chassisno,
        //            MotorNo = cbd != null ? cbd.BatteryNo : null,
        //            VehicleKMs = jc.Vehiclekms,

        //            PartyName = lm != null ? lm.LedgerName : null,
        //            MobileNo = lm != null ? lm.MobileNumber : null,

        //            SaleDate = vsh != null ? vsh.SaleDate : DateTime.Now,
        //            Days = vsh != null
        //                ? EF.Functions.DateDiffDay(vsh.SaleDate, DateTime.Today)
        //                : 0,

        //            Supervisor = jc.Supervisor,
        //            Technician = jc.Technician,
        //            ModelName = vsd != null ? vsd.ModelName : null,
        //            CouponNo = jc.Couponno,

        //            ServiceHead = sh.ServiceHeadName,
        //            ServiceType = st.ServiceTypeName,

        //            Rate = city != null && city.IsMetro == true
        //                ? fsr != null ? fsr.MetroRate : 0
        //                : fsr != null ? fsr.NonMetroRate : 0,

        //            GstAmount = city != null && city.IsMetro == true
        //                ? fsr != null ? (fsr.MetroRate * fsr.MetroGst / 100) : 0
        //                : fsr != null ? (fsr.NonMetroRate * fsr.NonMetroGst / 100) : 0,

        //            IsApproved = fscd.IsApproved,
        //            RejectReason = fscd.RejectReason
        //        })
        //        .AsNoTracking()
        //        .ToListAsync();
        //}


        async Task<IEnumerable<object>> IFreeServiceClaimRepo.GetPendingApprovalClaims(string? dealerCode)
        {
            return await (
                from fscd in _context.FreeServiceClaimDetails

                join fsch in _context.FreeServiceClaimHeaders
                    on fscd.HeaderClaimId equals fsch.Id

                join jc in _context.JobCardHeaders
                    on fscd.JobId equals jc.Id

                join rb in _context.RepairBillHeaders
                    on jc.Id equals rb.JobId

                join sh in _context.ServiceHeads
                    on jc.Servicehead equals sh.Id

                join st in _context.ServiceTypes
                    on jc.Servicetype equals st.Id

                join cd in _context.ChassisDetails
                    on jc.Chassisno equals cd.ChassisNo into chassisGroup
                from cd in chassisGroup.DefaultIfEmpty()

                join cbd in _context.ChassisBatteryDetails
                    on jc.Chassisno equals cbd.ChassisNo into batteryGroup
                from cbd in batteryGroup.DefaultIfEmpty()

                join vsd in _context.VehicleSaleBillDetails
                    on jc.Chassisno equals vsd.ChassisNo into saleBillGroup
                from vsd in saleBillGroup.DefaultIfEmpty()

                join vsh in _context.VehicleSaleBillHeaders
                    on vsd.VehicleSaleBillId equals vsh.Id into saleBillHeaderGroup
                from vsh in saleBillHeaderGroup.DefaultIfEmpty()

                join lm in _context.LedgerMasters
                    on vsh.LedgerId equals lm.Id into ledgerGroup
                from lm in ledgerGroup.DefaultIfEmpty()

                join im in _context.ItemMasters
                    on vsd.ItemCode equals im.Itemcode into itemGroup
                from im in itemGroup.DefaultIfEmpty()

                join omm in _context.OemmodelMasters
                    on im.Oemmodelname equals omm.ModelName into modelGroup
                from omm in modelGroup.DefaultIfEmpty()

                    
                from fsr in _context.FreeServiceRates
                    .Where(r => r.OemmodelId == omm.Id
                             && r.ServiceId == (st.ServiceTypeName.Contains("4th") ? 2 : 1)
                             && r.EffectiveDate <= DateTime.Today)
                    .OrderByDescending(r => r.EffectiveDate)
                    .ThenByDescending(r => r.Id)
                    .Take(1)
                    .DefaultIfEmpty()

                join dlm in _context.LedgerMasters
                    on jc.DealerCode equals dlm.LedgerCode into dealerLedgerGroup
                from dlm in dealerLedgerGroup.DefaultIfEmpty()

                join city in _context.Cities
                    on dlm.City equals city.CityId into cityGroup
                from city in cityGroup.DefaultIfEmpty()

                where fscd.IsApproved == null
                       && rb.IsDelete != true
                      && (string.IsNullOrEmpty(dealerCode) || fsch.DealerCode == dealerCode)

                orderby fsch.CreatedDate descending

                select new
                {
                    Id = fscd.Id,
                    HeaderClaimId = fscd.HeaderClaimId,

                    ClaimPrefix = fsch.ClaimPrefix,
                    ClaimNo = fsch.ClaimNo,
                    ClaimDate = fsch.ClaimDate,
                    LocationCode = fsch.LocationCode,
                    DealerCode = fsch.DealerCode,

                    JobCardId = jc.Id,
                    JobNo = jc.JobNo,
                    JobDate = jc.JobinDate,

                    RepairBillNo = rb.BillNo,
                    RepairBillDate = rb.CreatedDate,

                    RegistrationNo = cd != null ? cd.RegNo : null,
                    ChassisNo = jc.Chassisno,
                    MotorNo = cbd != null ? cbd.BatteryNo : null,
                    VehicleKMs = jc.Vehiclekms,

                    PartyName = lm != null ? lm.LedgerName : null,
                    MobileNo = lm != null ? lm.MobileNumber : null,

                    SaleDate = vsh != null ? vsh.SaleDate : DateTime.Now,
                    Days = vsh != null
                        ? EF.Functions.DateDiffDay(vsh.SaleDate, DateTime.Today)
                        : 0,

                    Supervisor = jc.Supervisor,
                    Technician = jc.Technician,
                    ModelName = vsd != null ? vsd.ModelName : null,
                    CouponNo = jc.Couponno,

                    ServiceHead = sh.ServiceHeadName,
                    ServiceType = st.ServiceTypeName,

                    //Rate = 0, 
                    //GstAmount = 0,

                    Rate = city != null && city.IsMetro == true
                        ? fsr != null ? fsr.MetroRate : 0
                        : fsr != null ? fsr.NonMetroRate : 0,

                    GstAmount = city != null && city.IsMetro == true
                        ? fsr != null ? (fsr.MetroRate * fsr.MetroGst / 100) : 0
                        : fsr != null ? (fsr.NonMetroRate * fsr.NonMetroGst / 100) : 0,

                    IsApproved = fscd.IsApproved,
                    RejectReason = fscd.RejectReason
                })
                .AsNoTracking()
                .ToListAsync();
        }

        async Task<bool> IFreeServiceClaimRepo.ApproveRejectClaimDetails(List<ApproveRejectDetailViewModel> items, string userId)
        {
            var ids = items.Select(x => x.Id).ToList();

            var details = await _context.FreeServiceClaimDetails
                .Where(d => ids.Contains(d.Id) && d.IsApproved == null)
                .ToListAsync();

            foreach (var d in details)
            {
                var item = items.First(x => x.Id == d.Id);

                d.IsApproved = item.IsApproved;
                d.RejectReason = item.IsApproved == false ? item.RejectReason?.Trim() : null;
                d.ApprovedRejectBy = userId;
                d.ApprovedRejectDate = DateTime.Now;
                d.UpdatedBy = userId;
                d.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }



        // FreeServiceClaimDetails Table Data Processing

        public async Task<bool> InsertServiceClaimDetails(List<FreeServiceClaimDetail> freeServiceClaimDetail)
        {
            await _context.FreeServiceClaimDetails.AddRangeAsync(freeServiceClaimDetail);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
