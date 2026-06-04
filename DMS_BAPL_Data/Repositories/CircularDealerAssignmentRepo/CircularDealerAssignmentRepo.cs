using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.CircularDealerAssignmentRepo
{
    public partial class CircularDealerAssignmentRepo : ICircularDealerAssignmentRepo
    {
        private readonly BapldmsvadContext _context;

        public CircularDealerAssignmentRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<object>> ICircularDealerAssignmentRepo.GetAssignmentByCircularId(int circularId)
        {
            var result = await (
                from d in _context.DealerMasters

                join cda in _context.CircularDealerAssignments
                    .Where(x => x.CircularId == circularId)
                    on d.Dealercode equals cda.DealerCode into assignments

                from a in assignments.DefaultIfEmpty()

                select new
                {
                    Id = a != null ? a.Id : 0,
                    DealerCode = d.Dealercode,
                    CircularId = a != null ? a.CircularId : circularId,
                    IsSelected = a != null,
                    CreatedBy = a != null ? a.CreatedBy : null,
                    CreatedDate = a != null ? a.CreatedDate : (DateTime?)null,

                    areaOfficeId = d.Areaofficeid,
                    dealerName = d.Compname
                }
            )
            .OrderByDescending(x => x.IsSelected)
            .ThenBy(x => x.dealerName)
            .ToListAsync();

            return result;
        }

        async Task<bool> ICircularDealerAssignmentRepo.AssignDealersToCircular(List<CircularDealerAssignment> circularDealerAssignment)
        {
            await _context.CircularDealerAssignments
                .AddRangeAsync(circularDealerAssignment);

            int result = await _context.SaveChangesAsync();

            return result > 0;
        }

        async Task<bool> ICircularDealerAssignmentRepo.DeleteDealersCircularPermission(List<CircularDealerAssignment> circularDealerAssignment)
        {
            _context.CircularDealerAssignments.RemoveRange(circularDealerAssignment);

            int result = await _context.SaveChangesAsync();

            return result > 0;
        }

    }
}
