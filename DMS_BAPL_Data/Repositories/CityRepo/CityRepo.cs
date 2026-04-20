using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.CityRepo
{
    public partial class CityRepo : ICityRepo
    {
        private readonly BapldmsvadContext _context;

        public CityRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.ToListAsync();
        }
        public async Task<List<CityResponseViewModel>> GetAllCitiesWithStateAsync()
        {
            try
            {
                return await _context.Cities
                    .Include(c => c.State)
                    .Select(c => new CityResponseViewModel
                    {
                        CityId = c.CityId,
                        CityName = c.CityName,
                        StateId = c.StateId,
                        StateName = c.State.StateName,
                        IsMetro = c.IsMetro,
                        IsActive = c.IsActive,
                        TierLevel = c.TierLevel,
                        Abbreviation = c.Abbreviation,
                    }).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<CityResponseViewModel> GetCityByIdAsync(int id)
        {
            try
            {
                return await _context.Cities
                               .Include(c => c.State)
                               .Where(c => c.CityId == id)
                               .Select(c => new CityResponseViewModel
                               {
                                   CityId = c.CityId,
                                   CityName = c.CityName,
                                   StateId = c.StateId,
                                   StateName = c.State.StateName,
                                   IsActive = c.IsActive,
                                   TierLevel = c.TierLevel,
                                   Abbreviation = c.Abbreviation,
                                   IsMetro = c.IsMetro,
                               }).FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }
        public async Task<City> CreateCityAsync(CityCreateEditViewModel model, string userId)
        {
            var city = new City
            {
                CityName = model.CityName,
                StateId = model.StateId,
                IsMetro = model.IsMetro,
                TierLevel = model.TierLevel,
                Abbreviation = model.Abbreviation,
                IsActive = model.IsActive,
                CreatedBy = userId,
                Createddate = DateTime.UtcNow
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return city;
        }
        public async Task<bool> UpdateCityAsync(int id, CityCreateEditViewModel model, string userId)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null) return false;

            city.CityName = model.CityName;
            city.StateId = model.StateId;
            city.IsMetro = model.IsMetro;
            city.TierLevel = model.TierLevel;
            city.Abbreviation = model.Abbreviation;
            city.IsActive = model.IsActive;
            city.UpdatedBy = userId;
            city.Updateddate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

    }
}

