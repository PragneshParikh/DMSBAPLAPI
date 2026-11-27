using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Helpers;
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

                // 27.11.2026 - same rule as the sidebar (MenuService): for a BG employee the roles ticked in
                // BG Employee Master replace the generic "Employee" role, and employee-wise rights
                // (BgEmployeeMenuRight) override everything. These rights drive the page guard AND the
                // Add / Edit / Delete / Print buttons (MenuAccessService).
                var bgEmp = await BgEmployeeAccess.ResolveAsync(_dmsContext,
                    _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value);
                if (bgEmp?.HasOwnRights == true)
                    return BgEmployeeAccess.AsRoleRights(bgEmp.OwnRights!);

                if (bgEmp != null && bgEmp.MappedRoleIds.Count > 0)
                {
                    var employeeRoleIds = await _identitycontext.Roles
                        .Where(r => r.NormalizedName == "EMPLOYEE")
                        .Select(r => r.Id)
                        .ToListAsync();
                    roleIds = roleIds.Where(id => !employeeRoleIds.Contains(id)).ToList();
                    roleIds.AddRange(bgEmp.MappedRoleIds);
                }
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
