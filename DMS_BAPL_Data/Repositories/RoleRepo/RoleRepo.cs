using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.RoleRepo
{
    public class RoleRepo : IRoleRepo
    {
        private readonly BapldmsvadContext _context;
        public RoleRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AspNetRole>> GetRoles()
        {
            return await _context.AspNetRoles.ToListAsync();
        }

        public async Task<AspNetRole?> GetRoleById(string id)
        {
            return await _context.AspNetRoles.FindAsync(id).AsTask();
        }

    }
}
