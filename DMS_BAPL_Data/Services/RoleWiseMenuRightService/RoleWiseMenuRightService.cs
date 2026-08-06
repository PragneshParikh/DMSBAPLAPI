using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.RoleWiseMenuRightRepo;
using DMS_BAPL_Utils.Helpers;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Services.RoleWiseMenuRightService
{
    public class RoleWiseMenuRightService : IRoleWiseMenuRightService
    {
        private readonly IRoleWiseMenuRightRepo _roleWiseMenuRightRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BAPLdbIdentityContext _identitycontext;

        public RoleWiseMenuRightService(IRoleWiseMenuRightRepo roleWiseMenuRightRepo, IHttpContextAccessor httpContextAccessor, BAPLdbIdentityContext identitycontext)
        {
            _roleWiseMenuRightRepo = roleWiseMenuRightRepo;
            _httpContextAccessor = httpContextAccessor;
            _identitycontext = identitycontext;
        }

        public Task<IEnumerable<RoleWiseMenuRight>> Get()
        {
            return _roleWiseMenuRightRepo.Get();
        }

        //public async Task<IEnumerable<RoleWiseMenuRight>> GetMenuRightByRoleId(string? roleId)
        //{

        //    if (string.IsNullOrEmpty(roleId))
        //    {
        //        var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);

        //        var userRoles = await _identitycontext.Set<IdentityUserRole<string>>()
        //                                      .Where(x => x.UserId == userId)
        //                                      .ToListAsync();

        //        roleId = userRoles.Select(x => x.RoleId).FirstOrDefault();
        //    }

        //    if (string.IsNullOrEmpty(roleId))
        //    {
        //        return new List<RoleWiseMenuRight>();
        //    }

        //    return await _roleWiseMenuRightRepo.GetMenuRightByRoleId(roleId);
        //}

        public async Task<IEnumerable<RoleWiseMenuRight>> GetMenuRightByRoleId(string? roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);

                var roleIds = await _identitycontext.Set<IdentityUserRole<string>>()
                    .Where(x => x.UserId == userId)
                    .Select(x => x.RoleId)
                    .Distinct()
                    .ToListAsync();

                if (roleIds.Count == 0)
                    return new List<RoleWiseMenuRight>();

                var allRights = new List<RoleWiseMenuRight>();
                foreach (var rid in roleIds)
                {
                    var rights = await _roleWiseMenuRightRepo.GetMenuRightByRoleId(rid);
                    if (rights != null)
                        allRights.AddRange(rights);
                }

                return allRights;
            }

            var single = await _roleWiseMenuRightRepo.GetMenuRightByRoleId(roleId);
            return single ?? new List<RoleWiseMenuRight>();
        }
    }
}
