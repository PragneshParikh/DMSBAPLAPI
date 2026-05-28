
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.CityRepo;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Data.Repositories.LedgerMasterRepo;
using DMS_BAPL_Data.Repositories.PrefixRepo;
using DMS_BAPL_Data.Repositories.StateRepo;
using DMS_BAPL_Data.Repositories.VehicleDispatchRepo;
using DMS_BAPL_Data.Repositories.VehicleSaleBillRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Data.Services.TaxServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Pkcs;

namespace DMS_BAPL_Data.Services.VehicleSaleBillService
{
    public class VehicleSaleBillService : IVehicleSaleBillService
    {
        #region declarations
        private readonly IVehicleSaleBillRepo _repo;
        private readonly IDealerMasterRepo _dealerRepo;
        private readonly ILedgerMasterRepo _ledgerRepo;
        private readonly ITaxServices _taxService;
        private readonly IStateRepo _stateRepo;
        private readonly ICityRepo _cityRepo;
        private readonly IJobCardRepo _jobCardRepo;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IitemMasterRepo _itemRepo;
        private readonly IVehicleInwardRepo _vehicleInwardRepo;
        private readonly IExcelService _excelService;
        private readonly IPrefixRepo _prefixRepo;
        #endregion
        public VehicleSaleBillService(IVehicleSaleBillRepo repo, ILedgerMasterRepo ledgerRepo,
            IHttpContextAccessor contextAccessor, ITaxServices taxServices, ICityRepo cityRepo,
            IStateRepo stateRepo, IJobCardRepo jobCardRepo, IDealerMasterRepo dealerMaster,
            IExcelService excelService, IPrefixRepo prefixRepo)
        {
            _repo = repo;
            _ledgerRepo = ledgerRepo;
            _taxService = taxServices;
            _contextAccessor = contextAccessor;
            _stateRepo = stateRepo;
            _cityRepo = cityRepo;
            _jobCardRepo = jobCardRepo;
            _dealerRepo = dealerMaster;
            _excelService = excelService;
            _prefixRepo = prefixRepo;
        }

        public async Task<int> CreateAsync(VehicleSaleBillEditCreateViewModel model)
        {
            try
            {
                var header = MapToEntity(model);
                header.CreatedBy = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);



                var result = await _repo.CreateWithJobUpdateAsync(header);
                if (result != 0)
                {
                    await _prefixRepo.UpdateNextNumberByDealerByModule(model.DealerCode, "sale_bill");
                }
                return result;
            }
            catch
            {
                throw;
            }
        }
        // ✅ GET BY ID
        public async Task<VehicleSaleBillResponseViewModel?> GetByIdAsync(int id)
        {
            try
            {
                var data = await _repo.GetVehicleWithMotorDetailsByIdAsync(id);
                //if (data == null) return null;
                //return MapToResponse(data);
                return data;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<VehicleSaleBillResponseViewModel>> GetAllAsync(string? dealerCode, string? search = null, DateTime? dateFrom = null, DateTime? dateTo = null, string? erpStatus = null)
        {
            try
            {
                var list = await _repo.GetAllAsync();


                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();

                    list = list.Where(x =>
                        (x.SaleBillNo != null && x.SaleBillNo.ToLower().Contains(search)) ||
                        (x.SaleType != null && x.SaleType.ToLower().Contains(search)) ||
                        (x.Erpstatus != null && x.Erpstatus.ToLower().Contains(search)) ||
                        (x.CustomerName != null && x.CustomerName.ToLower().Contains(search)) ||
                        (x.BillingName != null && x.BillingName.ToLower().Contains(search)) ||
                        (x.Location != null && x.Location.ToLower().Contains(search)) ||
                        (x.BillType != null && x.BillType.ToLower().Contains(search))
                    ).ToList();
                }

                // Apply date range filter
                if (dateFrom.HasValue)
                {
                    list = list.Where(x => x.SaleDate.Date >= dateFrom.Value.Date).ToList();
                }

                if (dateTo.HasValue)
                {
                    list = list.Where(x => x.SaleDate.Date <= dateTo.Value.Date).ToList();
                }
                if (!string.IsNullOrWhiteSpace(erpStatus))
                {
                    list = list.Where(x => x.Erpstatus.ToLower() == erpStatus.ToLower()).ToList();
                }
                if (!string.IsNullOrWhiteSpace(dealerCode))
                {
                    list = list.Where(i => i.DealerCode.ToLower() == dealerCode.ToLower()).ToList();
                }

                var result = list.Select(x => MapToResponse(x)).ToList();

                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task UpdateAsync(int id, VehicleSaleBillEditCreateViewModel model)
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);

                var existing = await _repo.GetByIdAsync(id);
                if (existing == null)
                    throw new Exception("Record not found");

                // UPDATE HEADER

                existing.BillFrom = model.BillFrom;
                existing.BillingName = model.BillingName;
                existing.Financier = model.Financier;
                existing.CustomerType = model.CustomerType;
                existing.IsD2d = model.IsD2d;
                existing.CashAccount = model.CashAccount;
                existing.BillType = model.BillType;
                existing.Location = model.Location;
                existing.SalesExecutive = model.SalesExecutive;
                existing.TempRegNo = model.TempRegNo;
                existing.SaleType = model.SaleType;
                existing.LedgerId = model.LedgerId;
                existing.CustomerName = model.CustomerName;
                existing.TotalAmount = model.TotalAmount;
                existing.UpdatedDate = DateTime.Now;
                existing.UpdatedBy = userId;

                // DETAIL UPDATE 


                var existingDetails = existing.VehicleSaleBillDetails.ToList();
                var existingChassisList = existing.VehicleSaleBillDetails.Select(c => c.ChassisNo).ToList();
                var newChassisList = model.Details.Select(c => c.ChassisNo).ToList();
                var deletedChassisList = existingChassisList.Except(newChassisList).ToList();
                // DELETE removed rows
                foreach (var old in existingDetails)
                {
                    if (!model.Details.Any(d => d.ChassisNo == old.ChassisNo))
                    {
                        existing.VehicleSaleBillDetails.Remove(old);
                    }
                }

                // Updating deails
                foreach (var d in model.Details)
                {
                    var detail = existing.VehicleSaleBillDetails
                        .FirstOrDefault(x => x.ChassisNo == d.ChassisNo);

                    if (detail != null)
                    {

                        // UPDATE EXISTING

                        detail.ChassisNo = d.ChassisNo;
                        detail.ItemRate = d.ItemRate;
                        detail.PreGstDiscount = d.PreGstDiscount;
                        detail.PostGstDisc = d.PostGstDiscount;
                        detail.FameIi = d.FameIIDisc;
                        detail.RegAmount = d.RegAmount;
                        detail.InsuranceAmount = d.InsuranceAmount;

                        detail.HasDevice = d.HasDevice;
                        detail.HasKit = d.HasKit;
                        detail.IsDelivered = d.IsDelivered;
                        detail.ItemCode = d.ItemCode;

                        detail.Segment = d.Segment;
                        detail.InstitutionalType = d.InstitutionalType;
                        detail.SchemeName = d.SchemeName;
                        detail.Narration = d.Narration;

                        detail.FinalAmount = d.FinalAmount;
                        detail.IsAgainstExchange = d.IsAgainstExchange;

                        detail.Sgstper = d.Sgstper;
                        detail.Sgstamnt = d.Sgstamnt;
                        detail.Cgstper = d.Cgstper;
                        detail.Cgstamnt = d.Cgstamnt;
                        detail.Igstper = d.Igstper;
                        detail.Igstamnt = d.Igstamnt;

                        detail.MfgYear = d.MfgYear;
                        detail.InsNo = d.InsNo;
                        detail.RegNo = d.RegNo;
                        detail.InsStartDate = d.InsStartDate;
                        detail.InsExpDate = d.InsExpDate;

                        detail.ModelName = d.ModelName ?? "";
                        detail.Colour = d.Colour ?? "";

                        detail.Battery = d.Battery ?? "";
                        detail.ConvertorNo = d.ConvertorNo ?? "";
                        detail.ChargerNo = d.ChargerNo ?? "";
                        detail.ControllerNo = d.ControllerNo ?? "";

                        detail.Key = d.Key ?? "";
                        detail.BookNo = d.BookNo ?? "";
                        detail.ExtWarranty = d.ExtWarranty ?? "";

                        detail.BatteryChemical = d.BatteryChemical ?? "";
                        detail.BatteryCapacity = d.BatteryCapacity ?? "";
                        detail.BatteryMake = d.BatteryMake ?? "";

                        detail.StockDetailsNo = d.StockDetailsNo ?? "";
                        detail.Vcu = d.Vcu ?? "";

                        detail.UpdatedDate = DateTime.Now;
                        detail.UpdatedBy = userId;
                    }
                    else
                    {


                        //New entries
                        existing.VehicleSaleBillDetails.Add(new VehicleSaleBillDetail
                        {
                            ChassisNo = d.ChassisNo,
                            ItemRate = d.ItemRate,
                            PreGstDiscount = d.PreGstDiscount,
                            PostGstDisc = d.PostGstDiscount,
                            FameIi = d.FameIIDisc,
                            RegAmount = d.RegAmount,
                            InsuranceAmount = d.InsuranceAmount,
                            ItemCode = d.ItemCode,

                            HasDevice = d.HasDevice,
                            HasKit = d.HasKit,
                            IsDelivered = d.IsDelivered,

                            Segment = d.Segment,
                            InstitutionalType = d.InstitutionalType,
                            SchemeName = d.SchemeName,
                            Narration = d.Narration,

                            FinalAmount = d.FinalAmount,
                            IsAgainstExchange = d.IsAgainstExchange,

                            Sgstper = d.Sgstper,
                            Sgstamnt = d.Sgstamnt,
                            Cgstper = d.Cgstper,
                            Cgstamnt = d.Cgstamnt,
                            Igstper = d.Igstper,
                            Igstamnt = d.Igstamnt,

                            MfgYear = d.MfgYear,
                            InsNo = d.InsNo,
                            RegNo = d.RegNo,
                            InsStartDate = d.InsStartDate,
                            InsExpDate = d.InsExpDate,

                            ModelName = d.ModelName ?? "",
                            Colour = d.Colour ?? "",

                            Battery = d.Battery ?? "",
                            ConvertorNo = d.ConvertorNo ?? "",
                            ChargerNo = d.ChargerNo ?? "",
                            ControllerNo = d.ControllerNo ?? "",

                            Key = d.Key ?? "",
                            BookNo = d.BookNo ?? "",
                            ExtWarranty = d.ExtWarranty ?? "",

                            BatteryChemical = d.BatteryChemical ?? "",
                            BatteryCapacity = d.BatteryCapacity ?? "",
                            BatteryMake = d.BatteryMake ?? "",

                            StockDetailsNo = d.StockDetailsNo ?? "",
                            Vcu = d.Vcu ?? "",

                            CreatedDate = DateTime.Now,
                            CreatedBy = userId
                        });
                    }
                }

                // JOB UPDATE

                var jobUpdates = model.Details
                    .Where(d => d.InsExpDate != null && !string.IsNullOrWhiteSpace(d.RegNo))
                    .Select(d => new UpdateSaleDetailsVM
                    {
                        ChassisNo = d.ChassisNo,
                        SaleDate = model.SaleDate,
                        InsuranceExpDate = d.InsExpDate,
                        RegisterNo = d.RegNo,

                    })
                    .ToList();

                if (!jobUpdates.Any())
                {
                    jobUpdates = null;
                }

                await _repo.UpdateWithJobUpdateAsync(existing, jobUpdates, deletedChassisList);
            }
            catch
            {
                throw;
            }
        }
        public async Task DeleteAsync(int id)
        {
            try
            {
                await _repo.DeleteAsync(id);
            }
            catch
            {
                throw;
            }
        }
        // Mapping Methods
        private VehicleSaleBillHeader MapToEntity(VehicleSaleBillEditCreateViewModel model)
        {
            try
            {

                return new VehicleSaleBillHeader
                {
                    SaleDate = model.SaleDate,
                    SaleBillNo = model.SaleBillNo,
                    IsD2d = model.IsD2d,
                    CustomerType = model.CustomerType,
                    Location = model.Location,
                    SaleType = model.SaleType,
                    CashAccount = model.CashAccount,
                    Financier = model.Financier,
                    BillType = model.BillType,
                    BillFrom = model.BillFrom,
                    CustomerName = model.CustomerName,
                    BillingName = model.BillingName,
                    SalesExecutive = model.SalesExecutive,
                    TempRegNo = model.TempRegNo,
                    BookingId = model.BookingId,
                    PrintType = model.PrintType,
                    RefName = model.RefName,
                    RefAddress = model.RefAddress,
                    RefEmail = model.RefEmail,
                    RefPoint = model.RefPoint,
                    RefRemarks = model.RefRemarks,
                    TotalAmount = model.TotalAmount,
                    LedgerId = model.LedgerId,
                    CreatedDate = DateTime.Now,
                    CreatedBy = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext),
                    Erpstatus = model.ErpStatus,
                    DealerCode = model.DealerCode,

                    VehicleSaleBillDetails = model.Details.Select(d => new VehicleSaleBillDetail
                    {
                        ChassisNo = d.ChassisNo,
                        ItemRate = d.ItemRate,
                        FameIi = d.FameIIDisc,
                        PreGstDiscount = d.PreGstDiscount,
                        PostGstDisc = d.PostGstDiscount,
                        RegAmount = d.RegAmount,
                        InsuranceAmount = d.InsuranceAmount,
                        HasDevice = d.HasDevice,
                        HasKit = d.HasKit,
                        IsDelivered = d.IsDelivered,
                        Segment = d.Segment,
                        InstitutionalType = d.InstitutionalType,
                        SchemeName = d.SchemeName,
                        Narration = d.Narration,
                        FinalAmount = d.FinalAmount,
                        IsAgainstExchange = d.IsAgainstExchange,
                        ItemCode = d.ItemCode,
                        Sgstper = d.Sgstper,
                        Sgstamnt = d.Sgstamnt,
                        Cgstamnt = d.Cgstamnt,
                        Cgstper = d.Cgstper,
                        Igstper = d.Igstper,
                        Igstamnt = d.Igstamnt,
                        InsNo = d.InsNo,
                        InsExpDate = d.InsExpDate,
                        InsStartDate = d.InsStartDate,
                        RegNo = d.RegNo,
                        MfgYear = d.MfgYear,
                        ModelName = d.ModelName ?? "",
                        Colour = d.Colour ?? "",
                        Battery = d.Battery ?? "",
                        ConvertorNo = d.ConvertorNo ?? "",
                        ChargerNo = d.ChargerNo ?? "",
                        ControllerNo = d.ControllerNo ?? "",
                        Key = d.Key ?? "",
                        BookNo = d.BookNo ?? "",
                        ExtWarranty = d.ExtWarranty ?? "",
                        BatteryChemical = d.BatteryChemical ?? "",
                        BatteryCapacity = d.BatteryCapacity ?? "",
                        BatteryMake = d.BatteryMake ?? "",
                        StockDetailsNo = d.StockDetailsNo ?? "",
                        Vcu = d.Vcu ?? "",
                        CreatedDate = DateTime.Now,
                        CreatedBy = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext)
                    }).ToList()
                };
            }
            catch
            {
                throw;
            }
        }
        private VehicleSaleBillResponseViewModel MapToResponse(VehicleSaleBillHeader data)
        {
            try
            {

                return new VehicleSaleBillResponseViewModel
                {
                    Id = data.Id,
                    SaleBillNo = data.SaleBillNo,
                    CustomerName = data.CustomerName ?? "",
                    TotalAmount = data.TotalAmount ?? 0,
                    Location = data.Location,
                    SaleDate = data.SaleDate,
                    SaleType = data.SaleType,
                    BillType = data.BillType,
                    BillingName = data.BillingName,
                    Financier = data.Financier,
                    CashAccount = data.CashAccount,
                    SalesExecutive = data.SalesExecutive,
                    isTempRegNo = data.TempRegNo,
                    isD2d = data.IsD2d,
                    CustomerType = data.CustomerType,
                    LedgerId = data.LedgerId,
                    ErpStatus = data.Erpstatus,
                    DealerCode = data.DealerCode,

                    Details = data.VehicleSaleBillDetails.Select(d => new VehicleSaleBillDetailVM
                    {
                        Id = d.Id,
                        ChassisNo = d.ChassisNo,
                        ItemRate = d.ItemRate,
                        PreGstDiscount = d.PreGstDiscount ?? 0,
                        RegAmount = d.RegAmount ?? 0,
                        InsuranceAmount = d.InsuranceAmount ?? 0,
                        HasDevice = d.HasDevice,
                        HasKit = d.HasKit,
                        IsDelivered = d.IsDelivered,
                        Segment = d.Segment ?? "",
                        InstitutionalType = d.InstitutionalType ?? "",
                        SchemeName = d.SchemeName ?? "",
                        Narration = d.Narration ?? "",
                        FinalAmount = d.FinalAmount,
                        IsAgainstExchange = d.IsAgainstExchange,
                        InsStartDate = d.InsStartDate,
                        InsExpDate = d.InsExpDate,
                        InsNo = d.InsNo,
                        Igstamnt = d.Igstamnt ?? 0,
                        Igstper = d.Igstper ?? 0,
                        Cgstper = d.Cgstper ?? 0,
                        Cgstamnt = d.Cgstamnt ?? 0,
                        Sgstamnt = d.Sgstamnt ?? 0,
                        Sgstper = d.Sgstper ?? 0,
                        MfgYear = d.MfgYear ?? 0,
                        RegNo = d.RegNo,
                        ModelName = d.ModelName ?? "",
                        Colour = d.Colour ?? "",
                        Battery = d.Battery ?? "",
                        ConvertorNo = d.ConvertorNo ?? "",
                        ChargerNo = d.ChargerNo ?? "",
                        ControllerNo = d.ControllerNo ?? "",
                        Key = d.Key ?? "",
                        BookNo = d.BookNo ?? "",
                        ExtWarranty = d.ExtWarranty ?? "",
                        BatteryChemical = d.BatteryChemical ?? "",
                        BatteryCapacity = d.BatteryCapacity ?? "",
                        BatteryMake = d.BatteryMake ?? "",
                        StockDetailsNo = d.StockDetailsNo ?? "",
                        Vcu = d.Vcu ?? "",
                        ItemCode = d.ItemCode ?? "",
                        FameIIDisc = d.FameIi ?? 0,
                        PostGstDiscount = d.PostGstDisc ?? 0

                    }).ToList()
                };
            }
            catch
            {
                throw;
            }
        }
        public async Task<string> GenerateNextVehicleSaleNo()
        {
            try
            {
                var lastReceiptNo = await _repo.GetLastSaleBillNo();

                if (string.IsNullOrEmpty(lastReceiptNo))
                    return "VHS001";

                // Remove prefix safely
                var numberPart = lastReceiptNo.Replace("VHS", "");

                if (!int.TryParse(numberPart, out int number))
                    throw new Exception("Invalid Sale Number Format");

                number++;

                return $"VHS{number.ToString("D3")}";
            }
            catch
            {
                throw;
            }
        }
        public async Task<VehicleSaleExportViewModel?> GetExportData(int id)
        {
            try
            {
                LedgerDetailViewModel? ledger = null;


                var header = await _repo.GetByIdAsync(id);
                if (header == null) return null;

                ledger = header.LedgerId.HasValue
                   ? await _ledgerRepo.GetLedgerById(header.LedgerId.Value)
                   : null;
                var states = await _stateRepo.GetStatesAsync();

                var result = new VehicleSaleExportViewModel
                {

                    User = new UserViewModel
                    {
                        Mobile = ledger?.MobileNumber ?? "",
                        FirstName = header.CustomerName ?? "",
                        EmailId = ledger?.EMail ?? "",

                        DateOfBirth = ledger?.DateOfBirth.HasValue == true
                ? ledger.DateOfBirth.Value.ToString("dd-MM-yyyy")
                : "",

                        DateOfAnniversary = "",
                        Id = "",
                        ModifiedOn = "",
                        IsDeleted = "",
                        DeletedOn = "",

                        Address1 = ledger?.Address ?? "",
                        Address2 = "",
                        State = ledger?.stateName ?? "",
                        City = ledger?.cityName ?? ""
                    },

                    Vehicle = header.VehicleSaleBillDetails.Select(detail => new VehicleViewModel
                    {
                        ChassisNo = detail.ChassisNo ?? "",
                        ModelId = "",
                        MotorId = "",
                        MotorControllerNo = "",
                        EcuSerialNo = "",
                        EcuImeiNo = "",

                        BikeSimId = "1",
                        BatterySerialNo = "",

                        RegNumber = header.TempRegNo ?? "",
                        StartDate = header.SaleDate.ToString("dd-MM-yyyy"),
                        DealerCode = header.Location ?? "",

                        SaleBillNo = header.SaleBillNo ?? "",
                        SaleBillDate = header.SaleDate.ToString("dd-MM-yyyy"),

                        FinancedBy = header.Financier ?? "",

                        ItemRate = detail.ItemRate.ToString(),
                        SGSTPer = detail.Sgstper?.ToString() ?? "0",
                        SGSTAmnt = detail.Sgstamnt?.ToString() ?? "0",
                        CGSTPer = detail.Cgstper?.ToString() ?? "0",
                        CGSTAmnt = detail.Cgstamnt?.ToString() ?? "0",
                        NetAmnt = detail.FinalAmount.ToString(),

                        BatteryMake = "",
                        ChargerNo = "",
                        SaleType = header.SaleType ?? ""
                    }).ToList()
                };
                //await _repo.UpdateERPStatus(id);

                return result;
            }
            catch
            {
                throw;
            }

        }
        public async Task<List<PdiOkVehicleChassisViewModel>> GetPdiVehiclesAsync(string dealerCode, int ledgerId)
        {
            dynamic customer;
            var rawData = await _repo.GetPdiRawDataAsync(dealerCode);
            customer = await _ledgerRepo.GetLedgerById(ledgerId);

            var dealerLocation = await _dealerRepo.GetDealerByCode(dealerCode);

            var result = new List<PdiOkVehicleChassisViewModel>();

            //  group by ItemCode
            var itemGroups = rawData.GroupBy(x => x.ItemCode);

            var taxCache = new Dictionary<string, List<TaxDetailViewModel>>();

            foreach (var group in itemGroups)
            {
                var itemCode = group.Key;

                var tax = await _taxService.GetTaxDetailsAsync(itemCode, dealerLocation.State, customer.stateName);
                taxCache[itemCode] = tax;
            }

            foreach (var item in rawData)
            {
                var taxes = taxCache[item.ItemCode];

                var vm = new PdiOkVehicleChassisViewModel
                {
                    ChassisNo = item.ChassisNo,
                    ItemCode = item.ItemCode,
                    ItemColor = item.ItemColor,
                    ItemName = item.ItemName,
                    MfgYear = item.MfgYear,

                    KeyNo = item.KeyNo,
                    BookNo = item.BookNo,

                    BatteryNo = item.BatteryNo,
                    BatteryChemical = item.BatteryChemical,
                    BatteryCapacity = item.BatteryCapacity,
                    BatteryMake = item.BatteryMake,

                    ChargerNo = item.ChargerNo,
                    ControllerNo = item.ControllerNo,
                    ConverterNo = item.ConverterNo,
                    CustomerPrice = item.CustomerPrice,
                    DealerPrice = item.DealerPrice,
                    //  PreGstDisc = item.PreGstDisc,
                    CustomerSaleDate = item.CustomerSaleDate,

                };

                // Tax Mapping
                foreach (var tax in taxes)
                {
                    if (tax.TaxCode.ToUpper().Contains("SGST"))
                    {
                        vm.SGSTPer = tax.TaxRate;
                        vm.SGST = tax.TaxRate;
                    }
                    if (tax.TaxCode.ToUpper().Contains("CGST"))
                    {
                        vm.CGSTPer = tax.TaxRate;
                        vm.CGST = tax.TaxRate;
                    }
                    if (tax.TaxCode.ToUpper().Contains("IGST"))
                    {
                        vm.IGSTPer = tax.TaxRate;
                        vm.IGST = tax.TaxRate;
                    }
                }

                result.Add(vm);
            }

            return result;
        }
        public async Task<int> ConfirmInvoiceAndReserveChassis(string saleBillNo)
        {
            try
            {
                var result = await _repo.ConfirmInvoiceAndReserveChassis(saleBillNo);
                await GetExportData(result);
                return result;


            }
            catch
            {
                return 0;
            }
        }
        public async Task<VehicleSaleBillHeader> UpdateRegistrationAndReserveChassis(string? saleBillNo, List<UpdateSaleDetailsVM> updateSaleDetails)
        {
            try
            {
                return await _repo.UpdateRegistrationAndReserveChassis(saleBillNo, updateSaleDetails);
            }
            catch
            {
                throw;
            }
        }
        public async Task<Form22SlipViewModel> GenerateForm22Report(string chassisNo)
        {
            try
            {
                return await _repo.GenerateForm22Report(chassisNo);
            }
            catch
            {
                throw;
            }
        }
        public async Task<byte[]> DownloadDealerExcel(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {

                var headerData = await _repo.GetAllAsync();

                if (dateFrom.HasValue)
                {
                    headerData = headerData.Where(x => x.SaleDate.Date >= dateFrom.Value.Date).ToList();
                }

                if (dateTo.HasValue)
                {
                    headerData = headerData.Where(x => x.SaleDate.Date <= dateTo.Value.Date).ToList();
                }

                var data = headerData.SelectMany(h => h.VehicleSaleBillDetails
                .Select(d =>

                        new VehicleSaleBillExcelViewModel
                        {
                            // Header Fields
                            SaleBillNo = h.SaleBillNo,
                            SaleDate = h.SaleDate,
                            CustomerName = h.CustomerName,
                            BillingName = h.BillingName,
                            CustomerType = h.CustomerType,
                            Location = h.Location,
                            SaleType = h.SaleType,
                            Financier = h.Financier,
                            SalesExecutive = h.SalesExecutive,
                            //TotalAmount = h.TotalAmount,

                            // Detail Fields
                            ChassisNo = d.ChassisNo,
                            ModelName = d.ModelName,
                            Colour = d.Colour,
                            ItemCode = d.ItemCode,
                            ItemRate = d.ItemRate,
                            FinalAmount = d.FinalAmount,
                            InsuranceAmount = d.InsuranceAmount,
                            RegAmount = d.RegAmount,
                            Battery = d.Battery,
                            ChargerNo = d.ChargerNo,
                            ControllerNo = d.ControllerNo,
                            ConvertorNo = d.ConvertorNo,
                            RegNo = d.RegNo
                        }))
                    .ToList();

                // DTO Properties
                var properties = typeof(VehicleSaleBillExcelViewModel)
                    .GetProperties()
                    .ToList();

                // Excel Columns
                var columns = properties
                    .Select(p => p.Name)
                    .ToList();

                // Excel Rows
                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        dict[prop.Name] = prop.GetValue(d) ?? "";
                    }

                    return dict;
                }).ToList();

                // Excel Model
                var model = new ExcelExportViewModel
                {
                    SheetName = StringConstants.VehicleSaleBillExcel,
                    Columns = columns,
                    Rows = rows
                };

                // Generate Excel
                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public async Task<IEnumerable<string>> GetPolicyNo(string chassisNo)
        {
            try
            {
                return await _repo.GetPolicyNo(chassisNo);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<ChassisListWithPDIStatus>> GetAllChassissListWithPDISatatus(string? dealerCode, int ledgerId)
        {
            try
            {
                dynamic customer;

                var rawData = await _repo.GetAllChassissListWithPDISatatus(dealerCode);
                customer = await _ledgerRepo.GetLedgerById(ledgerId);

                var dealerLocation = await _dealerRepo.GetDealerByCode(dealerCode);

                var result = new List<ChassisListWithPDIStatus>();

                //  group by ItemCode
                var itemGroups = rawData.GroupBy(x => x.ItemCode);

                var taxCache = new Dictionary<string, List<TaxDetailViewModel>>();

                foreach (var group in itemGroups)
                {
                    var itemCode = group.Key;

                    var tax = await _taxService.GetTaxDetailsAsync(itemCode, dealerLocation.State, customer.stateName);
                    taxCache[itemCode] = tax;
                }

                foreach (var item in rawData)
                {
                    var taxes = taxCache[item.ItemCode];

                    var vm = new ChassisListWithPDIStatus
                    {
                        ChassisNo = item.ChassisNo,
                        ItemCode = item.ItemCode,
                        ItemColor = item.ItemColor,
                        ItemName = item.ItemName,
                        MfgYear = item.MfgYear,

                        KeyNo = item.KeyNo,
                        BookNo = item.BookNo,

                        BatteryNo = item.BatteryNo,
                        BatteryChemical = item.BatteryChemical,
                        BatteryCapacity = item.BatteryCapacity,
                        BatteryMake = item.BatteryMake,

                        ChargerNo = item.ChargerNo,
                        ControllerNo = item.ControllerNo,
                        ConverterNo = item.ConverterNo,
                        CustomerPrice = item.CustomerPrice,
                        DealerPrice = item.DealerPrice,
                        //  PreGstDisc = item.PreGstDisc,
                        CustomerSaleDate = item.CustomerSaleDate,
                        PDIStatus = item.PDIStatus,
                        FameIIAmnt = item.FameIIAmnt,
                        ProformaCreated = item.ProformaCreated,



                    };

                    // Tax Mapping
                    foreach (var tax in taxes)
                    {
                        if (tax.TaxCode.ToUpper().Contains("SGST"))
                        {
                            vm.SGSTPER = tax.TaxRate;
                            vm.SGST = tax.TaxRate;
                        }
                        if (tax.TaxCode.ToUpper().Contains("CGST"))
                        {
                            vm.CGSTPER = tax.TaxRate;
                            vm.CGST = tax.TaxRate;
                        }
                        if (tax.TaxCode.ToUpper().Contains("IGST"))
                        {
                            vm.IGSTPER = tax.TaxRate;
                            vm.IGST = tax.TaxRate;
                        }
                    }
                    result.Add(vm);
                }
                return result;
            }
            catch
            {
                throw;
            }
        }
    }
}

