using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.ChassisDetailRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ChassisDetailsRepo
{
    public partial class ChassisDetailRepo : IChassisDetailRepo
    {
        private readonly BapldmsvadContext _context;

        public ChassisDetailRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<bool> IChassisDetailRepo.InsertChassis(ChassisDetail chassisDetail)
        {
            var item = await _context.ItemMasters
                .AsNoTracking()
                .Where(x => x.Itemcode == chassisDetail.ItemCode)
                .FirstOrDefaultAsync();

            chassisDetail.ItemName = item.Itemname;

            _context.ChassisDetails
                .Add(chassisDetail);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
