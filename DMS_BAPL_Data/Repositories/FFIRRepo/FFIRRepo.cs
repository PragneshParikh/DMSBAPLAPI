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
                return await AddVerificationStatus(results);

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
            return await AddVerificationStatus(results.OrderByDescending(x => x.Id).ToList());
        }

        private async Task<List<FFIRViewModelList>> AddVerificationStatus(List<FFIRViewModelList> rows)
        {
            if (rows.Count == 0)
                return rows;

            var ids = rows.Select(x => x.Id).Distinct().ToList();
            var verificationByFfirId = await _context.Ffirverifications
                .AsNoTracking()
                .Where(x => ids.Contains(x.FfirId))
                .ToDictionaryAsync(x => x.FfirId);

            foreach (var row in rows)
            {
                if (!verificationByFfirId.TryGetValue(row.Id, out var verification))
                    continue;

                row.VerificationStatus = verification.Status;
                row.VerificationRemarks = verification.Remarks;
            }

            return rows;
        }

        public async Task<List<FFIRVerificationViewModel>> GetVerificationList(
            DateTime? fromDate,
            DateTime? toDate,
            string? dealerCode,
            string? status,
            CancellationToken cancellationToken)
        {
            var rows = await (
                from ffir in _context.Ffirheaders.AsNoTracking()
                where ffir.B2BJobCardId == null && ffir.IsDelete != true
                join customerRow in _context.JobCardCustomers.AsNoTracking()
                    on ffir.JobCardCustomerId equals (int?)customerRow.Id into customerRows
                from customer in customerRows.DefaultIfEmpty()
                join jobRow in _context.JobCardHeaders.AsNoTracking()
                    on ffir.JobCardHeaderId equals (int?)jobRow.Id into jobRows
                from job in jobRows.DefaultIfEmpty()
                join dealerRow in _context.DealerMasters.AsNoTracking()
                    on ffir.DealerCode equals dealerRow.Dealercode into dealerRows
                from dealer in dealerRows.DefaultIfEmpty()
                select new FFIRVerificationViewModel
                {
                    Id = ffir.Id,
                    FFIRNumber = ffir.Ffirprefix + ffir.Cirno.ToString(),
                    FFIRDate = ffir.Cirdate ?? ffir.FailureDate,
                    DealerCode = ffir.DealerCode,
                    DealerName = dealer == null ? null : dealer.Compname,
                    City = dealer == null ? null : dealer.City,
                    ReportTitle = ffir.ReportTitle,
                    ReportPreparedBy = ffir.ReportPreparedBy,
                    TypeOfRoadSurface = ffir.TypeOfRoadSurface,
                    FFIRRemarks = ffir.Ffirremarks,
                    ChassisNo = customer == null ? ffir.FfirchassisNo : customer.ChassisNo,
                    RegisterNo = customer == null ? null : customer.RegisterNo,
                    CustomerMobile = customer == null ? null : customer.CustomerMobile,
                    BatteryNo = customer == null ? null : customer.BatteryNo,
                    Technician = job == null ? null : job.Technician,
                    JobType = "B2C",
                    JobCardNumber = job == null ? null : job.JobNo.ToString(),
                    JobCardDate = job == null || !job.JobinDate.HasValue
                        ? null
                        : job.JobinDate.Value.ToDateTime(TimeOnly.MinValue),
                    ServiceHead = job == null || job.ServiceheadNavigation == null ? null : job.ServiceheadNavigation.ServiceHeadName,
                    ServiceType = job == null || job.ServicetypeNavigation == null ? null : job.ServicetypeNavigation.ServiceTypeName,
                    SaleDate = customer == null ? null : customer.SaleDate,
                    CustomerName = customer == null ? null : customer.CustomerName,
                    ModelName = customer == null ? null : customer.ModelName,
                    Purpose = ffir.PurposeOfCir,
                    NoOfPassenger = ffir.NoOfPassenger,
                    RepeatFailure = ffir.RepeatFailure,
                    ChassisModified = ffir.ChassisModified,
                    CurrentKms = job == null ? null : (double?)job.Vehiclekms,
                    MotorNo = customer == null ? null : customer.MotorNo,
                    ChargerNo = null
                }).ToListAsync(cancellationToken);

            var b2bHeaders = await _context.Ffirheaders
                .AsNoTracking()
                .Where(x => x.B2BJobCardId.HasValue && x.IsDelete != true)
                .Select(x => new
                {
                    x.Id,
                    x.Ffirprefix,
                    x.Cirno,
                    x.Cirdate,
                    x.FailureDate,
                    x.DealerCode,
                    x.B2BJobCardId,
                    x.PurposeOfCir,
                    x.ReportTitle,
                    x.ReportPreparedBy,
                    x.TypeOfRoadSurface,
                    x.Ffirremarks,
                    x.FfirchassisNo,
                    x.NoOfPassenger,
                    x.RepeatFailure,
                    x.ChassisModified
                })
                .ToListAsync(cancellationToken);

            if (b2bHeaders.Count > 0)
            {
                var b2bIds = b2bHeaders.Select(x => x.B2BJobCardId!.Value).ToList();
                var jobCards = await _jobCardScannerContext.JobCards
                    .AsNoTracking()
                    .Where(x => b2bIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.JobCardNumber,
                        x.CreatedAt,
                        x.OdometerAtCheckIn,
                        x.BaplServiceHeadName,
                        x.BaplServiceTypeName,
                        x.BaplTechnicianName,
                        CustomerName = x.Customer == null ? null : x.Customer.Name,
                        CustomerMobile = x.Customer == null ? null : x.Customer.Mobile,
                        ModelName = x.Vehicle == null ? null : x.Vehicle.Model,
                        ChassisNo = x.Vehicle == null ? null : x.Vehicle.Vin,
                        RegisterNo = x.Vehicle == null ? null : x.Vehicle.RegNo,
                        BatteryNo = x.Vehicle == null ? null : x.Vehicle.BatteryNo,
                        SaleDate = x.Vehicle == null ? null : x.Vehicle.PurchaseDate,
                        MotorNo = x.Vehicle == null ? null : x.Vehicle.MotorNo,
                        ChargerNo = x.Vehicle == null ? null : x.Vehicle.ChargerNo,
                        DealerName = x.Dealer == null ? null : x.Dealer.Name,
                        DealerCity = x.Dealer == null ? null : x.Dealer.City
                    })
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

                rows.AddRange(b2bHeaders.Select(header =>
                {
                    jobCards.TryGetValue(header.B2BJobCardId!.Value, out var job);
                    return new FFIRVerificationViewModel
                    {
                        Id = header.Id,
                        FFIRNumber = header.Ffirprefix + header.Cirno,
                        FFIRDate = header.Cirdate ?? header.FailureDate,
                        DealerCode = header.DealerCode,
                        DealerName = job?.DealerName,
                        City = job?.DealerCity,
                        ReportTitle = header.ReportTitle,
                        ReportPreparedBy = header.ReportPreparedBy,
                        TypeOfRoadSurface = header.TypeOfRoadSurface,
                        FFIRRemarks = header.Ffirremarks,
                        ChassisNo = job?.ChassisNo ?? header.FfirchassisNo,
                        RegisterNo = job?.RegisterNo,
                        CustomerMobile = job?.CustomerMobile,
                        BatteryNo = job?.BatteryNo,
                        Technician = job?.BaplTechnicianName,
                        JobType = "B2B",
                        JobCardNumber = job?.JobCardNumber,
                        JobCardDate = job?.CreatedAt,
                        ServiceHead = job?.BaplServiceHeadName,
                        ServiceType = job?.BaplServiceTypeName,
                        SaleDate = job?.SaleDate?.ToDateTime(TimeOnly.MinValue),
                        CustomerName = job?.CustomerName,
                        ModelName = job?.ModelName,
                        Purpose = header.PurposeOfCir,
                        NoOfPassenger = header.NoOfPassenger,
                        RepeatFailure = header.RepeatFailure,
                        ChassisModified = header.ChassisModified,
                        CurrentKms = job?.OdometerAtCheckIn,
                        MotorNo = job?.MotorNo,
                        ChargerNo = job?.ChargerNo
                    };
                }));
            }

            var ffirIds = rows.Select(x => x.Id).Distinct().ToList();
            if (ffirIds.Count == 0)
                return rows;

            var jobCardReferences = await _context.Ffirheaders
                .AsNoTracking()
                .Where(x => ffirIds.Contains(x.Id))
                .Select(x => new { x.Id, x.JobCardHeaderId, x.B2BJobCardId, x.FfirchassisNo })
                .ToListAsync(cancellationToken);
            var chassisByFfirId = jobCardReferences.ToDictionary(x => x.Id, x => x.FfirchassisNo);
            var chassisNumbers = jobCardReferences
                .Select(x => x.FfirchassisNo)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
            var dispatchDates = chassisNumbers.Count == 0
                ? new Dictionary<string, DateTime>()
                : await _context.InvoiceDispatches
                    .AsNoTracking()
                    .Where(x => x.DispatchType == "VEHICLE"
                        && x.ChasisNo != null
                        && chassisNumbers.Contains(x.ChasisNo))
                    .GroupBy(x => x.ChasisNo!)
                    .Select(x => new { ChassisNo = x.Key, Date = x.Max(dispatch => dispatch.InvoiceDate) })
                    .ToDictionaryAsync(x => x.ChassisNo, x => x.Date, cancellationToken);
            var dmsJobCardIds = jobCardReferences
                .Where(x => x.JobCardHeaderId.HasValue)
                .Select(x => x.JobCardHeaderId!.Value)
                .Distinct()
                .ToList();
            var b2bJobCardIds = jobCardReferences
                .Where(x => x.B2BJobCardId.HasValue)
                .Select(x => x.B2BJobCardId!.Value)
                .Distinct()
                .ToList();
            var dmsChargerDetails = new List<(int JobCardHeaderId, string ChargerNo)>();
            if (dmsJobCardIds.Count > 0)
            {
                var chargerRows = await _context.JobCardBatteryDetails
                    .AsNoTracking()
                    .Where(x => dmsJobCardIds.Contains(x.JobCardHeaderId)
                        && !string.IsNullOrWhiteSpace(x.ChargerNo))
                    .Select(x => new { x.JobCardHeaderId, x.ChargerNo })
                    .ToListAsync(cancellationToken);
                dmsChargerDetails = chargerRows
                    .Select(x => (x.JobCardHeaderId, x.ChargerNo!))
                    .ToList();
            }
            var chargerNumbersByJobId = dmsChargerDetails
                .GroupBy(x => x.JobCardHeaderId)
                .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(detail => detail.ChargerNo).Distinct()));
            var dmsJobIdByFfirId = jobCardReferences
                .Where(x => x.JobCardHeaderId.HasValue)
                .ToDictionary(x => x.Id, x => x.JobCardHeaderId!.Value);
            var complaintsByFfirId = new Dictionary<int, List<string>>();
            var customerVoicesByFfirId = new Dictionary<int, List<string>>();

            if (dmsJobCardIds.Count > 0)
            {
                var dmsComplaints = await _context.JobCardComplaints
                    .AsNoTracking()
                    .Where(x => dmsJobCardIds.Contains(x.JobCardHeaderId))
                    .Select(x => new { x.JobCardHeaderId, x.Complaint, x.CustomerVoice })
                    .ToListAsync(cancellationToken);
                var ffirIdsByDmsJob = jobCardReferences
                    .Where(x => x.JobCardHeaderId.HasValue)
                    .GroupBy(x => x.JobCardHeaderId!.Value)
                    .ToDictionary(x => x.Key, x => x.Select(reference => reference.Id).ToList());

                foreach (var complaintGroup in dmsComplaints.GroupBy(x => x.JobCardHeaderId))
                {
                    if (!ffirIdsByDmsJob.TryGetValue(complaintGroup.Key, out var linkedFfirIds))
                        continue;
                    var complaintText = complaintGroup.Select(x => x.Complaint)
                        .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                    var customerVoice = complaintGroup.Select(x => x.CustomerVoice)
                        .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                    foreach (var linkedId in linkedFfirIds)
                    {
                        complaintsByFfirId[linkedId] = complaintText;
                        customerVoicesByFfirId[linkedId] = customerVoice;
                    }
                }
            }

            if (b2bJobCardIds.Count > 0)
            {
                var b2bComplaints = await _jobCardScannerContext.JobCardComplaints
                    .AsNoTracking()
                    .Where(x => b2bJobCardIds.Contains(x.JobCardId))
                    .Select(x => new { x.JobCardId, x.Description, x.IsCustomerVoice })
                    .ToListAsync(cancellationToken);
                var ffirIdsByB2bJob = jobCardReferences
                    .Where(x => x.B2BJobCardId.HasValue)
                    .GroupBy(x => x.B2BJobCardId!.Value)
                    .ToDictionary(x => x.Key, x => x.Select(reference => reference.Id).ToList());

                foreach (var complaintGroup in b2bComplaints.GroupBy(x => x.JobCardId))
                {
                    if (!ffirIdsByB2bJob.TryGetValue(complaintGroup.Key, out var linkedFfirIds))
                        continue;
                    var complaintText = complaintGroup.Select(x => x.Description)
                        .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                    var customerVoice = complaintGroup.Where(x => x.IsCustomerVoice)
                        .Select(x => x.Description)
                        .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                    foreach (var linkedId in linkedFfirIds)
                    {
                        complaintsByFfirId[linkedId] = complaintText;
                        customerVoicesByFfirId[linkedId] = customerVoice;
                    }
                }
            }

            var observations = await _context.FfirdetailObservations
                .AsNoTracking()
                .Where(x => ffirIds.Contains(x.Ffirid))
                .Select(x => new
                {
                    x.Ffirid,
                    x.ObservationFailedParts,
                    x.RootCauseofFailure,
                    x.CorrectiveAction,
                    x.ResolutionComplaint,
                    x.PresentStatusofVehicle,
                    x.VehicleOffRoadReason
                })
                .ToListAsync(cancellationToken);
            var observationsById = observations
                .GroupBy(x => x.Ffirid)
                .ToDictionary(x => x.Key, x => x.First());

            var parts = await _context.MainPartAffectedFfirs
                .AsNoTracking()
                .Where(x => ffirIds.Contains(x.Ffirid))
                .Select(x => new { x.Ffirid, x.PartAffectedName, x.PartAffectedDescription })
                .ToListAsync(cancellationToken);
            var partsById = parts
                .GroupBy(x => x.Ffirid)
                .ToDictionary(x => x.Key, x => string.Join(", ", x
                    .Select(part => part.PartAffectedName ?? part.PartAffectedDescription)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct()));

            var decisions = await _context.Ffirverifications
                .AsNoTracking()
                .Where(x => ffirIds.Contains(x.FfirId))
                .ToDictionaryAsync(x => x.FfirId, cancellationToken);

            foreach (var row in rows)
            {
                if (chassisByFfirId.TryGetValue(row.Id, out var chassisNo)
                    && !string.IsNullOrWhiteSpace(chassisNo)
                    && dispatchDates.TryGetValue(chassisNo, out var dispatchDate))
                    row.DispatchDate = dispatchDate;

                if (dmsJobIdByFfirId.TryGetValue(row.Id, out var dmsJobId)
                    && chargerNumbersByJobId.TryGetValue(dmsJobId, out var chargerNumber))
                    row.ChargerNo = chargerNumber;

                if (complaintsByFfirId.TryGetValue(row.Id, out var complaints))
                    row.Complaint = string.Join("; ", complaints);

                if (customerVoicesByFfirId.TryGetValue(row.Id, out var customerVoices))
                    row.CustomerVoice = string.Join("; ", customerVoices);

                if (observationsById.TryGetValue(row.Id, out var observation))
                {
                    row.DetailedObservation = observation.ObservationFailedParts;
                    row.RootCause = observation.RootCauseofFailure;
                    row.CorrectiveAction = observation.CorrectiveAction;
                    row.QuickSuggestion = observation.ResolutionComplaint;
                    row.PresentVehicleStatus = observation.PresentStatusofVehicle;
                    row.VehicleOffRoadReason = observation.VehicleOffRoadReason;
                }

                if (partsById.TryGetValue(row.Id, out var mainParts))
                    row.MainPartAffected = mainParts;

                if (decisions.TryGetValue(row.Id, out var decision))
                {
                    row.Status = decision.Status;
                    row.Remarks = decision.Remarks;
                }
            }

            if (fromDate.HasValue)
                rows = rows.Where(x => x.FFIRDate >= fromDate.Value.Date).ToList();
            if (toDate.HasValue)
                rows = rows.Where(x => x.FFIRDate < toDate.Value.Date.AddDays(1)).ToList();
            if (!string.IsNullOrWhiteSpace(dealerCode))
                rows = rows.Where(x => string.Equals(x.DealerCode, dealerCode, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
                rows = rows.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            return rows.OrderByDescending(x => x.FFIRDate).ThenByDescending(x => x.Id).ToList();
        }

        public async Task<bool> SetVerificationStatus(int ffirId, string status, string? remarks, string reviewedBy, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            if (!await _context.Ffirheaders.AnyAsync(x => x.Id == ffirId && x.IsDelete != true, cancellationToken))
                return false;

            var verification = await _context.Ffirverifications
                .FirstOrDefaultAsync(x => x.FfirId == ffirId, cancellationToken);
            if (verification is not null)
            {
                if (!verification.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    return false;

                verification.Status = status;
                verification.Remarks = remarks;
                verification.ReviewedBy = reviewedBy;
                verification.ReviewedDate = DateTime.UtcNow;
            }
            else
            {
                _context.Ffirverifications.Add(new Ffirverification
                {
                    FfirId = ffirId,
                    Status = status,
                    Remarks = remarks,
                    ReviewedBy = reviewedBy,
                    ReviewedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
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
