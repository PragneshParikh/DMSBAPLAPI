using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.MenuMasterRepo;
using DMS_BAPL_Data.Repositories.RoleRepo;
using DMS_BAPL_Data.Repositories.RoleWiseMenuRightRepo;
using DMS_BAPL_Utils.Helpers;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.MenuMasterService
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepo _menuRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly BAPLdbIdentityContext _context;
        private readonly IRoleWiseMenuRightRepo _roleWiseMenuRightRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MenuService> _logger;
        private readonly BapldmsvadContext _dmsContext;

        public MenuService(IMenuRepo menuRepo, IRoleRepo roleRepo, IHttpContextAccessor httpContextAccessor, IRoleWiseMenuRightRepo roleWiseMenuRightRepo, BAPLdbIdentityContext context, ILogger<MenuService> logger, BapldmsvadContext dmsContext)
        {
            _logger = logger;
            _dmsContext = dmsContext;
            _menuRepo = menuRepo;
            _roleRepo = roleRepo;
            _roleWiseMenuRightRepo = roleWiseMenuRightRepo;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        //public async Task<List<MenuMasterViewModel>> GetMenuItems()
        //{
        //    try
        //    {
        //        var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);

        //        var menus = await _menuRepo.GetMenuItems();

        //        var userRoles = await _context.Set<IdentityUserRole<string>>()
        //            .Where(x => x.UserId == userId)
        //            .ToListAsync();

        //        var roleId = userRoles.Select(x => x.RoleId).FirstOrDefault();

        //        var roleRights = await _roleWiseMenuRightRepo.GetMenuRightByRoleId(roleId);

        //        var allowedSubMenuIds = roleRights
        //            .Where(x => x.Permission > 0)
        //            .Select(x => x.SubMenuId)
        //            .ToHashSet();

        //        var parentMenus = menus
        //            .Where(x => x.ParentMenuId == null)
        //            .OrderBy(x => x.MenuName)
        //            .ThenBy(x => x.SerialNo)
        //            .Select(menu =>
        //            {
        //                var children = menus
        //                .Where(x => x.ParentMenuId == menu.Id
        //                && roleRights.Any(r => r.SubMenuId == x.Id && r.Permission > 0))
        //                .OrderBy(x => x.SerialNo)
        //                .Select(child => new MenuMasterViewModel
        //                {
        //                    id = child.Id,
        //                    label = child.MenuName,
        //                    link = child.PathName,
        //                    parentId = child.ParentMenuId,
        //                    module = child.ModuleName
        //                })
        //                .ToList();

        //                var hasChildren = children.Any();

        //                var hasAccess = roleRights
        //                .Any(r => r.SubMenuId == menu.Id && r.Permission > 0);

        //                if (hasChildren)
        //                {
        //                    return new MenuMasterViewModel
        //                    {
        //                        id = menu.Id,
        //                        label = menu.MenuName,
        //                        icon = "ri-dashboard-2-line",
        //                        isCollapsed = true,
        //                        subItems = children
        //                    };
        //                }

        //                if (hasAccess)
        //                {
        //                    return new MenuMasterViewModel
        //                    {
        //                        id = menu.Id,
        //                        label = menu.MenuName,
        //                        link = menu.PathName,
        //                        icon = "ri-dashboard-2-line"
        //                    };
        //                }

        //                return null;
        //            })
        //            .Where(x => x != null)
        //            .ToList();

        //        var result = new List<MenuMasterViewModel>
        //        {
        //            new MenuMasterViewModel
        //            {
        //                id = 1000,
        //                label = "MENU",
        //                isTitle = true
        //            }
        //        };

        //        result.AddRange(parentMenus);

        //        return result;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        /// <summary>26.11.2026 - AspNetRoles ids of the roles mapped to the logged-in BG employee.</summary>
        private async Task<List<string>> GetBgEmployeeRoleIdsAsync()
        {
            try
            {
                var email = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrWhiteSpace(email)) return new List<string>();
                var e = email.Trim().ToLower();

                var roleNames = await (
                    from m in _dmsContext.BgEmployeeRoleMappings
                    join emp in _dmsContext.BgEmployeeMasters on m.BgEmployeeId equals emp.Id
                    where emp.IsActive && emp.EmailId != null && emp.EmailId.ToLower() == e
                    select m.RoleName
                ).Distinct().ToListAsync();
                if (roleNames.Count == 0) return new List<string>();

                var upper = roleNames.Select(r => r.Trim().ToUpperInvariant()).ToList();
                return await _context.Roles
                    .Where(r => r.NormalizedName != null && upper.Contains(r.NormalizedName))
                    .Select(r => r.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetMenuItems: could not read BG employee roles");
                return new List<string>();
            }
        }

        public async Task<List<MenuMasterViewModel>> GetMenuItems()
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);

                var menus = await _menuRepo.GetMenuItems();


                var roleIds = new List<string>();

                var locationRoleId = _httpContextAccessor.HttpContext?.User?.FindFirst("LocationRoleId")?.Value;
                if (!string.IsNullOrWhiteSpace(locationRoleId))
                {
                    roleIds.Add(locationRoleId);
                }

                var userRoles = await _context.Set<IdentityUserRole<string>>()
                    .Where(x => x.UserId == userId)
                    .ToListAsync();
                roleIds.AddRange(userRoles.Select(x => x.RoleId));

                // 26.11.2026 - BG employees: the roles ticked in BG Employee Master (Sales / Service roles,
                // stored in BgEmployeeRoleMapping) were never used for the sidebar - only the generic
                // "Employee" login role was. Add them, so menus granted to e.g. the "Service" role in
                // Role Menu Rights (Warranty Claim, ...) show up for that employee.
                roleIds.AddRange(await GetBgEmployeeRoleIdsAsync());
                roleIds = roleIds.Distinct().ToList();

                var roleRights = new List<RoleWiseMenuRight>();
                foreach (var rid in roleIds)
                {
                    var rights = await _roleWiseMenuRightRepo.GetMenuRightByRoleId(rid);
                    roleRights.AddRange(rights);
                }

                var allowedSubMenuIds = roleRights
                    .Where(x => x.Permission > 0)
                    .Select(x => x.SubMenuId)
                    .ToHashSet();

                // SuperAdmin / Admin see every menu regardless of RoleWiseMenuRights.
                // Checked from the validated JWT role claims AND from AspNetRoles
                // (by role id) so it works whichever the login flow populated.
                var adminRoleNames = new[] { "SUPERADMIN", "ADMIN" };
                var tokenRoles = _httpContextAccessor.HttpContext?.User?
                    .FindAll(System.Security.Claims.ClaimTypes.Role)
                    .Select(c => (c.Value ?? string.Empty).ToUpperInvariant())
                    .ToList() ?? new List<string>();
                var dbRoleNames = roleIds.Count == 0
                    ? new List<string>()
                    : await _context.Roles
                        .Where(r => roleIds.Contains(r.Id))
                        .Select(r => r.NormalizedName ?? r.Name ?? string.Empty)
                        .ToListAsync();
                var isAdmin = tokenRoles.Any(r => adminRoleNames.Contains(r))
                           || dbRoleNames.Any(r => adminRoleNames.Contains(r.ToUpperInvariant()));

                if (isAdmin)
                {
                    allowedSubMenuIds = menus.Select(m => m.Id).ToHashSet();
                }

                if (allowedSubMenuIds.Count == 0)
                {
                    _logger.LogWarning("GetMenuItems: no menu rights for UserId={UserId}. RolesFound={RoleCount} [{RoleIds}], RightsRows={RightsCount}",
                        userId, roleIds.Count, string.Join(",", roleIds), roleRights.Count);
                }

                var parentMenus = menus
                    .Where(x => x.ParentMenuId == null)
                    .OrderBy(x => x.MenuName)
                    .ThenBy(x => x.SerialNo)
                    .Select(menu =>
                    {
                        var children = menus
                        .Where(x => x.ParentMenuId == menu.Id
                        && allowedSubMenuIds.Contains(x.Id))
                        .OrderBy(x => x.SerialNo)
                        .Select(child => new MenuMasterViewModel
                        {
                            id = child.Id,
                            label = child.MenuName,
                            link = child.PathName,
                            parentId = child.ParentMenuId,
                            module = child.ModuleName
                        })
                        .ToList();

                        var hasChildren = children.Any();

                        var hasAccess = allowedSubMenuIds.Contains(menu.Id);

                        if (hasChildren)
                        {
                            return new MenuMasterViewModel
                            {
                                id = menu.Id,
                                label = menu.MenuName,
                                icon = "ri-dashboard-2-line",
                                isCollapsed = true,
                                subItems = children
                            };
                        }

                        if (hasAccess)
                        {
                            return new MenuMasterViewModel
                            {
                                id = menu.Id,
                                label = menu.MenuName,
                                link = menu.PathName,
                                icon = "ri-dashboard-2-line"
                            };
                        }

                        return null;
                    })
                    .Where(x => x != null)
                    .ToList();

                var result = new List<MenuMasterViewModel>
        {
            new MenuMasterViewModel
            {
                id = 1000,
                label = "MENU",
                isTitle = true
            }
        };

                result.AddRange(parentMenus);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}