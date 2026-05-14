using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.FFIRRepo
{
    public class FFIRRepo : IFFIRRepo
    {
        private readonly BapldmsvadContext _context;

        public FFIRRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<List<PartDropdownviewmodel>> GetPartDropdownlist()
        {
            //var maxCIRNo = await _context.FFIRHeader
            //                   .MaxAsync(x => (int?)x.LotNo) ?? 0;
            var partDropdownlist = await Task.Run(() => _context.ItemMasters.Where(x => x.Grpidno == 1).Select(x => new PartDropdownviewmodel
            {
                itemId = x.Id,
                partAffectedName = x.Itemcode + "-" + x.Itemdesc,
            }).ToList());

            return partDropdownlist;
        }
        public async Task<List<FFirCompalintCodeListViewModel>> GetComplaintCodeList()
        {
            var complaintCodeList = await Task.Run(() => _context.JobCardComplaints.Select(x => new FFirCompalintCodeListViewModel
            {
                Id = x.Id,
                Complaint = x.Complaint
            }).ToList());
            return complaintCodeList;
        }

        public async Task<List<JobCardHistoryViewModel>> GetJobCardHistory(string chassisNo)
        {
            if (string.IsNullOrEmpty(chassisNo))
            {
                return new List<JobCardHistoryViewModel>();
            }

            var jobCardHistory = await (
                from jh in _context.JobCardHeaders

                join jc in _context.JobCardCustomers
                    on jh.Id equals jc.JobCardHeaderId

                join sh in _context.ServiceHeads
                    on jh.Servicehead equals sh.Id into shGroup
                from sh in shGroup.DefaultIfEmpty()

                join st in _context.ServiceTypes
                    on jh.Servicetype equals st.Id into stGroup
                from st in stGroup.DefaultIfEmpty()

                where jc.ChassisNo == chassisNo

                orderby jh.JobinDate descending

                select new JobCardHistoryViewModel
                {
                    JobCardId = jh.Id,
                    JobCardNo = jh.JobNo,
                    JobCardDate = jh.JobinDate,
                    VehicleJourney = jh.Vehiclekms,
                    Observation = jh.Observation,

                    ServiceHead = sh != null ? sh.ServiceHeadName : null,
                    ServiceType = st != null ? st.ServiceTypeName : null,

                    Remarks = jc.Remarks
                }

            ).ToListAsync();

            return jobCardHistory;
        }
        public async Task<int> InsertFFIRAsync(FFIRViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ffirHeader = new Ffirheader
                {
                    Ffirprefix = model.FFIRPrefix,
                    DealerCode = model.DealerCode,
                    Cirno = model.CIRNo,
                    Cirdate = model.CIRDate,
                    JobCardCustomerId = model.JobCardCustomerId,
                    JobCardHeaderId = model.JobCardHeaderId,
                    PurposeOfCir = model.PurposeOfCIR,
                    FfirchassisNo = model.FFIRChassisNo,
                    FailureDate = model.FailureDate,
                    ReportTitle = model.ReportTitle,
                    ReportPreparedBy = model.ReportPreparedBy,
                    NoOfPassenger = model.NoOfPassenger,
                    TypeOfRoadSurface = model.TypeOfRoadSurface,
                    RepeatFailure = model.RepeatFailure,
                    ChassisModified = model.ChassisModified,
                    Ffirremarks = model.FFIRRemarks,
                    CreatedBy = model.CreatedBy,
                    CreatedDate = DateTime.Now
                };

                _context.Ffirheaders.Add(ffirHeader);

                await _context.SaveChangesAsync();

                // Main Parts
                if (model.MainParts != null && model.MainParts.Any())
                {
                    var mainParts = model.MainParts.Select(x => new MainPartAffectedFfir
                    {
                        Ffirid = ffirHeader.Id,
                        PartAffectedName = x.PartAffectedName,
                        PartAffectedDescription = x.PartAffectedName,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = DateTime.Now
                    }).ToList();

                    _context.MainPartAffectedFfirs.AddRange(mainParts);
                }

                // Observation
                if (model.DetailObservation != null)
                {
                    var observation = new FfirdetailObservation
                    {
                        Ffirid = ffirHeader.Id,
                        ObservationFailedParts = model.DetailObservation.ObservationFailedParts,
                        RootCauseofFailure = model.DetailObservation.RootCauseofFailure,
                        CorrectiveAction = model.DetailObservation.CorrectiveAction,
                        ResolutionComplaint = model.DetailObservation.ResolutionComplaint,
                        PresentStatusofVehicle = model.DetailObservation.PresentStatusofVehicle,
                        VehicleOffRoadReason = model.DetailObservation.VehicleOffRoadReason,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = DateTime.Now
                    };

                    _context.FfirdetailObservations.Add(observation);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return ffirHeader.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<FFIRViewModelList>> GetFFIRDetailListing(
            string dealerCode,
            string? search)
        {
            var query =
                from ffir in _context.Ffirheaders

                join jh in _context.JobCardHeaders
                    on ffir.JobCardHeaderId equals jh.Id

                join jc in _context.JobCardCustomers
                    on ffir.JobCardCustomerId equals jc.Id

                select new FFIRViewModelList
                {
                    Id = ffir.Id,

                    FFIRPrefix = ffir.Ffirprefix,

                    DealerCode = ffir.DealerCode,

                    CIRNo = ffir.Cirno,

                    CIRDate = ffir.Cirdate,

                    JobNo = jh.JobNo,

                    jobDate = jh.JobinDate,

                    CustomerName = jc.CustomerName,

                    ModelName = _context.MainPartAffectedFfirs
                        .Where(x => x.Ffirid == ffir.Id)
                        .Join(
                            _context.ItemMasters,
                            mf => mf.PartAffectedName,
                            item => item.Itemcode,
                            (mf, item) => item.Itemdesc
                        )
                        .FirstOrDefault(),

                    PurposeOfCIR = ffir.PurposeOfCir,

                    CurrentKms = jh.Vehiclekms
                };

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                query = query.Where(x =>

                    x.CustomerName.ToLower().Contains(search)

                    || x.ModelName.ToLower().Contains(search)

                    || x.PurposeOfCIR.ToLower().Contains(search)

                    || x.CIRNo.ToString().Contains(search)

                    || x.JobNo.ToString().Contains(search)
                );
            }

            return await query
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

    }
}
