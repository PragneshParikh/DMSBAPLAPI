using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<LotInspectionChassisVM>> GetAllInspectedLotChassisAsync(string dealerCode)
        {
            try
            {
                var data = await (from h in _context.LotinspectionHeaders
                                  join d in _context.LotinspectionDetails
                                      on h.Id equals d.LotHeaderId
                                  join v in _context.VehicleDispatches
                                      on d.ChassisNo equals v.ChasisNo
                                  join i in _context.ItemMasters
                                      on v.ItemCode equals i.Itemcode
                                  join dm in _context.DealerMasters
                                      on h.DealerCode equals dm.Dealercode

                                  // OEM Model (LEFT JOIN)
                                  join o in _context.OemmodelMasters
                                      on i.Oemmodelname.Trim().ToLower()
                                      equals o.ModelName.Trim().ToLower()
                                      into oGroup
                                  from o in oGroup.DefaultIfEmpty()

                                  where h.IsLotInspected == true
                                        && h.DealerCode == dealerCode

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

        public async Task<List<JobCardDetailsViewModel>> GetJobCardListViewAsync(string dealerCode)
        {
            var jobCardsResult = await (
                from jh in _context.JobCardHeaders

                join inv in _context.LotinspectionHeaders
                    on jh.InvoiceNo equals inv.InvoiceNo into invJoin
                from inv in invJoin.DefaultIfEmpty()

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

                where jh.DealerCode == dealerCode

                select new JobCardDetailsViewModel
                {
                    //  Display purpose (Grid)
                    Jobtype = job != null ? job.JobTypeName : null,
                    Jobsource = js != null ? js.JobSourceName : null,
                    serviceHead = sh != null ? sh.ServiceHeadName : null,
                    serviceType = st != null ? st.ServiceTypeName : null,

                    //  Header FULL DATA (Edit ke liye)
                    JobCardHeader = new JobCardHeaderVM
                    {
                        Id = jh.Id,
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

                    JobCardBattery = new JobCardBatteryVM
                    {
                        JobCardHeaderId = jh.Id,
                        BatteryMake = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatteryMake)
                            .FirstOrDefault(),
                        BatterySerialNo = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatterySerialNo)
                            .FirstOrDefault(),
                        BatteryOcv = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatteryOcv)
                            .FirstOrDefault(),
                        BatteryCcv = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatteryCcv)
                            .FirstOrDefault(),
                        BatteryDischarge = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatteryDischarge)
                            .FirstOrDefault(),
                        BatteryCapacityAh = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatteryCapacityAh)
                            .FirstOrDefault(),
                        BatteryVoltage = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatteryVoltage)
                            .FirstOrDefault(),
                        MotorDrawing = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.MotorDrawing)
                            .FirstOrDefault(),
                        ChargerMake = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.ChargerMake)
                            .FirstOrDefault(),
                        ChargerNo = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.ChargerNo)
                            .FirstOrDefault(),
                        ConverterNo = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.ConverterNo)
                            .FirstOrDefault(),
                        ControllerNo = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.ControllerNo)
                            .FirstOrDefault(),
                        BatteryChemical = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatteryChemical)
                            .FirstOrDefault(),
                        BatteryCapacity = _context.JobCardBatteryDetails
                            .Where(x => x.JobCardHeaderId == jh.Id)
                            .Select(x => x.BatteryCapacity)
                            .FirstOrDefault()
                    },

                    // Customer
                    JobCardCustomer = new JobCardCustomerVM
                    {
                        JobCardHeaderId = jh.Id,
                        SaleDate = c.SaleDate,
                        RegisterNo = c != null ? c.RegisterNo : null,
                        ChassisNo = c != null ? c.ChassisNo : null,
                        ModelName = c != null ? c.ModelName : null,
                        CustomerName = c != null ? c.CustomerName : null,
                        CustomerMobile = c != null ? c.CustomerMobile : null,
                        CustomerAltMobile = c != null ? c.CustomerAltMobile : null,
                        MotorNo = c != null ? c.MotorNo : null,
                        BatteryNo = c != null ? c.BatteryNo : null,
                        InsuranceExpDate = c.InsuranceExpDate,
                        NextserviceDueDate = c.NextserviceDueDate,
                        RsarenewalDate = c.RsarenewalDate,
                        Remarks = c.Remarks
                    },

                    //  Complaint LIST (IMPORTANT FIX)
                    JobCardComplaint = _context.JobCardComplaints
                        .Where(x => x.JobCardHeaderId == jh.Id)
                        .Select(x => new JobCardComplaintVM
                        {
                            Id = x.Id,
                            JobCardHeaderId = x.JobCardHeaderId,
                            Complaint = x.Complaint,
                            ComplaintCode = x.ComplaintCode,
                            CustomerVoice = x.CustomerVoice
                        }).ToList(),

                    //  PDI LIST (VERY IMPORTANT for edit)
                    PdiChecklistChassiWise = _context.PdichecklistChassisWises
                          .Where(x => x.JobCardMasterId == jh.Id)
                          .Select(x => new PdiChecklistChassiWiseVM
                          {
                              Id = x.Id,
                              PdichecklistMasterId = x.PdichecklistMasterId,
                              JobCardMasterId = jh.Id,
                              IsStatus = x.IsStatus,
                              Remarks = x.Remarks,
                              CreatedBy = x.CreatedBy,
                              CreatedDate = x.CreatedDate
                          }).ToList(),

                    //  Old field (keep as it is — tumne bola remove mat karo)
                    Complaint = _context.JobCardComplaints
                        .Where(x => x.JobCardHeaderId == jh.Id)
                        .Select(x => x.Complaint)
                        .FirstOrDefault()
                }
            ).ToListAsync();

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

                // 🔹 UPDATE + INSERT
                foreach (var item in updateJobCardDetails.JobCardComplaint)
                {
                    if (item.Id > 0)
                    {
                        // UPDATE
                        var dbItem = existingComplaints.FirstOrDefault(x => x.Id == item.Id);

                        if (dbItem != null)
                        {
                            dbItem.CustomerVoice = item.CustomerVoice;
                            dbItem.ComplaintCode = item.ComplaintCode;
                            dbItem.Complaint = item.Complaint;
                            dbItem.UpdateBy = item.CreatedBy;
                            dbItem.UpdatedDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        // INSERT
                        var newItem = new JobCardComplaint
                        {
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
    }
}
