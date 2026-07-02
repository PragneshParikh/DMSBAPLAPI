using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ZoneMasterRepo
{
    public class ZoneMasterRepo : IZoneMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public ZoneMasterRepo(BapldmsvadContext context)
            => _context = context;

        // ── GET ALL ZONES ─────────────────────────────────────
        public async Task<IEnumerable<ZoneViewModel>> GetAllZonesAsync()
            => await _context.ZoneMasters
                .Where(z => z.IsActive == true)              // plain bool — no == true, no ?? null
                .GroupBy(z => z.Zone)
                .Select(g => new ZoneViewModel
                {
                    Id = g.First().Id,
                    Zone = g.Key,
                    IsActive = true                          // always true since we filtered above
                })
                .OrderBy(z => z.Zone)
                .ToListAsync();

        // ── GET DEALERS BY ZONE ───────────────────────────────
        public async Task<IEnumerable<ZoneDealerViewModel>> GetDealersByZoneAsync(string zone)
            => await (
                from zm in _context.ZoneMasters
                join dm in _context.DealerMasters
                     on zm.DealerId equals dm.Id
                join c in _context.Cities
                     on zm.CityId equals c.CityId into cg
                from c in cg.DefaultIfEmpty()
                join s in _context.States
                     on zm.StateId equals s.StateId into sg
                from s in sg.DefaultIfEmpty()
                where zm.Zone == zone && zm.IsActive == true                        // plain bool — no == true, no ?? null
                   && zm.DealerId != null
                select new ZoneDealerViewModel
                {
                    ZoneMasterId = zm.Id,
                    Zone = zm.Zone,
                    DealerId = dm.Id,
                    DealerName = dm.Compname,
                    DealerCode = dm.Dealercode,
                    City = dm.City,
                    State = dm.State,
                    CityName = c != null ? c.CityName : dm.City,
                    StateName = s != null ? s.StateName : dm.State
                }
            ).Distinct().OrderBy(d => d.DealerName).ToListAsync();

        // ── GET BY ID ─────────────────────────────────────────
        public async Task<ZoneMasterViewModel> GetZoneByIdAsync(int id)
        {
            var z = await _context.ZoneMasters.FindAsync(id);
            if (z == null) return null;
            return new ZoneMasterViewModel
            {
                Id = z.Id,
                Zone = z.Zone,
                IsActive = z.IsActive                       // plain bool — no ?? true
            };
        }

        // ── CREATE ────────────────────────────────────────────
        public async Task<ZoneMasterViewModel> CreateZoneAsync(ZoneMasterViewModel model)
        {
            var entity = new ZoneMaster
            {
                Zone = model.Zone,
                IsActive = model.IsActive
            };
            _context.ZoneMasters.Add(entity);
            await _context.SaveChangesAsync();
            model.Id = entity.Id;
            return model;
        }

        // ── UPDATE ────────────────────────────────────────────
        public async Task<bool> UpdateZoneAsync(ZoneMasterViewModel model)
        {
            var entity = await _context.ZoneMasters.FindAsync(model.Id);
            if (entity == null) return false;
            entity.Zone = model.Zone;
            entity.IsActive = model.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        // ── SOFT DELETE ───────────────────────────────────────
        public async Task<bool> DeleteZoneAsync(int id)
        {
            var entity = await _context.ZoneMasters.FindAsync(id);
            if (entity == null) return false;
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}