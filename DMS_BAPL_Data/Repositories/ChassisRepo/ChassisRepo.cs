using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ChassisRepo
{
    public partial class ChassisRepo : IChassisRepo
    {
        private readonly BapldmsvadContext _context;
        public ChassisRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<object> GetChassisDataAsync(string chassisNumber)
        {
            var result = await (from v in _context.VehicleInwards
                                join j in _context.JobCardHeaders on v.ChasisNo equals j.Chassisno into cv
                                from j in cv.DefaultIfEmpty()
                                join c in _context.JobCardCustomers on v.Id equals c.Id into cc
                                from c in cc.DefaultIfEmpty()
                                join i in _context.ItemMasters on v.ItemCode equals i.Itemcode

                                where v.ChasisNo == chassisNumber
                                select new
                                {
                                    v.ChasisNo,
                                    v.MotorNo,
                                    v.InvoiceNo,
                                    v.InvoiceDate,
                                    v.DealerCode,
                                    v.ColrCode,
                                    j.IsPdiSuccess,
                                    c.CustomerName,
                                    c.SaleDate,
                                    i.Itemname
                                }).FirstOrDefaultAsync();
            return result;
        }
    }
}
