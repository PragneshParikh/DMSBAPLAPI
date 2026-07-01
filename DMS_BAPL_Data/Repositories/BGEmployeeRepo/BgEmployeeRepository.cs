using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.BgEmployeeMasterRepo
{
    public partial class BgEmployeeMasterRepo : IBgEmployeeMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public BgEmployeeMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL
        // =====================================================

        async Task<IEnumerable<BgEmployeeMaster>> IBgEmployeeMasterRepo.Get()
        {
            try
            {
                return await Task.FromResult(_context.BgEmployeeMasters.ToList());
            }
            catch { throw; }
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        async Task<BgEmployeeMaster?> IBgEmployeeMasterRepo.GetById(int id)
        {
            try
            {
                return await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        // =====================================================
        // CREATE — plain insert, no duplicate-check here
        // =====================================================

        async Task<BgEmployeeMaster> IBgEmployeeMasterRepo.Create(BgEmployeeMaster bgEmployee)
        {
            try
            {
                bgEmployee.CreatedDate = DateTime.Now;
                bgEmployee.UpdatedDate = DateTime.Now;

                _context.BgEmployeeMasters.Add(bgEmployee);
                await _context.SaveChangesAsync();

                return bgEmployee;
            }
            catch { throw; }
        }

        // =====================================================
        // UPDATE
        // =====================================================

        async Task<int> IBgEmployeeMasterRepo.Update(BgEmployeeMaster bgEmployee)
        {
            try
            {
                var existing = await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == bgEmployee.Id);

                if (existing == null)
                    return 0;

                existing.FirstName = bgEmployee.FirstName;
                existing.LastName = bgEmployee.LastName;
                existing.Gender = bgEmployee.Gender;
                existing.Mobile = bgEmployee.Mobile;
                existing.State = bgEmployee.State;
                existing.City = bgEmployee.City;
                existing.Pincode = bgEmployee.Pincode;
                existing.DateOfBirth = bgEmployee.DateOfBirth;
                existing.DateOfJoin = bgEmployee.DateOfJoin;
                existing.EffectiveDate = bgEmployee.EffectiveDate;
                existing.ReportingTo = bgEmployee.ReportingTo;
                existing.IsActive = bgEmployee.IsActive;
                existing.Department = bgEmployee.Department;
                existing.ProfileId = bgEmployee.ProfileId;
                existing.EmailId = bgEmployee.EmailId;
                existing.MappedZones = bgEmployee.MappedZones;
                existing.MappedZoneIds = bgEmployee.MappedZoneIds;
                existing.ProfileImage = bgEmployee.ProfileImage;
                existing.DealerCode = bgEmployee.DealerCode;
                existing.LocationCode = bgEmployee.LocationCode;
                existing.UpdatedBy = "admin";
                existing.UpdatedDate = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(bgEmployee.Password))
                    existing.Password = bgEmployee.Password;

                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        // =====================================================
        // DELETE
        // =====================================================

        async Task<int> IBgEmployeeMasterRepo.Delete(int id)
        {
            try
            {
                var existing = await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existing == null)
                    return 0;

                _context.BgEmployeeMasters.Remove(existing);
                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        // =====================================================
        // GET BY EMAIL — string parameter, matches interface exactly
        // =====================================================

        async Task<BgEmployeeMaster?> IBgEmployeeMasterRepo.GetByEmail(string email)
        {
            try
            {
                var normalizedEmail = email?.Trim().ToLowerInvariant();

                return await _context.BgEmployeeMasters
                    .FirstOrDefaultAsync(x => x.EmailId.ToLower() == normalizedEmail);
            }
            catch { throw; }
        }
    }
}