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

        public async Task AddRoleCategoryMapping(RoleCategoryMapping mapping)
        {
            _context.RoleCategoryMappings.Add(mapping);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RoleCategoryMapping>> GetMappingsByCategory(string category)
        {
            return await _context.RoleCategoryMappings
                .Where(m => m.Category == category)
                .ToListAsync();
        }

        public async Task<List<RoleCategoryMapping>> GetAllMappings()
        {
            return await _context.RoleCategoryMappings
                .OrderBy(m => m.Category)
                .ThenBy(m => m.RoleName)
                .ToListAsync();
        }

        public async Task<bool> DeleteMapping(int id)
        {
            var mapping = await _context.RoleCategoryMappings.FindAsync(id);
            if (mapping == null) return false;
            _context.RoleCategoryMappings.Remove(mapping);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
