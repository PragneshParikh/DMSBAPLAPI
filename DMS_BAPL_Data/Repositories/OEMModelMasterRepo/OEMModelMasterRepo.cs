using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.OEMModelMasterRepo
{
    public class OEMModelMasterRepo : IOEMModelMasterRepo
    {

        private readonly BapldmsvadContext _context;
        public OEMModelMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        public async Task<List<OEMModelMasterViewModel>> GetAllOEMModels()
        {
            //var data = await _context.OemmodelMasters.ToListAsync();

            var data = await _context.OemmodelMasters
                                     .OrderByDescending(x => x.Id)
                                     .ToListAsync();

            List<OEMModelMasterViewModel> list = new List<OEMModelMasterViewModel>();

            foreach (var item in data)
            {
                OEMModelMasterViewModel oemViewModel = new OEMModelMasterViewModel();

                oemViewModel.Id = item.Id;
                oemViewModel.ModelName = item.ModelName;
                oemViewModel.ModelShortName = item.ModelShortName;
                oemViewModel.IsActive = item.IsActive;
                oemViewModel.CreatedBy = item.CreatedBy;
                oemViewModel.CreatedDate = item.CreatedDate;
                oemViewModel.UpdatedBy = item.UpdatedBy;
                oemViewModel.UpdatedDate = item.UpdatedDate;

                list.Add(oemViewModel);
            }

            return list;
        }
        public async Task<OEMModelMasterViewModel> GetOEMModelById(int id)
        {
            var item = await _context.OemmodelMasters
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return null;

            OEMModelMasterViewModel oemViewModel = new OEMModelMasterViewModel();

            oemViewModel.Id = item.Id;
            oemViewModel.ModelName = item.ModelName;
            oemViewModel.ModelShortName = item.ModelShortName;
            oemViewModel.IsActive = item.IsActive;
            oemViewModel.CreatedBy = item.CreatedBy;
            oemViewModel.CreatedDate = item.CreatedDate;
            oemViewModel.UpdatedBy = item.UpdatedBy;
            oemViewModel.UpdatedDate = item.UpdatedDate;

            return oemViewModel;
        }
        public async Task<bool> AddOEMModel(OEMModelMasterViewModel oemViewModel)
        {
            if (oemViewModel.Id == 0)
            {
                OemmodelMaster entity = new OemmodelMaster();

                entity.ModelName = oemViewModel.ModelName;
                entity.ModelShortName = oemViewModel.ModelShortName;
                entity.IsActive = oemViewModel.IsActive;
                entity.CreatedBy = oemViewModel.CreatedBy;
                entity.CreatedDate = DateTime.Now;

                _context.OemmodelMasters.Add(entity);
            }
            else
            {
                var entity = await _context.OemmodelMasters
                    .FirstOrDefaultAsync(x => x.Id == oemViewModel.Id);

                if (entity == null)
                    return false;

                entity.ModelName = oemViewModel.ModelName;
                entity.ModelShortName = oemViewModel.ModelShortName;
                entity.IsActive = oemViewModel.IsActive;
                entity.UpdatedBy = oemViewModel.UpdatedBy;
                entity.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeleteOEMModel(int id)
        {
            var data = await _context.OemmodelMasters
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
                return false;

            _context.OemmodelMasters.Remove(data);

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateOEMModel(OEMModelMasterViewModel oemViewModel)
        {
            var data = await _context.OemmodelMasters.FirstOrDefaultAsync(x => x.Id == oemViewModel.Id);

            if (data == null)
                return false;

            data.ModelName = oemViewModel.ModelName;
            data.ModelShortName = oemViewModel.ModelShortName;
            data.IsActive = oemViewModel.IsActive;
            data.UpdatedBy = oemViewModel.UpdatedBy;
            data.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<OemmodelMaster>> GetOEMModelByStatus(bool isActive)
        {
            try
            {
                return await _context.OemmodelMasters
                    .Where(x => x.IsActive == isActive)
                    .ToListAsync();
            }
            catch { throw; }
        }
    }
}
