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

        // get inspected lot chassis based on invoice number
        //public async Task<List<LotInspectionChassisVM>> GetAllInspectedLotChassisAsync(string dealerCode)
        //{
        //    var result = await (from h in _context.LotinspectionHeaders
        //                        join d in _context.LotinspectionDetails
        //                            on h.Id equals d.LotHeaderId
        //                        join v in _context.VehicleDispatches
        //                            on d.ChassisNo equals v.ChasisNo
        //                        where h.IsLotInspected == true
        //                              && h.DealerCode == dealerCode
        //                        join c in _context.DealerMasters
        //                            on h.DealerCode equals c.Dealercode
        //                            join i in _context.ItemMasters
        //                                on v.ItemCode equals i.Itemcode
        //                        select new LotInspectionChassisVM
        //                        {
        //                            InvoiceNo = h.InvoiceNo,
        //                            ChassisNumber = d.ChassisNo,
        //                            CustomerName = c.Compname,
        //                            CustomerMobile = c.Mobile,
        //                            CustomerAltMobile = c.PhoneOff,
        //                            ModelName = i.Itemname,
        //                            RegisterNo = v.Regnumber,
        //                            BatteryNumber = v.BatteryNo,
        //                            ChargerNumber = v.ChargerNo,
        //                            ControllerNo = v.ControllerNo,
        //                            BatteryMake = v.BatteryMake,
        //                            BatteryCapacity = v.BatteryCapacity,
        //                            BatteryChemestry = v.BatteryChemistry,
        //                            ConverterNo = v.Converter,
        //                            MotorNo = v.MotorNo
        //                        }).ToListAsync();

        //    return result;
        //}

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

                // ✅ Latest Warranty List
                var warranties = await _context.OemmodelWarranties
                    .GroupBy(x => x.OemmodelId)
                    .Select(g => g.OrderByDescending(x => x.EffectiveDate).FirstOrDefault())
                    .ToListAsync();

                var result = (from x in data
                                  // ✅ LEFT JOIN WARRANTY
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

                                  // ✅ Warranty (optional)
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


        public async Task<List<JobCardListViewModel>> GetJobCardListViewAsync(string dealerCode)
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

                select new JobCardListViewModel
                {
                    JobNo = jh.JobNo,
                    JobInDate = jh.JobinDate,
                    InvoiceNo = jh.InvoiceNo,

                    JobStatus = c.SaleDate,

                    ManualJobNo = jh.ManualjobNo,
                    Joblocation = jh.Serviceloc,

                    Jobtype = job != null ? job.JobTypeName : null,
                    Jobsource = js != null ? js.JobSourceName : null,

                    Supervisor = jh.Supervisor,

                    RegisterNo = c != null ? c.RegisterNo : null,
                    ChassisNo = c != null ? c.ChassisNo : null,
                    ModelName = c != null ? c.ModelName : null,
                    ModelType = "External",

                    serviceHead = sh != null ? sh.ServiceHeadName : null,
                    serviceType = st != null ? st.ServiceTypeName : null,

                    CustomerName = c != null ? c.CustomerName : null,
                    MobileNo = c != null ? c.CustomerMobile : null,

                    //  Avoid duplicate rows (IMPORTANT)
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
    }
}
