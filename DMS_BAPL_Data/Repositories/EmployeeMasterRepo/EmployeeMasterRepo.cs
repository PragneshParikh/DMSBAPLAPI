using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.EmployeeMasterRepo
{
    public partial class EmployeeMasterRepo : IEmployeeMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public EmployeeMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<EmployeeMaster>> IEmployeeMasterRepo.Get()
        {
            try
            {
                return await Task.FromResult(_context.EmployeeMasters.ToList());
            }
            catch { throw; }
        }

        async Task<EmployeeMaster?> IEmployeeMasterRepo.GetEmployeeById(int id)
        {
            try
            {
                return await _context.EmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        async Task<int> IEmployeeMasterRepo.CreateNewUser(EmployeeMaster employeeMaster)
        {
            try
            {
                _context.EmployeeMasters.Add(employeeMaster);
                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }
    }
}
