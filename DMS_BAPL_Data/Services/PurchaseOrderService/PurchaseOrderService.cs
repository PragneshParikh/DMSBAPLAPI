using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Repositories.PurchaseOrderRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DMS_BAPL_Data.Services.PurchaseOrder
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepo _repo;
        private readonly IDealerMasterRepo _dealerRepo;
        private readonly IColorMasterRepo _colorRepo;
        private readonly IitemMasterRepo _itemRepo;
        private readonly IExcelService _excelService;

        public PurchaseOrderService(IPurchaseOrderRepo repo, IDealerMasterRepo dealerMasterRepo, IitemMasterRepo itemMaster, IColorMasterRepo colorMasterRepo, IExcelService excelService)
        {
            _repo = repo;
            _dealerRepo = dealerMasterRepo;
            _colorRepo = colorMasterRepo;
            _itemRepo = itemMaster;
            _excelService = excelService;
        }

        // ── Tax calculation ─────────────────────────────────────────────────
        // Canonical tax "bucket" a TaxCode belongs to - "CGST", "SGST", or
        // null. (IGST is intentionally not resolved here - see below.)
        private static string? GetTaxBucket(string? taxCode)
        {
            if (string.IsNullOrEmpty(taxCode)) return null;
            var upper = taxCode.ToUpperInvariant();
            if (upper.Contains("CGST")) return "CGST";
            if (upper.Contains("SGST")) return "SGST";
            if (upper.Contains("IGST")) return "IGST";
            return null;
        }

        private static bool IsCgstOrSgstCode(string? taxCode)
        {
            var bucket = GetTaxBucket(taxCode);
            return bucket == "CGST" || bucket == "SGST";
        }

        // Selects at most one CGST row and one SGST row from an AtaxCode
        // group's tax codes (collapsing any duplicate/rate-suffixed rows,
        // e.g. "CGST" and "CGST9" both present, down to one per bucket).
        // Any separately-tagged "IGST" row in the same group is deliberately
        // ignored here - it is never used as the source of the IGST rate.
        private static List<AggregateTaxCode> SelectLocalCgstSgstTaxes(List<AggregateTaxCode> aggregateTaxes)
        {
            return aggregateTaxes
                .Where(agg => IsCgstOrSgstCode(agg.TaxCode))
                .GroupBy(agg => GetTaxBucket(agg.TaxCode))
                .Select(g => g.OrderBy(x => x.SrNo).First())
                .ToList();
        }

        // Computes the tax line(s) to apply to `lineAmount` (Dealer Price x
        // Qty). IGST is ALWAYS derived as CGST% + SGST% - under GST law the
        // interstate rate equals the combined intrastate rate for the same
        // HSN, so there is no independent "IGST rate" to look up. This is
        // what previously went wrong: a separately-maintained "IGST"
        // AggregateTaxCode row (9%) disagreed with the correct combined rate
        // (CGST 9% + SGST 9% = 18%). By deriving IGST here instead of
        // reading a second, independently-maintained row, the rate shown
        // when a part is added and the rate actually persisted on save can
        // never disagree again - both come from the same CGST/SGST source.
        private async Task<List<(string TaxCode, decimal TaxRate, decimal TaxAmount)>> ComputeTaxLinesAsync(
            string hsnCode, bool isInterState, DateTime poDate, decimal lineAmount)
        {
            // Always resolve the LOCAL (intrastate, StateFlag "S") HSN tax
            // mapping - this is where CGST and SGST are configured, and is
            // the single source of truth regardless of transaction direction.
            var localHsnTax = await _repo.GetHSNTaxWithFallbackAsync(hsnCode, "S", poDate);
            if (localHsnTax == null)
                throw new Exception($"{StringConstants.NoTaxConfig} {hsnCode} on {poDate}");

            var aggregateTaxes = await _repo.GetAggregateTaxesAsync(localHsnTax.AtaxCode);
            var localTaxes = SelectLocalCgstSgstTaxes(aggregateTaxes);

            decimal cgstRate = 0, sgstRate = 0;

            foreach (var agg in localTaxes)
            {
                // Date-aware lookup: the rate applied is always the one
                // effective as of THIS PO's date, so re-saving a different
                // PO later (after the tax master changes) can never change
                // what an already-saved PO computed.
                var taxMaster = await _repo.GetTaxMasterAsync(agg.TaxCode, poDate);
                if (taxMaster == null) continue;

                var bucket = GetTaxBucket(taxMaster.TaxCode);
                if (bucket == "CGST") cgstRate = taxMaster.TaxRate;
                else if (bucket == "SGST") sgstRate = taxMaster.TaxRate;
            }

            var lines = new List<(string, decimal, decimal)>();

            if (isInterState)
            {
                decimal igstRate = cgstRate + sgstRate;
                decimal igstAmount = (lineAmount * igstRate) / 100;
                lines.Add(("IGST", igstRate, igstAmount));
            }
            else
            {
                if (cgstRate > 0)
                    lines.Add(("CGST", cgstRate, (lineAmount * cgstRate) / 100));
                if (sgstRate > 0)
                    lines.Add(("SGST", sgstRate, (lineAmount * sgstRate) / 100));
            }

            return lines;
        }
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new Purchase Order with details and tax calculations.
        /// </summary>
        public async Task<bool> CreatePOAsync(PurchaseOrderViewModel model, string userId)
        {
            try
            {
                // Check if PO already exists, if so redirect to Update
                var existing = await _repo.GetPOByNumberAsync(model.PONumber);
                if (existing != null)
                {
                    return await UpdatePOAsync(model, userId);
                }

                await _repo.BeginTransactionAsync();
                int lineNumber = 1;
                decimal totalAmount = 0;
                decimal baseAmount = 0;

                // Get Dealer
                var dealer = await _dealerRepo.GetDealerByCode(model.CustomerCode);
                if (dealer == null)
                    throw new Exception(StringConstants.DealerNotFound);

                string dealerState = dealer.State?.Trim().ToLower();
                string companyState = StringConstants.CompanyLocation;

                string preferredFlag = dealerState == companyState ? "S" : "O";
                bool isInterState = preferredFlag == "O";

                // Create PO Header
                var po = new DBModels.PurchaseOrder
                {
                    Ponumber = model.PONumber,
                    PurchaseDate = model.PODate,
                    OrderType = model.POType,
                    CustomerCode = model.CustomerCode,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    TransactionType = model.TransactionType,
                    Remarks = model.Remarks,
                    LocCode = model.LocCode,
                    LedgerCode = model.LedgerCode,
                    Status = false,

                    SubOrderType = model.SubOrderType,
                    IsAgainstKit = model.IsAgainstKit,
                    JobId = model.JobId
                };

                await _repo.AddPOAsync(po);

                foreach (var item in model.Items)
                {
                    var itemMaster = await _repo.GetItemAsync(item.ItemCode);
                    if (itemMaster == null)
                        throw new Exception($"{StringConstants.ItemNotFound} {item.ItemCode}");
                    decimal rate = itemMaster.Dlrprice;
                    decimal mrpPerUnit = itemMaster.Custprice;
                    decimal lineAmount = item.Qty * rate;
                    decimal mrpTotal = item.Qty * mrpPerUnit;

                    var detail = new PurchaseOrderDetail
                    {
                        Ponumber = model.PONumber,
                        ItemCode = item.ItemCode,
                        Qty = (int)item.Qty,
                        Subsidy = itemMaster.Itemtype == 11 ? itemMaster.Fame2amount * item.Qty : 0,
                        Rate = rate,
                        Mrp = mrpTotal,
                        LineNumber = lineNumber,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        Status = false
                    };

                    await _repo.AddPODetailAsync(detail);

                    if (itemMaster.Hsncode == null)
                        throw new Exception($"{StringConstants.HSNCodeMissing} {item.ItemCode}");

                    var hsn = await _repo.GetHSNByCodeAsync(itemMaster.Hsncode);
                    if (hsn == null)
                        throw new Exception(StringConstants.HSNNotFound);

                    // lineAmount here is Dealer Price x Qty - the IGST (or
                    // CGST+SGST) rate is applied against this figure.
                    var taxLines = await ComputeTaxLinesAsync(hsn.Hsncode, isInterState, model.PODate, lineAmount);

                    int taxLine = 1;
                    decimal totalTax = 0;

                    foreach (var (taxCode, taxRate, taxAmount) in taxLines)
                    {
                        totalTax += taxAmount;

                        await _repo.AddTaxAsync(new TaxDetail
                        {
                            Ponumber = model.PONumber,
                            ItemCode = item.ItemCode,
                            PodetailsLineNumber = lineNumber,
                            TaxLineNumber = taxLine++,
                            TaxCode = taxCode,
                            TaxRate = taxRate,
                            TaxAmount = taxAmount,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        });
                    }
                    detail.LineAmount = lineAmount + totalTax;

                    totalAmount += lineAmount + totalTax;
                    baseAmount += lineAmount;
                    lineNumber++;
                }
                await _repo.UpdatePOAmountAsync(model.PONumber, totalAmount);

                await _repo.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _repo.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> CreatePartsPOAsync(PurchaseOrderViewModel model)
        {
            try
            {
                var existing = await _repo.GetPOByNumberAsync(model.PONumber);
                if (existing != null)
                {
                    return false;
                }

                await _repo.BeginTransactionAsync();
                int lineNumber = 1;
                decimal totalAmount = 0;

                // Get Dealer
                var dealer = await _dealerRepo.GetDealerByCode(model.CustomerCode);
                if (dealer == null)
                    throw new Exception(StringConstants.DealerNotFound);

                string dealerState = dealer.State?.Trim().ToLower();
                string companyState = StringConstants.CompanyLocation;
                string preferredFlag = dealerState == companyState ? "S" : "O";
                bool isInterState = preferredFlag == "O";

                // Create PO Header
                var po = new DBModels.PurchaseOrder
                {
                    Ponumber = model.PONumber,
                    PurchaseDate = model.PODate,
                    OrderType = model.POType,
                    CustomerCode = model.CustomerCode,
                    CreatedBy = model.CreatedBy,
                    CreatedDate = model.CreatedDate ?? DateTime.Now,
                    TransactionType = model.TransactionType,
                    Status = false
                };

                await _repo.AddPOAsync(po);

                foreach (var item in model.Items)
                {
                    var itemMaster = await _repo.GetItemAsync(item.ItemCode);

                    if (itemMaster == null)
                        throw new Exception($"{StringConstants.ItemNotFound} {item.ItemCode}");

                    // Validation for Parts only
                    if (itemMaster.Itemtype != 2)
                        throw new Exception($"Item {item.ItemCode} is not a part (ItemType != 2).");

                    decimal rate = itemMaster.Dlrprice;
                    decimal mrpPerUnit = itemMaster.Custprice;
                    decimal lineAmount = item.Qty * rate;
                    decimal mrpTotal = item.Qty * mrpPerUnit;

                    var detail = new PurchaseOrderDetail
                    {
                        Ponumber = model.PONumber,
                        ItemCode = item.ItemCode,
                        Qty = (int)item.Qty,
                        Subsidy = 0, // Subsidy only for Vehicles (itemtype 11)
                        Rate = rate,
                        Mrp = mrpTotal,
                        LineAmount = lineAmount,
                        LineNumber = lineNumber,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = model.CreatedDate ?? DateTime.Now,
                        Status = false
                    };

                    await _repo.AddPODetailAsync(detail);

                    // TAX FLOW
                    if (itemMaster.Hsncode == null)
                        throw new Exception($"{StringConstants.HSNCodeMissing} {item.ItemCode}");

                    var taxLines = await ComputeTaxLinesAsync(itemMaster.Hsncode, isInterState, model.PODate, lineAmount);

                    int taxLine = 1;
                    decimal totalTax = 0;

                    foreach (var (taxCode, taxRate, taxAmount) in taxLines)
                    {
                        totalTax += taxAmount;

                        await _repo.AddTaxAsync(new TaxDetail
                        {
                            Ponumber = model.PONumber,
                            ItemCode = item.ItemCode,
                            PodetailsLineNumber = lineNumber,
                            TaxLineNumber = taxLine++,
                            TaxCode = taxCode,
                            TaxRate = taxRate,
                            TaxAmount = taxAmount,
                            CreatedBy = model.CreatedBy,
                            CreatedDate = model.CreatedDate ?? DateTime.Now
                        });
                    }

                    detail.LineAmount = lineAmount + totalTax;

                    totalAmount += lineAmount + totalTax;
                    lineNumber++;
                }

                await _repo.UpdatePOAmountAsync(model.PONumber, totalAmount);
                await _repo.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _repo.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<PartsPurchaseOrderResponseViewModel>> GetPartsPOListAsync()
        {
            try
            {
                return await _repo.GetPartsPOListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a purchase order by its number.
        /// </summary>
        public async Task<PurchaseOrderResponseViewModel> GetPOByNumberAsync(string poNumber)
        {
            try
            {
                return await _repo.GetPOByNumberAsync(poNumber);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves all purchase orders.
        /// </summary>
        public async Task<PagedResponse<PurchaseOrderResponseViewModel>> GetPOListAsync(string? dealerCode, string orderType, int pageIndex, int pageSize, PurchaseOrderSearchViewModel purchaseOrderSearchViewModel)
        {
            try
            {
                return await _repo.GetPOListAsync(dealerCode, orderType, pageIndex, pageSize, purchaseOrderSearchViewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> ConvertPOToERPJsonAsync(object erpObject)
        {
            try
            {
                var result = await SendToERP(erpObject);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdatePOAsync(PurchaseOrderViewModel model, string userId)
        {
            await _repo.BeginTransactionAsync();

            try
            {
                // 1. Update Header
                var po = new DBModels.PurchaseOrder
                {
                    Ponumber = model.PONumber,
                    PurchaseDate = model.PODate,
                    OrderType = model.POType,
                    SubOrderType = model.SubOrderType,
                    CustomerCode = model.CustomerCode,
                    ConsigneeCode = model.LocCode,
                    TransactionType = model.TransactionType,
                    Remarks = model.Remarks,
                    LocCode = model.LocCode,
                    LedgerCode = model.LedgerCode,
                    IsAgainstKit = model.IsAgainstKit,
                    UpdatedBy = userId,
                    UpdatedDate = DateTime.Now
                };
                await _repo.UpdatePOHeaderAsync(po);

                // 2. Clear Existing Details & Taxes
                await _repo.DeleteTaxesByPOAsync(model.PONumber);
                await _repo.DeleteDetailsByPOAsync(model.PONumber);

                // 3. Re-insert Details & Taxes (Synchronized with Create logic)
                int lineNumber = 1;
                decimal totalAmount = 0;

                var dealer = await _dealerRepo.GetDealerByCode(model.CustomerCode);
                if (dealer == null)
                    throw new Exception(StringConstants.DealerNotFound);

                string dealerState = dealer.State?.Trim().ToLower();
                string companyState = StringConstants.CompanyLocation;
                string preferredFlag = dealerState == companyState ? "S" : "O";
                bool isInterState = preferredFlag == "O";

                foreach (var item in model.Items)
                {
                    var itemMaster = await _repo.GetItemAsync(item.ItemCode);
                    if (itemMaster == null)
                        throw new Exception($"{StringConstants.ItemNotFound} {item.ItemCode}");
                    decimal rate = itemMaster.Dlrprice;
                    decimal mrpPerUnit = itemMaster.Custprice;
                    decimal lineAmount = item.Qty * rate;
                    decimal mrpTotal = item.Qty * mrpPerUnit;

                    var detail = new PurchaseOrderDetail
                    {
                        Ponumber = model.PONumber,
                        ItemCode = item.ItemCode,
                        Qty = (int)item.Qty,
                        Subsidy = itemMaster.Itemtype == 11 ? itemMaster.Fame2amount * item.Qty : 0,
                        Rate = rate,
                        Mrp = mrpTotal,
                        LineNumber = lineNumber,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = userId,
                        UpdatedDate = DateTime.Now,
                        Status = false
                    };
                    await _repo.AddPODetailAsync(detail);

                    if (itemMaster.Hsncode == null)
                        throw new Exception($"{StringConstants.HSNCodeMissing} {item.ItemCode}");

                    var hsn = await _repo.GetHSNByCodeAsync(itemMaster.Hsncode);
                    if (hsn == null)
                        throw new Exception(StringConstants.HSNNotFound);

                    var taxLines = await ComputeTaxLinesAsync(hsn.Hsncode, isInterState, model.PODate, lineAmount);

                    int taxLine = 1;
                    decimal totalTax = 0;
                    foreach (var (taxCode, taxRate, taxAmount) in taxLines)
                    {
                        totalTax += taxAmount;

                        await _repo.AddTaxAsync(new TaxDetail
                        {
                            Ponumber = model.PONumber,
                            ItemCode = item.ItemCode,
                            PodetailsLineNumber = lineNumber,
                            TaxLineNumber = taxLine++,
                            TaxCode = taxCode,
                            TaxRate = taxRate,
                            TaxAmount = taxAmount,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now,
                            UpdatedBy = userId,
                            UpdatedDate = DateTime.Now
                        });
                    }

                    detail.LineAmount = lineAmount + totalTax;

                    totalAmount += lineAmount + totalTax;
                    lineNumber++;
                }

                await _repo.UpdatePOAmountAsync(model.PONumber, totalAmount);
                await _repo.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _repo.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeletePOItemsAsync(string poNumber)
        {
            await _repo.BeginTransactionAsync();
            try
            {
                await _repo.DeleteTaxesByPOAsync(poNumber);
                await _repo.DeleteDetailsByPOAsync(poNumber);
                await _repo.UpdatePOAmountAsync(poNumber, 0);
                await _repo.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _repo.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<decimal> GetSubsidyValueAsync()
        {
            return await _repo.GetSubsidyValue();
        }

        public async Task<byte[]> DownloadPurchaseOrderExcel(PurchaseOrderSearchViewModel filter)
        {
            try
            {
                if (filter.DealerCode == "null")
                {
                    filter.DealerCode = null;
                }

                //var data = await GetPOListAsync(null, filter.OrderType);
                var data = new List<PurchaseOrderViewModel>();

                // Apply Filters
                if (!string.IsNullOrEmpty(filter.PurchaseNo))
                {
                    data = data.Where(x => x.PONumber != null && x.PONumber.Contains(filter.PurchaseNo, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (filter.DateFrom.HasValue)
                {
                    data = data.Where(x => x.PODate >= filter.DateFrom.Value).ToList();
                }

                if (filter.DateTo.HasValue)
                {
                    data = data.Where(x => x.PODate <= filter.DateTo.Value.AddDays(1).AddTicks(-1)).ToList();
                }

                if (!string.IsNullOrEmpty(filter.TransactionType))
                {
                    data = data.Where(x => x.TransactionType == filter.TransactionType).ToList();
                }

                if (!string.IsNullOrEmpty(filter.IsSubmitted))
                {
                    bool isSub = filter.IsSubmitted == "Submited To Erp";
                    //data = data.Where(x => x.IsSubmitted == isSub).ToList();
                }

                var columns = new List<string>
                {
                    "Purchase No",
                    "Date",
                    "Trans Type",
                    "Party Name",
                    "Location",
                    "Order Amount",
                    "IsSubmittedToErp"
                };

                var rows = data.Select(po =>
                {
                    var dict = new Dictionary<string, object>();

                    //dict["Purchase No"] = po.PONumber;
                    //dict["Date"] = po.PODate?.ToString("dd-MM-yyyy") ?? "";
                    //dict["Trans Type"] = po.TransactionType;
                    //dict["Party Name"] = "BGAUSS AUTO PRIVATE LIMITED"; // Matching UI hardcoding
                    //dict["Location"] = po.LocationName ?? po.LocCode ?? "";
                    //dict["Order Amount"] = po.TotalAmount?.ToString("N2") ?? "0.00";
                    //dict["IsSubmittedToErp"] = (po.IsSubmitted == true) ? "Yes" : "No";

                    return dict;

                }).ToList();

                var excelModel = new ExcelExportViewModel
                {
                    SheetName = "PurchaseOrders",
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(excelModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<bool> UpdatePOStatusAsync(UpdatePOStatusViewModel updatePOStatusViewModel) => await _repo.UpdatePOStatusAsync(updatePOStatusViewModel);

        private async Task<string> SendToERP(object erpObject)
        {
            try
            {
                using var client = new HttpClient();

                var json = JsonSerializer.Serialize(erpObject);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(
                    "https://uatbaplai-cpapc4h7gvdkfxh4.centralindia-01.azurewebsites.net/api/BAPLSOHeader",
                    content
                );

                response.EnsureSuccessStatusCode();

                var _response = await response.Content.ReadAsStringAsync();

                return _response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task<object> GetOrderDetailsByItemCode(string itemCode, string dealerCode) => _repo.GetOrderDetailsByItemCode(itemCode, dealerCode);


    }
}