using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Crypto.Engines;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static DMS_BAPL_Utils.ViewModels.RepairBillViewModel;

namespace DMS_BAPL_Data.Repositories.JobCardRepo
{
    public class JobCardRepo : IJobCardRepo
    {
        private readonly BapldmsvadContext _context;

        public JobCardRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        // bind jobtype dropdown
        public async Task<List<JobCardViewModel>> GetJobtype()
        {
            return await _context.JobTypes
                .Select(j => new JobCardViewModel
                {
                    JobTypeId = j.Id,
                    JobtypeName = j.JobTypeName
                })
                .ToListAsync();
        }
        // bind service head dropdown based on job type
        public async Task<List<ServiceHeadViewModel>> GetServiceHead(int jobTypeId)
        {
            return await _context.ServiceHeads
                .Where(x => x.JobTypeId == jobTypeId)
                .Select(x => new ServiceHeadViewModel
                {
                    ServiceHeadId = x.Id,
                    ServiceHeadName = x.ServiceHeadName
                }).ToListAsync();
        }
        // bind service type dropdown based on service head
        public async Task<List<ServiceTypeViewModel>> GetServiceType(int serviceHeadId)
        {
            return await _context.ServiceTypes
                .Where(x => x.ServiceHeadId == serviceHeadId)
                .Select(x => new ServiceTypeViewModel
                {
                    ServiceTypeId = x.Id,
                    ServiceTypeName = x.ServiceTypeName
                }).ToListAsync();
        }
        // get service data based on job type
        public async Task<List<ServiceDataViewModel>> GetServiceDataByJobType(string jobTypeName)
        {
            var result = await (from jt in _context.JobTypes
                                join sh in _context.ServiceHeads on jt.JobTypeName equals sh.ServiceHeadName
                                join st in _context.ServiceTypes on sh.Id equals st.ServiceHeadId
                                where jt.JobTypeName == jobTypeName
                                select new ServiceDataViewModel
                                {
                                    JobTypeName = jt.JobTypeName,
                                    ServiceHead = sh.ServiceHeadName,
                                    ServiceType = st.ServiceTypeName
                                }).ToListAsync();

            return result;
        }
        public async Task<List<LotInspectionChassisVM>> GetAllInspectedLotChassisAsync(string dealerCode, int jobTypeId)

        {
            try
            {
                Console.WriteLine($"Dealer Code: [{dealerCode}]");
                Console.WriteLine($"Is Null: {dealerCode == null}");
                Console.WriteLine($"Is Empty: {string.IsNullOrEmpty(dealerCode)}");

                if (dealerCode?.Trim().ToLower() == "null")
                {
                    dealerCode = null;
                }

                bool isSuperAdmin = string.IsNullOrWhiteSpace(dealerCode);
                if (jobTypeId == 1)
                {
                    // ===================== PDI =====================
                    var data = await (
                        from h in _context.LotinspectionHeaders

                        join d in _context.LotinspectionDetails
                          on h.Id equals d.LotHeaderId
                        join v in _context.ChassisDetails
                          on d.ChassisNo equals v.ChassisNo

                        join dealerLg in _context.LedgerMasters.Where(x => x.LedgerType == "Dealer")
                        on v.DealerId equals dealerLg.DealerCode

                        join i in _context.ItemMasters
                            on v.ItemCode equals i.Itemcode

                        let vc = _context.ChassisBatteryDetails
                         .Where(x => x.ChassisNo == v.ChassisNo)
                         .OrderByDescending(x => x.CreatedDate)
                         .FirstOrDefault()


                        join o in _context.OemmodelMasters
                            on i.Oemmodelname.Trim().ToLower()
                            equals o.ModelName.Trim().ToLower()
                            into oGroup

                        from o in oGroup.DefaultIfEmpty()


                        where h.IsLotInspected == true
                         && v.SaleDate == null
                        //&& (h.IsD2d == true || h.IsD2d == null) 
                        && (isSuperAdmin || v.DealerId == dealerCode)

                        select new LotInspectionChassisVM
                        {
                            InvoiceNo = h.InvoiceNo,
                            ChassisNumber = v.ChassisNo,

                            CustomerLedgerId = dealerLg.Id,
                            CustomerName = dealerLg.LedgerName,
                            CustomerMobile = dealerLg.MobileNumber,


                            ModelName = i.Itemname,
                            RegisterNo = v.RegNo,
                            BatteryNumber = vc.BatteryNo,
                            ChargerNumber = vc.ChargerNo,
                            ControllerNo = vc.ControllerNo,
                            BatteryMake = vc.BatteryMake,
                            BatteryCapacity = vc.BatteryCapacity,
                            BatteryChemestry = vc.BatteryChemical,
                            ConverterNo = vc.ConverterNo,
                            MotorNo = vc.MotorNo,

                            oemModelId = o != null ? o.Id : 0
                        }
                    ).Distinct().ToListAsync();

                    return data;
                }
                else
                {
                    // ===================== SALE / SERVICE =====================
                    var data = await (
                    from h in _context.LotinspectionHeaders

                    join d in _context.LotinspectionDetails
                    on h.Id equals d.LotHeaderId

                    join v in _context.ChassisDetails
                    on d.ChassisNo equals v.ChassisNo

                    join i in _context.ItemMasters
                    on v.ItemCode equals i.Itemcode

                    join vsd in _context.VehicleSaleBillDetails
                    on d.ChassisNo equals vsd.ChassisNo

                    join vsh in _context.VehicleSaleBillHeaders
                    on vsd.VehicleSaleBillId equals vsh.Id

                    join custLg in _context.LedgerMasters
                    on v.LedgerId equals custLg.Id

                    join o in _context.OemmodelMasters
                    on i.Oemmodelname.Trim().ToLower()
                    equals o.ModelName.Trim().ToLower()
                    into oGroup

                    from o in oGroup.DefaultIfEmpty()

                    let vc = _context.ChassisBatteryDetails
                    .Where(x => x.ChassisNo == v.ChassisNo)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefault()

                    where h.IsLotInspected == true
                    && (isSuperAdmin || v.DealerId == dealerCode)
                    && v.SaleDate != null

                    select new LotInspectionChassisVM
                    {
                        InvoiceNo = h.InvoiceNo,
                        ChassisNumber = d.ChassisNo,
                        CustomerLedgerId = custLg.Id,
                        CustomerName = custLg.LedgerName,
                        CustomerMobile = custLg.MobileNumber,
                        SaleDate = v.SaleDate,
                        ModelName = i.Itemname,
                        RegisterNo = v.RegNo,

                        // Latest Battery Details
                        BatteryNumber = vc != null ? vc.BatteryNo : null,
                        ChargerNumber = vc != null ? vc.ChargerNo : null,
                        ControllerNo = vc != null ? vc.ControllerNo : null,
                        BatteryMake = vc != null ? vc.BatteryMake : null,
                        BatteryCapacity = vc != null ? vc.BatteryCapacity : null,
                        BatteryChemestry = vc != null ? vc.BatteryChemical : null,
                        ConverterNo = vc != null ? vc.ConverterNo : null,
                        MotorNo = vc != null ? vc.MotorNo : null,

                        oemModelId = o != null ? o.Id : 0
                    }
                    ).Distinct().ToListAsync();

                    foreach (var item in data)
                    {
                        if (!item.SaleDate.HasValue)
                            continue;

                        var completedServiceCount = await (
                        from jh in _context.JobCardHeaders
                        join jc in _context.JobCardCustomers
                        on jh.Id equals jc.JobCardHeaderId
                        where jc.ChassisNo == item.ChassisNumber
                        && jh.Jobtype != 1
                        select jh.Id
                        ).CountAsync();

                        var nextSchedule = await _context.ModelwiseServiceSchedules
                        .Where(x => x.OemmodelId == item.oemModelId)
                        .OrderBy(x => x.Seqno)
                        .Skip(completedServiceCount)
                        .FirstOrDefaultAsync();

                        if (nextSchedule != null)
                        {
                            item.NextserviceDueDate =
                            item.SaleDate.Value.Date.AddDays(nextSchedule.DaysFrom);
                        }
                    }

                    var warranties = await _context.OemmodelWarranties
                    .GroupBy(x => x.OemmodelId)
                    .Select(g => g.OrderByDescending(x => x.EffectiveDate).FirstOrDefault())
                    .ToListAsync();

                    foreach (var item in data)
                    {
                        var warranty = warranties
                        .FirstOrDefault(x => x.OemmodelId == item.oemModelId);

                        if (warranty != null)
                        {
                            item.OdoReading = warranty.Odoreading;
                            item.Duration = warranty.Duration;
                            item.DurationType = warranty.DurationType;
                            item.EffectiveDate = warranty.EffectiveDate;

                            item.ExpireWarrentyDate =
                            warranty.EffectiveDate == null
                            ? null
                            : warranty.DurationType == "MONTH"
                            ? warranty.EffectiveDate.Value.AddMonths((int)(warranty.Duration ?? 0))
                            : warranty.DurationType == "YEAR"
                            ? warranty.EffectiveDate.Value.AddYears((int)(warranty.Duration ?? 0))
                            : warranty.EffectiveDate;
                        }
                    }

                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<LotInspectionChassisVM>();
            }
        }
        public async Task<List<JobSourceViewModel>> GetJobSource()
        {
            return await _context.JobSources
                .Select(j => new JobSourceViewModel
                {
                    JobSourceId = j.Id,
                    JobSourceName = j.JobSourceName
                })
                .ToListAsync();
        }
        public async Task<List<PdichecklistMaster>> GetPdichecklist(int oemModelId)
        {
            return await _context.PdichecklistMasters
                .Where(x => x.OemModelId == oemModelId && x.Isactive == true)
                .OrderBy(x => x.Isactive == true)
                .ToListAsync();
        }
        public async Task<List<JobCardlistDetailsViewModel>> GetJobCardListViewAsync(JobCardSearchVM search)
        {
            try
            {
                var query =
                    from jh in _context.JobCardHeaders

                    join c in _context.JobCardCustomers
                        on jh.Id equals c.JobCardHeaderId into custJoin
                    from c in custJoin.DefaultIfEmpty()

                    join job in _context.JobTypes
                        on jh.Jobtype equals job.Id into jobJoin
                    from job in jobJoin.DefaultIfEmpty()

                    join sh in _context.ServiceHeads
                        on jh.Servicehead equals sh.Id into shJoin
                    from sh in shJoin.DefaultIfEmpty()

                    join st in _context.ServiceTypes
                        on jh.Servicetype equals st.Id into stJoin
                    from st in stJoin.DefaultIfEmpty()

                    join js in _context.JobSources
                        on jh.JobSource equals js.Id into jsJoin
                    from js in jsJoin.DefaultIfEmpty()

                    join loc in _context.LocationMasters
                        on jh.Serviceloc equals loc.Loccode into locJoin
                    from loc in locJoin.DefaultIfEmpty()

                    join lg in _context.LedgerMasters
                        on c.CustomerLedgerId equals lg.Id into lgJoin
                    from lg in lgJoin.DefaultIfEmpty()


                    join sta in _context.States
                        on lg.State equals sta.StateId into staJoin
                    from sta in staJoin.DefaultIfEmpty()

                    join rb in _context.RepairBillHeaders
                    on jh.Id equals rb.JobId into repairBillJoin
                    from rb in repairBillJoin.DefaultIfEmpty()

                    join fr in _context.Ffirheaders
                    on jh.Chassisno equals fr.FfirchassisNo into ffirJoin
                    from fr in ffirJoin.DefaultIfEmpty()


                    select new
                    {
                        jh,
                        c,
                        job,
                        sh,
                        st,
                        js,
                        loc,
                        lg,
                        sta,
                        rb,
                        fr
                    };
                //query = query.Where(x => !_context.RepairBillHeaders.Any(rbh => rbh.JobId == x.jh.Id && rbh.IsActive == true));

                // Dealer Filter
                if (!string.IsNullOrWhiteSpace(search.DealerCode))
                {
                    query = query.Where(x =>
                        x.jh.DealerCode == search.DealerCode);
                }

                // Date From
                if (search.DateFrom.HasValue)
                {
                    query = query.Where(x =>
                        x.jh.JobinDate >= search.DateFrom.Value);
                }

                // Date To
                if (search.DateTo.HasValue)
                {
                    query = query.Where(x =>
                        x.jh.JobinDate <= search.DateTo.Value);
                }

                // Job No
                if (search.JobNo.HasValue)
                {
                    query = query.Where(x =>
                        x.jh.JobNo == search.JobNo.Value);
                }

                // Register No
                if (!string.IsNullOrWhiteSpace(search.RegisterNo))
                {
                    query = query.Where(x =>
                        x.c != null &&
                        x.c.RegisterNo.Contains(search.RegisterNo));
                }

                if (!string.IsNullOrWhiteSpace(search.serviceLocation))
                {
                    query = query.Where(x =>
                        x.loc != null &&
                        x.loc.Locname.Contains(search.serviceLocation));
                }

                // Chassis No
                if (!string.IsNullOrWhiteSpace(search.ChassisNo))
                {
                    query = query.Where(x =>
                        x.c != null &&
                        x.c.ChassisNo.Contains(search.ChassisNo));
                }


                var jobCardsResult = await query
                    .Select(x => new JobCardlistDetailsViewModel
                    {
                        Jobtype = x.job != null ? x.job.JobTypeName : null,
                        Jobsource = x.js != null ? x.js.JobSourceName : null,
                        serviceHead = x.sh != null ? x.sh.ServiceHeadName : null,
                        serviceType = x.st != null ? x.st.ServiceTypeName : null,
                        Location = x.loc != null ? x.loc.Locname : null,

                        PartyName = x.lg != null ? x.lg.LedgerName : null,
                        PartyMobileNo = x.lg != null ? x.lg.MobileNumber : null,
                        PartyState = x.sta != null ? x.sta.StateName : null,
                        CustomerLedgerId = x.lg != null ? x.lg.Id : (int?)null,

                        IsMaterialTransfer = x.jh.IsMaterialTransfer,

                        JobCardHeader = new JobCardHeaderVM
                        {
                            Id = x.jh.Id,
                            DealerCode = x.jh.DealerCode,
                            Jobtype = x.jh.Jobtype,
                            Servicehead = x.jh.Servicehead,
                            Servicetype = x.jh.Servicetype,
                            JobSource = x.jh.JobSource,
                            Chassisno = x.jh.Chassisno,
                            Couponno = x.jh.Couponno,
                            Jobprefix = x.jh.Jobprefix,
                            JobNo = x.jh.JobNo,
                            Vehiclekms = x.jh.Vehiclekms,
                            JobinDate = x.jh.JobinDate,
                            JobinTime = x.jh.JobinTime,
                            EstdelDate = x.jh.EstdelDate,
                            EstdelTime = x.jh.EstdelTime,
                            InvoiceNo = x.jh.InvoiceNo,
                            ManualjobNo = x.jh.ManualjobNo,
                            Serviceloc = x.jh.Serviceloc,
                            Supervisor = x.jh.Supervisor,
                            Technician = x.jh.Technician,
                            Jobestmate = x.jh.Jobestmate,
                            AirpressureRearTyre = x.jh.AirpressureRearTyre,
                            AirpressurefrontTyre = x.jh.AirpressurefrontTyre,
                            IsPdiSuccess = x.jh.IsPdiSuccess,
                            Observation = x.jh.Observation,
                            SupervisorComment = x.jh.SupervisorComment,
                            CreatedDate = x.jh.CreatedDate,
                            UpdatedDate = x.jh.UpdatedDate,
                            JobStatus =
                                   x.rb != null && x.rb.RepairbillStatus == "Billed"
                                        ? "Closed"

                                   : x.rb != null && x.rb.TotalNetAmount > 0
                                        ? "Complete"

                                   : x.jh.IsMaterialTransfer == true
                                        ? "Material Transfer"
                                   : x.fr != null
                                        ? "FFIR Created"
                                   : x.fr != null &&
                                     DateTime.Now >= x.fr.CreatedDate.AddHours(24)
                                        ? "Work In Progress"
                                   : x.fr != null && x.fr.Ffirstatus == "Closed"
                                        ? "FFIR Closed"
                                   : "Open",
                        },

                        JobCardBattery = _context.JobCardBatteryDetails
                            .Where(b => b.JobCardHeaderId == x.jh.Id)
                            .Select(b => new JobCardBatteryVM
                            {
                                JobCardHeaderId = b.JobCardHeaderId,
                                BatteryMake = b.BatteryMake,
                                BatterySerialNo = b.BatterySerialNo,
                                BatteryOcv = b.BatteryOcv,
                                BatteryCcv = b.BatteryCcv,
                                BatteryDischarge = b.BatteryDischarge,
                                BatteryCapacityAh = b.BatteryCapacityAh,
                                BatteryVoltage = b.BatteryVoltage,
                                MotorDrawing = b.MotorDrawing,
                                ChargerMake = b.ChargerMake,
                                ChargerNo = b.ChargerNo,
                                ConverterNo = b.ConverterNo,
                                ControllerNo = b.ControllerNo,
                                BatteryChemical = b.BatteryChemical,
                                BatteryCapacity = b.BatteryCapacity
                            })
                            .FirstOrDefault(),

                        JobCardCustomer = x.c == null ? null : new JobCardCustomerVM
                        {
                            Id = x.c.Id,
                            JobCardHeaderId = x.c.JobCardHeaderId,
                            SaleDate = x.c.SaleDate,
                            RegisterNo = x.c.RegisterNo,
                            ChassisNo = x.c.ChassisNo,
                            ModelName = x.c.ModelName,
                            CustomerLedgerId = x.c.CustomerLedgerId,
                            CustomerName = x.c.CustomerName,
                            CustomerMobile = x.c.CustomerMobile,
                            CustomerAltMobile = x.c.CustomerAltMobile,
                            MotorNo = x.c.MotorNo,
                            BatteryNo = x.c.BatteryNo,
                            InsuranceExpDate = x.c.InsuranceExpDate,
                            NextserviceDueDate = x.c.NextserviceDueDate,
                            RsarenewalDate = x.c.RsarenewalDate,
                            Remarks = x.c.Remarks
                        },

                        JobCardComplaint = _context.JobCardComplaints
                            .Where(cc => cc.JobCardHeaderId == x.jh.Id)
                            .Select(cc => new JobCardComplaintVM
                            {
                                Id = cc.Id,
                                JobCardHeaderId = cc.JobCardHeaderId,
                                Complaint = cc.Complaint,
                                ComplaintCode = cc.ComplaintCode,
                                CustomerVoice = cc.CustomerVoice
                            })
                            .ToList(),

                        PdiChecklistChassiWise = _context.PdichecklistChassisWises
                            .Where(pdi => pdi.JobCardMasterId == x.jh.Id)
                            .Select(x => new PdiChecklistChassiWiseVM
                            {
                                Id = x.Id,
                                PdichecklistMasterId = x.PdichecklistMasterId,
                                JobCardMasterId = x.JobCardMasterId,
                                IsStatus = x.IsStatus,
                                Remarks = x.Remarks,
                                CreatedBy = x.CreatedBy,
                                CreatedDate = x.CreatedDate
                            })
                            .ToList(),

                        Complaint = _context.JobCardComplaints
                            .Where(cc => cc.JobCardHeaderId == x.jh.Id)
                            .Select(cc => cc.Complaint)
                            .FirstOrDefault(),

                    })
                    //.GroupBy(x => x.JobCardHeader.Id)
                    //.Select(g => g.First())
                    .OrderByDescending(x => x.JobCardHeader.Id)
                    .ToListAsync();


                return jobCardsResult;
            }
            catch (Exception ex)
            {
                throw new Exception(
            $"GetJobCardListViewAsync Error : {ex.Message} | Inner : {ex.InnerException?.Message}",
            ex);
            }
        }
        public async Task<int> InsertJobCardinfoDetails(JobCardDetailsViewModel jobCardDetails, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check vehicle sale details
                // 1. PDI not allowed after sale
                var vehicle = await _context.ChassisDetails
                    .FirstOrDefaultAsync(x => x.ChassisNo == jobCardDetails.JobCardHeader.Chassisno);
                if (vehicle?.SaleDate != null && jobCardDetails.JobCardHeader.Jobtype == 1)
                {
                    throw new Exception(StringConstants.aftervehiclesalestatus);
                }
                // 2. Existing open job card check

                var existingJobCards = await _context.RepairBillHeaders
                    .Where(x => x.JobId == jobCardDetails.JobCardHeader.Id && !x.IsDelete)
                    .ToListAsync();
                if (existingJobCards.Any())
                {
                    var jobCardIds = existingJobCards
                    .Select(x => x.Id)
                    .ToList();

                    var billedJobIds = await _context.RepairBillHeaders
                    .Where(x =>
                    jobCardIds.Contains(x.JobId) &&
                    !x.IsDelete &&
                    x.RepairbillStatus == "Billed")
                    .Select(x => x.JobId)
                    .ToListAsync();
                    var openJobCardExists = await _context.RepairBillHeaders
                        .Where(j => j.JobId == jobCardDetails.JobCardHeader.Id && !j.IsDelete)
                        .AnyAsync(j => !_context.RepairBillHeaders
                        .Any(r => r.JobId == j.Id && !r.IsDelete && r.RepairbillStatus == "Billed"));

                    //var openJobCardExists = existingJobCards.Any(j =>
                    //j.Status != "Closed" &&
                    //!billedJobIds.Contains(j.Id));

                    if (openJobCardExists)
                    {
                        throw new Exception(StringConstants.jobCardException);
                    }
                }

                // Insert Header

                var header = new JobCardHeader
                {
                    Jobtype = jobCardDetails.JobCardHeader.Jobtype,
                    DealerCode = jobCardDetails.JobCardHeader.DealerCode,
                    InvoiceNo = jobCardDetails.JobCardHeader.InvoiceNo,
                    Chassisno = jobCardDetails.JobCardHeader.Chassisno,
                    Vehiclekms = jobCardDetails.JobCardHeader.Vehiclekms,
                    Servicehead = jobCardDetails.JobCardHeader.Servicehead,
                    Servicetype = jobCardDetails.JobCardHeader.Servicetype,
                    Serviceloc = jobCardDetails.JobCardHeader.Serviceloc,
                    Couponno = jobCardDetails.JobCardHeader.Couponno,
                    Jobprefix = jobCardDetails.JobCardHeader.Jobprefix,
                    JobinDate = jobCardDetails.JobCardHeader.JobinDate ?? DateOnly.FromDateTime(DateTime.Now),
                    JobinTime = jobCardDetails.JobCardHeader.JobinTime,
                    JobNo = jobCardDetails.JobCardHeader.JobNo,
                    EstNo = jobCardDetails.JobCardHeader.EstimateNo,
                    ManualjobNo = jobCardDetails.JobCardHeader.ManualjobNo,
                    EstdelDate = jobCardDetails.JobCardHeader.EstdelDate ?? DateOnly.FromDateTime(DateTime.Now),
                    EstdelTime = jobCardDetails.JobCardHeader.EstdelTime,
                    JobSource = jobCardDetails.JobCardHeader.JobSource ?? 1,
                    Supervisor = jobCardDetails.JobCardHeader.Supervisor,
                    Technician = jobCardDetails.JobCardHeader.Technician,
                    Jobestmate = jobCardDetails.JobCardHeader.Jobestmate,
                    AirpressureRearTyre = jobCardDetails.JobCardHeader.AirpressureRearTyre,
                    AirpressurefrontTyre = jobCardDetails.JobCardHeader.AirpressurefrontTyre,
                    Observation = jobCardDetails.JobCardHeader.Observation,
                    SupervisorComment = jobCardDetails.JobCardHeader.SupervisorComment,
                    IsPdiSuccess = jobCardDetails.JobCardHeader.IsPdiSuccess,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                };

                _context.JobCardHeaders.Add(header);
                await _context.SaveChangesAsync();

                int headerId = header.Id;

                // Insert Battery
                if (jobCardDetails.JobCardBattery != null)
                {
                    var battery = new JobCardBatteryDetail
                    {
                        JobCardHeaderId = headerId,
                        DealerCode = jobCardDetails.JobCardBattery.DealerCode,
                        BatteryMake = jobCardDetails.JobCardBattery.BatteryMake,
                        BatterySerialNo = jobCardDetails.JobCardBattery.BatterySerialNo,
                        BatteryOcv = jobCardDetails.JobCardBattery.BatteryOcv,
                        BatteryCcv = jobCardDetails.JobCardBattery.BatteryCcv,
                        BatteryDischarge = jobCardDetails.JobCardBattery.BatteryDischarge,
                        BatteryCapacityAh = jobCardDetails.JobCardBattery.BatteryCapacityAh,
                        BatteryVoltage = jobCardDetails.JobCardBattery.BatteryVoltage,
                        MotorDrawing = jobCardDetails.JobCardBattery.MotorDrawing,
                        ChargerMake = jobCardDetails.JobCardBattery.ChargerMake,
                        ChargerNo = jobCardDetails.JobCardBattery.ChargerNo,
                        ConverterNo = jobCardDetails.JobCardBattery.ConverterNo,
                        ControllerNo = jobCardDetails.JobCardBattery.ControllerNo,
                        BatteryChemical = jobCardDetails.JobCardBattery.BatteryChemical,
                        BatteryCapacity = jobCardDetails.JobCardBattery.BatteryCapacity,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                    };

                    _context.JobCardBatteryDetails.Add(battery);
                }

                // Insert Customer
                if (jobCardDetails.JobCardCustomer != null)
                {
                    var customer = new JobCardCustomer
                    {
                        JobCardHeaderId = headerId,
                        CustomerLedgerId = jobCardDetails.JobCardCustomer.CustomerLedgerId,
                        CustomerName = jobCardDetails.JobCardCustomer.CustomerName,
                        CustomerMobile = jobCardDetails.JobCardCustomer.CustomerMobile,
                        CustomerAltMobile = jobCardDetails.JobCardCustomer.CustomerAltMobile,
                        ModelName = jobCardDetails.JobCardCustomer.ModelName,
                        ChassisNo = jobCardDetails.JobCardCustomer.ChassisNo,
                        RegisterNo = jobCardDetails.JobCardCustomer.RegisterNo,
                        MotorNo = jobCardDetails.JobCardCustomer.MotorNo,
                        BatteryNo = jobCardDetails.JobCardCustomer.BatteryNo,
                        SaleDate = jobCardDetails.JobCardCustomer.SaleDate,
                        InsuranceExpDate = jobCardDetails.JobCardCustomer.InsuranceExpDate,
                        NextserviceDueDate = jobCardDetails.JobCardCustomer.NextserviceDueDate,
                        RsarenewalDate = jobCardDetails.JobCardCustomer.RsarenewalDate,
                        Remarks = jobCardDetails.JobCardCustomer.Remarks,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                    };

                    _context.JobCardCustomers.Add(customer);
                }

                // Multiple Complaints Insert()
                if (jobCardDetails.JobCardComplaint != null && jobCardDetails.JobCardComplaint.Any())
                {
                    var complaints = jobCardDetails.JobCardComplaint.Select(c => new JobCardComplaint
                    {
                        DealerCode = jobCardDetails.JobCardHeader.DealerCode,
                        JobCardHeaderId = headerId,
                        CustomerVoice = c.CustomerVoice,
                        ComplaintCode = c.ComplaintCode,
                        Complaint = c.Complaint,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                    }).ToList();

                    _context.JobCardComplaints.AddRange(complaints);
                }

                // multiple PDI Checklist Insert()
                if (jobCardDetails.PdiChecklistChassiWise != null && jobCardDetails.PdiChecklistChassiWise.Any())
                {
                    var pdiList = jobCardDetails.PdiChecklistChassiWise.Select(p => new PdichecklistChassisWise
                    {
                        PdichecklistMasterId = p.PdichecklistMasterId,
                        JobCardMasterId = headerId,
                        OemmodelId = p.OemModelId,
                        IsStatus = p.IsStatus,
                        Remarks = p.Remarks,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    }).ToList();

                    _context.PdichecklistChassisWises.AddRange(pdiList);
                }
                // Save all
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return headerId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<int> UpdateJobCardinfoDetails(UpdateJobCardVM updateJobCardDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // HEADER UPDATE
                var header = await _context.JobCardHeaders
                    .FirstOrDefaultAsync(x => x.Id == updateJobCardDetails.JobCardHeader.Id);

                if (header == null)
                    return 0;

                header.Jobtype = updateJobCardDetails.JobCardHeader.Jobtype;
                header.Chassisno = updateJobCardDetails.JobCardHeader.Chassisno;
                header.Vehiclekms = updateJobCardDetails.JobCardHeader.Vehiclekms;
                header.Servicehead = updateJobCardDetails.JobCardHeader.Servicehead;
                header.Servicetype = updateJobCardDetails.JobCardHeader.Servicetype;
                header.Serviceloc = updateJobCardDetails.JobCardHeader.Serviceloc;
                header.Couponno = updateJobCardDetails.JobCardHeader.Couponno;
                header.Jobprefix = updateJobCardDetails.JobCardHeader.Jobprefix;
                header.JobinDate = updateJobCardDetails.JobCardHeader.JobinDate;
                header.JobinTime = updateJobCardDetails.JobCardHeader.JobinTime;
                header.JobNo = updateJobCardDetails.JobCardHeader.JobNo;
                header.ManualjobNo = updateJobCardDetails.JobCardHeader.ManualjobNo;
                header.EstdelDate = updateJobCardDetails.JobCardHeader.EstdelDate;
                header.EstdelTime = updateJobCardDetails.JobCardHeader.EstdelTime;
                header.JobSource = updateJobCardDetails.JobCardHeader.JobSource;
                header.Supervisor = updateJobCardDetails.JobCardHeader.Supervisor;
                header.Technician = updateJobCardDetails.JobCardHeader.Technician;
                header.Jobestmate = updateJobCardDetails.JobCardHeader.Jobestmate;
                header.AirpressureRearTyre = updateJobCardDetails.JobCardHeader.AirpressureRearTyre;
                header.AirpressurefrontTyre = updateJobCardDetails.JobCardHeader.AirpressurefrontTyre;
                header.Observation = updateJobCardDetails.JobCardHeader.Observation;
                header.SupervisorComment = updateJobCardDetails.JobCardHeader.SupervisorComment;
                header.IsPdiSuccess = updateJobCardDetails.JobCardHeader.IsPdiSuccess;
                header.UpdateBy = updateJobCardDetails.JobCardHeader.CreatedBy;
                header.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                int headerId = header.Id;

                //  BATTERY UPDATE
                var battery = await _context.JobCardBatteryDetails
                    .FirstOrDefaultAsync(x => x.JobCardHeaderId == headerId);

                if (battery != null)
                {
                    battery.BatteryMake = updateJobCardDetails.JobCardBattery.BatteryMake;
                    battery.BatterySerialNo = updateJobCardDetails.JobCardBattery.BatterySerialNo;
                    battery.BatteryOcv = updateJobCardDetails.JobCardBattery.BatteryOcv;
                    battery.BatteryCcv = updateJobCardDetails.JobCardBattery.BatteryCcv;
                    battery.BatteryDischarge = updateJobCardDetails.JobCardBattery.BatteryDischarge;
                    battery.BatteryCapacityAh = updateJobCardDetails.JobCardBattery.BatteryCapacityAh;
                    battery.BatteryVoltage = updateJobCardDetails.JobCardBattery.BatteryVoltage;
                    battery.MotorDrawing = updateJobCardDetails.JobCardBattery.MotorDrawing;
                    battery.ChargerMake = updateJobCardDetails.JobCardBattery.ChargerMake;
                    battery.ChargerNo = updateJobCardDetails.JobCardBattery.ChargerNo;
                    battery.ConverterNo = updateJobCardDetails.JobCardBattery.ConverterNo;
                    battery.ControllerNo = updateJobCardDetails.JobCardBattery.ControllerNo;
                    battery.BatteryChemical = updateJobCardDetails.JobCardBattery.BatteryChemical;
                    battery.BatteryCapacity = updateJobCardDetails.JobCardBattery.BatteryCapacity;
                    battery.UpdateBy = updateJobCardDetails.JobCardBattery.CreatedBy;
                    battery.UpdatedDate = DateTime.Now;
                }

                //  CUSTOMER UPDATE
                var customer = await _context.JobCardCustomers
                    .FirstOrDefaultAsync(x => x.JobCardHeaderId == headerId);

                if (customer != null)
                {
                    customer.CustomerName = updateJobCardDetails.JobCardCustomer.CustomerName;
                    customer.CustomerMobile = updateJobCardDetails.JobCardCustomer.CustomerMobile;
                    customer.CustomerAltMobile = updateJobCardDetails.JobCardCustomer.CustomerAltMobile;
                    customer.ModelName = updateJobCardDetails.JobCardCustomer.ModelName;
                    customer.ChassisNo = updateJobCardDetails.JobCardCustomer.ChassisNo;
                    customer.RegisterNo = updateJobCardDetails.JobCardCustomer.RegisterNo;
                    customer.MotorNo = updateJobCardDetails.JobCardCustomer.MotorNo;
                    customer.BatteryNo = updateJobCardDetails.JobCardCustomer.BatteryNo;
                    customer.SaleDate = updateJobCardDetails.JobCardCustomer.SaleDate;
                    customer.InsuranceExpDate = updateJobCardDetails.JobCardCustomer.InsuranceExpDate;
                    // customer.NextserviceDueDate = updateJobCardDetails.JobCardCustomer.NextServiceDueDate;
                    // customer.RsarenewalDate = updateJobCardDetails.JobCardCustomer.RsaRenewalDate;
                    customer.Remarks = updateJobCardDetails.JobCardCustomer.Remarks;
                    customer.UpdateBy = updateJobCardDetails.JobCardCustomer.CreatedBy;
                    customer.UpdatedDate = DateTime.Now;
                }

                //  COMPLAINT (DELETE + INSERT)
                // Existing DB data
                var existingComplaints = await _context.JobCardComplaints
                    .Where(x => x.JobCardHeaderId == headerId)
                    .ToListAsync();

                // Incoming IDs from frontend
                var incomingIds = updateJobCardDetails.JobCardComplaint
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToList();
                // ================= DELETE REMOVED RECORDS =================
                var deleteComplaints = existingComplaints
                    .Where(x => !incomingIds.Contains(x.Id))
                    .ToList();

                if (deleteComplaints.Any())
                {
                    _context.JobCardComplaints.RemoveRange(deleteComplaints);
                }
                // ================= UPDATE + INSERT =================
                foreach (var item in updateJobCardDetails.JobCardComplaint)
                {
                    // UPDATE
                    if (item.Id > 0)
                    {
                        var dbItem = existingComplaints
                            .FirstOrDefault(x => x.Id == item.Id);

                        if (dbItem != null)
                        {
                            dbItem.CustomerVoice = item.CustomerVoice;
                            dbItem.ComplaintCode = item.ComplaintCode;
                            dbItem.Complaint = item.Complaint;
                            dbItem.DealerCode = item.DealerCode;
                            dbItem.UpdateBy = item.UpdatedBy;
                            dbItem.UpdatedDate = DateTime.Now;
                        }
                    }

                    // INSERT
                    else
                    {
                        var newItem = new JobCardComplaint
                        {
                            DealerCode = item.DealerCode,
                            JobCardHeaderId = headerId,
                            CustomerVoice = item.CustomerVoice,
                            ComplaintCode = item.ComplaintCode,
                            Complaint = item.Complaint,
                            CreatedBy = item.CreatedBy,
                            CreatedDate = DateTime.Now
                        };

                        await _context.JobCardComplaints.AddAsync(newItem);
                    }
                }

                // PDI (DELETE + INSERT)
                var existingPdi = await _context.PdichecklistChassisWises
                                .Where(x => x.JobCardMasterId == headerId)
                                .ToListAsync();

                foreach (var item in updateJobCardDetails.PdiChecklistChassiWise)
                {
                    var dbItem = existingPdi.FirstOrDefault(x =>
                        x.PdichecklistMasterId == item.PdichecklistMasterId
                    );

                    if (dbItem != null)
                    {
                        // UPDATE
                        dbItem.IsStatus = item.IsStatus;
                        dbItem.Remarks = item.Remarks;
                    }
                    else
                    {
                        // INSERT (rare case)
                        var newPdi = new PdichecklistChassisWise
                        {
                            JobCardMasterId = headerId,
                            PdichecklistMasterId = item.PdichecklistMasterId,
                            IsStatus = item.IsStatus,
                            Remarks = item.Remarks,
                            CreatedBy = item.CreatedBy,
                            CreatedDate = DateTime.Now
                        };

                        await _context.PdichecklistChassisWises.AddAsync(newPdi);
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return 1;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<PagedResponse<object>> GetFilterdJobCardDetails(DateTime? fromDate, DateTime? toDate, int? jobNo, int? manualJobNo, int pageIndex, int pageSize)
        {
            try
            {
                var query = from jh in _context.JobCardHeaders
                            join st in _context.ServiceTypes
                                on jh.Servicetype equals st.Id
                            join jt in _context.JobTypes
                                on jh.Jobtype equals jt.Id
                            join sh in _context.ServiceHeads
                                on jh.Servicehead equals sh.Id
                            join lotDetail in _context.LotinspectionDetails
                                on jh.Chassisno equals lotDetail.ChassisNo
                            join item in _context.ItemMasters
                                on lotDetail.Itemcode equals item.Itemcode

                            join jc in _context.JobCardCustomers
                                on jh.Id equals jc.JobCardHeaderId into customerGroup

                            from jc in customerGroup.DefaultIfEmpty()

                            select new
                            {
                                Id = jh.Id,
                                DealerCode = jh.DealerCode,
                                JobType = sh.ServiceHeadName,
                                ChasisNo = jh.Chassisno,
                                ServiceLoc = jh.Serviceloc,
                                JobDate = jh.JobinDate,
                                JobNo = jh.JobNo,
                                ManualJobNo = jh.ManualjobNo,
                                CreatedDate = jh.CreatedDate,
                                ServiceType = st.ServiceTypeName,
                                CustomerName = jc.CustomerName,
                                RegistorNo = jc.RegisterNo,
                                ModelName = item.Itemdesc
                            };

                if (fromDate.HasValue)
                    query = query.Where(x => x.CreatedDate.Date >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(x => x.CreatedDate.Date <= toDate.Value.Date);

                if (jobNo.HasValue && jobNo > 0)
                    query = query.Where(x => x.JobNo == jobNo.Value);

                if (manualJobNo.HasValue && manualJobNo > 0)
                    query = query.Where(x => x.ManualJobNo == manualJobNo.Value);

                var totalRecords = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Cast<object>()
                    .ToListAsync();

                return new PagedResponse<object>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                };

            }
            catch { throw; }
        }
        public async Task<int> UpdateSaleDetails(UpdateSaleDetailsVM updateSale)
        {
            try
            {
                var job = await _context.JobCardCustomers.Where(i => i.ChassisNo == updateSale.ChassisNo).FirstOrDefaultAsync();
                job.SaleDate = updateSale.SaleDate;
                job.InsuranceExpDate = updateSale.InsuranceExpDate;
                job.RegisterNo = updateSale.RegisterNo;
                await _context.SaveChangesAsync();
                return 1;

            }
            catch
            {
                throw;

            }
        }
        public async Task<int> DeleteJobCard(int jobId)
        {
            var jobCard = await _context.JobCardHeaders.FindAsync(jobId);

            if (jobCard == null)
                return 0;

            //  IMPORTANT: Delete related data first 

            var complaints = _context.JobCardComplaints.Where(x => x.JobCardHeaderId == jobId);
            _context.JobCardComplaints.RemoveRange(complaints);

            var pdi = _context.PdichecklistChassisWises.Where(x => x.JobCardMasterId == jobId);
            _context.PdichecklistChassisWises.RemoveRange(pdi);

            var battery = _context.JobCardBatteryDetails.Where(x => x.JobCardHeaderId == jobId);
            _context.JobCardBatteryDetails.RemoveRange(battery);

            var customer = _context.JobCardCustomers.Where(x => x.JobCardHeaderId == jobId);
            _context.JobCardCustomers.RemoveRange(customer);

            // MAIN DELETE
            _context.JobCardHeaders.Remove(jobCard);

            return await _context.SaveChangesAsync();
        }
        public async Task<List<JobCardlistDetailsViewModel>> SearchJobCards(JobCardSearchModel model)
        {
            var query = from jc in _context.JobCardHeaders
                        join cust in _context.JobCardCustomers
                        on jc.Id equals cust.JobCardHeaderId
                        select new { jc, cust };

            // Dealer
            if (!string.IsNullOrWhiteSpace(model.DealerCode))
                query = query.Where(x => x.jc.DealerCode == model.DealerCode);

            // Date From
            if (model.FromDate.HasValue)
                query = query.Where(x => x.jc.JobinDate >= model.FromDate.Value);

            // Date To (FULL DAY FIX)
            if (model.ToDate.HasValue)
                query = query.Where(x => x.jc.JobinDate < model.ToDate.Value.AddDays(1));

            // Location
            if (!string.IsNullOrWhiteSpace(model.ServiceLocation))
                query = query.Where(x => x.jc.Serviceloc == model.ServiceLocation);

            // Job No (IMPORTANT FIX)
            if (model.JobNo.HasValue && model.JobNo > 0)
                query = query.Where(x => x.jc.JobNo == model.JobNo.Value);

            // Customer Name
            if (!string.IsNullOrWhiteSpace(model.CustomerName))
                query = query.Where(x => x.cust.CustomerName.Contains(model.CustomerName));

            // Chassis (FIX: use jc not cust if needed)
            if (!string.IsNullOrWhiteSpace(model.ChassisNo))
                query = query.Where(x => x.jc.Chassisno.Contains(model.ChassisNo));

            // FINAL SELECT
            var result = await query.Select(x => new JobCardlistDetailsViewModel
            {
                JobCardHeader = new JobCardHeaderVM
                {
                    Id = x.jc.Id, // IMPORTANT (for edit/delete)
                    JobNo = x.jc.JobNo,
                    JobinDate = x.jc.JobinDate,
                    InvoiceNo = x.jc.InvoiceNo,
                    ManualjobNo = x.jc.ManualjobNo,
                    Serviceloc = x.jc.Serviceloc,
                },

                JobCardCustomer = new JobCardCustomerVM
                {
                    CustomerName = x.cust.CustomerName,
                    ChassisNo = x.cust.ChassisNo,
                    ModelName = x.cust.ModelName,
                    RegisterNo = x.cust.RegisterNo,
                    CustomerMobile = x.cust.CustomerMobile
                },

                // JOIN OPTIMIZED (no subquery performance issue)
                Jobtype = _context.JobTypes
                            .Where(j => j.Id == x.jc.Jobtype)
                            .Select(j => j.JobTypeName)
                            .FirstOrDefault(),

                Jobsource = _context.JobSources
                            .Where(js => js.Id == x.jc.JobSource)
                            .Select(js => js.JobSourceName)
                            .FirstOrDefault(),

                serviceHead = _context.ServiceHeads
                            .Where(s => s.Id == x.jc.Servicehead)
                            .Select(s => s.ServiceHeadName)
                            .FirstOrDefault(),

                serviceType = _context.ServiceTypes
                            .Where(s => s.Id == x.jc.Servicetype)
                            .Select(s => s.ServiceTypeName)
                            .FirstOrDefault(),

                Complaint = _context.JobCardComplaints
                            .Where(c => c.JobCardHeaderId == x.jc.Id)
                            .Select(c => c.Complaint)
                            .FirstOrDefault(),

            }).ToListAsync();

            return result;
        }
        //public async Task<JobCardHeader?> GetJobCardById(int id)
        //{
        //    return await _context.JobCardHeaders
        //        .FirstOrDefaultAsync(x => x.Id == id);
        //}
        public async Task<JsonResult?> GetJobCardById(int id)
        {
            var result = await (from JCH in _context.JobCardHeaders

                                join DM in _context.DealerMasters
                                    on JCH.DealerCode equals DM.Dealercode

                                join JCC in _context.JobCardCustomers
                                    on JCH.Id equals JCC.JobCardHeaderId

                                join LM in _context.LedgerMasters
                                    on JCC.CustomerLedgerId equals LM.Id

                                join S in _context.States
                                    on DM.State equals S.StateName

                                where JCH.Id == id

                                let taxType = S.StateId == LM.State ? "S" : "O"

                                select new
                                {
                                    Id = JCH.Id,
                                    DealerCode = JCH.DealerCode,
                                    InvoiceNo = JCH.InvoiceNo,
                                    Jobtype = JCH.Jobtype,
                                    Chassisno = JCH.Chassisno,
                                    Vehiclekms = JCH.Vehiclekms,
                                    Servicehead = JCH.Servicehead,
                                    Servicetype = JCH.Servicetype,
                                    Serviceloc = JCH.Serviceloc,
                                    Couponno = JCH.Couponno,
                                    Jobprefix = JCH.Jobprefix,
                                    JobinDate = JCH.JobinDate,
                                    JobinTime = JCH.JobinTime,
                                    JobNo = JCH.JobNo,
                                    ManualjobNo = JCH.ManualjobNo,
                                    EstdelDate = JCH.EstdelDate,
                                    EstdelTime = JCH.EstdelTime,
                                    JobSource = JCH.JobSource,
                                    Supervisor = JCH.Supervisor,
                                    Technician = JCH.Technician,
                                    Jobestmate = JCH.Jobestmate,
                                    AirpressureRearTyre = JCH.AirpressureRearTyre,
                                    AirpressurefrontTyre = JCH.AirpressurefrontTyre,
                                    Observation = JCH.Observation,
                                    SupervisorComment = JCH.SupervisorComment,
                                    IsPdiSuccess = JCH.IsPdiSuccess,
                                    IsMaterialTransfer = JCH.IsMaterialTransfer,
                                    CreatedBy = JCH.CreatedBy,
                                    CreatedDate = JCH.CreatedDate,
                                    UpdateBy = JCH.UpdateBy,
                                    UpdatedDate = JCH.UpdatedDate,
                                    JobStatus = JCH.JobStatus,
                                    ISSameState = taxType == "S" ? true : false
                                })
                                .FirstOrDefaultAsync();

            return new JsonResult(result);
        }
        public async Task<List<ServiceHistoryViewModel>> GetServiceHistoryViewModellist(string chassisNo, int? jobCardId)
        {
            try
            {
                // All completed services for this chassis
                var completedServices = await (
                    from jh in _context.JobCardHeaders
                    join jc in _context.JobCardCustomers
                        on jh.Id equals jc.JobCardHeaderId
                    where jc.ChassisNo == chassisNo
                    select new
                    {
                        jh.Servicehead,
                        jh.Servicetype,
                        jh.JobinDate
                    }
                ).ToListAsync();

                var rawData = await (
                    from jh in _context.JobCardHeaders

                    join loc in _context.LocationMasters
                        on jh.Serviceloc equals loc.Loccode

                    join jc in _context.JobCardCustomers
                        on jh.Id equals jc.JobCardHeaderId

                    join ch in _context.ChassisDetails
                        on jc.ChassisNo equals ch.ChassisNo

                    join i in _context.ItemMasters
                        on jc.ModelName equals i.Itemname

                    join oem in _context.OemmodelMasters
                        on i.Oemmodelname equals oem.ModelName

                    join sch in _context.ModelwiseServiceSchedules
                        on oem.Id equals sch.OemmodelId

                    join sh in _context.ServiceHeads
                        on sch.ServiceHead equals sh.Id

                    join st in _context.ServiceTypes
                        on sch.ServiceType equals st.Id

                    where jc.ChassisNo == chassisNo
                          && (!jobCardId.HasValue || jh.Id == jobCardId)
                          && sch.EffectiveDate ==
                                _context.ModelwiseServiceSchedules
                                    .Where(x => x.OemmodelId == oem.Id)
                                    .Max(x => x.EffectiveDate)

                    orderby sch.Seqno

                    select new
                    {
                        jh,
                        loc,
                        jc,
                        ch,
                        i,
                        oem,
                        sch,
                        sh,
                        st
                    }
                ).ToListAsync();

                var finalResult = rawData.Select(x =>
                {
                    DateTime? dueDate = null;
                    DateTime? graceDate = null;
                    DateOnly? claimDate = null;

                    string status = "Upcoming";

                    if (x.ch.SaleDate.HasValue)
                    {
                        var saleDate = x.ch.SaleDate.Value.Date;

                        dueDate = saleDate.AddDays(x.sch.DaysFrom);
                        graceDate = dueDate.Value.AddDays(15);

                        // Check if this service already availed
                        var availedService = completedServices
                            .FirstOrDefault(s =>
                                s.Servicehead == x.sch.ServiceHead &&
                                s.Servicetype == x.sch.ServiceType);

                        if (availedService != null)
                        {
                            status = "Availed";
                            claimDate = availedService.JobinDate;
                        }
                        else if (graceDate.HasValue && DateTime.Today > graceDate.Value)
                        {
                            status = "Lapsed";
                        }
                        else
                        {
                            status = "Upcoming";
                        }
                    }

                    return new ServiceHistoryViewModel
                    {
                        srno = x.sch.Id,
                        serviceseq = x.sch.Seqno,
                        serviceHead = x.sh.ServiceHeadName,
                        serviceType = x.st.ServiceTypeName,
                        DealerName = x.loc.Locname,
                        DueDate = dueDate,
                        GraceDate = graceDate,
                        ServiceStatus = status,
                        ClaimDate = claimDate
                    };
                }).ToList();

                return finalResult;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching service history", ex);
            }
        }
        public async Task<CIRJobcardViewModel> GetCIRJobCardDetails(int id)
        {
            var ObjCIRJobcardViewModel = await (from jh in _context.JobCardHeaders
                                                join jc in _context.JobCardCustomers on jh.Id equals jc.JobCardHeaderId
                                                join loc in _context.LocationMasters on jh.Serviceloc equals loc.Loccode
                                                where jh.Id == id
                                                select new CIRJobcardViewModel
                                                {
                                                    JobNo = jh.JobNo,
                                                    ChassisNo = jh.Chassisno,
                                                    CustomerName = jc.CustomerName,
                                                    JobCardCustomerId = jc.Id,
                                                    ModelName = jc.ModelName,
                                                    Vehiclekms = jh.Vehiclekms,
                                                    RegisterNo = jc.RegisterNo,
                                                    Observation = jh.Observation,
                                                    Serviceloc = jh.Serviceloc,
                                                    LocationName = loc.Locname,
                                                    VehicleSaleDate = jc.SaleDate,
                                                    Complaints = _context.JobCardComplaints
                                                       .Where(c => c.JobCardHeaderId == jh.Id)
                                                       .Select(c => new ComplaintVM
                                                       {
                                                           Id = c.Id,
                                                           CustomerVoice = c.CustomerVoice,
                                                           ComplaintCode = c.ComplaintCode,
                                                           Complaint = c.Complaint
                                                       })
                                                       .ToList()
                                                }).FirstOrDefaultAsync();

            return ObjCIRJobcardViewModel;
        }
        public async Task<int> GetNextJobNumber(string dealerCode)
        {
            int maxId = await _context.JobCardHeaders
                .Where(x => x.DealerCode == dealerCode)
                .MaxAsync(x => (int?)x.JobNo) ?? 0;

            return maxId + 1;
        }
        public async Task<List<MaterialedJobCardListVM>> GetMaterialedJobCardList(int? jobId, string? dealerCode)
        {
            var CustomerStateId = await (
                from c in _context.JobCardCustomers
                join lg in _context.LedgerMasters
                    on c.CustomerLedgerId equals lg.Id into lgJoin
                from lg in lgJoin.DefaultIfEmpty()
                where c.JobCardHeaderId == jobId
                select lg.State
            ).FirstOrDefaultAsync();
            var custState = await _context.States
                .Where(custst => custst.StateId == CustomerStateId)
                .Select(custst => custst.StateName)
                .FirstOrDefaultAsync();
            var DealerState = await _context.DealerMasters
                .Where(ds => ds.Dealercode == dealerCode)
                .Select(ds => ds.State)
                .FirstOrDefaultAsync();


            // First get customer city tier
            var cityTier = await (
                from c in _context.JobCardCustomers

                join lg in _context.LedgerMasters
                    on c.CustomerLedgerId equals lg.Id into lgJoin
                from lg in lgJoin.DefaultIfEmpty()

                join ct in _context.Cities
                    on lg.City equals ct.CityId into ctJoin
                from ct in ctJoin.DefaultIfEmpty()

                where c.JobCardHeaderId == jobId

                select ct.TierLevel
            ).FirstOrDefaultAsync();
            // Material Transfer + Item Details
            var stateFlag = string.Equals(custState, DealerState, StringComparison.OrdinalIgnoreCase) ? "S" : "O";

            var data = await (
                from m in _context.MaterialTransfers
                join i in _context.ItemMasters
                    on m.ItemId equals i.Id
                join hsn in _context.HsnwiseTaxCodes
                    on i.Hsncode equals hsn.Hsncode
                where m.JobId == jobId
                      && hsn.StateFlag == stateFlag
                select new MaterialedJobCardListVM
                {
                    ItemId = i.Id,
                    MaterialTransferId = m.Id,
                    PartCode = i.Itemcode,
                    PartDesc = i.Itemdesc,
                    PartQty = m.Quantity,
                    PartRate = m.ItemRate,
                    PartHsnCode = i.Hsncode,
                    PartMRP = i.Custprice,

                    Cgst = stateFlag == "S"
                        ? (
                            from a in _context.AggregateTaxCodes
                            join t in _context.TaxCodeMasters
                                on a.TaxCode equals t.TaxCode
                            where a.AtaxCode == hsn.AtaxCode
                                  && a.TaxCode.StartsWith("CGST")
                                  && t.EffectiveDate <= DateTime.Now
                            orderby t.EffectiveDate descending
                            select t.TaxRate
                          ).FirstOrDefault()
                        : 0,

                    Sgst = stateFlag == "S"
                        ? (
                            from a in _context.AggregateTaxCodes
                            join t in _context.TaxCodeMasters
                                on a.TaxCode equals t.TaxCode
                            where a.AtaxCode == hsn.AtaxCode
                                  && a.TaxCode.StartsWith("SGST")
                                  && t.EffectiveDate <= DateTime.Now
                            orderby t.EffectiveDate descending
                            select t.TaxRate
                          ).FirstOrDefault()
                        : 0,

                    Igst = stateFlag == "O"
                        ? (
                            from a in _context.AggregateTaxCodes
                            join t in _context.TaxCodeMasters
                                on a.TaxCode equals t.TaxCode
                            where a.AtaxCode == hsn.AtaxCode
                                  && a.TaxCode.StartsWith("IGST")
                                  && t.EffectiveDate <= DateTime.Now
                            orderby t.EffectiveDate descending
                            select t.TaxRate
                          ).FirstOrDefault()
                        : 0,


                    IssueType = m.IssueType
                }).ToListAsync();
            foreach (var item in data)
            {
                decimal? taxableAmount = item.PartQty * item.PartRate;

                item.CgstAmount = Math.Round((decimal)((taxableAmount * (item.Cgst ?? 0)) / 100), 2);
                item.SgstAmount = Math.Round((decimal)((taxableAmount * (item.Sgst ?? 0)) / 100), 2);
                item.IgstAmount = Math.Round((decimal)((taxableAmount * (item.Igst ?? 0)) / 100), 2);
            }


            //var data = await (
            //    from m in _context.MaterialTransfers

            //    join i in _context.ItemMasters
            //        on m.ItemId equals i.Id into itemJoin
            //    from i in itemJoin.DefaultIfEmpty()


            //    where m.JobId == jobId


            //    select new MaterialedJobCardListVM
            //    {

            //        ItemId = i.Id,
            //        MaterialTransferId = m.Id,
            //        PartCode = i.Itemcode,
            //        PartDesc = i.Itemdesc,
            //        PartQty = m.Quantity,
            //        PartRate = m.ItemRate,
            //        PartHsnCode = i.Hsncode,
            //        PartMRP = i.Custprice,

            //        //Igst = i.Igst,
            //        //Cgst = i.Cgst,
            //        //Sgst = i.Sgst,
            //        IssueType = m.IssueType,

            //        // Labour Codes
            //        LabourCodeDetailslist = _context.PartWiseLabourMasters
            //                .Where(pl =>
            //                    pl.PartCode == i.Itemcode &&
            //                    pl.CityTier == cityTier)
            //                .Select(pl => new LabourCodeDetails
            //                {
            //                    PartwiseLabourId = pl.Id,
            //                    //LabourId = pl.Id,
            //                    LabourCode = pl.LabourCode,
            //                    LabourName = pl.LabourName,
            //                    LabourRate = pl.LabourRate,
            //                    LabourHsnCode = pl.Hsncode,
            //                    CityTier = pl.CityTier,
            //                    Igst = pl.Igst,
            //                    Cgst = pl.Cgst,
            //                    Sgst = pl.Sgst
            //                })
            //                .ToList()
            //    }

            //).ToListAsync();

            return data;
        }
        public async Task<bool> UpdateMaterialTransferStatus(int jobId, bool status)
        {
            var jobCard = await _context.JobCardHeaders
                    .FirstOrDefaultAsync(x => x.Id == jobId);

            if (jobCard == null)
                throw new Exception("Job card not found");

            jobCard.IsMaterialTransfer = status;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<InspectedChassisListVM> GetInspectedChassisListDropdown(string dealerCode)
        {
            var result = await (
                from h in _context.LotinspectionHeaders
                join d in _context.LotinspectionDetails
                    on h.Id equals d.LotHeaderId
                where h.IsLotInspected == true
                      && h.DealerCode == dealerCode
                select d.ChassisNo
            )
            .Distinct()
            .ToListAsync();

            return new InspectedChassisListVM
            {
                ChassisNo = result
            };
        }
        public async Task<List<JobCardlistDetailsViewModel>> GetJobCardListRepairBill(JobCardSearchVM search)
        {
            try
            {
                var query =
                    from jh in _context.JobCardHeaders

                    join c in _context.JobCardCustomers
                        on jh.Id equals c.JobCardHeaderId into custJoin
                    from c in custJoin.DefaultIfEmpty()

                    join job in _context.JobTypes
                        on jh.Jobtype equals job.Id into jobJoin
                    from job in jobJoin.DefaultIfEmpty()

                    join sh in _context.ServiceHeads
                        on jh.Servicehead equals sh.Id into shJoin
                    from sh in shJoin.DefaultIfEmpty()

                    join st in _context.ServiceTypes
                        on jh.Servicetype equals st.Id into stJoin
                    from st in stJoin.DefaultIfEmpty()

                    join js in _context.JobSources
                        on jh.JobSource equals js.Id into jsJoin
                    from js in jsJoin.DefaultIfEmpty()

                    join loc in _context.LocationMasters
                        on jh.Serviceloc equals loc.Loccode into locJoin
                    from loc in locJoin.DefaultIfEmpty()

                    join lg in _context.LedgerMasters
                        on c.CustomerLedgerId equals lg.Id into lgJoin
                    from lg in lgJoin.DefaultIfEmpty()
                    join sta in _context.States
                        on lg.State equals sta.StateId into staJoin
                    from sta in staJoin.DefaultIfEmpty()

                    join rb in _context.RepairBillHeaders
                    on jh.Id equals rb.JobId into repairBillJoin
                    from rb in repairBillJoin.DefaultIfEmpty()

                    join fr in _context.Ffirheaders
                    on jh.Chassisno equals fr.FfirchassisNo into ffirJoin
                    from fr in ffirJoin.DefaultIfEmpty()


                    select new
                    {
                        jh,
                        c,
                        job,
                        sh,
                        st,
                        js,
                        loc,
                        lg,
                        sta,
                        rb,
                        fr
                    };
                query = query.Where(x => !_context.RepairBillHeaders.Any(rbh => rbh.JobId == x.jh.Id && rbh.IsActive == true));

                // Dealer Filter
                if (!string.IsNullOrWhiteSpace(search.DealerCode))
                {
                    query = query.Where(x =>
                        x.jh.DealerCode == search.DealerCode);
                }

                // Date From
                if (search.DateFrom.HasValue)
                {
                    query = query.Where(x =>
                        x.jh.JobinDate >= search.DateFrom.Value);
                }

                // Date To
                if (search.DateTo.HasValue)
                {
                    query = query.Where(x =>
                        x.jh.JobinDate <= search.DateTo.Value);
                }

                // Job No
                if (search.JobNo.HasValue)
                {
                    query = query.Where(x =>
                        x.jh.JobNo == search.JobNo.Value);
                }

                // Register No
                if (!string.IsNullOrWhiteSpace(search.RegisterNo))
                {
                    query = query.Where(x =>
                        x.c != null &&
                        x.c.RegisterNo.Contains(search.RegisterNo));
                }

                // Chassis No
                if (!string.IsNullOrWhiteSpace(search.ChassisNo))
                {
                    query = query.Where(x =>
                        x.c != null &&
                        x.c.ChassisNo.Contains(search.ChassisNo));
                }


                var jobCardsResult = await query
                        .Select(x => new JobCardlistDetailsViewModel
                        {
                            Jobtype = x.job != null ? x.job.JobTypeName : null,
                            Jobsource = x.js != null ? x.js.JobSourceName : null,
                            serviceHead = x.sh != null ? x.sh.ServiceHeadName : null,
                            serviceType = x.st != null ? x.st.ServiceTypeName : null,
                            Location = x.loc != null ? x.loc.Locname : null,

                            PartyName = x.lg != null ? x.lg.LedgerName : null,
                            PartyMobileNo = x.lg != null ? x.lg.MobileNumber : null,
                            PartyState = x.sta != null ? x.sta.StateName : null,
                            CustomerLedgerId = x.lg != null ? x.lg.Id : (int?)null,
                            IsMaterialTransfer = x.jh.IsMaterialTransfer,

                            JobCardHeader = new JobCardHeaderVM
                            {
                                Id = x.jh.Id,
                                DealerCode = x.jh.DealerCode,
                                Jobtype = x.jh.Jobtype,
                                Servicehead = x.jh.Servicehead,
                                Servicetype = x.jh.Servicetype,
                                JobSource = x.jh.JobSource,
                                Chassisno = x.jh.Chassisno,
                                Couponno = x.jh.Couponno,
                                Jobprefix = x.jh.Jobprefix,
                                JobNo = x.jh.JobNo,
                                Vehiclekms = x.jh.Vehiclekms,
                                JobinDate = x.jh.JobinDate,
                                JobinTime = x.jh.JobinTime,
                                EstdelDate = x.jh.EstdelDate,
                                EstdelTime = x.jh.EstdelTime,
                                InvoiceNo = x.jh.InvoiceNo,
                                ManualjobNo = x.jh.ManualjobNo,
                                Serviceloc = x.jh.Serviceloc,
                                Supervisor = x.jh.Supervisor,
                                Technician = x.jh.Technician,
                                Jobestmate = x.jh.Jobestmate,
                                AirpressureRearTyre = x.jh.AirpressureRearTyre,
                                AirpressurefrontTyre = x.jh.AirpressurefrontTyre,
                                IsPdiSuccess = x.jh.IsPdiSuccess,
                                Observation = x.jh.Observation,
                                SupervisorComment = x.jh.SupervisorComment,

                                JobStatus =
                                    x.rb != null && x.rb.RepairbillStatus == "Billed"
                                        ? "Closed"
                                    : x.rb != null && x.rb.TotalNetAmount > 0
                                        ? "Complete"
                                    : x.jh.IsMaterialTransfer == true
                                        ? "Material Transfer"
                                    : x.fr != null && x.fr.Ffirstatus == "Closed"
                                        ? "FFIR Closed"
                                    : x.fr != null
                                        ? "FFIR Created"
                                    : "Open"
                            },

                            JobCardCustomer = x.c == null ? null : new JobCardCustomerVM
                            {
                                Id = x.c.Id,
                                JobCardHeaderId = x.c.JobCardHeaderId,
                                SaleDate = x.c.SaleDate,
                                RegisterNo = x.c.RegisterNo,
                                ChassisNo = x.c.ChassisNo,
                                ModelName = x.c.ModelName,

                                // If SaleDate not available then use LedgerMaster Customer
                                CustomerLedgerId =
                                    x.c.SaleDate == null
                                        ? x.lg != null ? x.lg.Id : x.c.CustomerLedgerId
                                        : x.c.CustomerLedgerId,

                                CustomerName =
                                    x.c.SaleDate == null
                                        ? (x.lg != null ? x.lg.LedgerName : x.c.CustomerName)
                                        : x.c.CustomerName,

                                CustomerMobile =
                                    x.c.SaleDate == null
                                        ? (x.lg != null ? x.lg.MobileNumber : x.c.CustomerMobile)
                                        : x.c.CustomerMobile,

                                CustomerAltMobile = x.c.CustomerAltMobile,
                                MotorNo = x.c.MotorNo,
                                BatteryNo = x.c.BatteryNo,
                                InsuranceExpDate = x.c.InsuranceExpDate,
                                NextserviceDueDate = x.c.NextserviceDueDate,
                                RsarenewalDate = x.c.RsarenewalDate,
                                Remarks = x.c.Remarks
                            }
                        })
    .OrderByDescending(x => x.JobCardHeader.Id)
    .ToListAsync();


                return jobCardsResult;
            }
            catch (Exception ex)
            {
                throw new Exception(
            $"GetJobCardListViewAsync Error : {ex.Message} | Inner : {ex.InnerException?.Message}",
            ex);
            }
        }
        public async Task<PagedResponse<object>> GetJobCardByStatus(DateTime? fromDate, DateTime? toDate, int? jobNo, int? manualJobNo, bool isClosed, int pageIndex, int pageSize, string? dealerCode)
        {
            try
            {
                var query = from jh in _context.JobCardHeaders
                            join st in _context.ServiceTypes
                                on jh.Servicetype equals st.Id
                            join jt in _context.JobTypes
                                on jh.Jobtype equals jt.Id
                            join sh in _context.ServiceHeads
                                on jh.Servicehead equals sh.Id
                            join lotDetail in _context.LotinspectionDetails
                                on jh.Chassisno equals lotDetail.ChassisNo
                            join item in _context.ItemMasters
                                on lotDetail.Itemcode equals item.Itemcode

                            join rb in _context.RepairBillHeaders
                                on jh.Id equals rb.JobId into repairBillGroup

                            from rb in repairBillGroup.DefaultIfEmpty()

                            join jc in _context.JobCardCustomers
                                on jh.Id equals jc.JobCardHeaderId into customerGroup

                            from jc in customerGroup.DefaultIfEmpty()

                            select new
                            {
                                Id = jh.Id,
                                DealerCode = jh.DealerCode,
                                JobType = sh.ServiceHeadName,
                                ChasisNo = jh.Chassisno,
                                ServiceLoc = jh.Serviceloc,
                                JobDate = jh.JobinDate,
                                JobNo = jh.JobNo,
                                ManualJobNo = jh.ManualjobNo,
                                CreatedDate = jh.CreatedDate,
                                ServiceType = st.ServiceTypeName,
                                CustomerName = jc.CustomerName,
                                RegistorNo = jc.RegisterNo,
                                ModelName = item.Itemdesc,
                                Status = rb.RepairbillStatus
                            };

                if (!string.IsNullOrEmpty(dealerCode))
                    query = query.Where(x => x.DealerCode == dealerCode);

                if (fromDate.HasValue)
                    query = query.Where(x => x.CreatedDate.Date >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(x => x.CreatedDate.Date <= toDate.Value.Date);

                if (jobNo.HasValue && jobNo > 0)
                    query = query.Where(x => x.JobNo == jobNo.Value);

                if (manualJobNo.HasValue && manualJobNo > 0)
                    query = query.Where(x => x.ManualJobNo == manualJobNo.Value);

                if (!isClosed)
                    query = query.Where(x => x.Status == null || x.Status == "Performa created");

                var totalRecords = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Cast<object>()
                    .ToListAsync();

                return new PagedResponse<object>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                };

            }
            catch { throw; }
        }
        public async Task<JobCardPrintVM?> GetJobCardForPrint(int jobId)

        {

            var data = await (

                from jh in _context.JobCardHeaders

                where jh.Id == jobId

                join c in _context.JobCardCustomers

                    on jh.Id equals c.JobCardHeaderId into cJoin

                from c in cJoin.DefaultIfEmpty()

                join job in _context.JobTypes on jh.Jobtype equals job.Id into jobJoin

                from job in jobJoin.DefaultIfEmpty()

                join js in _context.JobSources on jh.JobSource equals js.Id into jsJoin

                from js in jsJoin.DefaultIfEmpty()

                join sh in _context.ServiceHeads on jh.Servicehead equals sh.Id into shJoin

                from sh in shJoin.DefaultIfEmpty()

                join st in _context.ServiceTypes on jh.Servicetype equals st.Id into stJoin

                from st in stJoin.DefaultIfEmpty()

                join loc in _context.LocationMasters on jh.Serviceloc equals loc.Loccode into locJoin

                from loc in locJoin.DefaultIfEmpty()

                    // ledger party (real customer / dealer ledger)

                join lg in _context.LedgerMasters on c.CustomerLedgerId equals lg.Id into lgJoin

                from lg in lgJoin.DefaultIfEmpty()

                join sta in _context.States on lg.State equals sta.StateId into staJoin

                from sta in staJoin.DefaultIfEmpty()

                join cty in _context.Cities on lg.City equals cty.CityId into ctyJoin

                from cty in ctyJoin.DefaultIfEmpty()

                    // OEM model + colour via the item that matches the saved model name

                join i in _context.ItemMasters on c.ModelName equals i.Itemname into iJoin

                from i in iJoin.DefaultIfEmpty()

                join o in _context.OemmodelMasters

                    on i.Oemmodelname.Trim().ToLower() equals o.ModelName.Trim().ToLower() into oJoin

                from o in oJoin.DefaultIfEmpty()

                join clr in _context.ColorMasters on i.Colorcode equals clr.Colorcode into clrJoin

                from clr in clrJoin.DefaultIfEmpty()

                select new JobCardPrintVM

                {

                    DealerCode = jh.DealerCode,

                    Location = loc != null ? loc.Locname : null,

                    InvoiceNo = jh.InvoiceNo,

                    JobNo = jh.JobNo,

                    JobinDate = jh.JobinDate,

                    EstdelDate = jh.EstdelDate,

                    EstdelTime = jh.EstdelTime,

                    Vehiclekms = jh.Vehiclekms,

                    ManualjobNo = jh.ManualjobNo,

                    Supervisor = jh.Supervisor,

                    Remarks = c != null ? c.Remarks : null,

                    Technician = jh.Technician,

                    Observation = jh.Observation,

                    SupervisorComment = jh.SupervisorComment,

                    Jobtype = job != null ? job.JobTypeName : null,

                    Jobsource = js != null ? js.JobSourceName : null,

                    ServiceHead = sh != null ? sh.ServiceHeadName : null,

                    ServiceType = st != null ? st.ServiceTypeName : null,

                    CustomerName = lg != null ? lg.LedgerName : (c != null ? c.CustomerName : null),

                    CustomerMobile = lg != null ? lg.MobileNumber : (c != null ? c.CustomerMobile : null),

                    CustomerAltMobile = c != null ? c.CustomerAltMobile : null,

                    Address = lg != null ? lg.Address : null,     // adjust to your LedgerMaster column

                    City = cty != null ? cty.CityName : null,

                    Pincode = lg != null ? lg.Pin : null,     // adjust to your LedgerMaster column

                    State = sta != null ? sta.StateName : null,

                    GstNo = lg != null ? lg.Gstno : null,          // adjust to your LedgerMaster column

                    ChassisNo = c != null ? c.ChassisNo : jh.Chassisno,

                    RegisterNo = c != null ? c.RegisterNo : null,

                    ModelName = c != null ? c.ModelName : null,

                    OemModelName = o != null ? o.ModelName : null,

                    Colour = clr != null ? clr.Colorname : null,

                    SaleDate = c != null ? c.SaleDate : null,

                    InsuranceExpDate = c != null ? c.InsuranceExpDate : null,

                }

            ).FirstOrDefaultAsync();

            if (data == null) return null;

            data.Battery = await _context.JobCardBatteryDetails

                .Where(b => b.JobCardHeaderId == jobId)

                .Select(b => new JobCardBatteryVM

                {

                    BatteryMake = b.BatteryMake,

                    BatterySerialNo = b.BatterySerialNo,

                    BatteryOcv = b.BatteryOcv,

                    BatteryCcv = b.BatteryCcv,

                    BatteryDischarge = b.BatteryDischarge,

                    BatteryCapacityAh = b.BatteryCapacityAh,

                    BatteryVoltage = b.BatteryVoltage,

                    MotorDrawing = b.MotorDrawing,

                    ChargerMake = b.ChargerMake,

                    ChargerNo = b.ChargerNo,

                    ConverterNo = b.ConverterNo,

                    ControllerNo = b.ControllerNo,

                    BatteryChemical = b.BatteryChemical,

                    BatteryCapacity = b.BatteryCapacity

                })

                .FirstOrDefaultAsync();

            data.Complaints = await _context.JobCardComplaints

                .Where(cc => cc.JobCardHeaderId == jobId)

                .Select(cc => new JobCardComplaintVM

                {

                    Id = cc.Id,

                    CustomerVoice = cc.CustomerVoice,

                    ComplaintCode = cc.ComplaintCode,

                    Complaint = cc.Complaint

                })

                .ToListAsync();

            return data;

        }
        public async Task<List<IssueTypebasedJobDetails>> GetIssueTypebasedJobDetail(string? dealerCode, int? jobNo, string? serviceloc, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query =
                    from rbh in _context.RepairBillHeaders
                    join jh in _context.JobCardHeaders on rbh.JobId equals jh.Id
                    join jc in _context.JobCardCustomers on jh.Id equals jc.JobCardHeaderId
                    join loc in _context.LocationMasters on jh.Serviceloc equals loc.Loccode
                    join lg in _context.LedgerMasters on jc.CustomerLedgerId equals lg.Id
                    join jt in _context.JobTypes on jh.Jobtype equals jt.Id
                    join sh in _context.ServiceHeads on jh.Servicehead equals sh.Id
                    join st in _context.ServiceTypes on jh.Servicetype equals st.Id
                    join vs in _context.VehicleSaleBillDetails on jh.Chassisno equals vs.ChassisNo
                    join ch in _context.ChassisDetails on jh.Chassisno equals ch.ChassisNo

                    join chb in _context.ChassisBatteryDetails on jh.Chassisno equals chb.ChassisNo into chbGroup
                    from chb in chbGroup.DefaultIfEmpty()

                    join ffir in _context.Ffirheaders on jh.Id equals ffir.JobCardHeaderId into ffirGroup
                    from ffir in ffirGroup.DefaultIfEmpty()

                    where rbh.RepairbillStatus == "Billed"
                          && jh.Jobtype != 2
                          && _context.RepairBillDetails.Any(d => d.RepairBillId == rbh.Id && d.IssutypeId == 2)
                    select new { rbh, jh, jc, loc, lg, jt, sh, st, vs, ch, chb, ffir };

                // Dynamic Filters
                if (!string.IsNullOrWhiteSpace(dealerCode)) query = query.Where(x => x.rbh.DealerCode == dealerCode);
                if (jobNo.HasValue && jobNo.Value > 0) query = query.Where(x => x.jh.JobNo == jobNo.Value);
                if (!string.IsNullOrWhiteSpace(serviceloc) && serviceloc.Trim().ToLower() != "null") query = query.Where(x => x.jh.Serviceloc == serviceloc);
                if (fromDate.HasValue) query = query.Where(x => x.rbh.CreatedDate >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(x => x.rbh.CreatedDate <= toDate.Value);

                var result = await query
                    .Select(x => new IssueTypebasedJobDetails
                    {
                        RepairBillHeaderId = x.rbh.Id,
                        JobcardId = x.jh.Id,
                        JobNo = x.jh.JobNo,
                        JobType = x.jt.JobTypeName,
                        JobInDate = x.jh.JobinDate,
                        JobLocation = x.loc.Locname,
                        serviceHead = x.sh.ServiceHeadName,
                        serviceType = x.st.ServiceTypeName,
                        CustomerLedgerId = x.lg.Id,
                        CustomerName = x.lg.LedgerName,
                        ChassisNo = x.jh.Chassisno,
                        ModelName = x.jc.ModelName,
                        MotorNo = x.chb != null ? x.chb.MotorNo : null,
                        Vehiclekms = x.jh.Vehiclekms,
                        RegistrationNo = x.vs.RegNo,
                        SaleDate = x.ch.SaleDate,
                        FailureDate = x.ffir != null ? x.ffir.FailureDate : null,
                        RepairBillNo = x.rbh.BillNo,
                        RepairBillDate = x.rbh.CreatedDate
                    })
                    .ToListAsync();

                if (!result.Any()) return result;

                // 1. Fetch non-null Header IDs from your main result list
                var headerIds = result.Select(r => r.RepairBillHeaderId).ToList();

                // 2. Fetch all details where the nullable RepairBillId matches your header collection
                var allDetails = await (
                    from rbd in _context.RepairBillDetails
                    join lm in _context.LabourMasters on rbd.LabourMasterId equals lm.Id into labourGroup
                    from lm in labourGroup.DefaultIfEmpty()
                    join pwm in _context.PartWiseLabourMasters on rbd.PartWiseLabourId equals pwm.Id into partWiseGroup
                    from pwm in partWiseGroup.DefaultIfEmpty()
                    join mt in _context.MaterialTransfers on rbd.MaterialId equals mt.Id into materialGroup
                    from mt in materialGroup.DefaultIfEmpty()
                    join im in _context.ItemMasters on mt.ItemId equals im.Id into itemGroup
                    from im in itemGroup.DefaultIfEmpty()
                        // Safely use Value or handle matching on nullable foreign keys
                    where rbd.RepairBillId.HasValue && headerIds.Contains(rbd.RepairBillId.Value)
                    select new
                    {
                        rbd,
                        lm,
                        pwm,
                        mt,
                        im
                    }
                ).ToListAsync();

                // 3. Map and group the details cleanly in memory
                foreach (var item in result)
                {
                    item.RepairBillDetails = allDetails
                        // Match the values safely using GetValueOrDefault() to handle the int? structure
                        .Where(d => d.rbd.RepairBillId.GetValueOrDefault() == item.RepairBillHeaderId)
                        .Select(d =>
                        {
                            bool isLabour = d.rbd.ItemType == "Labour";
                            bool isPart = d.rbd.ItemType == "Part";

                            decimal rate = isLabour ? (d.lm?.LabourRate ?? 0) : (d.pwm?.LabourRate ?? 0);
                            decimal qty = isLabour ? (d.rbd.LabourQty ?? 0) : (d.rbd.PartQty ?? 0);

                            decimal partRate = isPart ? (d.mt?.ItemRate ?? 0) : 0;
                            decimal partQty = isPart ? (d.mt?.Quantity ?? 0) : 0;

                            // Direct, clean amount calculation
                            decimal lineSubtotal = isLabour ? (qty * rate) : (partQty * partRate);
                            decimal taxAmount = (d.rbd.Igstamount ?? 0) > 0
                                ? d.rbd.Igstamount.Value
                                : ((d.rbd.Cgstamount ?? 0) + (d.rbd.Sgstamount ?? 0));

                            return new IssueTypebasedJobDetails
                            {
                                RepairBillDetailsId = d.rbd.Id,
                                ItemType = d.rbd.ItemType,
                                MaterialId = d.rbd.MaterialId,
                                LabourId = d.rbd.LabourMasterId,
                                PartWiseLabourId = d.rbd.PartWiseLabourId,
                                PartItemId = d.rbd.PartItemId,

                                // Pure Labour Grid Data
                                LabourQty = isLabour ? qty : 0,
                                LabourName = isLabour ? d.lm?.LabourCode : null,
                                LabourDesc = isLabour ? d.lm?.LabourDescription : null,
                                LabourRate = isLabour ? rate : 0,

                                // Pure Part Labour Grid Data
                                PartLabourQty = !isLabour ? qty : 0,
                                PartLabourName = !isLabour ? d.pwm?.PartCode : null,
                                PartLabourDesc = !isLabour ? d.pwm?.PartDescription : null,
                                PartLabourRate = !isLabour ? rate : 0,

                                // Pure Part Inventory Grid Data
                                PartitemName = isPart ? (d.im != null ? d.im.Itemcode : "") : "",
                                PartitemDesc = isPart ? (d.im != null ? d.im.Itemname : "") : "",
                                PartItemQty = partQty,
                                PartItemRate = partRate,

                                // Calculated values to resolve frontend grid mismatches
                                IgstAmount = taxAmount,
                                RowSubTotal = lineSubtotal,
                                TotalWithTax = lineSubtotal + taxAmount,
                                ClaimType = "Warranty"
                            };
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching issue type based job details", ex);
            }
        }
        public async Task<bool> GetJobCardStatusById(int id)
        {
            return await _context.RepairBillHeaders
                .AnyAsync(x => x.JobId == id && x.RepairbillStatus == "Billed");
        }


        public async Task<int> DeleteJobCardDetails(int jobCardHeaderId, bool isSuperAdmin)
        {
            var jobCardHeader = await _context.JobCardHeaders.FirstOrDefaultAsync(x => x.Id == jobCardHeaderId);
            if (jobCardHeader == null)
            {
                throw new Exception("Job card header not found");
            }
            if (!isSuperAdmin)
            {
                throw new Exception("Only SuperAdmin can delete job cards.");
            }
            // Delete Repair Bill if exists
            var repairBills = await _context.RepairBillHeaders
                .Where(x => x.JobId == jobCardHeaderId)
                .ToListAsync();
            if (repairBills.Any())
            {
                _context.RepairBillHeaders.RemoveRange(repairBills);
            }

            // Delete FFIR if exists
            var ffirs = await _context.Ffirheaders
            .Where(x => x.JobCardHeaderId == jobCardHeaderId)
            .ToListAsync();
            if (ffirs.Any())
            {
                var ffirHeaderDetails = _context.Ffirheaders;
                ffirs.ForEach(ffir =>
                {
                    var ffirDetails = _context.Ffirheaders.Where(d => d.JobCardHeaderId == jobCardHeaderId).ToList();
                    ffirDetails.ForEach(detail =>
                    {
                        detail.IsDelete = true;
                        detail.UpdatedBy = "Admin";
                        detail.UpdatedDate = DateTime.UtcNow;
                    });
                });
            }

            // Delete Material Transfer if exists
            var materialTransfers = await _context.MaterialTransfers
            .Where(x => x.JobId == jobCardHeaderId)
            .ToListAsync();
            if (materialTransfers.Any())
            {
                var materialTransferDetails = _context.MaterialTransfers;
                materialTransfers.ForEach(mt =>
                {
                    var mtDetails = _context.MaterialTransfers.Where(d => d.JobId == jobCardHeaderId).ToList();
                    mtDetails.ForEach(detail =>
                    {
                        detail.IsDelete = true;
                        detail.UpdatedBy = "Admin";
                        detail.UpdatedDate = DateTime.UtcNow;
                    });
                });
            }
            // Soft Delete Job Card
            jobCardHeader.IsDelete = true;
            jobCardHeader.UpdateBy = "Admin";
            jobCardHeader.UpdatedDate = DateTime.UtcNow; // optional

            _context.JobCardHeaders.Update(jobCardHeader);
            return await _context.SaveChangesAsync();
        }
    }
}
