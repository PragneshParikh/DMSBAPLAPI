using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ChassisBatteryDetailRepo
{
    public partial class ChassisBatteryDetailRepo : IChassisBatteryDetailRepo
    {
        private readonly BapldmsvadContext _context;

        public ChassisBatteryDetailRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<bool> IChassisBatteryDetailRepo.InsertBatteryDetail(ChassisBatteryDetail chassisBatteryDetail)
        {
            _context.ChassisBatteryDetails
                .Add(chassisBatteryDetail);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
