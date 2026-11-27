using System;
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
        // 26.11.2026 - zone_master holds only a few dealer rows per zone, so the employee form
        // showed 8-10 dealers. Every DealerMaster dealer is now placed in a zone:
        //   1. "Zone Master" - the dealer has its own active row in zone_master (wins over 2.)
        //   2. "State"       - the dealer's state appears in that zone's zone_master rows
        //                      (if a state appears in several zones, the zone with most rows wins)
        // Dealers whose state is in no zone are reported by GetZoneDealerCountsAsync.
        public async Task<IEnumerable<ZoneDealerViewModel>> GetDealersByZoneAsync(string zone)
        {
            var (rows, _) = await BuildZoneDealersAsync();
            return rows
                .Where(r => string.Equals(r.Zone, (zone ?? "").Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.IsActive)
                .ThenBy(r => r.Source == "Zone Master" ? 0 : 1)
                .ThenBy(r => r.DealerName)
                .ToList();
        }

        public async Task<ZoneDealerCountResponse> GetZoneDealerCountsAsync()
        {
            var (rows, notInZone) = await BuildZoneDealersAsync();
            var dealers = await _context.DealerMasters.AsNoTracking().Select(d => d.IsActive).ToListAsync();

            return new ZoneDealerCountResponse
            {
                Zones = rows
                    .GroupBy(r => r.Zone)
                    .Select(g => new ZoneDealerCountViewModel
                    {
                        Zone = g.Key,
                        Mapped = g.Count(r => r.Source == "Zone Master"),
                        ByState = g.Count(r => r.Source == "State"),
                        Total = g.Count(),
                        Active = g.Count(r => r.IsActive)
                    })
                    .OrderBy(z => z.Zone)
                    .ToList(),
                TotalDealers = dealers.Count,
                ActiveDealers = dealers.Count(a => a),
                NotInZone = notInZone.Count,
                NotInZoneActive = notInZone.Count(d => d.IsActive),
                NotInZoneStates = notInZone
                    .Select(d => string.IsNullOrWhiteSpace(d.StateName) ? "(no state)" : d.StateName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList()
            };
        }

        private async Task<(List<ZoneDealerViewModel> Rows, List<ZoneDealerViewModel> NotInZone)> BuildZoneDealersAsync()
        {
            var zoneRows = await _context.ZoneMasters.AsNoTracking()
                .Where(z => z.IsActive && z.Zone != null && z.Zone != "")
                .Select(z => new { z.Id, Zone = z.Zone!.Trim(), z.DealerId, z.StateId, z.CityId })
                .ToListAsync();
            var states = await _context.States.AsNoTracking().Select(s => new { s.StateId, s.StateName }).ToListAsync();
            var cities = await _context.Cities.AsNoTracking().Select(c => new { c.CityId, c.CityName, c.StateId }).ToListAsync();
            var dealers = await _context.DealerMasters.AsNoTracking()
                .Select(d => new { d.Id, d.Compname, d.Dealercode, d.City, d.State, d.IsActive })
                .ToListAsync();

            static string Key(string? v) => (v ?? "").Trim().ToUpperInvariant();
            var stateById = states.ToDictionary(s => s.StateId, s => s.StateName);
            var stateByName = states.GroupBy(s => Key(s.StateName)).ToDictionary(g => g.Key, g => g.First().StateId);
            var cityById = cities.ToDictionary(c => c.CityId);
            var cityByName = cities.GroupBy(c => Key(c.CityName)).ToDictionary(g => g.Key, g => g.First());

            int? StateIdOf(string? state, string? city)
            {
                if (int.TryParse((state ?? "").Trim(), out var sid) && stateById.ContainsKey(sid)) return sid;
                if (stateByName.TryGetValue(Key(state), out var byName)) return byName;
                if (int.TryParse((city ?? "").Trim(), out var cid) && cityById.TryGetValue(cid, out var c1)) return c1.StateId;
                if (cityByName.TryGetValue(Key(city), out var c2)) return c2.StateId;
                return null;
            }
            string CityNameOf(string? city) =>
                int.TryParse((city ?? "").Trim(), out var cid) && cityById.TryGetValue(cid, out var c) ? c.CityName : (city ?? "");

            // state -> zone, from every zone_master row that has a state (or a city)
            var stateZone = zoneRows
                .Select(z => new { z.Zone, StateId = z.StateId ?? (z.CityId.HasValue && cityById.TryGetValue(z.CityId.Value, out var zc) ? zc.StateId : (int?)null) })
                .Where(x => x.StateId.HasValue)
                .GroupBy(x => x.StateId!.Value)
                .ToDictionary(g => g.Key, g => g.GroupBy(x => x.Zone).OrderByDescending(z => z.Count()).First().Key);

            var directByDealer = zoneRows.Where(z => z.DealerId.HasValue)
                .GroupBy(z => z.DealerId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(z => (z.Zone, z.Id)).Distinct().ToList());

            var rows = new List<ZoneDealerViewModel>();
            var notInZone = new List<ZoneDealerViewModel>();

            foreach (var d in dealers)
            {
                var sid = StateIdOf(d.State, d.City);
                var row = new ZoneDealerViewModel
                {
                    DealerId = d.Id,
                    DealerName = d.Compname,
                    DealerCode = d.Dealercode,
                    City = d.City,
                    State = d.State,
                    CityName = CityNameOf(d.City),
                    StateName = sid.HasValue && stateById.TryGetValue(sid.Value, out var sn) ? sn : d.State,
                    IsActive = d.IsActive
                };

                if (directByDealer.TryGetValue(d.Id, out var direct))
                {
                    foreach (var (zone, zmId) in direct.GroupBy(x => x.Zone, StringComparer.OrdinalIgnoreCase).Select(g => g.First()))
                        rows.Add(Clone(row, zone, zmId, "Zone Master"));
                }
                else if (sid.HasValue && stateZone.TryGetValue(sid.Value, out var zoneOfState))
                {
                    rows.Add(Clone(row, zoneOfState, 0, "State"));
                }
                else
                {
                    notInZone.Add(row);
                }
            }

            return (rows, notInZone);
        }

        private static ZoneDealerViewModel Clone(ZoneDealerViewModel r, string zone, int zoneMasterId, string source) => new()
        {
            ZoneMasterId = zoneMasterId,
            Zone = zone,
            DealerId = r.DealerId,
            DealerName = r.DealerName,
            DealerCode = r.DealerCode,
            City = r.City,
            State = r.State,
            CityName = r.CityName,
            StateName = r.StateName,
            IsActive = r.IsActive,
            Source = source
        };

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