using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.APITracking
{
    public class APITrackingRepo : IAPITrackingRepo
    {
        private readonly BapldmsvadContext _context;

        public APITrackingRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        Task<List<Apitracking>> IAPITrackingRepo.GetAPITracking()
        {
            return Task.FromResult(_context.Apitrackings.ToList());
        }

        async Task<List<Apitracking>> IAPITrackingRepo.GetFilterRecords(DateTime startDate, DateTime endDate, string endPoint, string status)
        {

            var query = _context.Apitrackings
                    .Where(a => a.Dateofhit >= startDate && a.Dateofhit <= endDate);

            if (!string.IsNullOrEmpty(endPoint))
            {
                query = query.Where(a => a.Endpoint == endPoint);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            return await query
                .OrderByDescending(a => a.Dateofhit)
                .ToListAsync();
        }
    }
}
