using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DepartmentRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.DesignationRepo
{
    public partial class DesignationRepo : IDesignationRepo
    {
        private readonly BapldmsvadContext _context;

        public DesignationRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<DesignationMaster>> IDesignationRepo.Get()
        {
            return await _context.DesignationMasters
                .AsNoTracking()
                .ToListAsync();
        }
        async Task<bool> IDesignationRepo.Insert(DesignationMaster designationMaster)
        {
            await _context.DesignationMasters.AddAsync(designationMaster);
            await _context.SaveChangesAsync();

            return true;
        }
        async Task<int> IDesignationRepo.Update(DesignationMaster designationMaster)
        {
            _context.DesignationMasters.Update(designationMaster);
            return await _context.SaveChangesAsync();
        }
    }
}
