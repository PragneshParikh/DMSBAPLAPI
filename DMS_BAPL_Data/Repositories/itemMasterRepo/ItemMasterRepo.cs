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
                var itemMasterEntity = new ItemMaster
                {
                    Id = 0,
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
            var existingItem = await _context.ItemMasters.FindAsync(item.Id);

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
    }
}
