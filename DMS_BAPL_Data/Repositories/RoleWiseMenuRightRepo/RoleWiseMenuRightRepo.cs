using DMS_BAPL_Data.DBModels;
using DocumentFormat.OpenXml.Office2021.Excel.RichDataWebImage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.RoleWiseMenuRightRepo
{
    public class RoleWiseMenuRightRepo : IRoleWiseMenuRightRepo
    {
        private readonly BapldmsvadContext _context;
        public RoleWiseMenuRightRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<RoleWiseMenuRight>> Get()
        {
            return await _context.RoleWiseMenuRights.ToListAsync();
        }

        public async Task<IEnumerable<RoleWiseMenuRight>?> GetMenuRightByRoleId(string? roleId)
        {
            return await _context.RoleWiseMenuRights
                                 .Where(x => x.RoleId == roleId)
                                 .ToListAsync();
        }
    }
}
