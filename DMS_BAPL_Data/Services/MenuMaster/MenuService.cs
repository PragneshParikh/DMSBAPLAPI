using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.MenuMasterRepo;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
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

        public MenuService(IMenuRepo menuRepo)
        {
            _menuRepo = menuRepo;
        }

        public async Task<List<MenuMasterViewModel>> GetMenuItems()
        {
            try
            {
                var menus = await _menuRepo.GetMenuItems();

                var parentMenus = menus
                        .Where(x => x.ParentMenuId == null)
                        .Select(parent =>
                        {
                            var children = menus
                                .Where(x => x.ParentMenuId == parent.Id)
                                .Select(child => new MenuMasterViewModel
                                {
                                    id = child.Id,
                                    label = child.MenuName,
                                    link = child.PathName,
                                    parentId = child.ParentMenuId,
                                    module = child.ModuleName,
                                    subItems = null // or [] if you prefer empty array for Angular
                                })
                                .ToList();

                            return new MenuMasterViewModel
                            {
                                id = parent.Id,
                                label = parent.MenuName,
                                icon = "ri-dashboard-2-line",
                                isCollapsed = true, // only parent has this
                                subItems = children.Any() ? children : null
                            };
                        })
                        .ToList();

                var result = new List<MenuMasterViewModel>
                    {
                        new MenuMasterViewModel
                        {
                            id = 1000,
                            label = "MENUITEMS.MENU.TEXT",
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
