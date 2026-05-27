using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

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
                var data = await (from h in _context.LotinspectionHeaders
                                  join d in _context.LotinspectionDetails
                                      on h.Id equals d.LotHeaderId
                                  join v in _context.VehicleInwards
                                      on d.ChassisNo equals v.ChasisNo
                                  join i in _context.ItemMasters
                                      on v.ItemCode equals i.Itemcode
                                  join dm in _context.DealerMasters
                                      on h.DealerCode equals dm.Dealercode
                                  
                                  join jc in _context.JobCardCustomers
                                  on d.ChassisNo equals jc.ChassisNo
                                    into jcGroup
                                    from jc in jcGroup.DefaultIfEmpty() 

                                  // OEM Model (LEFT JOIN)
                                  join o in _context.OemmodelMasters
                                      on i.Oemmodelname.Trim().ToLower()
                                      equals o.ModelName.Trim().ToLower()
                                      into oGroup
                                  from o in oGroup.DefaultIfEmpty()

                                  where h.IsLotInspected == true
                                        && h.DealerCode == dealerCode
                                          &&
                                            (jobTypeId == 1 ? (jc == null || jc.SaleDate == null) : true)   // 👈 ONLY unsold
                                  select new { h, d, v, i, dm, o })
                                  .ToListAsync();

                // Latest Warranty List
                var warranties = await _context.OemmodelWarranties
                    .GroupBy(x => x.OemmodelId)
                    .Select(g => g.OrderByDescending(x => x.EffectiveDate).FirstOrDefault())
                    .ToListAsync();

                var result = (from x in data
                                  // LEFT JOIN WARRANTY
                              join ow in warranties
                                  on x.o != null ? x.o.Id : 0 equals ow.OemmodelId
                                  into wGroup
                              from ow in wGroup.DefaultIfEmpty()

                              select new LotInspectionChassisVM
                              {
                                  InvoiceNo = x.h.InvoiceNo,
                                  ChassisNumber = x.d.ChassisNo,
                                  CustomerName = x.dm.Compname,
                                  CustomerMobile = x.dm.Mobile,
                                  CustomerAltMobile = x.dm.PhoneOff,
                                  ModelName = x.i.Itemname,
                                  RegisterNo = x.v.Regnumber,
                                  BatteryNumber = x.v.BatteryNo,
                                  ChargerNumber = x.v.ChargerNo,
                                  ControllerNo = x.v.ControllerNo,
                                  BatteryMake = x.v.BatteryMake,
                                  BatteryCapacity = x.v.BatteryCapacity,
                                  BatteryChemestry = x.v.BatteryChemistry,
                                  ConverterNo = x.v.Converter,
                                  MotorNo = x.v.MotorNo,

                                  //  Warranty (optional)
                                  OdoReading = ow?.Odoreading,
                                  Duration = ow?.Duration,
                                  DurationType = ow?.DurationType,
                                  EffectiveDate = ow?.EffectiveDate,

                                  ExpireWarrentyDate = ow?.EffectiveDate == null ? null :
                                      ow.DurationType == "MONTH"
                                          ? ow.EffectiveDate.Value.AddMonths((int)(ow.Duration ?? 0))
                                          : ow.DurationType == "YEAR"
                                              ? ow.EffectiveDate.Value.AddYears((int)(ow.Duration ?? 0))
                                              : ow.EffectiveDate
                              }).ToList();

                return result;
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
        public async Task<List<PdichecklistMaster>> GetPdichecklist()
        {
            return await _context.PdichecklistMasters.ToListAsync();
        }
        public async Task<List<JobCardDetailsViewModel>> GetJobCardListViewAsync(string? dealerCode)
        {
            var jobCardsResult = await (

                from jh in _context.JobCardHeaders

                    // CUSTOMER
                join c in _context.JobCardCustomers
                    on jh.Id equals c.JobCardHeaderId into custJoin
                from c in custJoin.DefaultIfEmpty()

                    // JOB TYPE
                join job in _context.JobTypes
                    on jh.Jobtype equals job.Id into jobJoin
                from job in jobJoin.DefaultIfEmpty()

                    // SERVICE HEAD
                join sh in _context.ServiceHeads
                    on jh.Servicehead equals sh.Id into shJoin
                from sh in shJoin.DefaultIfEmpty()

                    // SERVICE TYPE
                join st in _context.ServiceTypes
                    on jh.Servicetype equals st.Id into stJoin
                from st in stJoin.DefaultIfEmpty()

                    // JOB SOURCE
                join js in _context.JobSources
                    on jh.JobSource equals js.Id into jsJoin
                from js in jsJoin.DefaultIfEmpty()

                    // LOCATION
                join loc in _context.LocationMasters
                    on jh.Serviceloc equals loc.Loccode into locJoin
                from loc in locJoin.DefaultIfEmpty()

                    // LEDGER
                join lg in _context.LedgerMasters
                    on c.CustomerLedgerId equals lg.Id into lgJoin
                from lg in lgJoin.DefaultIfEmpty()

                    // STATE
                join sta in _context.States
                    on lg.State equals sta.StateId into staJoin
                from sta in staJoin.DefaultIfEmpty()

                select new JobCardDetailsViewModel
                {
                    // ================= DISPLAY =================

                    Jobtype = job != null ? job.JobTypeName : null,
                    Jobsource = js != null ? js.JobSourceName : null,
                    serviceHead = sh != null ? sh.ServiceHeadName : null,
                    serviceType = st != null ? st.ServiceTypeName : null,
                    Location = loc != null ? loc.Locname : null,

                    PartyName = lg != null ? lg.LedgerName : null,
                    PartyMobileNo = lg != null ? lg.MobileNumber : null,
                    PartyState = sta != null ? sta.StateName : null,

                    // ================= HEADER =================

                    JobCardHeader = new JobCardHeaderVM
                    {
                        Id = jh.Id,
                        DealerCode = jh.DealerCode,
                        Jobtype = jh.Jobtype,
                        Servicehead = jh.Servicehead,
                        Servicetype = jh.Servicetype,
                        JobSource = jh.JobSource,
                        Chassisno = jh.Chassisno,
                        Couponno = jh.Couponno,
                        Jobprefix = jh.Jobprefix,
                        JobNo = jh.JobNo,
                        Vehiclekms = jh.Vehiclekms,
                        JobinDate = jh.JobinDate,
                        JobinTime = jh.JobinTime,
                        EstdelDate = jh.EstdelDate,
                        EstdelTime = jh.EstdelTime,
                        InvoiceNo = jh.InvoiceNo,
                        ManualjobNo = jh.ManualjobNo,
                        Serviceloc = jh.Serviceloc,
                        Supervisor = jh.Supervisor,
                        Technician = jh.Technician,
                        Jobestmate = jh.Jobestmate,
                        AirpressureRearTyre = jh.AirpressureRearTyre,
                        AirpressurefrontTyre = jh.AirpressurefrontTyre,
                        IsPdiSuccess = jh.IsPdiSuccess,
                        Observation = jh.Observation,
                        SupervisorComment = jh.SupervisorComment
                    },

                    // ================= BATTERY =================

                    JobCardBattery = _context.JobCardBatteryDetails
                        .Where(x => x.JobCardHeaderId == jh.Id)
                        .Select(x => new JobCardBatteryVM
                        {
                            JobCardHeaderId = x.JobCardHeaderId,
                            BatteryMake = x.BatteryMake,
                            BatterySerialNo = x.BatterySerialNo,
                            BatteryOcv = x.BatteryOcv,
                            BatteryCcv = x.BatteryCcv,
                            BatteryDischarge = x.BatteryDischarge,
                            BatteryCapacityAh = x.BatteryCapacityAh,
                            BatteryVoltage = x.BatteryVoltage,
                            MotorDrawing = x.MotorDrawing,
                            ChargerMake = x.ChargerMake,
                            ChargerNo = x.ChargerNo,
                            ConverterNo = x.ConverterNo,
                            ControllerNo = x.ControllerNo,
                            BatteryChemical = x.BatteryChemical,
                            BatteryCapacity = x.BatteryCapacity
                        })
                        .FirstOrDefault(),

                    // ================= CUSTOMER =================

                    JobCardCustomer = c == null ? null : new JobCardCustomerVM
                    {
                        Id = c.Id,
                        JobCardHeaderId = c.JobCardHeaderId,
                        SaleDate = c.SaleDate,
                        RegisterNo = c.RegisterNo,
                        ChassisNo = c.ChassisNo,
                        ModelName = c.ModelName,
                        CustomerName = c.CustomerName,
                        CustomerMobile = c.CustomerMobile,
                        CustomerAltMobile = c.CustomerAltMobile,
                        MotorNo = c.MotorNo,
                        BatteryNo = c.BatteryNo,
                        InsuranceExpDate = c.InsuranceExpDate,
                        NextserviceDueDate = c.NextserviceDueDate,
                        RsarenewalDate = c.RsarenewalDate,
                        Remarks = c.Remarks
                    },

                    // ================= COMPLAINT LIST =================

                    JobCardComplaint = _context.JobCardComplaints
                        .Where(x => x.JobCardHeaderId == jh.Id)
                        .Select(x => new JobCardComplaintVM
                        {
                            Id = x.Id,
                            JobCardHeaderId = x.JobCardHeaderId,
                            Complaint = x.Complaint,
                            ComplaintCode = x.ComplaintCode,
                            CustomerVoice = x.CustomerVoice
                        })
                        .ToList(),

                    // ================= PDI =================

                    PdiChecklistChassiWise = _context.PdichecklistChassisWises
                        .Where(x => x.JobCardMasterId == jh.Id)
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

                    // ================= FIRST COMPLAINT =================

                    Complaint = _context.JobCardComplaints
                        .Where(x => x.JobCardHeaderId == jh.Id)
                        .Select(x => x.Complaint)
                        .FirstOrDefault()
                }

            )
            .GroupBy(x => x.JobCardHeader.Id)
            .Select(g => g.First())
            .ToListAsync();



            // DEALER FILTER
            if (!string.IsNullOrWhiteSpace(dealerCode))
            {
                jobCardsResult = jobCardsResult
                    .Where(x => x.JobCardHeader.DealerCode == dealerCode)
                    .ToList();
            }

            return jobCardsResult;
        }
        public async Task<int> InsertJobCardinfoDetails(JobCardDetailsViewModel jobCardDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                // Insert Header

                var header = new JobCardHeader
                {
                    Jobtype = jobCardDetails.JobCardHeader.Jobtype,
                    DealerCode = jobCardDetails.JobCardHeader.DealerCode,
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
                    CreatedBy = jobCardDetails.JobCardHeader.CreatedBy,
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
                        CreatedBy = jobCardDetails.JobCardBattery.CreatedBy,
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
                        CreatedBy = jobCardDetails.JobCardCustomer.CreatedBy,
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
                        CreatedBy = c.CreatedBy,
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
                        IsStatus = p.IsStatus,
                        Remarks = p.Remarks,
                        CreatedBy = p.CreatedBy,
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

        public async Task<List<JobCardDetailsViewModel>> SearchJobCards(JobCardSearchModel model)
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
            var result = await query.Select(x => new JobCardDetailsViewModel
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
        //public async Task<JobCardViewModel> GetJobCardById(int id)
        //{
        //    try
        //    {
        //        var result = "";
        //    }
        //    catch { throw; }
        //}
        public async Task<JobCardHeader?> GetJobCardById(int id)
        {
            return await _context.JobCardHeaders
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ServiceHistoryViewModel>> GetServiceHistoryViewModellist(string chassisNo)
        {
            try
            {
                var rawData = await (from o in _context.OemmodelMasters
                                     join i in _context.ItemMasters on o.ModelName equals i.Oemmodelname
                                     join m in _context.ModelwiseServiceSchedules on o.Id equals m.OemmodelId
                                     join j in _context.JobCardCustomers on i.Itemname equals j.ModelName
                                     join sh in _context.ServiceHeads on m.ServiceHead equals sh.Id
                                     join st in _context.ServiceTypes on m.ServiceType equals st.Id
                                     where j.ChassisNo == chassisNo && m.EffectiveDate == _context.ModelwiseServiceSchedules
                                     .Where(x => x.OemmodelId == o.Id).Max(x => x.EffectiveDate)
                                     orderby m.Seqno
                                     select new { m, j, sh, st }).ToListAsync();
                // Memory filter (DateOnly issue fix) 

                // Final mapping
                var finalResult = rawData.Select(x =>
                {
                    DateTime? dueDate = null; DateTime? graceDate = null;
                    string status = "Pending";
                    if (x.j.SaleDate.HasValue)
                    {
                        dueDate = x.j.SaleDate.Value.AddDays(x.m.DaysTo);
                        graceDate = dueDate.Value.AddDays(15);
                        var today = DateTime.Today;

                        //pending when clain service table create i uncommet it
                        //if (x.j.ClaimDate != null)
                        //{
                        //    status = "Availed";
                        //}
                        //else if (today < dueDate)
                        //{
                        //    status = "Upcoming";
                        //}
                        //else if (today >= dueDate && today <= graceDate)
                        //{
                        //    status = "Due";
                        //}
                        //else if (today > graceDate)
                        //{
                        //    status = "Overdue";
                        //}
                    }
                    return new ServiceHistoryViewModel
                    {
                        srno = x.m.Id,
                        serviceseq = x.m.Seqno,
                        serviceHead = x.sh.ServiceHeadName,
                        serviceType = x.st.ServiceTypeName,
                        DealerName = x.j.CustomerName,
                        DueDate = dueDate,
                        GraceDate = graceDate,
                        ServiceStatus = status,
                        ClaimDate = DateTime.Now
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
                                                    ModelName = jc.ModelName,
                                                    Vehiclekms = jh.Vehiclekms,
                                                    RegisterNo = jc.RegisterNo,
                                                    Observation = jh.Observation,
                                                    Serviceloc = jh.Serviceloc,
                                                    LocationName = loc.Locname,
                                                    VehicleSaleDate = jc.SaleDate,
                                                    CustomerVoice = _context.JobCardComplaints
                                                                .Where(c => c.JobCardHeaderId == jh.Id)
                                                                .Select(c => c.CustomerVoice)
                                                                .FirstOrDefault(),
                                                    ComplaintCode = _context.JobCardComplaints
                                                                .Where(c => c.JobCardHeaderId == jh.Id)
                                                                .Select(c => c.ComplaintCode)
                                                                .FirstOrDefault(),
                                                    Complaint = _context.JobCardComplaints
                                                                .Where(c => c.JobCardHeaderId == jh.Id)
                                                                .Select(c => c.Complaint)
                                                                .FirstOrDefault(),
                                                }).FirstOrDefaultAsync();

            return ObjCIRJobcardViewModel;
        }
    }
}
