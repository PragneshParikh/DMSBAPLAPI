using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.MenuMasterAdminService
{
    // ---------------------------------------------------------------- view models
    /// <summary>One MenuMaster row as the Menu Master screen shows it.</summary>
    public class MenuAdminRowVM
    {
        public int Id { get; set; }
        public string MenuName { get; set; } = "";
        public int? ParentMenuId { get; set; }
        public string? PathName { get; set; }
        public int? SerialNo { get; set; }
        public string? ModuleName { get; set; }
        /// <summary>Roles that can open this page (permission &gt; 0). Sub-menus only.</summary>
        public int RoleCount { get; set; }
        /// <summary>Sub-menus under this menu. Top-level menus only.</summary>
        public int ChildCount { get; set; }
    }

    /// <summary>Create / update request. ParentMenuId null = top-level menu (a sidebar group such as "Reports").</summary>
    public class MenuAdminSaveVM
    {
        public string MenuName { get; set; } = "";
        public int? ParentMenuId { get; set; }
        public string? PathName { get; set; }
        public int? SerialNo { get; set; }
        public string? ModuleName { get; set; }
        /// <summary>Create only: roles that get this sub-menu straight away.</summary>
        public List<MenuAdminRoleVM>? Roles { get; set; }
    }

    /// <summary>A role and its permission on one sub-menu (bitmask View=1, Create=2, Edit=4, Delete=8, Print=16; 0 = no access).</summary>
    public class MenuAdminRoleVM
    {
        public string RoleId { get; set; } = "";
        public string? RoleName { get; set; }
        public int Permission { get; set; }
    }

    public interface IMenuMasterAdminService
    {
        Task<List<MenuAdminRowVM>> GetAllAsync();
        Task<MenuAdminRowVM> CreateAsync(MenuAdminSaveVM model, string userId);
        Task<MenuAdminRowVM> UpdateAsync(int id, MenuAdminSaveVM model, string userId);
        Task DeleteAsync(int id);
        Task<List<MenuAdminRoleVM>> GetRolesAsync(int subMenuId);
        Task SaveRolesAsync(int subMenuId, List<MenuAdminRoleVM> roles, string userId);
    }

    /// <summary>
    /// Menu Master screen: create / edit / delete MenuMaster rows and give a sub-menu to roles
    /// (RoleWiseMenuRights) - the same steps the menu SQL scripts did by hand.
    /// Validation problems throw ArgumentException (controller returns 400 with the message).
    /// </summary>
    public class MenuMasterAdminService : IMenuMasterAdminService
    {
        private const int AllRights = 31;
        private static readonly string[] Modules = { "showroom", "workshop", "account" };
        private readonly BapldmsvadContext _context;

        public MenuMasterAdminService(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<List<MenuAdminRowVM>> GetAllAsync()
        {
            var menus = await _context.MenuMasters.AsNoTracking().ToListAsync();
            var roleCounts = await _context.RoleWiseMenuRights.AsNoTracking()
                .Where(r => r.Permission > 0)
                .GroupBy(r => r.SubMenuId)
                .Select(g => new { g.Key, Count = g.Select(x => x.RoleId).Distinct().Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
            var childCounts = menus.Where(m => m.ParentMenuId != null)
                .GroupBy(m => m.ParentMenuId!.Value).ToDictionary(g => g.Key, g => g.Count());

            return menus
                .OrderBy(m => m.ParentMenuId.HasValue).ThenBy(m => m.SerialNo ?? int.MaxValue).ThenBy(m => m.MenuName)
                .Select(m => new MenuAdminRowVM
                {
                    Id = m.Id,
                    MenuName = m.MenuName,
                    ParentMenuId = m.ParentMenuId,
                    PathName = m.PathName,
                    SerialNo = m.SerialNo,
                    ModuleName = m.ModuleName,
                    RoleCount = roleCounts.TryGetValue(m.Id, out var rc) ? rc : 0,
                    ChildCount = childCounts.TryGetValue(m.Id, out var cc) ? cc : 0
                }).ToList();
        }

        public async Task<MenuAdminRowVM> CreateAsync(MenuAdminSaveVM model, string userId)
        {
            var clean = await ValidateAsync(model, null);
            var entity = new MenuMaster
            {
                MenuName = clean.MenuName,
                ParentMenuId = clean.ParentMenuId,
                PathName = clean.PathName,
                ModuleName = clean.ModuleName,
                SerialNo = clean.SerialNo ?? await NextSerialAsync(clean.ParentMenuId),
                CreatedBy = Cut(userId, 100) ?? "Admin",
                CreatedDate = DateTime.Now
            };

            await using var tx = await _context.Database.BeginTransactionAsync();
            _context.MenuMasters.Add(entity);
            await _context.SaveChangesAsync();

            if (entity.ParentMenuId != null && model.Roles != null && model.Roles.Count > 0)
                await ApplyRolesAsync(entity, model.Roles, userId);

            await tx.CommitAsync();
            return (await GetAllAsync()).First(r => r.Id == entity.Id);
        }

        public async Task<MenuAdminRowVM> UpdateAsync(int id, MenuAdminSaveVM model, string userId)
        {
            var entity = await _context.MenuMasters.FirstOrDefaultAsync(m => m.Id == id)
                         ?? throw new ArgumentException($"Menu {id} was not found.");
            var clean = await ValidateAsync(model, entity);

            var parentChanged = entity.ParentMenuId != clean.ParentMenuId;
            entity.MenuName = clean.MenuName;
            entity.ParentMenuId = clean.ParentMenuId;
            entity.PathName = clean.PathName;
            entity.ModuleName = clean.ModuleName;
            entity.SerialNo = clean.SerialNo ?? entity.SerialNo ?? await NextSerialAsync(clean.ParentMenuId);
            entity.UpdatedBy = Cut(userId, 100);
            entity.UpdatedDate = DateTime.Now;

            await using var tx = await _context.Database.BeginTransactionAsync();
            await _context.SaveChangesAsync();
            if (parentChanged && entity.ParentMenuId != null)
            {
                // rights rows carry the parent id: keep them under the new parent
                var rows = await _context.RoleWiseMenuRights.Where(r => r.SubMenuId == id).ToListAsync();
                foreach (var r in rows) { r.MenuId = entity.ParentMenuId.Value; r.UpdatedBy = Cut(userId, 100); r.UpdatedDate = DateTime.Now; }
                await _context.SaveChangesAsync();
            }
            await tx.CommitAsync();
            return (await GetAllAsync()).First(r => r.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.MenuMasters.FirstOrDefaultAsync(m => m.Id == id)
                         ?? throw new ArgumentException($"Menu {id} was not found.");
            var children = await _context.MenuMasters.CountAsync(m => m.ParentMenuId == id);
            if (children > 0)
                throw new ArgumentException($"\"{entity.MenuName}\" still has {children} sub-menu(s). Move or delete them first.");

            await using var tx = await _context.Database.BeginTransactionAsync();
            var rights = await _context.RoleWiseMenuRights.Where(r => r.SubMenuId == id || r.MenuId == id).ToListAsync();
            _context.RoleWiseMenuRights.RemoveRange(rights);
            _context.MenuMasters.Remove(entity);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task<List<MenuAdminRoleVM>> GetRolesAsync(int subMenuId)
        {
            await RequireSubMenuAsync(subMenuId);
            var roles = await _context.AspNetRoles.AsNoTracking().Select(r => new { r.Id, r.Name }).ToListAsync();
            var rights = await _context.RoleWiseMenuRights.AsNoTracking()
                .Where(r => r.SubMenuId == subMenuId)
                .GroupBy(r => r.RoleId)
                .Select(g => new { RoleId = g.Key, Permission = g.Max(x => x.Permission) })
                .ToDictionaryAsync(x => x.RoleId, x => x.Permission);

            return roles
                .Select(r => new MenuAdminRoleVM { RoleId = r.Id, RoleName = r.Name, Permission = rights.TryGetValue(r.Id, out var p) ? p : 0 })
                .OrderByDescending(r => r.Permission > 0).ThenBy(r => r.RoleName)
                .ToList();
        }

        public async Task SaveRolesAsync(int subMenuId, List<MenuAdminRoleVM> roles, string userId)
        {
            var sub = await RequireSubMenuAsync(subMenuId);
            await using var tx = await _context.Database.BeginTransactionAsync();
            await ApplyRolesAsync(sub, roles ?? new List<MenuAdminRoleVM>(), userId);
            await tx.CommitAsync();
        }

        // ---------------------------------------------------------------- helpers
        /// <summary>Inserts / updates / removes RoleWiseMenuRights rows for one sub-menu (one row per role).</summary>
        private async Task ApplyRolesAsync(MenuMaster sub, List<MenuAdminRoleVM> roles, string userId)
        {
            var parentId = sub.ParentMenuId ?? throw new ArgumentException("Rights can only be given on a sub-menu (a page), not on a top-level menu.");
            var known = (await _context.AspNetRoles.AsNoTracking().Select(r => r.Id).ToListAsync()).ToHashSet();
            var existing = await _context.RoleWiseMenuRights.Where(r => r.SubMenuId == sub.Id).ToListAsync();
            var by = Cut(userId, 100) ?? "Admin";

            foreach (var role in roles.Where(r => !string.IsNullOrWhiteSpace(r.RoleId)).GroupBy(r => r.RoleId).Select(g => g.Last()))
            {
                if (!known.Contains(role.RoleId)) throw new ArgumentException($"Role {role.RoleId} was not found.");
                var permission = Math.Clamp(role.Permission, 0, AllRights);
                if (permission > 0) permission |= 1;   // any right implies View
                var rows = existing.Where(r => r.RoleId == role.RoleId).ToList();

                if (permission == 0)
                {
                    _context.RoleWiseMenuRights.RemoveRange(rows);
                    continue;
                }
                if (rows.Count == 0)
                {
                    _context.RoleWiseMenuRights.Add(new RoleWiseMenuRight
                    {
                        RoleId = role.RoleId, MenuId = parentId, SubMenuId = sub.Id, Permission = permission,
                        CreatedBy = by, CreatedDate = DateTime.Now
                    });
                    continue;
                }
                var keep = rows[0];
                keep.MenuId = parentId;
                keep.Permission = permission;
                keep.UpdatedBy = by;
                keep.UpdatedDate = DateTime.Now;
                _context.RoleWiseMenuRights.RemoveRange(rows.Skip(1));   // duplicates
            }
            await _context.SaveChangesAsync();
        }

        private async Task<MenuMaster> RequireSubMenuAsync(int id)
        {
            var m = await _context.MenuMasters.FirstOrDefaultAsync(x => x.Id == id)
                    ?? throw new ArgumentException($"Menu {id} was not found.");
            if (m.ParentMenuId == null) throw new ArgumentException($"\"{m.MenuName}\" is a top-level menu. Rights are given on its sub-menus (pages).");
            return m;
        }

        private async Task<int> NextSerialAsync(int? parentId) =>
            (await _context.MenuMasters.Where(m => m.ParentMenuId == parentId).MaxAsync(m => (int?)m.SerialNo) ?? 0) + 1;

        private async Task<MenuAdminSaveVM> ValidateAsync(MenuAdminSaveVM model, MenuMaster? current)
        {
            if (model == null) throw new ArgumentException("Nothing to save.");
            var name = (model.MenuName ?? "").Trim();
            if (name.Length == 0) throw new ArgumentException("Menu name is required.");
            if (name.Length > 100) throw new ArgumentException("Menu name can be at most 100 characters.");

            var module = (model.ModuleName ?? "").Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
            if (module == "accounts") module = "account";
            if (module.Length > 0 && !Modules.Contains(module))
                throw new ArgumentException("Module must be ShowRoom, WorkShop, Account or blank (all modules).");

            var path = (model.PathName ?? "").Trim();
            if (path.Length > 0 && !path.StartsWith("/")) path = "/" + path;

            int? parentId = model.ParentMenuId is > 0 ? model.ParentMenuId : null;
            if (parentId != null)
            {
                if (current != null && parentId == current.Id) throw new ArgumentException("A menu cannot be its own parent.");
                var parent = await _context.MenuMasters.AsNoTracking().FirstOrDefaultAsync(m => m.Id == parentId)
                             ?? throw new ArgumentException($"Parent menu {parentId} was not found.");
                if (parent.ParentMenuId != null) throw new ArgumentException($"\"{parent.MenuName}\" is itself a sub-menu. Choose a top-level menu as parent.");
                if (current != null && await _context.MenuMasters.AnyAsync(m => m.ParentMenuId == current.Id))
                    throw new ArgumentException($"\"{current.MenuName}\" has sub-menus, so it must stay a top-level menu.");
                if (path.Length == 0) throw new ArgumentException("Page path is required for a sub-menu (e.g. /vehicle-sale-report).");
            }
            else path = "";   // top-level menus are groups, not pages

            var currentId = current?.Id ?? 0;
            var sameName = await _context.MenuMasters.AsNoTracking()
                .Where(m => m.Id != currentId && m.ParentMenuId == parentId)
                .Select(m => new { m.MenuName, m.ModuleName }).ToListAsync();
            if (sameName.Any(m => string.Equals(m.MenuName.Trim(), name, StringComparison.OrdinalIgnoreCase)
                                  && string.Equals((m.ModuleName ?? "").Trim(), module, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"\"{name}\" already exists under this parent for the same module.");

            if (path.Length > 0)
            {
                var samePath = await _context.MenuMasters.AsNoTracking()
                    .Where(m => m.Id != currentId && m.PathName == path)
                    .Select(m => new { m.Id, m.MenuName, m.ModuleName }).ToListAsync();
                var clash = samePath.FirstOrDefault(m => string.Equals((m.ModuleName ?? "").Trim(), module, StringComparison.OrdinalIgnoreCase));
                if (clash != null) throw new ArgumentException($"Path {path} is already used by \"{clash.MenuName}\" (id {clash.Id}) in the same module.");
            }
            if (model.SerialNo is < 0) throw new ArgumentException("Serial no cannot be negative.");

            return new MenuAdminSaveVM
            {
                MenuName = name,
                ParentMenuId = parentId,
                PathName = path.Length > 0 ? Cut(path, 200) : null,
                ModuleName = module.Length > 0 ? module : null,
                SerialNo = model.SerialNo
            };
        }

        private static string? Cut(string? s, int max) =>
            string.IsNullOrWhiteSpace(s) ? null : (s.Trim().Length <= max ? s.Trim() : s.Trim()[..max]);
    }
}
