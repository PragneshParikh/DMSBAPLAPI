
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
using System.IO.Compression;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
        // GET BY ID
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
                        (x.Status != null && x.Status.ToLower().Contains(search)) ||
                        (x.CustomerName != null && x.CustomerName.ToLower().Contains(search)) ||
                        (x.BillingName != null && x.BillingName.ToLower().Contains(search)) ||
                        (x.Location != null && x.Location.ToLower().Contains(search))
                    // (x.BillType != null && x.BillType.ToLower().Contains(search))
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
                    list = list.Where(x => x.Status.ToLower() == erpStatus.ToLower()).ToList();
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
                        detail.InsuranceLedgerId = d.InsuranceId ?? null;

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
                            InsuranceLedgerId = d.InsuranceId ?? null,

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
                    Status = model.Status,
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
                        InsuranceLedgerId = d.InsuranceId ?? null,
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
                    Status = data.Status,
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
        //public async Task<VehicleSaleExportViewModel?> GetExportData(int id)
        //{
        //    try
        //    {
        //        LedgerDetailViewModel? ledger = null;
        //        LedgerDetailViewModel? financierLedger = null;



        //        var header = await _repo.GetByIdAsync(id);
        //        if (header == null) return null;
        //        if (header.Financier.HasValue)
        //        {
        //            financierLedger = await _ledgerRepo.GetLedgerById(header.Financier.Value);
        //        }

        //        ledger = header.LedgerId.HasValue
        //           ? await _ledgerRepo.GetLedgerById(header.LedgerId.Value)
        //           : null;
        //        var states = await _stateRepo.GetStatesAsync();

        //        var result = new VehicleSaleExportViewModel
        //        {

        //            User = new UserViewModel
        //            {
        //                Mobile = ledger?.MobileNumber ?? "",
        //                FirstName = header.CustomerName ?? "",
        //                EmailId = ledger?.EMail ?? "",

        //                DateOfBirth = ledger?.DateOfBirth.HasValue == true
        //        ? ledger.DateOfBirth.Value.ToString("dd-MM-yyyy")
        //        : "",

        //                DateOfAnniversary = "",
        //                Id = "",
        //                ModifiedOn = "",
        //                IsDeleted = "",
        //                DeletedOn = "",

        //                Address1 = ledger?.Address ?? "",
        //                Address2 = "",
        //                State = ledger?.stateName ?? "",
        //                City = ledger?.cityName ?? ""
        //            },

        //            Vehicle = header.VehicleSaleBillDetails.Select(detail => new VehicleViewModel
        //            {
        //                ChassisNo = detail.ChassisNo ?? "",
        //                ModelId = "",
        //                MotorId = "",
        //                MotorControllerNo = "",
        //                EcuSerialNo = "",
        //                EcuImeiNo = "",

        //                BikeSimId = "1",
        //                BatterySerialNo = "",

        //                RegNumber = header.TempRegNo ?? "",
        //                StartDate = header.SaleDate.ToString("dd-MM-yyyy"),
        //                DealerCode = header.Location ?? "",

        //                SaleBillNo = header.SaleBillNo ?? "",
        //                SaleBillDate = header.SaleDate.ToString("dd-MM-yyyy"),

        //                FinancedBy = financierLedger.LedgerName ?? "",

        //               // ItemRate = detail.ItemRate.ToString(),
        //                //SGSTPer = detail.Sgstper?.ToString() ?? "0",
        //                //SGSTAmnt = detail.Sgstamnt?.ToString() ?? "0",
        //                //CGSTPer = detail.Cgstper?.ToString() ?? "0",
        //                //CGSTAmnt = detail.Cgstamnt?.ToString() ?? "0",
        //                //NetAmnt = detail.FinalAmount.ToString(),

        //                BatteryMake = "",
        //                ChargerNo = "",
        //                SaleType = header.SaleType ?? ""
        //            }).ToList()
        //        };
        //        //await _repo.UpdateERPStatus(id);

        //        return result;
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //}
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

        public async Task<byte[]> DownloadSaleBillExcel(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {

                var headerData = await _repo.GetAllAsync();
                var financiers = await _ledgerRepo.GetAll();
                var financierLookup = financiers.ToDictionary(
                            x => x.Id,
                            x => x.LedgerName
                        );

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
                            Financier = h.Financier.HasValue
                                        && financierLookup.ContainsKey(h.Financier.Value)
                                            ? financierLookup[h.Financier.Value]
                                            : "",

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
                            BookNo=d.BookNo,
                            BatteryCapacity=d.BatteryCapacity,
                            ExtWarranty=d.ExtWarranty,
                            BatteryChemical=d.BatteryChemical,
                            BatteryMake=d.BatteryMake,
                            Key=d.Key,
                            ChargerNo = d.ChargerNo,
                            ControllerNo = d.ControllerNo,
                            ConvertorNo = d.ConvertorNo,
                            RegNo = d.RegNo
                        }))
                    .ToList();

                // DTO Properties
                var properties = typeof(VehicleSaleBillExcelViewModel).GetProperties().ToList();

                // Excel Columns
                var columns = properties.Select(p => p.Name).ToList();

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
                        LocationCode =item.LocationCode,


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

        public async Task<VehicleSaleExportViewModel?> GetExportDetails(string dealerCode, int id)
        {
            try
            {
                return await _repo.GetExportDetails(dealerCode, id);
            }
            catch
            {
                throw;
            }
        }


        public async Task<byte[]> DownloadSaleBillPdf(int id)
        {
            var bill = await GetByIdAsync(id);

            if (bill == null)
                throw new Exception("Sale Bill not found");

            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("EX-SHOWROOM INVOICE")
                            .FontSize(16).Bold().AlignCenter();
                        col.Item().Text($"Bill No: {bill.SaleBillNo}")
                            .FontSize(11).AlignCenter();
                        col.Item().LineHorizontal(1);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            void AddRow(string label, string value)
                            {
                                table.Cell().Padding(4).Text(label).Bold();
                                table.Cell().Padding(4).Text(value ?? "-");
                            }

                            AddRow("Sale Date:", bill.SaleDate.ToString("dd-MM-yyyy"));
                            AddRow("Customer Name:", bill.CustomerName);
                            AddRow("Billing Name:", bill.BillingName);
                            AddRow("Sale Type:", bill.SaleType);
                            AddRow("Bill Type:", (bill.BillType ?? 0).ToString());
                            AddRow("Location:", bill.Location);
                            AddRow("Sales Executive:", bill.SalesExecutive ?? "-");
                            AddRow("Total Amount:", (bill.TotalAmount ?? 0m).ToString());
                            AddRow("Status:", bill.Status ?? "-");
                        });

                        col.Item().PaddingTop(10).Text("Vehicle Details").FontSize(12).Bold();
                        col.Item().LineHorizontal(1);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(25);
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            static IContainer HeaderCell(IContainer c) =>
                                c.Background(Colors.Grey.Lighten2).Padding(4);

                            table.Header(h =>
                            {
                                h.Cell().Element(HeaderCell).Text("#").Bold();
                                h.Cell().Element(HeaderCell).Text("Chassis No").Bold();
                                h.Cell().Element(HeaderCell).Text("Model").Bold();
                                h.Cell().Element(HeaderCell).Text("Colour").Bold();
                                h.Cell().Element(HeaderCell).Text("Item Rate").Bold();
                                h.Cell().Element(HeaderCell).Text("Final Amount").Bold();
                            });

                            int i = 1;
                            foreach (var d in bill.Details)
                            {
                                table.Cell().Padding(3).Text(i++.ToString());
                                table.Cell().Padding(3).Text(d.ChassisNo ?? "-");
                                table.Cell().Padding(3).Text(d.ModelName ?? "-");
                                table.Cell().Padding(3).Text(d.Colour ?? "-");
                                table.Cell().Padding(3).Text(d.ItemRate.ToString());
                                table.Cell().Padding(3).Text(d.FinalAmount.ToString());
                            }
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();

            return pdfBytes;
        }

        public async Task<byte[]> DownloadExShowroomInvoicePdf(int id)
        {
            var model = await _repo.GetProformaInvoiceDataAsync(id);
            if (model == null)
                throw new Exception("Sale Bill not found");

            return GenerateExShowroomInvoicePdf(model);
        }

        private byte[] GenerateExShowroomInvoicePdf(ProformaInvoicePdfModel m)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string Val(string? v) => string.IsNullOrWhiteSpace(v) ? "-" : v!;
            string Money(decimal v) => v.ToString("N2");

            const string HeaderBg = "#6F72A0";

            // Ex-showroom total = taxable + GST - subsidy.
            // Registration & insurance are on-road extras and are intentionally excluded.
            var taxTotal = m.IsIgst ? m.IgstTotal : (m.CgstTotal + m.SgstTotal);
            var exShowroomTotal = m.TaxableTotal + taxTotal - m.SubsidyTotal;
            var amountInWords = ProformaInvoicePdfModel.IndianCurrencyWords(exShowroomTotal);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(col =>
                    {
                        // -- Dealer header --
                        col.Item().AlignCenter().Column(h =>
                        {
                            if (!string.IsNullOrWhiteSpace(m.DealerName))
                                h.Item().AlignCenter().Text(m.DealerName).Bold().FontSize(12);
                            if (!string.IsNullOrWhiteSpace(m.DealerAddress))
                                h.Item().AlignCenter().Text(m.DealerAddress).FontSize(8);
                            h.Item().AlignCenter().Text($"Phone No.: {Val(m.DealerPhone)}").FontSize(8);
                            h.Item().AlignCenter().Text($"Email: {Val(m.DealerEmail)}").FontSize(8);
                            h.Item().AlignCenter()
                                .Text($"GSTIN No.: {Val(m.DealerGstin)}   |   PAN No.: {Val(m.DealerPan)}").FontSize(8);
                        });

                        col.Item().PaddingTop(4).LineHorizontal(1);
                        col.Item().AlignCenter().Text("EX-SHOWROOM INVOICE").Bold().FontSize(12);
                        col.Item().LineHorizontal(1);

                        // -- Customer + invoice meta (NO grid) --
                        void KV(ColumnDescriptor c, string label, string? value)
                        {
                            c.Item().PaddingVertical(1).Row(r =>
                            {
                                r.ConstantItem(120).Text(label).Bold().FontSize(8);
                                r.RelativeItem().Text($": {Val(value)}").FontSize(8);
                            });
                        }

                        col.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                KV(left, "Party Name", m.CustomerName);
                                KV(left, "Billing Name", m.BillingName);
                                KV(left, "Mobile No", m.CustomerMobile);
                                KV(left, "Address", m.CustomerAddress);
                                KV(left, "State", m.CustomerState);
                                KV(left, "GSTIN", m.CustomerGstin);
                            });
                            row.RelativeItem().Column(right =>
                            {
                                KV(right, "Invoice No", m.ProformaNo);
                                KV(right, "Invoice Date", m.InvoiceDate.ToString("dd-MM-yyyy"));
                                KV(right, "Sale Bill No", m.SaleBillNo);
                                KV(right, "Sale Type", m.SaleType);
                                KV(right, "Customer Type", m.CustomerType);
                                KV(right, "Financier", m.FinancedBy);
                            });
                        });

                        // -- Item / detail table (full grid) --
                        col.Item().PaddingTop(8).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(24);    // S.No
                                c.RelativeColumn(2.2f);  // Model
                                c.RelativeColumn(1.1f);  // HSN
                                c.RelativeColumn(2f);    // Chassis No
                                c.RelativeColumn(1.8f);  // Motor No
                                c.RelativeColumn(1.2f);  // Colour
                                c.RelativeColumn(1.4f);  // Taxable
                                c.RelativeColumn(1.2f);  // GST
                                c.RelativeColumn(1.4f);  // Amount
                            });

                            void HCell(string text, bool right = false)
                            {
                                var cell = t.Cell().Background(HeaderBg)
                                    .Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4);
                                var span = (right ? cell.AlignRight() : cell).Text(text);
                                span.Bold().FontSize(8).FontColor(Colors.White);
                            }

                            t.Header(hr =>
                            {
                                HCell("S.No"); HCell("Model"); HCell("HSN"); HCell("Chassis No");
                                HCell("Motor No"); HCell("Colour");
                                HCell("Taxable", right: true); HCell("GST", right: true); HCell("Amount", right: true);
                            });

                            foreach (var l in m.Lines)
                            {
                                var lineTax = m.IsIgst ? l.IgstAmt : (l.CgstAmt + l.SgstAmt);
                                var lineAmount = l.TaxableValue + lineTax - l.FameII;

                                IContainer Body(IContainer c) =>
                                    c.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4);

                                Body(t.Cell()).Text(l.SrNo.ToString()).FontSize(8);
                                Body(t.Cell()).Text(Val(l.Description)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.Hsn)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.ChassisNo)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.MotorNo)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.Colour)).FontSize(8);
                                Body(t.Cell()).AlignRight().Text(Money(l.TaxableValue)).FontSize(8);
                                Body(t.Cell()).AlignRight().Text(Money(lineTax)).FontSize(8);
                                Body(t.Cell()).AlignRight().Text(Money(lineAmount)).FontSize(8);
                            }
                        });

                        // -- Totals grid (full width, full grid) --
                        col.Item().PaddingTop(10).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(5f);     // label
                                c.RelativeColumn(1.5f);   // amount
                            });

                            void GridRow(string label, string amount, bool emphasize = false)
                            {
                                var bg = emphasize ? Colors.Grey.Lighten3 : Colors.White;
                                var border = emphasize ? 1f : 0.5f;
                                var size = emphasize ? 10 : 9;

                                var lCell = t.Cell().Background(bg).Border(border).BorderColor(Colors.Grey.Medium)
                                    .PaddingVertical(emphasize ? 6 : 4).PaddingHorizontal(6);
                                var lText = lCell.Text(label).FontSize(size);
                                if (emphasize) lText.Bold();

                                var aCell = t.Cell().Background(bg).Border(border).BorderColor(Colors.Grey.Medium)
                                    .PaddingVertical(emphasize ? 6 : 4).PaddingHorizontal(6);
                                var aText = aCell.AlignRight().Text(amount).FontSize(size);
                                if (emphasize) aText.Bold();
                            }

                            GridRow("Taxable Price", Money(m.TaxableTotal));
                            if (m.IsIgst)
                                GridRow($"IGST@{m.IgstPer:0.##}%", Money(m.IgstTotal));
                            else
                            {
                                GridRow($"CGST@{m.CgstPer:0.##}%", Money(m.CgstTotal));
                                GridRow($"SGST@{m.SgstPer:0.##}%", Money(m.SgstTotal));
                            }
                            if (m.SubsidyTotal > 0)
                                GridRow("Less PM E-DRIVE Incentive from Govt. of India", $"({Money(m.SubsidyTotal)})");

                            GridRow("Ex-Showroom Total", Money(exShowroomTotal), emphasize: true);
                        });

                        // -- Amount in words (single bordered band) --
                        col.Item().PaddingTop(8).Border(0.5f).BorderColor(Colors.Grey.Medium)
                            .PaddingVertical(5).PaddingHorizontal(6).Row(r =>
                            {
                                r.ConstantItem(95).Text("Amount in words").Bold().FontSize(9);
                                r.ConstantItem(10).Text(":").Bold().FontSize(9);
                                r.RelativeItem().Text(Val(amountInWords)).Bold().FontSize(9);
                            });

                        // -- Battery / component table (full grid) --
                        col.Item().PaddingTop(8).Text("Battery / Component Details").Bold().FontSize(9);
                        col.Item().PaddingTop(2).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2f); c.RelativeColumn(1.3f); c.RelativeColumn(1.3f);
                                c.RelativeColumn(1.6f); c.RelativeColumn(1.8f); c.RelativeColumn(1.8f);
                                c.RelativeColumn(1.4f); c.RelativeColumn(1f);
                            });

                            void HC(string text) =>
                                t.Cell().Background(HeaderBg).Border(0.5f).BorderColor(Colors.Grey.Medium)
                                    .Padding(4).Text(text).Bold().FontSize(8).FontColor(Colors.White);

                            t.Header(hr =>
                            {
                                HC("Battery Sr. No."); HC("Chemistry"); HC("Capacity"); HC("Make");
                                HC("Charger No."); HC("Controller No."); HC("VCU"); HC("Mfg. Year");
                            });

                            foreach (var l in m.Lines)
                            {
                                void VC(string? text) =>
                                    t.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium)
                                        .Padding(4).Text(Val(text)).FontSize(8);

                                VC(l.BatteryNo); VC(l.Chemistry); VC(l.BatteryCapacity); VC(l.BatteryMake);
                                VC(l.ChargerNo); VC(l.ControllerNo); VC(l.Vcu); VC(l.MfgYear?.ToString());
                            }
                        });

                        // -- Terms (NO grid) --
                        col.Item().PaddingTop(10).Text("Terms & Conditions").Bold().FontSize(9);
                        col.Item().PaddingLeft(12).Column(tc =>
                        {
                            tc.Item().Text("1.  Goods once sold will not be taken back.").FontSize(8);
                            tc.Item().Text("2.  Subject to respective jurisdiction only.").FontSize(8);
                            tc.Item().Text("3.  E&OE.").FontSize(8);
                            tc.Item().Text("4.  Ex-Showroom invoice. RTO, insurance & other charges are billed separately.").FontSize(8);
                        });

                        // -- Signatures --
                        col.Item().PaddingTop(36).Row(r =>
                        {
                            r.RelativeItem().Text("Customer Signature").Bold().FontSize(9);
                            r.RelativeItem().AlignRight().Text($"For {Val(m.DealerName)}").Bold().FontSize(9);
                        });

                        col.Item().PaddingTop(10).AlignCenter().Text("THANK YOU").Bold().FontSize(9);
                    });
                });
            }).GeneratePdf();
        }
        public async Task<byte[]> DownloadMultipleSaleBills(List<int> ids)
        {
            using var memory = new MemoryStream();

            using (var archive =
                   new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                foreach (var id in ids)
                {
                    var pdfBytes = await DownloadExShowroomInvoicePdf(id);

                    var entry =
                        archive.CreateEntry($"SaleBill_{id}.pdf");

                    using var stream = entry.Open();

                    await stream.WriteAsync(
                        pdfBytes,
                        0,
                        pdfBytes.Length);
                }
            }

            return memory.ToArray();
        }

        public async Task<byte[]> DownloadForm22Pdf(int id)
        {
            var model = await _repo.GetForm22DataAsync(id);
            if (model == null)
                throw new Exception("Sale Bill not found");

            return GenerateForm22Pdf(model);
        }

        private byte[] GenerateForm22Pdf(Form22PdfModel m)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string Val(string? v) => string.IsNullOrWhiteSpace(v) ? "-" : v!;

            // Builds one full copy of the form into a column, tagged with copyLabel.
            void Compose(ColumnDescriptor col, string copyLabel)
            {
                // -- Copy badge (top-right) --
                col.Item().Row(r =>
                {
                    r.RelativeItem();
                    r.AutoItem().Border(1).BorderColor(Colors.Grey.Darken1)
                        .PaddingVertical(2).PaddingHorizontal(6)
                        .Text(copyLabel).Bold().FontSize(9);
                });

                // -- Issuer header --
                col.Item().AlignCenter().Text(Val(m.DealerName)).Bold().FontSize(12);
                if (!string.IsNullOrWhiteSpace(m.DealerAddress))
                    col.Item().AlignCenter().Text(m.DealerAddress).FontSize(8);
                col.Item().AlignCenter().Text(
                    $"Phone: {Val(m.DealerPhone)}   |   GSTIN: {Val(m.DealerGstin)}" +
                    $"   |   Trade Cert No: {Val(m.DealerTradeCertNo)}").FontSize(8);

                col.Item().PaddingTop(6).LineHorizontal(1);
                col.Item().AlignCenter().Text("FORM 22").Bold().FontSize(14);
                col.Item().AlignCenter().Text("(See rule 127)").FontSize(8);
                col.Item().AlignCenter().Text(
                    "INITIAL CERTIFICATE OF COMPLIANCE WITH POLLUTION STANDARDS, " +
                    "SAFETY STANDARDS OF COMPONENTS AND ROADWORTHINESS")
                    .Bold().FontSize(8).AlignCenter();
                col.Item().LineHorizontal(1);

                // -- Sale bill + owner block --
                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text(t => { t.Span("Bill No: ").Bold(); t.Span(Val(m.SaleBillNo)); });
                        left.Item().Text(t => { t.Span("Owner Name: ").Bold(); t.Span(Val(m.OwnerName)); });
                        left.Item().Text(t => { t.Span("Address: ").Bold(); t.Span(Val(m.OwnerAddress)); });
                        left.Item().Text(t => { t.Span("Mobile: ").Bold(); t.Span(Val(m.OwnerMobile)); });
                    });
                    row.ConstantItem(150).AlignRight().Column(right =>
                    {
                        right.Item().Text(t => { t.Span("Date: ").Bold(); t.Span(m.SaleDate.ToString("dd-MM-yyyy")); });
                    });
                });

                // -- Certification line --
                col.Item().PaddingTop(6).Text(
                    "Certified that the motor vehicle(s) described below comply with the " +
                    "provisions of the Motor Vehicles Act, 1988 and the rules made thereunder, " +
                    "and that the particulars given are true.").FontSize(8);

                // -- One block per vehicle --
                foreach (var v in m.Vehicles)
                {
                    col.Item().PaddingTop(8).Border(1).BorderColor(Colors.Grey.Medium).Padding(8)
                        .Column(box =>
                        {
                            box.Item().Text($"Vehicle {v.SrNo}").Bold().FontSize(10);

                            box.Item().PaddingTop(3).Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(1.6f); c.RelativeColumn(2f);
                                    c.RelativeColumn(1.6f); c.RelativeColumn(2f);
                                });

                                void Lbl(string text) =>
                                    t.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                        .Background(Colors.Grey.Lighten4).Padding(4)
                                        .Text(text).Bold().FontSize(8);

                                void Vl(string? text) =>
                                    t.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                        .Padding(4).Text(Val(text)).FontSize(8);

                                void Pair(string l, string? val) { Lbl(l); Vl(val); }

                                Pair("Type Approval Cert No", v.TypeApprovalCertNo);
                                Pair("Make / Model", v.MakeModel);

                                Pair("Chassis No", v.ChassisNo);
                                Pair("Motor / Engine No", v.MotorNo);

                                Pair("Month & Year of Mfg", v.MfgYear?.ToString());
                                Pair("Colour", v.Colour);

                                Pair("Emission Norms", v.Emission);
                                Pair("Sound Level of Horn", v.SoundLevelHorn);

                                Pair("Pass-by Noise Level", v.NoiseLevel);
                                Pair("Battery No", v.BatteryNo);

                                Pair("Battery Make", v.BatteryMake);
                                Pair("Battery Capacity", v.BatteryCapacity);

                                Pair("Charger No", v.ChargerNo);
                                Pair("Controller No", v.ControllerNo);
                            });
                        });
                }

                // -- Declaration + signature --
                col.Item().PaddingTop(12).Text(
                    "I/We hereby certify that the above vehicle(s) conform to the provisions " +
                    "of the Central Motor Vehicles Rules, 1989 in respect of pollution, safety " +
                    "of components and roadworthiness on the date of sale.").FontSize(8);

                col.Item().PaddingTop(34).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Date & Place").Bold();
                    });
                    r.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text($"For {Val(m.DealerName)}").Bold();
                        c.Item().PaddingTop(18).AlignRight().Text("Authorized Signatory").Bold();
                    });
                });
            }

            return Document.Create(container =>
            {
                foreach (var copyLabel in new[] { "RTO COPY", "CUSTOMER COPY" })
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(28);
                        page.DefaultTextStyle(x => x.FontSize(9));
                        page.Content().Column(col => Compose(col, copyLabel));
                    });
                }
            }).GeneratePdf();
        }

        // In the service — these call separate PDF generators per type
        public async Task<byte[]> DownloadMultipleForm22(List<int> ids)
        {
            using var memory = new MemoryStream();
            using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                foreach (var id in ids)
                {
                    var pdfBytes = await DownloadForm22Pdf(id); // implement separately
                    var entry = archive.CreateEntry($"Form22_{id}.pdf");
                    using var stream = entry.Open();
                    await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }
            }
            return memory.ToArray();
        }

        public async Task<byte[]> DownloadMultipleInvoices(List<int> ids)
        {
            using var memory = new MemoryStream();
            using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                foreach (var id in ids)
                {
                    var pdfBytes = await DownloadSaleBillPdf(id); // reuse existing
                    var entry = archive.CreateEntry($"ExShowroom_{id}.pdf");
                    using var stream = entry.Open();
                    await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }
            }
            return memory.ToArray();
        }

        public async Task<byte[]> DownloadMultipleCombined(List<int> form22Ids, List<int> invoiceIds)
        {
            using var memory = new MemoryStream();

            using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                foreach (var id in form22Ids)
                {
                    var pdfBytes = await DownloadForm22Pdf(id);
                    var entry = archive.CreateEntry($"Form22_{id}.pdf");
                    using var stream = entry.Open();
                    await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }
                foreach (var id in invoiceIds)
                {
                    var pdfBytes = await DownloadExShowroomInvoicePdf(id);   // changed
                    var entry = archive.CreateEntry($"ExShowroom_Invoice_{id}.pdf");
                    using var stream = entry.Open();
                    await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                }
            }

            return memory.ToArray();
        }

        public async Task<byte[]> DownloadProformaInvoicePdf(int id)
        {
            var model = await _repo.GetProformaInvoiceDataAsync(id);
            if (model == null)
                throw new Exception("Sale Bill not found");

            return GenerateProformaInvoicePdf(model);
        }

        private byte[] GenerateProformaInvoicePdf(ProformaInvoicePdfModel m)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string Val(string? v) => string.IsNullOrWhiteSpace(v) ? "-" : v!;
            string Money(decimal v) => v.ToString("N2");

            const string HeaderBg = "#6F72A0";
            var copies = m.Lines.Count > 1 ? "Multiple" : "Single";
            var taxTotal = m.IsIgst ? m.IgstTotal : (m.CgstTotal + m.SgstTotal);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(col =>
                    {
                        // -- Dealer header (centered) --
                        col.Item().AlignCenter().Column(h =>
                        {
                            if (!string.IsNullOrWhiteSpace(m.DealerName))
                                h.Item().AlignCenter().Text(m.DealerName).Bold().FontSize(12);
                            if (!string.IsNullOrWhiteSpace(m.DealerAddress))
                                h.Item().AlignCenter().Text(m.DealerAddress).FontSize(8);
                            h.Item().AlignCenter().Text($"Phone No.: {Val(m.DealerPhone)}").FontSize(8);
                            h.Item().AlignCenter().Text($"Email: {Val(m.DealerEmail)}").FontSize(8);
                            h.Item().AlignCenter().Text($"GSTIN No.: {Val(m.DealerGstin)}").FontSize(8);
                            h.Item().AlignCenter().Text($"PAN No.: {Val(m.DealerPan)}").FontSize(8);
                            h.Item().AlignCenter().Text($"Trade Certificate No.: {Val(m.DealerTradeCertNo)}").FontSize(8);
                        });

                        col.Item().PaddingTop(4).LineHorizontal(1);
                        col.Item().AlignCenter().Text("PROFORMA INVOICE").Bold().FontSize(12);
                        col.Item().LineHorizontal(1);

                        // -- Customer details --
                        col.Item().PaddingTop(6).Text("Customer Details").Bold().FontSize(10);

                        void KV(ColumnDescriptor c, string label, string? value)
                        {
                            c.Item().PaddingVertical(1).Row(r =>
                            {
                                r.ConstantItem(120).Text(label).Bold().FontSize(8);
                                r.RelativeItem().Text($": {Val(value)}").FontSize(8);
                            });
                        }

                        col.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                KV(left, "Party Name", m.CustomerName);
                                KV(left, "Mobile No", m.CustomerMobile);
                                KV(left, "Email", m.CustomerEmail);
                                KV(left, "Address", m.CustomerAddress);
                                KV(left, "State", m.CustomerState);
                                KV(left, "Country", m.CustomerCountry);
                            });
                            row.RelativeItem().Column(right =>
                            {
                                KV(right, "Proforma Invoice No", m.ProformaNo);
                                KV(right, "Proforma Invoice Date", m.InvoiceDate.ToString("dd-MM-yyyy"));
                                KV(right, "Bill Type", $"{Val(m.BillTypeText)}[{copies}]");
                                KV(right, "Sale Type", m.SaleType);
                                KV(right, "Customer Type", m.CustomerType);
                                KV(right, "Financier", m.FinancedBy);
                            });
                        });

                        // -- Item table --
                        col.Item().PaddingTop(8).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(32);    // S.No
                                c.RelativeColumn(2.6f);  // Model
                                c.RelativeColumn(1.8f);  // Product Code
                                c.RelativeColumn(1.3f);  // Color
                                c.RelativeColumn(1.2f);  // HSN
                                c.RelativeColumn(2f);    // Chassis No
                                c.RelativeColumn(2f);    // Motor No
                                c.RelativeColumn(1.4f);  // Amount
                            });

                            void HCell(string text, bool right = false, bool center = false)
                            {
                                var cell = t.Cell().Background(HeaderBg)
                                    .Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4);
                                var aligned = right ? cell.AlignRight() : (center ? cell.AlignCenter() : cell);
                                aligned.Text(text).Bold().FontSize(8).FontColor(Colors.White);
                            }

                            t.Header(hr =>
                            {
                                HCell("S.No", center: true);
                                HCell("Model"); HCell("HSN"); HCell("Chassis No");
                                HCell("Motor No"); HCell("Colour");
                                HCell("Taxable", right: true); HCell("GST", right: true); HCell("Amount", right: true);
                            });

                            foreach (var l in m.Lines)
                            {
                                IContainer Body(IContainer c) =>
                                    c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);

                                Body(t.Cell()).AlignCenter().Text(l.SrNo.ToString()).FontSize(8);   
                                Body(t.Cell()).Text(Val(l.Description)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.ProductCode)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.Colour)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.Hsn)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.ChassisNo)).FontSize(8);
                                Body(t.Cell()).Text(Val(l.MotorNo)).FontSize(8);
                                Body(t.Cell()).AlignRight().Text(Money(l.ItemRate)).FontSize(8);
                            }
                        });

                        // -- Totals (full-width label + right amount) --
                        void TotalRow(string label, string amount, bool bold = false)
                        {
                            col.Item().PaddingTop(2).Row(r =>
                            {
                                var lbl = r.RelativeItem().Text(label);
                                lbl.FontSize(9);
                                if (bold) lbl.Bold();

                                var amt = r.ConstantItem(120).AlignRight().Text(amount);
                                amt.FontSize(9);
                                if (bold) amt.Bold();
                            });
                        }

                        col.Item().PaddingTop(4);
                        TotalRow("Taxable Price", Money(m.TaxableTotal));
                        if (m.IsIgst)
                            TotalRow($"IGST@{m.IgstPer:0.##}%", Money(m.IgstTotal));
                        else
                        {
                            TotalRow($"CGST@{m.CgstPer:0.##}%", Money(m.CgstTotal));
                            TotalRow($"SGST@{m.SgstPer:0.##}%", Money(m.SgstTotal));
                        }
                        if (m.SubsidyTotal > 0)
                            TotalRow("Less PM E-DRIVE Incentive from Govt. of India", $"({Money(m.SubsidyTotal)})");
                        if (m.RegTotal > 0) TotalRow("Registration", Money(m.RegTotal));
                        if (m.InsuranceTotal > 0) TotalRow("Insurance", Money(m.InsuranceTotal));
                        TotalRow("Proforma Total", Money(m.GrandTotal), bold: true);

                        // -- Amount in words --
                        col.Item().PaddingTop(4).Row(r =>
                        {
                            r.ConstantItem(110).Text("Amount in words :").Bold().FontSize(9);
                            r.RelativeItem().Text(Val(m.AmountInWords)).Bold().FontSize(9);
                        });

                        // -- Battery table --
                        col.Item().PaddingTop(8).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2.2f); c.RelativeColumn(1.3f); c.RelativeColumn(1.3f);
                                c.RelativeColumn(2.2f); c.RelativeColumn(1.3f);
                            });

                            void HC(string text) =>
                                t.Cell().Background(HeaderBg).Padding(4).Text(text).Bold().FontSize(8).FontColor(Colors.White);

                            t.Header(hr =>
                            {
                                HC("Battery Sr. No."); HC("Chemistry"); HC("Capacity"); HC("Make"); HC("Coupon No.");
                            });

                            foreach (var l in m.Lines)
                            {
                                void VC(string? text) =>
                                    t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4).Text(Val(text)).FontSize(8);

                                VC(l.BatteryNo);
                                VC(l.Chemistry);
                                VC(l.BatteryCapacity);
                                VC(l.BatteryMake);
                                VC(string.IsNullOrWhiteSpace(l.CouponNo) ? "NA" : l.CouponNo);
                            }
                        });

                        // -- Charger table --
                        col.Item().PaddingTop(4).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2.2f); c.RelativeColumn(1.2f);
                                c.RelativeColumn(2.2f); c.RelativeColumn(1.2f);
                            });

                            void HC(string text) =>
                                t.Cell().Background(HeaderBg).Padding(4).Text(text).Bold().FontSize(8).FontColor(Colors.White);

                            t.Header(hr =>
                            {
                                HC("Charger No."); HC("VCU"); HC("Controller No."); HC("Mfg. Year");
                            });

                            foreach (var l in m.Lines)
                            {
                                void VC(string? text) =>
                                    t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4).Text(Val(text)).FontSize(8);

                                VC(l.ChargerNo);
                                VC(l.Vcu);
                                VC(l.ControllerNo);
                                VC(l.MfgYear?.ToString());
                            }
                        });

                        // -- Terms & Conditions --
                        col.Item().PaddingTop(10).Text("Terms & Conditions").Bold().FontSize(9);
                        col.Item().PaddingLeft(12).Column(tc =>
                        {
                            tc.Item().Text("1.  Goods once sold will not be taken back").FontSize(8);
                            tc.Item().Text("2.  Subjected to Respective Jurisdiction only.").FontSize(8);
                            tc.Item().Text("3.  E&OE").FontSize(8);
                            tc.Item().Text("4.  RTO, Accessories & other charges will be additional.").FontSize(8);
                        });

                        col.Item().PaddingTop(8).Text(t =>
                        {
                            t.Span("Acknowledgement ").Bold().FontSize(8);
                            t.Span("Received Vehicle (with above Vin Number) in good condition along with this " +
                                   "invoice. The details have been verified and found satisfactory.").FontSize(8);
                        });

                        // -- Signatures --
                        col.Item().PaddingTop(36).Row(r =>
                        {
                            r.RelativeItem().Text("Customer Signature").Bold().FontSize(9);
                            r.RelativeItem().AlignRight().Text("For Authorized Signatory").Bold().FontSize(9);
                        });

                        col.Item().PaddingTop(10).AlignCenter().Text("THANK YOU").Bold().FontSize(9);
                    });
                });
            }).GeneratePdf();
        }
    }
}

