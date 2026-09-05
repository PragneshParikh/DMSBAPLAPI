using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LocationMasterRepo
{
    public class LocationMasterRepo : ILocationMasterRepo
    {
        private readonly BapldmsvadContext _context;
        private const string DefaultFormType = "DL";
        public LocationMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        public async Task<List<LocationMasterViewModel>> GetAllLocationMaster()
        {
            var data = await _context.LocationMasters.ToListAsync();

            List<LocationMasterViewModel> list = new List<LocationMasterViewModel>();

            foreach (var item in data)
            {
                LocationMasterViewModel locationMasterViewModel = new LocationMasterViewModel();

                locationMasterViewModel.Id = item.Id;
                locationMasterViewModel.Action = item.Action;
                locationMasterViewModel.Loccode = item.Loccode;
                locationMasterViewModel.Locname = item.Locname;
                locationMasterViewModel.Locareaidno = item.Locareaidno;
                locationMasterViewModel.Add1 = item.Add1;
                locationMasterViewModel.Add2 = item.Add2;
                locationMasterViewModel.State = item.State;
                locationMasterViewModel.City = item.City;
                locationMasterViewModel.Pincode = item.Pincode;
                locationMasterViewModel.Gstinno = item.Gstinno;
                locationMasterViewModel.Email = item.Email;
                locationMasterViewModel.Mobileno = item.Mobileno;
                locationMasterViewModel.Contpername1 = item.Contpername1;
                locationMasterViewModel.Contpername2 = item.Contpername2;
                locationMasterViewModel.Contpermob1 = item.Contpermob1;
                locationMasterViewModel.Contpermob2 = item.Contpermob2;
                locationMasterViewModel.Contperemail1 = item.Contperemail1;
                locationMasterViewModel.Contperemail2 = item.Contperemail2;
                locationMasterViewModel.Compid = item.Compid;
                locationMasterViewModel.Acntidno = item.Acntidno;
                locationMasterViewModel.Formtype = item.Formtype;
                locationMasterViewModel.Dealercode = item.Dealercode;
                locationMasterViewModel.Lineno = item.Lineno;
                locationMasterViewModel.Rrglocationidno = item.Rrglocationidno;
                locationMasterViewModel.Active = item.Active;
                locationMasterViewModel.CreatedBy = item.CreatedBy;
                locationMasterViewModel.CreatedDate = item.CreatedDate ?? DateTime.Now;
                locationMasterViewModel.UpdateBy = item.UpdatedBy;
                locationMasterViewModel.UpdatedDate = item.UpdatedDate;

                list.Add(locationMasterViewModel);
            }

            return list;
        }
        public async Task<LocationMasterViewModel> GetLocationMasterById(int id)
        {
            var item = await _context.LocationMasters.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return null;

            LocationMasterViewModel locationMasterViewModel = new LocationMasterViewModel();

            locationMasterViewModel.Id = item.Id;
            locationMasterViewModel.Action = item.Action;
            locationMasterViewModel.Loccode = item.Loccode;
            locationMasterViewModel.Locname = item.Locname;
            locationMasterViewModel.Locareaidno = item.Locareaidno;
            locationMasterViewModel.Add1 = item.Add1;
            locationMasterViewModel.Add2 = item.Add2;
            locationMasterViewModel.State = item.State;
            locationMasterViewModel.City = item.City;
            locationMasterViewModel.Pincode = item.Pincode;
            locationMasterViewModel.Gstinno = item.Gstinno;
            locationMasterViewModel.Email = item.Email;
            locationMasterViewModel.Mobileno = item.Mobileno;
            locationMasterViewModel.Contpername1 = item.Contpername1;
            locationMasterViewModel.Contpername2 = item.Contpername2;
            locationMasterViewModel.Contpermob1 = item.Contpermob1;
            locationMasterViewModel.Contpermob2 = item.Contpermob2;
            locationMasterViewModel.Contperemail1 = item.Contperemail1;
            locationMasterViewModel.Contperemail2 = item.Contperemail2;
            locationMasterViewModel.Compid = item.Compid;
            locationMasterViewModel.Acntidno = item.Acntidno;
            locationMasterViewModel.Formtype = item.Formtype;
            locationMasterViewModel.Dealercode = item.Dealercode;
            locationMasterViewModel.Lineno = item.Lineno;
            locationMasterViewModel.Rrglocationidno = item.Rrglocationidno;
            locationMasterViewModel.Active = item.Active;
            locationMasterViewModel.CreatedBy = item.CreatedBy;
            locationMasterViewModel.CreatedDate = item.CreatedDate ?? DateTime.Now;
            locationMasterViewModel.UpdateBy = item.UpdatedBy;
            locationMasterViewModel.UpdatedDate = item.UpdatedDate;

            return locationMasterViewModel;
        }
        public async Task<bool> AddLocationMaster(LocationMasterViewModel model)
        {
            // ADDED — this method previously inserted unconditionally, even when a
            // location with the same code already existed. Any repeated call (a
            // double-click, or a sync job re-adding the same location) created a
            // second row, which is very likely how at least some of the existing
            // duplicates got in. Now checks first, case/whitespace-insensitively,
            // same comparison style as UpdateByLocationCode already uses.
            var normalizedCode = model.Loccode?.Trim().ToUpper();

            var existing = await _context.LocationMasters
                .FirstOrDefaultAsync(x => x.Loccode.ToUpper() == normalizedCode);

            if (existing != null)
            {
                // Already exists — update instead of creating a duplicate.
                existing.Action = model.Action;
                existing.Locname = model.Locname;
                existing.Locareaidno = ResolveAreaId(model.Locareaidno, model.Loccode);
                existing.Add1 = model.Add1;
                existing.Add2 = model.Add2;
                existing.State = model.State;
                existing.City = model.City;
                existing.Pincode = model.Pincode;
                existing.Gstinno = model.Gstinno;
                existing.Email = model.Email;
                existing.Mobileno = model.Mobileno;
                existing.Contpername1 = model.Contpername1;
                existing.Contpername2 = model.Contpername2;
                existing.Contpermob1 = model.Contpermob1;
                existing.Contpermob2 = model.Contpermob2;
                existing.Contperemail1 = model.Contperemail1;
                existing.Contperemail2 = model.Contperemail2;
                existing.Compid = model.Compid;
                existing.Acntidno = model.Acntidno;
                existing.Formtype = ResolveFormType(model.Formtype, existing.Formtype);
                existing.Dealercode = model.Dealercode;
                existing.Lineno = model.Lineno;
                existing.Rrglocationidno = model.Rrglocationidno;
                existing.Active = model.Active;
                existing.UpdatedBy = model.CreatedBy;
                existing.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }

            LocationMaster locationMaster = new LocationMaster();

            locationMaster.Action = model.Action;
            locationMaster.Loccode = model.Loccode;
            locationMaster.Locname = model.Locname;
            locationMaster.Locareaidno = ResolveAreaId(model.Locareaidno, model.Loccode);
            locationMaster.Add1 = model.Add1;
            locationMaster.Add2 = model.Add2;
            locationMaster.State = model.State;
            locationMaster.City = model.City;
            locationMaster.Pincode = model.Pincode;
            locationMaster.Gstinno = model.Gstinno;
            locationMaster.Email = model.Email;
            locationMaster.Mobileno = model.Mobileno;
            locationMaster.Contpername1 = model.Contpername1;
            locationMaster.Contpername2 = model.Contpername2;
            locationMaster.Contpermob1 = model.Contpermob1;
            locationMaster.Contpermob2 = model.Contpermob2;
            locationMaster.Contperemail1 = model.Contperemail1;
            locationMaster.Contperemail2 = model.Contperemail2;
            locationMaster.Compid = model.Compid;
            locationMaster.Acntidno = model.Acntidno;
            locationMaster.Formtype = ResolveFormType(model.Formtype);
            locationMaster.Dealercode = model.Dealercode;
            locationMaster.Lineno = model.Lineno;
            locationMaster.Rrglocationidno = model.Rrglocationidno;
            locationMaster.Active = model.Active;
            locationMaster.CreatedBy = model.CreatedBy ?? "CUS0345A";
            locationMaster.CreatedDate = DateTime.Now;

            await _context.LocationMasters.AddAsync(locationMaster);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateLocationMaster(LocationMasterViewModel model)
        {
            var normalizedCode = model.Loccode?.Trim().ToUpper();

            var location = await _context.LocationMasters.FirstOrDefaultAsync(x => x.Loccode.ToUpper() == normalizedCode);

            if (location == null)
            {
                var locationMaster = new LocationMaster
                {
                    Action = model.Action,
                    Loccode = model.Loccode,
                    Locname = model.Locname,
                    Locareaidno = ResolveAreaId(model.Locareaidno, model.Loccode),   // CHANGED
                    Add1 = model.Add1,
                    Add2 = model.Add2,
                    State = model.State,
                    City = model.City,
                    Pincode = model.Pincode,
                    Gstinno = model.Gstinno,
                    Email = model.Email,
                    Mobileno = model.Mobileno,
                    Contpername1 = model.Contpername1,
                    Contpername2 = model.Contpername2,
                    Contpermob1 = model.Contpermob1,
                    Contpermob2 = model.Contpermob2,
                    Contperemail1 = model.Contperemail1,
                    Contperemail2 = model.Contperemail2,
                    Compid = model.Compid,
                    Acntidno = model.Acntidno,
                    Formtype = ResolveFormType(model.Formtype),                     // CHANGED
                    Dealercode = model.Dealercode,
                    Lineno = model.Lineno,
                    Rrglocationidno = model.Rrglocationidno,
                    Active = model.Active,
                    CreatedBy = model.CreatedBy ?? "CUS0345A",
                    CreatedDate = DateTime.Now,
                };

                await _context.LocationMasters.AddAsync(locationMaster);
            }
            else
            {
                location.Action = model.Action;
                location.Loccode = model.Loccode;
                location.Locname = model.Locname;
                location.Locareaidno = ResolveAreaId(model.Locareaidno, model.Loccode);              // CHANGED
                location.Add1 = model.Add1;
                location.Add2 = model.Add2;
                location.State = model.State;
                location.City = model.City;
                location.Pincode = model.Pincode;
                location.Gstinno = model.Gstinno;
                location.Email = model.Email;
                location.Mobileno = model.Mobileno;
                location.Contpername1 = model.Contpername1;
                location.Contpername2 = model.Contpername2;
                location.Contpermob1 = model.Contpermob1;
                location.Contpermob2 = model.Contpermob2;
                location.Contperemail1 = model.Contperemail1;
                location.Contperemail2 = model.Contperemail2;
                location.Compid = model.Compid;
                location.Acntidno = model.Acntidno;
                location.Formtype = ResolveFormType(model.Formtype, location.Formtype);              // CHANGED
                location.Dealercode = model.Dealercode;
                location.Lineno = model.Lineno;
                location.Rrglocationidno = model.Rrglocationidno;
                location.Active = model.Active;
                location.UpdatedBy = model.UpdateBy;
                location.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<LocationNameViewModel>> GetLocationByDealerCode(string dealerCode)
        {
            try
            {
                var data = await _context.LocationMasters
                            .Where(x => x.Dealercode == dealerCode && x.Locareaidno == 1)
                            .ToListAsync();

                List<LocationNameViewModel> list = new List<LocationNameViewModel>();

                foreach (var item in data)
                {
                    LocationNameViewModel locationName = new LocationNameViewModel();

                    locationName.Loccode = item.Loccode;
                    locationName.Locname = item.Locname;

                    list.Add(locationName);
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching location names", ex);
            }
        }

        public async Task<List<LocationTypewiseNameViewModel>> GetLocationNameTypewiseListAsync(string? dealerCode)
        {
            Console.WriteLine($"DealerCode = '{dealerCode}'");

            var rawResult = await _context.LocationMasters
                .Where(x => x.Active == "Y" &&
                            (dealerCode == "null" || x.Dealercode == dealerCode))
                .Select(x => new LocationTypewiseNameViewModel
                {
                    locname = x.Locname,
                    locCode = x.Loccode,
                    locareadidNo = x.Locareaidno
                })
                .ToListAsync();

            // ADDED — de-duplicate by normalized loccode, same pattern already used
            // in GetAllLocationByDealerCode elsewhere in this file. Guards against
            // duplicate/near-duplicate rows (exact repeats from AddLocationMaster's
            // missing existence check, or casing/whitespace variants that slipped
            // past UpdateLocationMaster's exact-match comparison) so this dropdown
            // never shows the same location twice regardless of how the DB got there.
            var result = rawResult
                .GroupBy(x => (x.locCode ?? "").Trim().ToUpperInvariant())
                .Select(g => g.First())
                .OrderBy(x => x.locname)
                .ToList();

            return result;
        }

        public async Task<(LocationMaster Location, bool IsNew)> UpdateByLocationCode(string userId, LocationMasterViewModel locationMasterViewModel)
        {
            var locCode = locationMasterViewModel.Loccode?.Trim().ToUpper();

            var existingLocation = await _context.LocationMasters
                .FirstOrDefaultAsync(x => x.Loccode.ToUpper() == locCode);

            bool isNew = existingLocation == null;

            if (existingLocation == null)
            {
                existingLocation = new LocationMaster
                {
                    Action = locationMasterViewModel.Action,
                    Loccode = locationMasterViewModel.Loccode,
                    Locname = locationMasterViewModel.Locname,
                    Locareaidno = ResolveAreaId(locationMasterViewModel.Locareaidno, locationMasterViewModel.Loccode),  // CHANGED
                    Add1 = locationMasterViewModel.Add1,
                    Add2 = locationMasterViewModel.Add2,
                    State = locationMasterViewModel.State,
                    City = locationMasterViewModel.City,
                    Pincode = locationMasterViewModel.Pincode,
                    Gstinno = locationMasterViewModel.Gstinno,
                    Email = locationMasterViewModel.Email,
                    Mobileno = locationMasterViewModel.Mobileno,
                    Contpername1 = locationMasterViewModel.Contpername1,
                    Contpername2 = locationMasterViewModel.Contpername2,
                    Contpermob1 = locationMasterViewModel.Contpermob1,
                    Contpermob2 = locationMasterViewModel.Contpermob2,
                    Contperemail1 = locationMasterViewModel.Contperemail1,
                    Contperemail2 = locationMasterViewModel.Contperemail2,
                    Compid = locationMasterViewModel.Compid,
                    Acntidno = locationMasterViewModel.Acntidno,
                    Formtype = ResolveFormType(locationMasterViewModel.Formtype),                                       // CHANGED
                    Dealercode = locationMasterViewModel.Dealercode,
                    Lineno = locationMasterViewModel.Lineno,
                    Rrglocationidno = locationMasterViewModel.Rrglocationidno,
                    Active = locationMasterViewModel.Active,
                    CreatedBy = locationMasterViewModel.CreatedBy ?? userId ?? "CUS0345A",
                    CreatedDate = DateTime.Now,
                };

                await _context.LocationMasters.AddAsync(existingLocation);
            }
            else
            {
                existingLocation.Action = locationMasterViewModel.Action;
                existingLocation.Loccode = locationMasterViewModel.Loccode;
                existingLocation.Locname = locationMasterViewModel.Locname;
                existingLocation.Locareaidno = ResolveAreaId(locationMasterViewModel.Locareaidno, locationMasterViewModel.Loccode);   // CHANGED
                existingLocation.Add1 = locationMasterViewModel.Add1;
                existingLocation.Add2 = locationMasterViewModel.Add2;
                existingLocation.State = locationMasterViewModel.State;
                existingLocation.City = locationMasterViewModel.City;
                existingLocation.Pincode = locationMasterViewModel.Pincode;
                existingLocation.Gstinno = locationMasterViewModel.Gstinno;
                existingLocation.Email = locationMasterViewModel.Email;
                existingLocation.Mobileno = locationMasterViewModel.Mobileno;
                existingLocation.Contpername1 = locationMasterViewModel.Contpername1;
                existingLocation.Contpername2 = locationMasterViewModel.Contpername2;
                existingLocation.Contpermob1 = locationMasterViewModel.Contpermob1;
                existingLocation.Contpermob2 = locationMasterViewModel.Contpermob2;
                existingLocation.Contperemail1 = locationMasterViewModel.Contperemail1;
                existingLocation.Contperemail2 = locationMasterViewModel.Contperemail2;
                existingLocation.Compid = locationMasterViewModel.Compid;
                existingLocation.Acntidno = locationMasterViewModel.Acntidno;
                existingLocation.Formtype = ResolveFormType(locationMasterViewModel.Formtype, existingLocation.Formtype);            // CHANGED — no longer blanks Source on a partial ERP push
                existingLocation.Dealercode = locationMasterViewModel.Dealercode;
                existingLocation.Lineno = locationMasterViewModel.Lineno;
                existingLocation.Rrglocationidno = locationMasterViewModel.Rrglocationidno;
                existingLocation.Active = locationMasterViewModel.Active;
                existingLocation.UpdatedBy = userId;
                existingLocation.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return (existingLocation, isNew);
        }

        public async Task<LocationMaster?> GetLocationByCode(string loccode)
        {
            return await _context.LocationMasters
                .FirstOrDefaultAsync(x => x.Loccode == loccode);
        }

        private static int ResolveAreaId(int? providedAreaId, string? loccode)
        {
            if (providedAreaId is 1 or 2 or 3)
                return providedAreaId.Value;

            if (string.IsNullOrWhiteSpace(loccode))
                return providedAreaId ?? 0;

            var areaLetter = char.ToUpperInvariant(
                loccode.Trim().Reverse().FirstOrDefault(char.IsLetter));

            return areaLetter switch
            {
                'S' => 1, // Showroom
                'W' => 2, // Workshop
                'G' => 3, // Yard
                _ => providedAreaId ?? 0
            };
        }

        private static string ResolveFormType(string? providedFormType, string? existingFormType = null)
        {
            if (!string.IsNullOrWhiteSpace(providedFormType))
                return providedFormType;

            // Update: keep whatever was already there rather than blanking it
            // out just because this particular ERP call didn't include Source.
            if (!string.IsNullOrWhiteSpace(existingFormType))
                return existingFormType;

            // Insert with nothing provided at all.
            return DefaultFormType;
        }

        public async Task<IEnumerable<LocationNameViewModel>> GetLocationByDealerByAreaId(string? dealerCode, int areaId)
        {
            try
            {
                var data = await _context.LocationMasters
                    .Where(x => (dealerCode == null || x.Dealercode == dealerCode)
                             && x.Locareaidno == areaId)
                    .ToListAsync();

                List<LocationNameViewModel> list = new();

                foreach (var item in data)
                {
                    list.Add(new LocationNameViewModel
                    {
                        Loccode = item.Loccode,
                        Locname = item.Locname,
                        DealerCode = item.Dealercode
                    });
                }

                return list;
            }
            catch { throw; }
        }
        async Task<IEnumerable<object>> ILocationMasterRepo.GetDealerPrimaryLocationByAreaId(int areaId, string locCode, string? dealerCode)
        {
            var query = await _context.LocationMasters
                .Where(x => x.Locareaidno == areaId &&
                x.Loccode.EndsWith(locCode) &&
                (string.IsNullOrEmpty(dealerCode) || x.Dealercode == dealerCode))
                .ToListAsync();

            return query.Select(x => new LocationMasterViewModel
            {
                Id = x.Id,
                Loccode = x.Loccode,
                Locname = x.Locname,
                Locareaidno = x.Locareaidno,
                Dealercode = x.Dealercode,
                State = x.State
            });
        }

        public async Task<List<LocationNameViewModel>> GetAllLocationByDealerCode(string dealerCode)
        {
            var data = await _context.LocationMasters
                        .Where(x => x.Dealercode == dealerCode && x.Active == "Y")
                        .ToListAsync();

            return data
                .GroupBy(x => (x.Loccode ?? "").Trim().ToUpperInvariant())
                .Select(g => g.First())
                .Select(item => new LocationNameViewModel
                {
                    Loccode = item.Loccode,
                    Locname = item.Locname,
                    City = item.City
                })
                .OrderBy(x => x.Locname)
                .ToList();
        }
        public async Task<IEnumerable<LocationMasterViewModel>> GetLocationDropdownByDealerCode(string? dealerCode)
        {
            var query = _context.LocationMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dealerCode))
            {
                query = query.Where(x => x.Dealercode == dealerCode);
            }

            return await query
                .Select(x => new LocationMasterViewModel
                {
                    Id = x.Id,
                    Locname = x.Locname,
                    Dealercode = x.Dealercode,
                    Locareaidno = x.Locareaidno,
                    Loccode = x.Loccode
                })
                .ToListAsync();
        }

        public async Task<(string? RoleId, string? RoleName)> GetRoleByDealerAndLocationCodeAsync(string? dealerCode, string? locationCode)
        {
            if (string.IsNullOrWhiteSpace(dealerCode) || string.IsNullOrWhiteSpace(locationCode))
                return (null, null);

            var location = await _context.LocationMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.Dealercode == dealerCode &&
                    l.Loccode == locationCode);

            if (location == null)
                return (null, null);
            var mapping = await _context.BgRoleCategoryMappings
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.LocationId == location.Id);

            return (mapping?.RoleId, mapping?.RoleName);
        }

    }
}