using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.MenuMasterRepo
{
    public class MenuRepo : IMenuRepo
    {
        private readonly BapldmsvadContext _context;
        public MenuRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        public async Task<List<MenuMaster>> GetMenuItems()
        {
            try
            {
                return _context.MenuMasters.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
