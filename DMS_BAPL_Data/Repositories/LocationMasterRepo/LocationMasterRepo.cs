using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
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
                locationMasterViewModel.CreatedDate = item.CreatedDate;
                locationMasterViewModel.UpdateBy = item.UpdateBy;
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
            locationMasterViewModel.CreatedDate = item.CreatedDate;
            locationMasterViewModel.UpdateBy = item.UpdateBy;
            locationMasterViewModel.UpdatedDate = item.UpdatedDate;

            return locationMasterViewModel;
        }
        public async Task<bool> AddLocationMaster(LocationMasterViewModel model)
        {
            LocationMaster locationMaster = new LocationMaster();

            locationMaster.Action = model.Action;
            locationMaster.Loccode = model.Loccode;
            locationMaster.Locname = model.Locname;
            locationMaster.Locareaidno = model.Locareaidno;
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
            locationMaster.Formtype = model.Formtype;
            locationMaster.Dealercode = model.Dealercode;
            locationMaster.Lineno = model.Lineno;
            locationMaster.Rrglocationidno = model.Rrglocationidno;
            locationMaster.Active = model.Active;
            locationMaster.CreatedBy = model.CreatedBy;
            locationMaster.CreatedDate = DateTime.Now;

            await _context.LocationMasters.AddAsync(locationMaster);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateLocationMaster(LocationMasterViewModel model)
        {
            var location = await _context.LocationMasters.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (location == null)
                return false;

            location.Action = model.Action;
            location.Loccode = model.Loccode;
            location.Locname = model.Locname;
            location.Locareaidno = model.Locareaidno;
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
            location.Formtype = model.Formtype;
            location.Dealercode = model.Dealercode;
            location.Lineno = model.Lineno;
            location.Rrglocationidno = model.Rrglocationidno;
            location.Active = model.Active;
            location.UpdateBy = model.UpdateBy;
            location.UpdatedDate = DateTime.Now;

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

        public async Task<List<LocationTypewiseNameViewModel>> GetLocationNameTypewiseListAsync(string dealerCode)
        {
            var result = await _context.LocationMasters
                .Where(x => x.Dealercode == dealerCode)
                .Select(x => new LocationTypewiseNameViewModel
                {
                    locname = x.Locname,
                    locareadidNo = x.Locareaidno
                })
                .ToListAsync();

            return result;
        }

        public async Task<object> UpdateByLocationCode(string locCode, string userId, LocationMasterViewModel locationMasterViewModel)
        {
            var _existingLocation = await _context.LocationMasters
                .FirstOrDefaultAsync(x => x.Loccode == locCode);

            if (_existingLocation == null)
                return null;

            _existingLocation.Action = locationMasterViewModel.Action;
            _existingLocation.Loccode = locationMasterViewModel.Loccode;
            _existingLocation.Locname = locationMasterViewModel.Locname;
            _existingLocation.Locareaidno = locationMasterViewModel.Locareaidno;
            _existingLocation.Add1 = locationMasterViewModel.Add1;
            _existingLocation.Add2 = locationMasterViewModel.Add2;
            _existingLocation.State = locationMasterViewModel.State;
            _existingLocation.City = locationMasterViewModel.City;
            _existingLocation.Pincode = locationMasterViewModel.Pincode;
            _existingLocation.Gstinno = locationMasterViewModel.Gstinno;
            _existingLocation.Email = locationMasterViewModel.Email;
            _existingLocation.Mobileno = locationMasterViewModel.Mobileno;
            _existingLocation.Contpername1 = locationMasterViewModel.Contpername1;
            _existingLocation.Contpername2 = locationMasterViewModel.Contpername2;
            _existingLocation.Contpermob1 = locationMasterViewModel.Contpermob1;
            _existingLocation.Contpermob2 = locationMasterViewModel.Contpermob2;
            _existingLocation.Contperemail1 = locationMasterViewModel.Contperemail1;
            _existingLocation.Contperemail2 = locationMasterViewModel.Contperemail2;
            _existingLocation.Compid = locationMasterViewModel.Compid;
            _existingLocation.Acntidno = locationMasterViewModel.Acntidno;
            _existingLocation.Formtype = locationMasterViewModel.Formtype;
            _existingLocation.Dealercode = locationMasterViewModel.Dealercode;
            _existingLocation.Lineno = locationMasterViewModel.Lineno;
            _existingLocation.Rrglocationidno = locationMasterViewModel.Rrglocationidno;
            _existingLocation.Active = locationMasterViewModel.Active;
            _existingLocation.UpdateBy = userId;
            _existingLocation.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return _existingLocation;
        }

        public async Task<LocationMaster?> GetLocationByCode(string loccode)
        {
            return await _context.LocationMasters
                .FirstOrDefaultAsync(x => x.Loccode == loccode);
        }
    }
}
