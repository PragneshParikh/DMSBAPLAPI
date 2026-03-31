using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ReceiptEntryRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.ReceiptEntryService
{
    public class ReceiptEntryService : IReceiptEntryService
    {
        private readonly IReceiptEntryRepo _receiptEntryRepo;
        private readonly IExcelService _excelService;
        public ReceiptEntryService(IReceiptEntryRepo receiptEntryRepo, IExcelService excelService)
        {
            _receiptEntryRepo = receiptEntryRepo;
            _excelService = excelService;
        }

        public async Task<string> GenerateNextReceiptNoAsync()
        {
            var lastReceiptNo = await _receiptEntryRepo.GetLastReceiptNoAsync();

            if (string.IsNullOrEmpty(lastReceiptNo))
                return "RCPT001";

            // Remove prefix safely
            var numberPart = lastReceiptNo.Replace("RCPT", "");

            if (!int.TryParse(numberPart, out int number))
                throw new Exception("Invalid Receipt Number Format");

            number++;

            return $"RCPT{number.ToString("D3")}";
        }

        //public async Task<List<ReceiptEntry>> GetReceiptEntryListAsync()
        //{
        //    try
        //    {
        //        return await _receiptEntryRepo.GetReceiptEntryListAsync();
        //    }

        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //}

        public async Task<List<ReceiptEntry>> GetReceiptEntryListAsync(ReceiptFilterViewModel filter)
        {
            try
            {
                return await _receiptEntryRepo.GetReceiptEntryListAsync(filter);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ReceiptEntry> AddReceiptEntryAsync(ReceiptEntryViewModel receiptEntry, string userId)
        {
            try
            {
                return await _receiptEntryRepo.AddReceiptEntryAsync(receiptEntry, userId);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<LedgerMaster>> GetLedgerMasterDetailsByTypeAsync(string ledgerType)
        {
            try
            {
                return await _receiptEntryRepo.GetLedgerMasterDetailsByTypeAsync(ledgerType);
            }

            catch
            {
                throw;
            }
        }

        public async Task<ReceiptEntryEditViewModel?> GetReceiptByIdAsync(int id)
        {
            try
            {
                return await _receiptEntryRepo.GetReceiptByIdAsync(id);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ReceiptEntry?> UpdateReceiptEntryAsync(int id, ReceiptEntryViewModel receiptEntry, string userId)
        {
            try
            {
                return await _receiptEntryRepo.UpdateReceiptEntryAsync(id, receiptEntry, userId);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> CheckReceiptExist(string? mobileNo, string? bookingId)
        {
            try
            {
                return await _receiptEntryRepo.CheckReceiptExist(mobileNo, bookingId);
            }
            catch {
                throw;
            }
        }

        public async Task<byte[]> downloadReceiptExcel()
        {
            try
            {
                var data = await _receiptEntryRepo.GetReceiptEntryListAsync(null);

                var properties = typeof(ReceiptEntry)
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
                    SheetName = StringConstants.DealerExcelSheetName,
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); // optional logging
                throw;
            }
        }

       public async Task<List<ReceiptEntryEditViewModel>> GetReceiptEntryListAsyncWithSearch(string? searchTerm)
        {
            try
            {
                return await _receiptEntryRepo.GetReceiptEntryListAsyncWithSearch(searchTerm);
            }
            catch
            {
                throw;
            }
        }


    }
}
