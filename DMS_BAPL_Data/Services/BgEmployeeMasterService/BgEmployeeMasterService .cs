using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.BgEmployeeMasterRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.BgEmployeeMasterService
{
    public class BgEmployeeMasterService : IBgEmployeeMasterService
    {
        private readonly IBgEmployeeMasterRepo _repo;
        private readonly UserManager<ApplicationUser> _userManager;  // NEW


        public BgEmployeeMasterService(IBgEmployeeMasterRepo repo, UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<BgEmployeeMaster>> Get()
        {
            try
            {
                return await _repo.Get();
            }
            catch { throw; }
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<BgEmployeeMaster?> GetById(int id)
        {
            try
            {
                return await _repo.GetById(id);
            }
            catch { throw; }
        }

        // =====================================================
        // CREATE
        // =====================================================

        public async Task<BgEmployeeMaster> Create(BgEmployeeViewModel model)
        {
            model.EmailId = model.EmailId?.Trim().ToLowerInvariant();

            var existingEmployee = await _repo.GetByEmail(model.EmailId);
            if (existingEmployee != null)
                throw new InvalidOperationException("An employee with this email already exists.");

            var entity = MapToEntity(model);
            entity.CreatedBy = model.CreatedBy ?? "admin";
            entity.CreatedDate = DateTime.Now;
            entity.UpdatedBy = model.CreatedBy ?? "admin";
            entity.UpdatedDate = DateTime.Now;

            var savedEmployee = await _repo.Create(entity);

            // NEW — login-creation failures no longer 500 the whole request.
            // The employee row is already saved at this point; treat login setup
            // as best-effort and log instead of throwing.
            if (!string.IsNullOrWhiteSpace(savedEmployee.EmailId))
            {
                try
                {
                    var existingUser = await _userManager.FindByNameAsync(savedEmployee.EmailId);

                    if (existingUser == null)
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = savedEmployee.EmailId,
                            Email = savedEmployee.EmailId,
                            EmailConfirmed = true
                        };

                        var passwordToUse = !string.IsNullOrWhiteSpace(model.Password)
                            ? model.Password
                            : StringConstants.BgEmployeeDefaultPassword;

                        var userResult = await _userManager.CreateAsync(newUser, passwordToUse);

                        if (userResult.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(newUser, StringConstants.BgEmployeeText);
                        }
                        else
                        {
                            // log but don't throw — employee record is already saved successfully
                            Console.WriteLine($"Login creation failed for {savedEmployee.EmailId}: " +
                                string.Join(", ", userResult.Errors.Select(e => e.Description)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // same — log, don't fail the whole save
                    Console.WriteLine($"Login creation exception for {savedEmployee.EmailId}: {ex.Message}");
                }
            }

            return savedEmployee;
        }

        // =====================================================
        // UPDATE
        // =====================================================

        public async Task<int> Update(BgEmployeeViewModel model)
        {
            try
            {
                var entity = MapToEntity(model);
                entity.UpdatedBy = model.UpdatedBy ?? "admin";
                entity.UpdatedDate = DateTime.Now;

                return await _repo.Update(entity);
            }
            catch { throw; }
        }

        // =====================================================
        // DELETE
        // =====================================================

        public async Task<int> Delete(int id)
        {
            try
            {
                return await _repo.Delete(id);
            }
            catch { throw; }
        }

        // =====================================================
        // GET BY EMAIL
        // =====================================================

        //public async Task<BgEmployeeMaster?> GetByEmail(string email)
        //{
        //    try
        //    {
        //        return await _repo.GetByEmail(email);
        //    }
        //    catch { throw; }
        //}

        public Task<BgEmployeeMaster?> GetByEmail(string email) => _repo.GetByEmail(email);

        // =====================================================
        // PRIVATE — ViewModel → Entity
        // =====================================================

        private static BgEmployeeMaster MapToEntity(BgEmployeeViewModel model)
        {
            return new BgEmployeeMaster
            {
                Id = model.Id,
                EmployeeCode = model.EmployeeCode,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                Mobile = model.Mobile,
                State = model.State,
                City = model.City,
                Pincode = model.Pincode,
                DateOfBirth = model.DateOfBirth,
                DateOfJoin = model.DateOfJoin,
                EffectiveDate = model.EffectiveDate,
                ReportingTo = model.ReportingTo,
                IsActive = model.IsActive,
                Department = model.Department,
                ProfileId = model.ProfileId,
                EmailId = model.EmailId,
                Password = model.Password,
                //Zones = model.Zones,
                MappedZones = model.MappedZones,
                MappedZoneIds = model.MappedZoneIds,
                ProfileImage = model.ProfileImage,
                DealerCode = model.DealerCode,
                LocationCode = model.LocationCode,
                CreatedBy = model.CreatedBy,
                CreatedDate = model.CreatedDate,
                UpdatedBy = model.UpdatedBy,
                UpdatedDate = model.UpdatedDate,
            };
        }
    }
}

