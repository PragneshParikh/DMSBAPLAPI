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
                        from c in colorGroup.DefaultIfEmpty() // LEFT JOIN
                        select new ItemMasterViewModel
                        {

                            Itemtype = i.Itemtype,
                            Itemname = i.Itemname,
                            Itemcode = i.Itemcode,
                            Itemdesc = i.Itemdesc,
                            Status = i.Status,
                            Hsncode = i.Hsncode,
                            Dlrprice = i.Dlrprice,
                            Custprice = i.Custprice,
                            Moq = i.Moq,
                            Boq = i.Boq,
                            Sgst = i.Sgst,
                            Cgst = i.Cgst,
                            Igst = i.Igst,
                            Ugst = i.Ugst,
                            Grpidno = i.Grpidno,
                            Ipurrate = i.Ipurrate,
                            Iselectric = i.Iselectric,
                            Vehtype = i.Vehtype,
                            Noofbatteries = i.Noofbatteries,
                            ColorName = c != null ? c.Colorname : null,
                            Itemcc = i.Itemcc,
                            Batterytypeidno = i.Batterytypeidno,
                            Fame2amount = i.Fame2amount,
                            Compcode = i.Compcode,
                            Displayname = i.Displayname,
                            Oemmodelname = i.Oemmodelname,
                        };

            // Filter by Group Id
            if (grpidno.HasValue)
            {
                query = query.Where(i => i.Grpidno == grpidno.Value);
            }

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i =>
                    i.Itemname.Contains(search) ||
                    i.Itemcode.Contains(search) ||
                    i.Itemdesc.Contains(search) ||
                    i.Hsncode.Contains(search) ||
                    i.Oemmodelname.Contains(search) ||
                    i.Displayname.Contains(search) ||
                    i.ColorName.Contains(search)   // optional
                );
            }

            return await query.ToListAsync();
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
    }
}
