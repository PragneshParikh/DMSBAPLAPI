using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.BgEmployeeMasterRepo;
using DMS_BAPL_Utils.ViewModels;
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

        public BgEmployeeMasterService(IBgEmployeeMasterRepo repo)
        {
            _repo = repo;
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
            try
            {
                var entity = MapToEntity(model);
                entity.CreatedBy = model.CreatedBy ?? "admin";
                entity.CreatedDate = DateTime.Now;
                entity.UpdatedBy = model.CreatedBy ?? "admin";
                entity.UpdatedDate = DateTime.Now;

                return await _repo.Create(entity);
            }
            catch { throw; }
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

        public async Task<BgEmployeeMaster?> GetByEmail(string email)
        {
            try
            {
                return await _repo.GetByEmail(email);
            }
            catch { throw; }
        }

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

