using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.BgRoleRepo
{
    public class BgRoleRepo : IBgRoleRepo
    {
        private readonly BapldmsvadContext _context;

        public BgRoleRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AspNetRole>> GetRoles()
        {
            // Same shared AspNetRoles table as the plain Role Master screen —
            // BG roles ARE real Identity roles too, just tracked in a
            // separate category-mapping table so they list independently.
            return await _context.AspNetRoles.ToListAsync();
        }

        public async Task<AspNetRole?> GetRoleById(string id)
        {
            return await _context.AspNetRoles.FindAsync(id);
        }

        public async Task AddRoleCategoryMapping(BgRoleCategoryMapping mapping)
        {
            _context.BgRoleCategoryMappings.Add(mapping);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BgRoleCategoryMapping>> GetMappingsByCategory(string category)
        {
            return await _context.BgRoleCategoryMappings
                .Where(m => m.Category == category)
                .ToListAsync();
        }

        public async Task<List<BgRoleCategoryMapping>> GetAllMappings()
        {
            return await _context.BgRoleCategoryMappings
                .OrderBy(m => m.Category)
                .ThenBy(m => m.RoleName)
                .ToListAsync();
        }

        public async Task<BgRoleCategoryMapping?> GetMappingById(int id)
        {
            return await _context.BgRoleCategoryMappings.FindAsync(id);
        }

        public async Task<bool> UpdateMappingNameAndCategory(int id, string roleName, string category)
        {
            var mapping = await _context.BgRoleCategoryMappings.FindAsync(id);
            if (mapping == null) return false;

            mapping.RoleName = roleName;
            mapping.Category = category;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteMapping(int id)
        {
            var mapping = await _context.BgRoleCategoryMappings.FindAsync(id);
            if (mapping == null) return false;

            _context.BgRoleCategoryMappings.Remove(mapping);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}