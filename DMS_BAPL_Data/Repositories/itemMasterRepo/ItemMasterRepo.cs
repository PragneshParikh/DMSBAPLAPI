using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
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

        public ItemMasterRepo(BapldmsvadContext context)
        {
            _context = context;
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
                        select new { i, c }; //  keep original entity

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
        public async Task UpdateItemAsync(ItemMaster item)
        {
            var existingItem = await _context.ItemMasters.FindAsync(item.Itemcode);

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
                existingItem.Oemmodelname = item.Oemmodelname;
                existingItem.UpdatedBy = "Null";
                existingItem.UpdatedDate = DateTime.UtcNow;
                existingItem.CreatedBy = "Kajal Tiwari";
                existingItem.CreatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
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
                                if (isLocalEntry || (!isInterEntry && taxRates.Any(x => (x.TaxCode ?? "").ToUpper().Contains("S") || (x.TaxCode ?? "").ToUpper().Contains("C"))))
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

        public async Task<object> UpdateByItemCode(string itemCode, string userId, insertItemMasterViewModel insertItemMasterViewModel)
        {
            var existingItem = await _context.ItemMasters
                    .FirstOrDefaultAsync(x => x.Itemcode == insertItemMasterViewModel.Itemcode);

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

                existingItem.UpdatedBy = userId;
                existingItem.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return existingItem;
        }
    }
}
