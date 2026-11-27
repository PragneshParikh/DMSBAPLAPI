using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.JobCardScanner;
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
        private readonly JobCardScannerDbContext _jobCardScannerContext;

        public FFIRRepo(BapldmsvadContext context, JobCardScannerDbContext jobCardScannerContext)
        {
            _context = context;
            _jobCardScannerContext = jobCardScannerContext;
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
            try
            {
                if (string.IsNullOrWhiteSpace(chassisNo))
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
            catch (Exception ex)
            {
                // Temporary debugging
                Console.WriteLine("GetJobCardHistory Error");
                Console.WriteLine(ex.ToString());

                throw; // actual exception controller tak pahunch jayegi
            }
        }
        public async Task<int> InsertFFIRAsync(FFIRViewModel model,string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            //var maxCirNo = await _context.Ffirheaders
            //                    .MaxAsync(x => (int?)x.Cirno) ?? 0;

            //var nextCirNo = maxCirNo + 1;

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
                    B2BJobCardId = model.B2BJobCardId,
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
                    CreatedBy = userId,
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
                        PartAffectedDescription = x.PartAffectedDescription,
                        CreatedBy = userId,
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
                        CreatedBy = userId,
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

                    JobNo = jh.JobNo.ToString(),

                    jobDate = jh.JobinDate,

                    JobType = "B2C",

                    IsDelete = ffir.IsDelete,

                    CustomerName = jc.CustomerName,
                    B2BJobCardId = ffir.B2BJobCardId,

                    ModelName = jc.ModelName,

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

                    || (x.JobNo ?? "").Contains(search)
                );
            }

            if (!string.IsNullOrWhiteSpace(dealerCode))
                query = query.Where(x => x.DealerCode == dealerCode);

            var results = await query
                .Where(x => x.IsDelete != true)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var b2bHeadersQuery = _context.Ffirheaders
                .AsNoTracking()
                .Where(x => x.B2BJobCardId.HasValue && x.IsDelete != true);
            if (!string.IsNullOrWhiteSpace(dealerCode))
                b2bHeadersQuery = b2bHeadersQuery.Where(x => x.DealerCode == dealerCode);

            var b2bHeaders = await b2bHeadersQuery
                .Select(x => new
                {
                    x.Id,
                    x.Ffirprefix,
                    x.DealerCode,
                    x.Cirno,
                    x.Cirdate,
                    x.B2BJobCardId,
                    x.FfirchassisNo,
                    x.PurposeOfCir
                })
                .ToListAsync();

            if (b2bHeaders.Count == 0)
                return results;

            var b2bIds = b2bHeaders
                .Where(x => x.B2BJobCardId.HasValue)
                .Select(x => x.B2BJobCardId!.Value)
                .ToList();
            var b2bJobCards = await _jobCardScannerContext.JobCards
                .AsNoTracking()
                .Where(x => b2bIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.JobCardNumber,
                    x.CreatedAt,
                    CustomerName = x.Customer != null ? x.Customer.Name : null,
                    ModelName = x.Vehicle != null ? x.Vehicle.Model : null,
                    CurrentKms = (int?)x.OdometerAtCheckIn
                })
                .ToListAsync();
            var b2bJobCardsById = b2bJobCards.ToDictionary(x => x.Id);

            var b2bRows = b2bHeaders.Select(header =>
            {
                var b2bId = header.B2BJobCardId!.Value;
                b2bJobCardsById.TryGetValue(b2bId, out var jobCard);
                return new FFIRViewModelList
                {
                    Id = header.Id,
                    FFIRPrefix = header.Ffirprefix,
                    DealerCode = header.DealerCode,
                    CIRNo = header.Cirno,
                    CIRDate = header.Cirdate,
                    JobNo = jobCard?.JobCardNumber ?? b2bId.ToString(),
                    jobDate = jobCard == null ? null : DateOnly.FromDateTime(jobCard.CreatedAt),
                    JobType = "B2B",
                    CustomerName = jobCard?.CustomerName ?? "",
                    ModelName = jobCard?.ModelName ?? header.FfirchassisNo,
                    PurposeOfCIR = header.PurposeOfCir,
                    CurrentKms = jobCard?.CurrentKms,
                    B2BJobCardId = b2bId,
                    IsDelete = false
                };
            }).Where(x => string.IsNullOrWhiteSpace(search)
                || (x.CustomerName ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                || (x.ModelName ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                || (x.PurposeOfCIR ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.CIRNo.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)
                || (x.JobNo ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));

            results.AddRange(b2bRows);
            return results.OrderByDescending(x => x.Id).ToList();
        }

        public async Task<FFIRViewModel> GetFFIRById(int id)
        {
            
            var data = await _context.Ffirheaders
                    .Where(x => x.Id == id)
                    .Select(x => new FFIRViewModel
                    {
                        Id = x.Id,
                        CIRNo = x.Cirno,
                        FFIRPrefix = x.Ffirprefix,
                        DealerCode = x.DealerCode,
                        JobCardHeaderId = x.JobCardHeaderId,
                        JobCardCustomerId = x.JobCardCustomerId,
                        B2BJobCardId = x.B2BJobCardId,
                        CIRDate = x.Cirdate,
                        PurposeOfCIR = x.PurposeOfCir,
                        FFIRChassisNo = x.FfirchassisNo,
                        FailureDate = x.FailureDate,
                        ReportTitle = x.ReportTitle,
                        ReportPreparedBy = x.ReportPreparedBy,
                        NoOfPassenger = x.NoOfPassenger,
                        TypeOfRoadSurface = x.TypeOfRoadSurface,
                        RepeatFailure = x.RepeatFailure,
                        ChassisModified = x.ChassisModified,
                        FFIRRemarks = x.Ffirremarks,

                        // Main Parts
                        MainParts = _context.MainPartAffectedFfirs
                            .Where(m => m.Ffirid == x.Id)
                            .Select(m => new MainPartAffectedFFIRViewModel
                            {
                                PartAffectedName = m.PartAffectedName,
                                PartAffectedDescription = m.PartAffectedDescription
                            }).ToList(),

                        // Complaints
                        FFIRJobcardComplaints = _context.JobCardComplaints
                            .Where(c => c.JobCardHeaderId == x.JobCardHeaderId)
                            .Select(c => new FFirCompalintCodeListViewModel
                            {
                                Id = c.Id,
                                Complaint = c.Complaint
                            }).ToList(),

                        // Observation
                        DetailObservation = _context.FfirdetailObservations
                            .Where(o => o.Ffirid == x.Id)
                            .Select(o => new FFIRDetailObservationViewModel
                            {
                                ObservationFailedParts = o.ObservationFailedParts,
                                RootCauseofFailure = o.RootCauseofFailure,
                                CorrectiveAction = o.CorrectiveAction,
                                ResolutionComplaint = o.ResolutionComplaint,
                                PresentStatusofVehicle = o.PresentStatusofVehicle,
                                VehicleOffRoadReason = o.VehicleOffRoadReason
                            })
                            .FirstOrDefault()

                    })
                    .FirstOrDefaultAsync();

            return data;
        }
        public async Task<bool> UpdateFFIRAsync(int id, FFIRViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ffirHeader = await _context.Ffirheaders
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (ffirHeader == null)
                    return false;

                // Header Update
                ffirHeader.Cirdate = model.CIRDate;
                ffirHeader.PurposeOfCir = model.PurposeOfCIR;
                ffirHeader.FfirchassisNo = model.FFIRChassisNo;
                ffirHeader.FailureDate = model.FailureDate;
                ffirHeader.ReportTitle = model.ReportTitle;
                ffirHeader.ReportPreparedBy = model.ReportPreparedBy;
                ffirHeader.NoOfPassenger = model.NoOfPassenger;
                ffirHeader.TypeOfRoadSurface = model.TypeOfRoadSurface;
                ffirHeader.RepeatFailure = model.RepeatFailure;
                ffirHeader.ChassisModified = model.ChassisModified;
                ffirHeader.Ffirremarks = model.FFIRRemarks;

                ffirHeader.UpdatedBy = "Admin";
                ffirHeader.UpdatedDate = DateTime.Now;

                // Remove Old Main Parts
                var oldParts = await _context.MainPartAffectedFfirs
                    .Where(x => x.Ffirid == id)
                    .ToListAsync();

                _context.MainPartAffectedFfirs.RemoveRange(oldParts);

                // Add New Main Parts
                if (model.MainParts != null && model.MainParts.Any())
                {
                    var mainParts = model.MainParts.Select(x => new MainPartAffectedFfir
                    {
                        Ffirid = id,
                        PartAffectedName = x.PartAffectedName,
                        PartAffectedDescription = x.PartAffectedDescription,
                        CreatedBy = "Admin",
                        CreatedDate = DateTime.Now
                    }).ToList();

                    _context.MainPartAffectedFfirs.AddRange(mainParts);
                }

                // Observation Update
                var observation = await _context.FfirdetailObservations
                    .FirstOrDefaultAsync(x => x.Ffirid == id);

                if (observation != null)
                {
                    observation.ObservationFailedParts = model.DetailObservation.ObservationFailedParts;
                    observation.RootCauseofFailure = model.DetailObservation.RootCauseofFailure;
                    observation.CorrectiveAction = model.DetailObservation.CorrectiveAction;
                    observation.ResolutionComplaint = model.DetailObservation.ResolutionComplaint;
                    observation.PresentStatusofVehicle = model.DetailObservation.PresentStatusofVehicle;
                    observation.VehicleOffRoadReason = model.DetailObservation.VehicleOffRoadReason;

                    observation.UpdatedBy = "Admin";
                    observation.UpdatedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> DeleteFFIR(int ffirId, string role)
        {

            var ffirHeader = await _context.Ffirheaders.FirstOrDefaultAsync(x => x.Id == ffirId);
            if (ffirHeader == null)
            {
                throw new Exception("FFIR header not found");
            }
            if (role != "SuperAdmin")
            {
                throw new Exception("Only SuperAdmin can delete FFIR.");
            }
           

            // Delete FFIR if exists
            var ffirs = await _context.Ffirheaders
            .Where(x => x.Id == ffirId)
            .ToListAsync();
            if (ffirs.Any())
            {
                var ffirHeaderDetails = _context.Ffirheaders;
                ffirs.ForEach(ffir =>
                {
                    var ffirDetails = _context.Ffirheaders.Where(d => d.Id == ffirId).ToList();
                    ffirDetails.ForEach(detail =>
                    {
                        detail.IsDelete = true;
                        detail.UpdatedBy = role;
                        detail.UpdatedDate = DateTime.UtcNow;
                    });
                });
            }

            _context.Ffirheaders.Update(ffirHeader);
            return await _context.SaveChangesAsync();
        }


    }
}
