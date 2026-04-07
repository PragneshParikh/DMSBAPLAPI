using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.Form22MasterRepo
{
    public class Form22MasterRepo : IForm22MasterRepo
    {
        private readonly BapldmsvadContext _context;

        public Form22MasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<Form22MasterViewModel> IForm22MasterRepo.InsertForm22MasterAsync(Form22MasterViewModel form22MasterViewModel)
        {
            try
            {
                // Fetch OEM model from master table using FK
                //temporary cmment for testing purpose
                var oemModel = await _context.OemmodelMasters
                    .FirstOrDefaultAsync(o => o.Id == form22MasterViewModel.OemmodelId);

                if (oemModel == null)
                {
                    throw new Exception("Invalid OEM Model Id");
                }

                var form22masterValue = new Form22Master
                {

                    OemmodelId = oemModel.Id,
                    OemModelName = oemModel.ModelName,

                    SoundLevelHorn = form22MasterViewModel.SoundLevelHorn,
                    PassbyNoiseLevel = form22MasterViewModel.PassbyNoiseLevel,
                    ApprovalCertificateNo = form22MasterViewModel.ApprovalCertificateNo,
                    Isactive = form22MasterViewModel.IsActive,

                    CreatedBy = form22MasterViewModel.CreatedBy,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Form22Masters.Add(form22masterValue);
                await _context.SaveChangesAsync();

                return form22MasterViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while inserting Form22Master: {ex.Message}");
                throw;
            }
        }
        public async Task<List<Form22Master>> GetForm22MastersAsync(string? search)
        {
            IQueryable<Form22Master> query = _context.Form22Masters;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => EF.Functions.Like(f.OemModelName, $"%{search}%") ||
                EF.Functions.Like(f.SoundLevelHorn, $"%{search}%") ||
                EF.Functions.Like(f.PassbyNoiseLevel, $"%{search}%") ||
                EF.Functions.Like(f.ApprovalCertificateNo, $"%{search}%")
                //(search == "yes" && f.Isactive) ||
                //(search == "no" && !f.Isactive);
                );
            }
            query = query.OrderByDescending(f => f.CreatedDate);
            return await query.ToListAsync();
        }

        public async Task<List<OemModelViewModel>> GetOemmodelMastersList()
        {
            return await _context.OemmodelMasters
                .Where(o => o.IsActive)
                .Select(o => new OemModelViewModel
                {
                    OemmodelId = o.Id,
                    OemModelName = o.ModelName.Trim()
                })
                .ToListAsync();
        }
        public async Task<Form22Master> GetForm22MasterByIdAsync(int id)
        {
            return await _context.Form22Masters.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Form22Master> UpdateForm22MasterAsync(int id, Form22MasterViewModel form22MasterViewModel)
        {
            //particular one id find then update
            var existingForm22Master = await _context.Form22Masters
                                                     .FindAsync(form22MasterViewModel.Id);

            var oemModel = await _context.OemmodelMasters
                    .Where(o => o.Id == form22MasterViewModel.OemmodelId)
                    .Select(o => o.ModelName)
                    .FirstOrDefaultAsync();

            if (existingForm22Master == null)
            {
                throw new Exception("Form22Master not found");
            }

            // Update fields
            existingForm22Master.OemModelName = oemModel;
            existingForm22Master.OemmodelId = form22MasterViewModel.OemmodelId;
            existingForm22Master.SoundLevelHorn = form22MasterViewModel.SoundLevelHorn;
            existingForm22Master.PassbyNoiseLevel = form22MasterViewModel.PassbyNoiseLevel;
            existingForm22Master.ApprovalCertificateNo = form22MasterViewModel.ApprovalCertificateNo;
            existingForm22Master.Isactive = form22MasterViewModel.IsActive;

            existingForm22Master.UpdatedBy = form22MasterViewModel.UpdatedBy;
            existingForm22Master.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingForm22Master;
        }
    }
}
