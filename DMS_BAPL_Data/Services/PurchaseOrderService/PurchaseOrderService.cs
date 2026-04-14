using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Repositories.PurchaseOrderRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PurchaseOrder
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepo _repo;
        private readonly IDealerMasterRepo _dealerRepo;
        private readonly IColorMasterRepo _colorRepo;
        private readonly IitemMasterRepo _itemRepo;

        public PurchaseOrderService(IPurchaseOrderRepo repo, IDealerMasterRepo dealerMasterRepo, IitemMasterRepo itemMaster, IColorMasterRepo colorMasterRepo)
        {
            _repo = repo;
            _dealerRepo = dealerMasterRepo;
            _colorRepo = colorMasterRepo;
            _itemRepo = itemMaster;
        }

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

                // Get Dealer
                var dealer = await _dealerRepo.GetDealerByCode(model.CustomerCode);
                if (dealer == null)
                    throw new Exception(StringConstants.DealerNotFound);

                string dealerState = dealer.State?.Trim().ToLower();
                string companyState = StringConstants.CompanyLocation;

                string preferredFlag = dealerState == companyState ? "S" : "O";

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
                    Status = false
                };

                await _repo.AddPOAsync(po);

                foreach (var item in model.Items)
                {
                    var itemMaster = await _repo.GetItemAsync(item.ItemCode);

                    if (itemMaster == null)
                        throw new Exception($"{StringConstants.ItemNotFound} {item.ItemCode}");

                    decimal rate = itemMaster.Dlrprice;
                    decimal lineAmount = item.Qty * rate;

                    //if (itemMaster.Itemtype == 2)
                    //    lineAmount -= 20;

                    var detail = new PurchaseOrderDetail
                    {
                        Ponumber = model.PONumber,
                        ItemCode = item.ItemCode,
                        Qty = (int)item.Qty,
                        Subsidy = itemMaster.Itemtype == 11 ? itemMaster.Fame2amount * item.Qty : 0,
                        Rate = rate,
                        LineAmount = lineAmount,
                        LineNumber = lineNumber,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        Status = false
                    };

                    await _repo.AddPODetailAsync(detail);

                    // TAX FLOW
                    if (itemMaster.Hsncode == null)
                        throw new Exception($"{StringConstants.HSNCodeMissing} {item.ItemCode}");

                    var hsn = await _repo.GetHSNByCodeAsync(itemMaster.Hsncode);

                    if (hsn == null)
                        throw new Exception(StringConstants.HSNNotFound);

                    var hsnTax = await _repo.GetHSNTaxWithFallbackAsync(
                        hsn.Hsncode,
                        preferredFlag,
                        model.PODate
                    );

                    if (hsnTax == null)
                        throw new Exception(
                            $"{StringConstants.NoTaxConfig} {hsn.Hsncode} on {model.PODate}"
                        );

                    var aggregateTaxes = await _repo.GetAggregateTaxesAsync(hsnTax.AtaxCode);

                    int taxLine = 1;
                    decimal totalTax = 0;

                    foreach (var agg in aggregateTaxes)
                    {
                        var taxMaster = await _repo.GetTaxMasterAsync(agg.TaxCode);

                        if (taxMaster == null)
                            continue;

                        decimal taxAmount = (lineAmount * taxMaster.TaxRate) / 100;
                        totalTax += taxAmount;

                        await _repo.AddTaxAsync(new TaxDetail
                        {
                            Ponumber = model.PONumber,
                            ItemCode = item.ItemCode,
                            PodetailsLineNumber = lineNumber,
                            TaxLineNumber = taxLine++,
                            TaxCode = taxMaster.TaxCode,
                            TaxRate = taxMaster.TaxRate,
                            TaxAmount = taxAmount,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        });
                    }

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

        public async Task<bool> CreatePartsPOAsync(PartsPurchaseOrderViewModel model, string userId)
        {
            try
            {
                // Check if PO already exists
                var existing = await _repo.GetPOByNumberAsync(model.PONumber);
                if (existing != null)
                {
                    // For now, we return false if exists to prevent accidental overwrite of Vehicle POs
                    // or we could implement UpdatePartsPOAsync later.
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
                    Status = false
                };

                await _repo.AddPOAsync(po);

                foreach (var item in model.Items)
                {
                    var itemMaster = await _repo.GetItemAsync(item.ItemCode);

                    if (itemMaster == null)
                        throw new Exception($"{StringConstants.ItemNotFound} {item.ItemCode}");

                    // Validation for Parts only
                    if (itemMaster.Itemtype != 1)
                        throw new Exception($"Item {item.ItemCode} is not a part (ItemType != 1).");

                    decimal rate = itemMaster.Dlrprice;
                    decimal lineAmount = item.Qty * rate;

                    var detail = new PurchaseOrderDetail
                    {
                        Ponumber = model.PONumber,
                        ItemCode = item.ItemCode,
                        Qty = (int)item.Qty,
                        Subsidy = 0, // Subsidy only for Vehicles (itemtype 11)
                        Rate = rate,
                        LineAmount = lineAmount,
                        LineNumber = lineNumber,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        Status = false
                    };

                    await _repo.AddPODetailAsync(detail);

                    // TAX FLOW
                    if (itemMaster.Hsncode == null)
                        throw new Exception($"{StringConstants.HSNCodeMissing} {item.ItemCode}");

                    var hsnTax = await _repo.GetHSNTaxWithFallbackAsync(
                        itemMaster.Hsncode,
                        preferredFlag,
                        model.PODate
                    );

                    if (hsnTax == null)
                        throw new Exception($"{StringConstants.NoTaxConfig} {itemMaster.Hsncode}");

                    var aggregateTaxes = await _repo.GetAggregateTaxesAsync(hsnTax.AtaxCode);

                    int taxLine = 1;
                    decimal totalTax = 0;

                    foreach (var agg in aggregateTaxes)
                    {
                        var taxMaster = await _repo.GetTaxMasterAsync(agg.TaxCode);
                        if (taxMaster == null) continue;

                        decimal taxAmount = (lineAmount * taxMaster.TaxRate) / 100;
                        totalTax += taxAmount;

                        await _repo.AddTaxAsync(new TaxDetail
                        {
                            Ponumber = model.PONumber,
                            ItemCode = item.ItemCode,
                            PodetailsLineNumber = lineNumber,
                            TaxLineNumber = taxLine++,
                            TaxCode = taxMaster.TaxCode,
                            TaxRate = taxMaster.TaxRate,
                            TaxAmount = taxAmount,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        });
                    }

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
        public async Task<List<PurchaseOrderResponseViewModel>> GetPOListAsync()
        {
            try
            {
                return await _repo.GetPOListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<POERPRequestViewModel> ConvertPOToERPJsonAsync(string poNumber)
        {
            try
            {
                // Get PO
                var po = await _repo.GetPOByNumberAsync(poNumber);

                if (po == null)
                    throw new Exception(StringConstants.PONotFound);
                await _repo.UpdateStatus(po.PONumber);
                // Consignee Logic
                string suffix = "S1";
                string consigneeCode = po.CustomerCode + suffix;

                var soLines = new List<SOLine>();

                foreach (var item in po.Items)
                {
                    // Get Item Master
                    var itemMaster = await _itemRepo.GetItemByCodeAsync(item.ItemCode);

                    if (itemMaster == null)
                        throw new Exception(StringConstants.ItemNotFound + " " + item.ItemCode);

                    // Get Color
                    var color = await _colorRepo.GetColorByCodeAsync(itemMaster.Colorcode);

                    // Build SO Line
                    soLines.Add(new SOLine
                    {
                        Itemname = item.ItemCode,
                        modlname = item.ItemCode,
                        descriptions = itemMaster.Itemname,
                        Unit = "NOS",
                        qty = item.Qty.ToString(),
                        itemmodelname = "",
                        colridno = color?.Rrgcoloridno.ToString() ?? "0",
                        colrcode = color?.Colorcode ?? "",
                        dmspordridno = po.PONumber
                    });
                }

                // Return ERP JSON
                return new POERPRequestViewModel
                {
                    soHeader = new SOHeader
                    {
                        refno = po.PONumber,
                        pordrdate = po.PODate?.ToString("dd-MM-yyyy") ?? "",
                        pordr_type = "",
                        ordrtype = "V",
                        testcertificate = "N",
                        consigneecode = consigneeCode,
                        customercode = po.CustomerCode,
                        amount = po.TotalAmount.ToString(),
                        FameIIFlag = "Y",
                        TransactionType = "",


                    },
                    soLine = soLines
                };
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
                    CustomerCode = model.CustomerCode,
                    TransactionType = model.TransactionType,
                    Remarks = model.Remarks,
                    LocCode = model.LocCode,
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

                foreach (var item in model.Items)
                {
                    var itemMaster = await _repo.GetItemAsync(item.ItemCode);
                    if (itemMaster == null)
                        throw new Exception($"{StringConstants.ItemNotFound} {item.ItemCode}");

                    decimal rate = itemMaster.Dlrprice;
                    decimal lineAmount = item.Qty * rate;

                    var detail = new PurchaseOrderDetail
                    {
                        Ponumber = model.PONumber,
                        ItemCode = item.ItemCode,
                        Qty = (int)item.Qty,
                        Subsidy = itemMaster.Itemtype == 11 ? itemMaster.Fame2amount * item.Qty : 0,
                        Rate = rate,
                        LineAmount = lineAmount,
                        LineNumber = lineNumber,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        Status = false
                    };
                    await _repo.AddPODetailAsync(detail);

                    // TAX FLOW
                    if (itemMaster.Hsncode == null)
                        throw new Exception($"{StringConstants.HSNCodeMissing} {item.ItemCode}");

                    var hsn = await _repo.GetHSNByCodeAsync(itemMaster.Hsncode);
                    if (hsn == null)
                        throw new Exception(StringConstants.HSNNotFound);

                    var hsnTax = await _repo.GetHSNTaxWithFallbackAsync(hsn.Hsncode, preferredFlag, model.PODate);
                    if (hsnTax == null)
                        throw new Exception($"{StringConstants.NoTaxConfig} {hsn.Hsncode} on {model.PODate}");

                    var aggregateTaxes = await _repo.GetAggregateTaxesAsync(hsnTax.AtaxCode);

                    int taxLine = 1;
                    decimal totalTax = 0;
                    foreach (var agg in aggregateTaxes)
                    {
                        var taxMaster = await _repo.GetTaxMasterAsync(agg.TaxCode);
                        if (taxMaster == null) continue;

                        decimal taxAmount = (lineAmount * taxMaster.TaxRate) / 100;
                        totalTax += taxAmount;

                        await _repo.AddTaxAsync(new TaxDetail
                        {
                            Ponumber = model.PONumber,
                            ItemCode = item.ItemCode,
                            PodetailsLineNumber = lineNumber,
                            TaxLineNumber = taxLine++,
                            TaxCode = taxMaster.TaxCode,
                            TaxRate = taxMaster.TaxRate,
                            TaxAmount = taxAmount,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        });
                    }

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
    }
}