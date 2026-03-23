using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Repositories.PurchaseOrderRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.PurchaseOrder
{
    
   
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepo _repo;
        private readonly IDealerMasterRepo _dealerRepo;

        public PurchaseOrderService(IPurchaseOrderRepo repo, IDealerMasterRepo dealerMasterRepo)
        {
            _repo = repo;
            _dealerRepo = dealerMasterRepo;
        }

        /// <summary>
        /// Creates a new Purchase Order with details and tax calculations.
        /// </summary>
        public async Task<bool> CreatePOAsync(PurchaseOrderViewModel model, string userId)
        {
            await _repo.BeginTransactionAsync();

            try
            {
                int lineNumber = 1;
                decimal totalAmount = 0;
                decimal subsidy = 20;

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

                    if (itemMaster.Itemtype == 2)
                        lineAmount -= 20;

                    var detail = new PurchaseOrderDetail
                    {
                        Ponumber = model.PONumber,
                        ItemCode = item.ItemCode,
                        Qty = (int)item.Qty,
                        Subsidy = itemMaster.Itemtype == 1 ? 0 : 20 * item.Qty,
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
    }
}