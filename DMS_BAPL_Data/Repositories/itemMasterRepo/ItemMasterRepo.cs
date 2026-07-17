using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.TaxServices;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.itemMasterRepo
{
    public class ItemMasterRepo : IitemMasterRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly ITaxServices _taxService;

        public ItemMasterRepo(BapldmsvadContext context, ITaxServices taxService)
        {
            _context = context;
            _taxService = taxService;
        }

        // add new item to the database
        async Task<insertItemMasterViewModel> IitemMasterRepo.InsertItemAsync(insertItemMasterViewModel item, string userId)
        {
            try
            {
                var existingItem = await _context.ItemMasters
                    .FirstOrDefaultAsync(x => x.Itemcode == item.Itemcode);

                if (existingItem != null)
                {
                    existingItem.Itemtype = item.Itemtype;
                    existingItem.Itemname = item.Itemname;
                    existingItem.Itemdesc = item.Itemdesc;
                    existingItem.Status = item.Status;
                    existingItem.Hsncode = item.Hsncode;
                    existingItem.Dlrprice = item.Dlrprice;
                    existingItem.Custprice = item.Custprice;
                    existingItem.Moq = item.Moq;
                    existingItem.Boq = item.Boq;
                    existingItem.Sgst = item.Sgst;
                    existingItem.Cgst = item.Cgst;
                    existingItem.Igst = item.Igst;
                    existingItem.Ugst = item.Ugst;
                    existingItem.Grpidno = item.Grpidno;
                    existingItem.Ipurrate = item.Ipurrate;
                    existingItem.Iselectric = item.Iselectric;
                    existingItem.Vehtype = item.Vehtype;
                    existingItem.Noofbatteries = item.Noofbatteries;
                    existingItem.Colorcode = item.Colorcode;
                    existingItem.Rrgitemidno = item.Rrgitemidno;
                    existingItem.Itemcc = item.Itemcc;
                    existingItem.Batterytypeidno = item.Batterytypeidno;
                    existingItem.Fame2amount = item.Fame2amount;
                    existingItem.Compcode = item.Compcode;
                    existingItem.Displayname = item.Displayname;
                    existingItem.Oemmodelname = item.Oemmodelname;
                    existingItem.SupplierId = item.SupplierId;
                    existingItem.DealerCode = item.Dealercode;
                    existingItem.Uom = item.UOM;


                    existingItem.UpdatedBy = userId;
                    existingItem.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    var itemMasterEntity = new ItemMaster
                    {
                        Itemtype = item.Itemtype,
                        Itemname = item.Itemname,
                        Itemcode = item.Itemcode,
                        Itemdesc = item.Itemdesc,
                        Status = item.Status,
                        Hsncode = item.Hsncode,
                        Dlrprice = item.Dlrprice,
                        Custprice = item.Custprice,
                        Uom = item.UOM,
                        MinBillQty = item.MinBillQty,
                        MinOrderQty = item.MinOrderQty,
                        Moq = item.Moq,
                        Boq = item.Boq,
                        Sgst = item.Sgst,
                        Cgst = item.Cgst,
                        Igst = item.Igst,
                        Ugst = item.Ugst,
                        Grpidno = item.Grpidno,
                        Ipurrate = item.Ipurrate,
                        Iselectric = item.Iselectric,
                        Vehtype = item.Vehtype,
                        Noofbatteries = item.Noofbatteries,
                        Colorcode = item.Colorcode,
                        Rrgitemidno = item.Rrgitemidno,
                        Itemcc = item.Itemcc,
                        Batterytypeidno = item.Batterytypeidno,
                        Fame2amount = item.Fame2amount,
                        Compcode = item.Compcode,
                        Displayname = item.Displayname,
                        Oemmodelname = item.Oemmodelname,
                        DealerCode = item.Dealercode,
                        SupplierId = item.SupplierId,
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.ItemMasters.Add(itemMasterEntity);
                }

                await _context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //Get all items from the database
        public async Task<List<ItemMasterViewModel>> GetAllItemsAsync(int? grpidno, string? search)
        {
            var query = from i in _context.ItemMasters
                        join c in _context.ColorMasters
                        on i.Colorcode equals c.Colorcode into colorGroup
                        from c in colorGroup.DefaultIfEmpty()
                        join l in _context.LedgerMasters
                        on i.SupplierId equals l.Id into ledgerGroup
                        from l in ledgerGroup.DefaultIfEmpty()
                        select new { i, c, l }; //  keep original entity

            // Filter by Group Id
            if (grpidno.HasValue)
            {
                query = query.Where(x => x.i.Grpidno == grpidno.Value);
            }

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.i.Itemname.Contains(search) ||
                    x.i.Itemcode.Contains(search) ||
                    x.i.Itemdesc.Contains(search) ||
                    x.i.Hsncode.Contains(search) ||
                    x.i.Oemmodelname.Contains(search) ||
                    x.i.Displayname.Contains(search) ||
                    (x.c != null && x.c.Colorname.Contains(search))
                );
            }

            //  Apply OrderBy BEFORE projection
            var result = await query
                .OrderByDescending(x => x.i.CreatedDate)
                .Select(x => new ItemMasterViewModel
                {
                    Id = x.i.Id,
                    Itemtype = x.i.Itemtype,
                    Itemname = x.i.Itemname,
                    Itemcode = x.i.Itemcode,
                    Itemdesc = x.i.Itemdesc,
                    Status = x.i.Status,
                    Hsncode = x.i.Hsncode,
                    Dlrprice = x.i.Dlrprice,
                    Custprice = x.i.Custprice,
                    Moq = x.i.Moq,
                    Boq = x.i.Boq,
                    Sgst = x.i.Sgst,
                    Cgst = x.i.Cgst,
                    Igst = x.i.Igst,
                    Ugst = x.i.Ugst,
                    Grpidno = x.i.Grpidno,
                    Ipurrate = x.i.Ipurrate,
                    Iselectric = x.i.Iselectric,
                    Vehtype = x.i.Vehtype,
                    Noofbatteries = x.i.Noofbatteries,
                    ColorName = x.c != null ? x.c.Colorname : null,
                    Itemcc = x.i.Itemcc,
                    Batterytypeidno = x.i.Batterytypeidno,
                    Fame2amount = x.i.Fame2amount,
                    Compcode = x.i.Compcode,
                    Displayname = x.i.Displayname,
                    Oemmodelname = x.i.Oemmodelname,
                    MinBillQty = x.i.MinBillQty,
                    MinOrderQty = x.i.MinOrderQty,
                    WarrantyPeriod = x.i.WarrantyPeriod,
                    WarrantyDurationType = x.i.WarrantyDurationType,
                    WarrantyKms = x.i.WarrantyKms,
                    IsWarrantyApproval = x.i.IsWarrantyApproval,
                    VORRate = x.i.Vorrate,
                    IsVOR = x.i.IsVor,
                    Remarks = x.i.Remarks,
                    IsExempted = x.i.IsExempted,
                    IsToolkitFirstAid = x.i.IsToolkitFirstAid,
                    IsStockRequired = x.i.IsStockRequired,
                    IsHelmet = x.i.IsHelmet,
                    IsInventory = x.i.IsInventory,
                    IsInEligibleInput = x.i.IsInEligibleInput,
                    Uom = x.i.Uom,
                    Dealercode = x.i.DealerCode,
                    SupplierId = x.i.SupplierId,
                    LedgerName = x.l.LedgerName,
                    CreatedBy = x.i.CreatedBy,
                    CreatedDate = x.i.CreatedDate,
                    UpdatedBy = x.i.UpdatedBy,
                    UpdatedDate = x.i.UpdatedDate
                })
                .ToListAsync();

            return result;
        }
        public async Task<List<ItemMaster>> GetAllExcelItemsAsync()
        {
            return await _context.ItemMasters.ToListAsync();
        }
        public async Task<ItemMaster> GetItemByCodeAsync(string itemCode)
        {
            try
            {
                var item = await _context.ItemMasters
                    .FirstOrDefaultAsync(i => i.Itemcode == itemCode);

                return item;
            }
            catch (Exception)
            {
                throw;
            }
        }
        //update data particular on Item ID
        public async Task<ItemMaster> UpdateItemAsync(ItemMaster item)
        {
            var existingItem = await _context.ItemMasters.FirstOrDefaultAsync(x => x.Itemcode == item.Itemcode);

            if (existingItem == null)
                return null;

            if (existingItem != null)
            {
                existingItem.Itemtype = item.Itemtype;
                existingItem.Itemname = item.Itemname;
                existingItem.Itemcode = item.Itemcode;
                existingItem.Itemdesc = item.Itemdesc;
                existingItem.Status = item.Status;
                existingItem.Status = item.Status;
                existingItem.Hsncode = item.Hsncode;
                existingItem.Dlrprice = item.Dlrprice;
                existingItem.Custprice = item.Custprice;
                existingItem.Moq = item.Moq;
                existingItem.Boq = item.Boq;
                existingItem.Sgst = item.Sgst;
                existingItem.Cgst = item.Cgst;
                existingItem.Igst = item.Igst;
                existingItem.Ugst = item.Ugst;
                existingItem.Grpidno = item.Grpidno;
                existingItem.Ipurrate = item.Ipurrate;
                existingItem.Iselectric = item.Iselectric;
                existingItem.Vehtype = item.Vehtype;
                existingItem.Noofbatteries = item.Noofbatteries;
                existingItem.Colorcode = item.Colorcode;
                existingItem.Rrgitemidno = item.Rrgitemidno;
                existingItem.Itemcc = item.Itemcc;
                existingItem.Batterytypeidno = item.Batterytypeidno;
                existingItem.Fame2amount = item.Fame2amount;
                existingItem.Compcode = item.Compcode;
                existingItem.Displayname = item.Displayname;
                existingItem.MinBillQty = item.MinBillQty;
                existingItem.MinOrderQty = item.MinOrderQty;
                existingItem.WarrantyPeriod = item.WarrantyPeriod;
                existingItem.WarrantyDurationType = item.WarrantyDurationType;
                existingItem.WarrantyKms = item.WarrantyKms;
                existingItem.IsWarrantyApproval = item.IsWarrantyApproval;
                existingItem.Vorrate = item.Vorrate;
                existingItem.IsVor = item.IsVor;
                existingItem.Remarks = item.Remarks;
                existingItem.IsExempted = item.IsExempted;
                existingItem.IsToolkitFirstAid = item.IsToolkitFirstAid;
                existingItem.IsStockRequired = item.IsStockRequired;
                existingItem.IsHelmet = item.IsHelmet;
                existingItem.IsInventory = item.IsInventory;
                existingItem.IsInEligibleInput = item.IsInEligibleInput;
                existingItem.Oemmodelname = item.Oemmodelname;
                existingItem.DealerCode = item.DealerCode;
                existingItem.Uom = item.Uom;
                existingItem.SupplierId = item.SupplierId;
                existingItem.Status = item.Status;
                existingItem.CreatedBy = item.CreatedBy;
                existingItem.CreatedDate = item.CreatedDate;
                existingItem.UpdatedBy = item.UpdatedBy;
                existingItem.UpdatedDate = item.UpdatedDate;

                await _context.SaveChangesAsync();
            }

            return existingItem;
        }
        //Get purchase details by model no
        public async Task<ItemMasterViewModel> GetPurchaseDetailsByModelNo(string modelNo)
        {
            try
            {
                var item = await _context.ItemMasters
                            .FirstOrDefaultAsync(x => x.Itemcode == modelNo);

                if (item == null)
                    return null;

                ItemMasterViewModel modelItemDetail = new ItemMasterViewModel();
                modelItemDetail.Grpidno = item.Grpidno;
                modelItemDetail.Itemtype = item.Itemtype;
                modelItemDetail.Itemcode = item.Itemcode;
                modelItemDetail.Itemdesc = item.Itemdesc;
                //modelItemDetail.Colorcode = item.Colorcode;
                //modelItemDetail.ColorName = item.Colorcode;
                modelItemDetail.Colorcode = _context.ColorMasters.Where(x => x.Colorcode == item.Colorcode).Select(x => x.Colorname).FirstOrDefault();
                modelItemDetail.Ipurrate = item.Ipurrate;
                modelItemDetail.Sgst = item.Sgst;
                modelItemDetail.Cgst = item.Cgst;
                modelItemDetail.Igst = item.Igst;
                modelItemDetail.Fame2amount = item.Fame2amount;

                return modelItemDetail;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching purchase details by Model No", ex);
            }
        }
        public async Task<IEnumerable<ItemMaster>> GetItemByItemType(int itemType)
        {
            try
            {
                return await _context.ItemMasters
                    .AsNoTracking()
                    .Where(x => x.Itemtype == itemType)
                    .ToListAsync();
            }
            catch { throw; }
        }
        //Get Purchase Details With Hsn Tax By Model No
        public async Task<ItemMasterViewModel> GetPurchaseDetailsWithHsnTaxByModelNo(string modelNo)
        {
            try
            {
                if (string.IsNullOrEmpty(modelNo)) return null;
                var searchModel = modelNo.Trim().ToUpper();

                // 1. Find the item
                var items = await _context.ItemMasters
                            .Include(x => x.HsncodeNavigation)
                            .Where(x => x.Itemcode.Trim().ToUpper() == searchModel)
                            .ToListAsync();

                if (items == null || !items.Any()) return null;

                // Pick the best record (one with HSN data)
                var item = items.OrderByDescending(x => !string.IsNullOrEmpty(x.Hsncode) || x.HsncodeId != null).First();

                // Initial ViewModel (baseline metadata)
                ItemMasterViewModel modelItemDetail = new ItemMasterViewModel
                {
                    Id = item.Id,
                    Itemtype = item.Itemtype,
                    Itemcode = item.Itemcode,
                    Itemdesc = item.Itemdesc,
                    Hsncode = item.Hsncode,
                    Colorcode = _context.ColorMasters.Where(x => x.Colorcode == item.Colorcode).Select(x => x.Colorname).FirstOrDefault(),
                    Ipurrate = item.Ipurrate,
                    Fame2amount = item.Fame2amount,
                    Grpidno = item.Grpidno,
                    Sgst = 0,
                    Cgst = 0,
                    Igst = 0 // Start with 0 as per "no static" request
                };

                // 2. Determine HSN string
                string hsn = (item.Hsncode ?? "").Trim();
                if (string.IsNullOrEmpty(hsn) && item.HsncodeNavigation != null) hsn = (item.HsncodeNavigation.Hsncode ?? "").Trim();

                if (!string.IsNullOrEmpty(hsn))
                {
                    if (string.IsNullOrEmpty(modelItemDetail.Hsncode)) modelItemDetail.Hsncode = hsn;

                    // Fetch mappings using prefix matching
                    var allHsnCodes = await _context.HsnwiseTaxCodes.ToListAsync();
                    var matchedMappings = allHsnCodes
                        .Where(x => x.Hsncode != null && (hsn.StartsWith(x.Hsncode.Trim()) || x.Hsncode.Trim().StartsWith(hsn)))
                        .ToList();

                    if (matchedMappings.Any())
                    {
                        var latestMappings = matchedMappings
                            .GroupBy(x => (x.StateFlag ?? "").Trim().ToUpper())
                            .Select(g => g.OrderByDescending(x => x.EffectiveDate).First())
                            .ToList();

                        foreach (var mapping in latestMappings)
                        {
                            var sFlag = (mapping.StateFlag ?? "").Trim().ToUpper();
                            var taxRates = await _context.AggregateTaxCodes
                                .Where(x => x.AtaxCode == mapping.AtaxCode)
                                .ToListAsync();

                            if (taxRates != null && taxRates.Any())
                            {
                                // Priority mapping based on flag
                                var isLocalEntry = sFlag.StartsWith("L") || sFlag.Contains("LOC");
                                var isInterEntry = sFlag.StartsWith("I") || sFlag.Contains("INT");

                                // Local taxes
                                if (isLocalEntry || (!isInterEntry && taxRates.Any(x => (x.TaxCode ?? "").ToUpper().StartsWith("S") || (x.TaxCode ?? "").ToUpper().StartsWith("C"))))
                                {
                                    var sgst = taxRates.FirstOrDefault(x => (x.TaxCode ?? "").ToUpper().Contains("S"))?.TaxRate;
                                    var cgst = taxRates.FirstOrDefault(x => (x.TaxCode ?? "").ToUpper().Contains("C"))?.TaxRate;
                                    if (sgst != null) modelItemDetail.Sgst = sgst.Value;
                                    if (cgst != null) modelItemDetail.Cgst = cgst.Value;
                                }

                                // Interstate taxes (IGST)
                                if (isInterEntry || (!isLocalEntry && taxRates.Any(x => (x.TaxCode ?? "").ToUpper().Contains("I"))))
                                {
                                    var igst = taxRates.FirstOrDefault(x => (x.TaxCode ?? "").ToUpper().Contains("I"))?.TaxRate;
                                    if (igst == null && taxRates.Count == 1) igst = taxRates[0].TaxRate;
                                    if (igst != null) modelItemDetail.Igst = igst.Value;
                                }
                            }
                        }
                    }
                }
                return modelItemDetail;
            }
            catch (Exception ex)
            {
                try
                {
                    var item = await _context.ItemMasters.FirstOrDefaultAsync(x => x.Itemcode == modelNo);
                    if (item != null) return new ItemMasterViewModel { Itemcode = item.Itemcode, Sgst = 0, Cgst = 0, Igst = 0, Ipurrate = item.Ipurrate };
                }
                catch { }
                throw new Exception("Error while fetching purchase details with HSN tax by Model No", ex);
            }
        }

        public async Task<object> UpdateByItemCode(string userId, insertItemMasterViewModel insertItemMasterViewModel)
        {
            var existingItem = await _context.ItemMasters
                    .FirstOrDefaultAsync(x => x.Itemcode == insertItemMasterViewModel.Itemcode);

            if (existingItem == null)
            {
                existingItem = new ItemMaster
                {
                    Itemtype = insertItemMasterViewModel.Itemtype,
                    Itemname = insertItemMasterViewModel.Itemname,
                    Itemcode = insertItemMasterViewModel.Itemcode,
                    Itemdesc = insertItemMasterViewModel.Itemdesc,
                    Status = insertItemMasterViewModel.Status,
                    Hsncode = insertItemMasterViewModel.Hsncode,
                    Dlrprice = insertItemMasterViewModel.Dlrprice,
                    Custprice = insertItemMasterViewModel.Custprice,
                    Moq = insertItemMasterViewModel.Moq,
                    Boq = insertItemMasterViewModel.Boq,
                    Sgst = insertItemMasterViewModel.Sgst,
                    Cgst = insertItemMasterViewModel.Cgst,
                    Igst = insertItemMasterViewModel.Igst,
                    Ugst = insertItemMasterViewModel.Ugst,
                    Grpidno = insertItemMasterViewModel.Grpidno,
                    Ipurrate = insertItemMasterViewModel.Ipurrate,
                    Iselectric = insertItemMasterViewModel.Iselectric,
                    Vehtype = insertItemMasterViewModel.Vehtype,
                    Noofbatteries = insertItemMasterViewModel.Noofbatteries,
                    Colorcode = insertItemMasterViewModel.Colorcode,
                    Rrgitemidno = insertItemMasterViewModel.Rrgitemidno,
                    Itemcc = insertItemMasterViewModel.Itemcc,
                    Batterytypeidno = insertItemMasterViewModel.Batterytypeidno,
                    Fame2amount = insertItemMasterViewModel.Fame2amount,
                    Compcode = insertItemMasterViewModel.Compcode,
                    Displayname = insertItemMasterViewModel.Displayname,
                    DealerCode = insertItemMasterViewModel.Dealercode,
                    SupplierId = insertItemMasterViewModel.SupplierId,
                    Uom = insertItemMasterViewModel.UOM,
                    //MinBillQty = insertItemMasterViewModel.MinBillQty;
                    //MinOrderQty = insertItemMasterViewModel.MinOrderQty;
                    //WarrantyPeriod = insertItemMasterViewModel.WarrantyPeriod;
                    //WarrantyDurationType = insertItemMasterViewModel.WarrantyDurationType;
                    //WarrantyKms = insertItemMasterViewModel.WarrantyKms;
                    //IsWarrantyApproval = insertItemMasterViewModel.IsWarrantyApproval;
                    //Vorrate = insertItemMasterViewModel.Vorrate;
                    //IsVor = insertItemMasterViewModel.IsVor;
                    //Remarks = insertItemMasterViewModel.Remarks;
                    //IsExempted = insertItemMasterViewModel.IsExempted;
                    //IsToolkitFirstAid = insertItemMasterViewModel.IsToolkitFirstAid;
                    //IsStockRequired = insertItemMasterViewModel.IsStockRequired;
                    //IsHelmet = insertItemMasterViewModel.IsHelmet;
                    //IsInventory = insertItemMasterViewModel.IsInventory;
                    //IsInEligibleInput = insertItemMasterViewModel.IsInEligibleInput;
                    Oemmodelname = insertItemMasterViewModel.Oemmodelname,
                    CreatedBy = insertItemMasterViewModel.CreatedBy,
                    CreatedDate = DateTime.Now
                };

                await _context.ItemMasters.AddAsync(existingItem);
            }
            else
            {
                if (existingItem != null)
                {
                    existingItem.Itemtype = insertItemMasterViewModel.Itemtype;
                    existingItem.Itemname = insertItemMasterViewModel.Itemname;
                    existingItem.Itemdesc = insertItemMasterViewModel.Itemdesc;
                    existingItem.Status = insertItemMasterViewModel.Status;
                    existingItem.Hsncode = insertItemMasterViewModel.Hsncode;
                    existingItem.Dlrprice = insertItemMasterViewModel.Dlrprice;
                    existingItem.Custprice = insertItemMasterViewModel.Custprice;
                    existingItem.Moq = insertItemMasterViewModel.Moq;
                    existingItem.Boq = insertItemMasterViewModel.Boq;
                    existingItem.Sgst = insertItemMasterViewModel.Sgst;
                    existingItem.Cgst = insertItemMasterViewModel.Cgst;
                    existingItem.Igst = insertItemMasterViewModel.Igst;
                    existingItem.Ugst = insertItemMasterViewModel.Ugst;
                    existingItem.Grpidno = insertItemMasterViewModel.Grpidno;
                    existingItem.Ipurrate = insertItemMasterViewModel.Ipurrate;
                    existingItem.Iselectric = insertItemMasterViewModel.Iselectric;
                    existingItem.Vehtype = insertItemMasterViewModel.Vehtype;
                    existingItem.Noofbatteries = insertItemMasterViewModel.Noofbatteries;
                    existingItem.Colorcode = insertItemMasterViewModel.Colorcode;
                    existingItem.Rrgitemidno = insertItemMasterViewModel.Rrgitemidno;
                    existingItem.Itemcc = insertItemMasterViewModel.Itemcc;
                    existingItem.Batterytypeidno = insertItemMasterViewModel.Batterytypeidno;
                    existingItem.Fame2amount = insertItemMasterViewModel.Fame2amount;
                    existingItem.Compcode = insertItemMasterViewModel.Compcode;
                    existingItem.Displayname = insertItemMasterViewModel.Displayname;
                    existingItem.Oemmodelname = insertItemMasterViewModel.Oemmodelname;
                    existingItem.DealerCode = insertItemMasterViewModel.Dealercode;
                    existingItem.SupplierId = insertItemMasterViewModel.SupplierId;
                    existingItem.Uom = insertItemMasterViewModel.UOM;

                    existingItem.UpdatedBy = userId;
                    existingItem.UpdatedDate = DateTime.UtcNow;
                }

            }
            await _context.SaveChangesAsync();
            return existingItem;
        }
        public async Task<IEnumerable<ItemMaster>> GetItemsByOEMModel(int id)
        {
            try
            {
                var result = await (
                    from IM in _context.ItemMasters
                    join MM in _context.OemmodelMasters
                        on IM.Oemmodelname equals MM.ModelName
                    where MM.Id == id
                    select IM
                    )
                    .ToListAsync();

                return result;
            }
            catch { throw; }
        }

        public async Task<List<ItemMaster>> GetByItemCodesAsync(List<string> itemCodes)
        {
            try
            {
                return await _context.ItemMasters
               .Where(x => x.Itemcode != null && itemCodes.Contains(x.Itemcode))
               .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        //public async Task<IEnumerable<object>> GetItemsWithHSNTaxGroupId(int? groupId)
        //{
        //    try
        //    {
        //        var result = await (
        //            from IM in _context.ItemMasters

        //            join HM in _context.HsncodeMasters
        //                on IM.Hsncode equals HM.Hsncode

        //            join PI in _context.PartsInventories
        //                    .Where(x => x.FinalStockFlag == "Y")
        //                on IM.Itemcode equals PI.ItemCode into PIGroup
        //            from PI in PIGroup.DefaultIfEmpty()

        //            join HSNT in _context.HsnwiseTaxCodes
        //                ATC in _context.aggregateTaxCode
        //                on HSNT.ATaxCode equal ATC.AtaxCode
        //                on IM.Hsncode equals HSNT.Hsncode && HSNT.StateFlag = "S" into taxGroup
        //            from HSNT in taxGroup.DefaultIfEmpty()

        //            where IM.Grpidno == groupId

        //            select new
        //            {
        //                IM.Id,
        //                IM.Itemcode,
        //                IM.Itemname,
        //                IM.Itemdesc,
        //                IM.Hsncode,
        //                IM.Dlrprice,
        //                IM.Custprice,
        //                IM.Sgst,
        //                IM.Cgst,
        //                IM.Igst,
        //                IM.Ugst,
        //                IM.Grpidno,
        //                IM.Colorcode,
        //                BatchClosingQty = PI != null ? PI.BatchClosingQty : 0
        //            })
        //            .Distinct()
        //            .ToListAsync();

        //        return result;
        //    }
        //    catch { throw; }
        //}

        //public async Task<IEnumerable<object>> GetItemsWithHSNTaxGroupId(int? groupId)
        //{
        //    try
        //    {
        //        var result = await (
        //            from IM in _context.ItemMasters
        //            join HM in _context.HsncodeMasters
        //                on IM.Hsncode equals HM.Hsncode
        //            join PI in _context.PartsInventories
        //                    .Where(x => x.FinalStockFlag == "Y")
        //                on IM.Itemcode equals PI.ItemCode into PIGroup
        //            from PI in PIGroup.DefaultIfEmpty()
        //            where IM.Grpidno == groupId

        //            //let taxType = "S"

        //            //let taxCode = _context.HsnwiseTaxCodes
        //            //    .Where(x => x.Hsncode == IM.Hsncode && x.StateFlag == taxType)
        //            //    .OrderByDescending(x => x.EffectiveDate)
        //            //    .FirstOrDefault()
        //            let taxCode = _context.HsnwiseTaxCodes
        //                .Where(x => x.Hsncode == IM.Hsncode)
        //                .OrderByDescending(x => x.EffectiveDate)
        //                .FirstOrDefault()

        //            let cgstPercentage = _context.AggregateTaxCodes
        //                .Where(x => x.AtaxCode == taxCode.AtaxCode && x.TaxCode.StartsWith("CGST"))
        //                .Sum(x => (decimal?)x.TaxRate) ?? 0

        //            let sgstPercentage = _context.AggregateTaxCodes
        //                .Where(x => x.AtaxCode == taxCode.AtaxCode && x.TaxCode.StartsWith("SGST"))
        //                .Sum(x => (decimal?)x.TaxRate) ?? 0

        //            let igstPercentage = _context.AggregateTaxCodes
        //                .Where(x => x.AtaxCode == taxCode.AtaxCode && x.TaxCode.StartsWith("IGST"))
        //                .Sum(x => (decimal?)x.TaxRate) ?? 0
        //            select new
        //            {
        //                IM.Id,
        //                IM.Itemcode,
        //                IM.Itemname,
        //                IM.Itemdesc,
        //                IM.Hsncode,
        //                IM.Dlrprice,
        //                IM.Custprice,
        //                IM.Grpidno,
        //                IM.Colorcode,
        //                BatchClosingQty = PI != null ? PI.BatchClosingQty : 0,
        //                CgstPercentage = cgstPercentage,
        //                SgstPercentage = sgstPercentage,
        //                IgstPercentage = igstPercentage
        //            })
        //            .Distinct()
        //            .ToListAsync();
        //        return result;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<object>> GetItemsWithHSNTaxGroupId(int? groupId)
        {
            try
            {
                var result = await (
                    from IM in _context.ItemMasters

                    join HM in _context.HsncodeMasters
                        on IM.Hsncode equals HM.Hsncode

                    join PI in _context.PartsInventories
                            .Where(x => x.FinalStockFlag == "Y")
                        on IM.Itemcode equals PI.ItemCode into PIGroup

                    from PI in PIGroup.DefaultIfEmpty()

                    where IM.Grpidno == groupId

                    let stateTaxCode = _context.HsnwiseTaxCodes
                        .Where(x =>
                            x.Hsncode == IM.Hsncode &&
                            x.StateFlag.Trim() == "S")
                        .OrderByDescending(x => x.EffectiveDate)
                        .FirstOrDefault()

                    let otherTaxCode = _context.HsnwiseTaxCodes
                        .Where(x =>
                            x.Hsncode == IM.Hsncode &&
                            x.StateFlag.Trim() == "O")
                        .OrderByDescending(x => x.EffectiveDate)
                        .FirstOrDefault()

                    let cgstPercentage = _context.AggregateTaxCodes
                        .Where(x =>
                            stateTaxCode != null &&
                            x.AtaxCode == stateTaxCode.AtaxCode &&
                            x.TaxCode.StartsWith("CGST"))
                        .Select(x => (decimal?)x.TaxRate)
                        .FirstOrDefault() ?? 0

                    let sgstPercentage = _context.AggregateTaxCodes
                        .Where(x =>
                            stateTaxCode != null &&
                            x.AtaxCode == stateTaxCode.AtaxCode &&
                            x.TaxCode.StartsWith("SGST"))
                        .Select(x => (decimal?)x.TaxRate)
                        .FirstOrDefault() ?? 0

                    let igstPercentage = _context.AggregateTaxCodes
                        .Where(x =>
                            otherTaxCode != null &&
                            x.AtaxCode == otherTaxCode.AtaxCode &&
                            x.TaxCode.StartsWith("IGST"))
                        .Select(x => (decimal?)x.TaxRate)
                        .FirstOrDefault() ?? 0

                    select new
                    {
                        IM.Id,
                        IM.Itemcode,
                        IM.Itemname,
                        IM.Itemdesc,
                        IM.Hsncode,
                        IM.Dlrprice,
                        IM.Custprice,
                        IM.Grpidno,
                        IM.Colorcode,

                        BatchClosingQty = PI != null
                            ? PI.BatchClosingQty
                            : 0,

                        CgstPercentage = cgstPercentage,
                        SgstPercentage = sgstPercentage,
                        IgstPercentage = igstPercentage,

                        TotalGSTSameState = cgstPercentage + sgstPercentage,
                        TotalGSTOtherState = igstPercentage
                    }
                )
                .Distinct()
                .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ItemPartsByLocationViewModel>> GetItemsByLocation(
      string dealerLocation,
      string customerLocation)
        {
            try
            {
                var items = await _context.ItemMasters
                    .Where(im => im.Grpidno == 1)
                    .Select(im => new
                    {
                        ItemCode = im.Itemcode,
                        ItemName = im.Itemname,

                        // Latest MRP
                        ItemMrp = _context.PartsInwards
                            .Where(p => p.PartNo == im.Itemcode && p.IsAccepted == true)
                            .OrderByDescending(p => p.Id)
                            .Select(p => (decimal?)p.ItemMrp)
                            .FirstOrDefault() ?? 0,

                        // Latest Stock
                        ItemStock = _context.PartsInventories
                            .Where(p =>
                                p.ItemCode == im.Itemcode &&
                                p.DealerLocation == dealerLocation &&
                                p.FinalStockFlag == "Y")
                            .OrderByDescending(p => p.Id)
                            .Select(p => (decimal?)p.BatchClosingQty)
                            .FirstOrDefault() ?? 0
                    })
                    .ToListAsync();

                var dealerState = await _context.LocationMasters
                    .Where(x => x.Loccode == dealerLocation)
                    .Select(x => x.State)
                    .FirstOrDefaultAsync();

                List<TaxDetailViewModel> taxes = new();

                if (items.Any())
                {
                    taxes = await _taxService.GetTaxDetailsAsync(
                        items.First().ItemCode,
                        dealerState,
                        customerLocation);
                }

                var sgstPer = taxes
                    .FirstOrDefault(x =>
                        x.TaxCode != null &&
                        x.TaxCode.Contains("SGST", StringComparison.OrdinalIgnoreCase))
                    ?.TaxRate ?? 0;

                var cgstPer = taxes
                    .FirstOrDefault(x =>
                        x.TaxCode != null &&
                        x.TaxCode.Contains("CGST", StringComparison.OrdinalIgnoreCase))
                    ?.TaxRate ?? 0;

                var igstPer = taxes
                    .FirstOrDefault(x =>
                        x.TaxCode != null &&
                        x.TaxCode.Contains("IGST", StringComparison.OrdinalIgnoreCase))
                    ?.TaxRate ?? 0;

                return items.Select(item => new ItemPartsByLocationViewModel
                {
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    ItemMrp = item.ItemMrp,
                    ItemStock = item.ItemStock,

                    SGSTPer = sgstPer,
                    CGSTPer = cgstPer,
                    IGSTPer = igstPer,

                    SGSTAmount = (item.ItemMrp * sgstPer) / 100,
                    CGSTAmount = (item.ItemMrp * cgstPer) / 100,
                    IGSTAmount = (item.ItemMrp * igstPer) / 100
                }).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ItemMasterViewModel>> GetItemModelist()
        {
            try
            {
                return await _context.ItemMasters
                    .Where(x => x.Grpidno == 6)
                    .OrderByDescending(x => x.CreatedDate)
                    .Select(x => new ItemMasterViewModel
                    {
                        Id = x.Id,
                        Itemtype = x.Itemtype,
                        Itemname = x.Itemname,
                        Itemcode = x.Itemcode,
                        Itemdesc = x.Itemdesc,
                        ColorName = _context.ColorMasters
                            .Where(c => c.Colorcode == x.Colorcode)
                            .Select(c => c.Colorname)
                            .FirstOrDefault(),
                        Status = x.Status
                    })
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
