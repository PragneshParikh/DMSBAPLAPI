using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.DepartmentRepo
{
    public partial class DepartmentRepo : IDepartmentRepo
    {
        private readonly BapldmsvadContext _context;

        public DepartmentRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<DepartmentMaster>> IDepartmentRepo.Get()
        {
            return await _context.DepartmentMasters
                .ToListAsync();
        }
        async Task<bool> IDepartmentRepo.Insert(DepartmentMaster departmentMaster)
        {
            await _context.DepartmentMasters.AddAsync(departmentMaster);
            await _context.SaveChangesAsync();

            return true;
        }
        async Task<int> IDepartmentRepo.Update(DepartmentMaster departmentMaster)
        {
            _context.DepartmentMasters.Update(departmentMaster);
            return await _context.SaveChangesAsync();
        }
    }
}
