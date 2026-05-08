using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.StockReportRepo
{
    public class StockReportRepo : IStockReportRepo
    {
        private readonly BapldmsvadContext _context;

        public StockReportRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<List<StockReportViewModel>> GetDealerWiseReportAsync()
        {
            try
            {
                var vehicleInwards = await _context.VehicleInwards
                    .AsNoTracking()
                    .Where(vi => vi.DealerCode != null)
                    .ToListAsync();

                var dealers = await _context.DealerMasters
                    .AsNoTracking()
                    .ToListAsync();

                var items = await _context.ItemMasters
                    .AsNoTracking()
                    .ToListAsync();

                var colors = await _context.ColorMasters
                    .AsNoTracking()
                    .ToListAsync();

                var result = (
                    from vi in vehicleInwards
                    join dm in dealers
                        on vi.DealerCode equals dm.Dealercode into dealerGroup
                    from dm in dealerGroup.DefaultIfEmpty()
                    join im in items
                        on vi.ItemCode equals im.Itemcode into itemGroup
                    from im in itemGroup.DefaultIfEmpty()
                    join cm in colors
                        on vi.ColrCode equals cm.Colorcode into colorGroup
                    from cm in colorGroup.DefaultIfEmpty()
                    group new { vi, dm, im, cm }
                        by new
                        {
                            DealerCode = vi.DealerCode ?? "Unknown",
                            DealerName = dm != null ? dm.Compname : (vi.DealerCode ?? "Unknown"),
                            Model = im != null ? (im.Oemmodelname ?? im.Itemcode ?? "Unknown")
                                                    : (vi.ItemCode ?? "Unknown"),
                            Colour = cm != null ? cm.Colorname
                                                    : (vi.ColrCode ?? "Unknown")
                        }
                    into g
                    orderby g.Key.DealerName, g.Key.Model, g.Key.Colour
                    select new StockReportViewModel
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        Model = g.Key.Model,
                        Colour = g.Key.Colour,
                        TotalQty = g.Count()
                    }
                ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetDealerWiseReportAsync: " + ex.Message, ex);
            }
        }

        public async Task<List<StockReportViewModel>> GetColourWiseReportAsync()
        {
            try
            {
                var vehicleInwards = await _context.VehicleInwards
                    .AsNoTracking()
                    .Where(vi => vi.DealerCode != null)
                    .ToListAsync();

                var dealers = await _context.DealerMasters
                    .AsNoTracking()
                    .ToListAsync();

                var items = await _context.ItemMasters
                    .AsNoTracking()
                    .ToListAsync();

                var colors = await _context.ColorMasters
                    .AsNoTracking()
                    .ToListAsync();

                var result = (
                    from vi in vehicleInwards
                    join dm in dealers
                        on vi.DealerCode equals dm.Dealercode into dealerGroup
                    from dm in dealerGroup.DefaultIfEmpty()
                    join im in items
                        on vi.ItemCode equals im.Itemcode into itemGroup
                    from im in itemGroup.DefaultIfEmpty()
                    join cm in colors
                        on vi.ColrCode equals cm.Colorcode into colorGroup
                    from cm in colorGroup.DefaultIfEmpty()
                    group new { vi, dm, im, cm }
                        by new
                        {
                            DealerCode = vi.DealerCode ?? "Unknown",
                            DealerName = dm != null ? dm.Compname : (vi.DealerCode ?? "Unknown"),
                            Model = im != null ? (im.Oemmodelname ?? im.Itemcode ?? "Unknown")
                                                    : (vi.ItemCode ?? "Unknown"),
                            Colour = cm != null ? cm.Colorname
                                                    : (vi.ColrCode ?? "Unknown")
                        }
                    into g
                    orderby g.Key.Colour, g.Key.DealerName, g.Key.Model
                    select new StockReportViewModel
                    {
                        DealerCode = g.Key.DealerCode,
                        DealerName = g.Key.DealerName,
                        Model = g.Key.Model,
                        Colour = g.Key.Colour,
                        TotalQty = g.Count()
                    }
                ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetColourWiseReportAsync: " + ex.Message, ex);
            }
        }
    }
}