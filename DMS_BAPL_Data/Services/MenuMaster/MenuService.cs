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

        public MenuService(IMenuRepo menuRepo, IRoleRepo roleRepo, IHttpContextAccessor httpContextAccessor, IRoleWiseMenuRightRepo roleWiseMenuRightRepo, BAPLdbIdentityContext context)
        {
            _menuRepo = menuRepo;
            _roleRepo = roleRepo;
            _roleWiseMenuRightRepo = roleWiseMenuRightRepo;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<MenuMasterViewModel>> GetMenuItems()
        {
            try
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);

                var menus = await _menuRepo.GetMenuItems();

                var userRoles = await _context.Set<IdentityUserRole<string>>()
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                var roleId = userRoles.Select(x => x.RoleId).FirstOrDefault();

                var roleRights = await _roleWiseMenuRightRepo.GetMenuRightByRoleId(roleId);

                var allowedSubMenuIds = roleRights
                    .Where(x => x.Permission > 0)
                    .Select(x => x.SubMenuId)
                    .ToHashSet();

                var parentMenus = menus
                    .Where(x => x.ParentMenuId == null)
                    .OrderBy(x => x.MenuName)
                    .ThenBy(x => x.SerialNo)
                    .Select(menu =>
                    {
                        var children = menus
                        .Where(x => x.ParentMenuId == menu.Id
                        && roleRights.Any(r => r.SubMenuId == x.Id && r.Permission > 0))
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

                        var hasAccess = roleRights
                        .Any(r => r.SubMenuId == menu.Id && r.Permission > 0);

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
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
