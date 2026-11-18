using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.LineOrderReportRepo
{
    public partial class LineOrderReportRepo : ILineOrderReportRepo
    {
        private readonly BapldmsvadContext _context;

        //private static readonly string[] LabourTypes = { "Labour", "Outside Work" };
        //private static readonly string[] PartTypes = { "Parts", "Part" };
        private static readonly string[] LabourTypes = { "Labour", "Outside Work" };
        private static readonly string[] PartTypes = { "Parts", "Part", "Accessory", "Battery/E-Device" };

        public LineOrderReportRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        private IQueryable<DmsLineOrderReport> BuildQuery(LineOrderReportFilterModel filter)
        {
            var query = _context.DmsLineOrderReports.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.DealerCode == filter.DealerCode);

            if (filter.FromDate.HasValue)
            {
                var from = DateOnly.FromDateTime(filter.FromDate.Value.Date);
                query = query.Where(x => x.DocDate >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = DateOnly.FromDateTime(filter.ToDate.Value.Date);
                query = query.Where(x => x.DocDate <= to);
            }

            //query = string.Equals(filter.LineType, "Part", StringComparison.OrdinalIgnoreCase)
            //    ? query.Where(x => PartTypes.Contains(x.ItemType))
            //    : query.Where(x => LabourTypes.Contains(x.ItemType));

           
            if (string.Equals(filter.LineType, "Part", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => PartTypes.Contains(x.ItemType));
            }
            else if (string.Equals(filter.LineType, "Labour", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => LabourTypes.Contains(x.ItemType));
            }
           

            return query;
        }

        async Task<PagedResponse<object>> ILineOrderReportRepo.GetPagedAsync(LineOrderReportFilterModel filter)
        {
            var query = BuildQuery(filter);

            int totalRecords = await query.CountAsync();

            var result = await query
               // .OrderByDescending(x => x.DocDate)
                .OrderBy(x => x.DocDate)
                .ThenBy(x => x.DocNo)
                .ThenBy(x => x.Id)
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResponse<object>
            {
                Data = result.Cast<object>().ToList(),
                TotalRecords = totalRecords
            };
        }

        async Task<List<DmsLineOrderReport>> ILineOrderReportRepo.GetAllAsync(LineOrderReportFilterModel filter)
        {
            return await BuildQuery(filter)
                //.OrderByDescending(x => x.DocDate)
                .OrderBy(x => x.DocDate)
                .ThenBy(x => x.DocNo)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }
    }
}