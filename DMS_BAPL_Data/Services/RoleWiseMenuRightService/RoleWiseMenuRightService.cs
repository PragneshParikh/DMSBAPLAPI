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
        private readonly BapldmsvadContext _dmsContext;

        public RoleWiseMenuRightService(IRoleWiseMenuRightRepo roleWiseMenuRightRepo, IHttpContextAccessor httpContextAccessor, BAPLdbIdentityContext identitycontext, BapldmsvadContext dmsContext)
        {
            _dmsContext = dmsContext;
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

        //    public async Task<IEnumerable<RoleWiseMenuRight>> GetMenuRightByRoleId(string? roleId)
        //    {
        //        if (string.IsNullOrEmpty(roleId))
        //        {
        //            var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);

        //            var roleIds = await _identitycontext.Set<IdentityUserRole<string>>()
        //                .Where(x => x.UserId == userId)
        //                .Select(x => x.RoleId)
        //                .Distinct()
        //                .ToListAsync();

        //            if (roleIds.Count == 0)
        //                return new List<RoleWiseMenuRight>();

        //            var allRights = new List<RoleWiseMenuRight>();
        //            foreach (var rid in roleIds)
        //            {
        //                var rights = await _roleWiseMenuRightRepo.GetMenuRightByRoleId(rid);
        //                if (rights != null)
        //                    allRights.AddRange(rights);
        //            }

        //            return allRights;
        //        }

        //        var single = await _roleWiseMenuRightRepo.GetMenuRightByRoleId(roleId);
        //        return single ?? new List<RoleWiseMenuRight>();
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
                return await _identitycontext.Roles
                    .Where(r => r.NormalizedName != null && upper.Contains(r.NormalizedName))
                    .Select(r => r.Id)
                    .ToListAsync();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<IEnumerable<RoleWiseMenuRight>> GetMenuRightByRoleId(string? roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                var roleIds = new List<string>();

                var locationRoleId = _httpContextAccessor.HttpContext?.User?.FindFirst("LocationRoleId")?.Value;
                if (!string.IsNullOrWhiteSpace(locationRoleId))
                {
                    roleIds.Add(locationRoleId);
                }

                var userId = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);
                var identityRoleIds = await _identitycontext.Set<IdentityUserRole<string>>()
                    .Where(x => x.UserId == userId)
                    .Select(x => x.RoleId)
                    .Distinct()
                    .ToListAsync();

                roleIds.AddRange(identityRoleIds);

                // 26.11.2026 - same rule as the sidebar (MenuService): the roles ticked for a BG employee in
                // BG Employee Master also grant page access. Without this the sidebar showed e.g. Warranty Claim
                // (from the "Service" role) but the page guard - which reads these rights - refused to open it.
                roleIds.AddRange(await GetBgEmployeeRoleIdsAsync());
                roleIds = roleIds.Distinct().ToList();

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
