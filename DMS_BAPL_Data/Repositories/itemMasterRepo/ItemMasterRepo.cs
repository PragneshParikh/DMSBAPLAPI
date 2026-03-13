using DMS_BAPL_Data.DBModels;
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
        public async Task InsertItemAsync(ItemMaster item)
        {
            await _context.ItemMasters.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        //Get all items from the database
        public async Task<List<ItemMaster>> GetAllItemsAsync(int? grpidno, string? search)
        {
            var query = _context.ItemMasters.AsQueryable();

            // Filter by Group Id
            if (grpidno.HasValue)
            {
                query = query.Where(i => i.Grpidno == grpidno.Value);
            }

            // Search in multiple columns
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i =>
                    i.Itemname.Contains(search) ||
                    i.Itemcode.Contains(search) ||
                    i.Itemdesc.Contains(search) ||
                    i.Hsncode.Contains(search)  ||
                    i.Oemmodelname.Contains(search)||
                    i.Displayname.Contains(search)


                );
            }

            return await query.ToListAsync();
        }

        public async Task<List<ItemMaster>> GetAllExcelItemsAsync()
        {
            return await _context.ItemMasters.ToListAsync();
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
