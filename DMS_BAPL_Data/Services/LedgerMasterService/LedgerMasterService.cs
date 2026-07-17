using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LedgerMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LedgerMasterService
{
    public partial class LedgerMasterService : ILedgerMasterService
    {
        private readonly ILedgerMasterRepo _ledgerMasterRepo;
        private readonly IExcelService _excelService;
        public LedgerMasterService(ILedgerMasterRepo ledgerMasterRepo, IExcelService excelService)
        {
            _ledgerMasterRepo = ledgerMasterRepo;
            _excelService = excelService;
        }

        Task<IEnumerable<LedgerMaster>> ILedgerMasterService.GetAll() => _ledgerMasterRepo.GetAll();
        Task<PagedResponse<object>> ILedgerMasterService.GetLedgerByPagedAsync(string? searchTerm, int pageIndex, int pageSize, string dealerCode, string filter) => _ledgerMasterRepo.GetLedgerByPagedAsync(searchTerm, pageIndex, pageSize, dealerCode, filter);
        Task<LedgerDetailViewModel?> ILedgerMasterService.GetLedgerByIdAsync(int id) => _ledgerMasterRepo.GetLedgerById(id);
        Task<int> ILedgerMasterService.InsertLedgerDetail(LedgerMaster ledgerMaster, string userId)
        {
            ledgerMaster.CreatedBy = userId;
            ledgerMaster.CreatedDate = DateTime.Now;
            return _ledgerMasterRepo.InsertLedgerDetail(ledgerMaster);
        }
        Task<bool> ILedgerMasterService.UpdateLedgerDetail(LedgerMaster ledgerMaster, string userId)
        {
            ledgerMaster.UpdatedBy = userId;
            ledgerMaster.UpdatedDate = DateTime.Now;
            return _ledgerMasterRepo.UpdateLedgerDetail(ledgerMaster);
        }
        Task<IEnumerable<LedgerDetailViewModel>> ILedgerMasterService.GetCompanyLedgersAsync() => _ledgerMasterRepo.GetCompanyLedgers();
        Task<IEnumerable<LedgerMaster>> ILedgerMasterService.GetInsuranceLedgersAsync() => _ledgerMasterRepo.GetInsuranceLedgers();

        public async Task<List<LedgerMaster>> GetLedgerByLedgerType(string ledgerType)
        {
            try
            {
                return await _ledgerMasterRepo.GetLedgerByLedgerType(ledgerType);
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<string>> GetAllMobileNumberByDealerCode(string dealerCode)
        {
            try
            {
                return await _ledgerMasterRepo.GetAllMobileNumberByDealerCode(dealerCode);
            }
            catch
            {
                throw;
            }
        }
        public async Task<string> GetNextLedId(string dealerCode)
        {
            try
            {
                return await _ledgerMasterRepo.GetNextLedCode(dealerCode);
            }
            catch
            {
                throw;
            }
        }

        // Export ledger list to Excel
        public async Task<byte[]> DownloadExcel(string? dealerCode)
        {
            try
            {
                var data = await _ledgerMasterRepo.GetExcelData();
                if (dealerCode != null)
                {
                    data = data.Where(i => i.DealerCode == dealerCode).ToList();
                }

                var properties = typeof(LedgerExcelViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);
                        dict[prop.Name] = entityProp != null
                            ? entityProp.GetValue(d)
                            : null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = "LedgerExcel",
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<LedgerMaster>> GetLedgerForSale(string? dealerCode, bool isSuperAdmin)
        {
            try
            {
                return await _ledgerMasterRepo.GetLedgerForSale(dealerCode, isSuperAdmin);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<LedgerMaster>> GetLotRelatedLedgers(string? dealerCode, bool? IsD2D)
        {
            try
            {
                return await _ledgerMasterRepo.GetLotRelatedLedgers(dealerCode, IsD2D);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool?> GetD2DProvision(string? dealerCode)
        {
            try
            {
                return await _ledgerMasterRepo.GetD2DProvision(dealerCode);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<LedgerMaster>> GetSupplierLedgers(string? dealerCode)
        {
            try
            {
                return await _ledgerMasterRepo.GetSupplierLedgers(dealerCode);
            }
            catch
            {
                throw;
            }
        }
    }
}
