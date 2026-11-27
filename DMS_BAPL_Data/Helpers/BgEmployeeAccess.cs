using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Helpers
{
    /// <summary>
    /// 27.11.2026 - menu / page / button rights of a logged-in BG employee.
    ///   1. Employee-wise rights (table BgEmployeeMenuRight) - when present they are the ONLY source.
    ///   2. Otherwise the roles ticked in BG Employee Master (BgEmployeeRoleMapping, e.g. Service / Sales).
    ///   3. Otherwise the login role (Employee) - unchanged behaviour.
    /// </summary>
    public static class BgEmployeeAccess
    {
        public class RightRow
        {
            public int MenuId { get; set; }
            public int SubMenuId { get; set; }
            public int Permission { get; set; }
        }

        public class EmployeeAccess
        {
            public int EmployeeId { get; set; }
            /// <summary>AspNetRoles ids of the roles ticked in BG Employee Master.</summary>
            public List<string> MappedRoleIds { get; set; } = new();
            /// <summary>Employee-wise rights; null / empty = use the roles.</summary>
            public List<RightRow>? OwnRights { get; set; }
            public bool HasOwnRights => OwnRights != null && OwnRights.Count > 0;
        }

        /// <summary>Null when the email is not an active BG employee.</summary>
        public static async Task<EmployeeAccess?> ResolveAsync(BapldmsvadContext ctx, string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var e = email.Trim().ToLower();

            var empId = await ctx.BgEmployeeMasters.AsNoTracking()
                .Where(x => x.IsActive && x.EmailId != null && x.EmailId.ToLower() == e)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
            if (empId == null) return null;

            var roleNames = await ctx.BgEmployeeRoleMappings.AsNoTracking()
                .Where(m => m.BgEmployeeId == empId.Value)
                .Select(m => m.RoleName)
                .Distinct()
                .ToListAsync();
            var upper = roleNames.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim().ToUpper()).ToList();

            var roleIds = upper.Count == 0
                ? new List<string>()
                : await ctx.AspNetRoles.AsNoTracking()
                    .Where(r => r.NormalizedName != null && upper.Contains(r.NormalizedName))
                    .Select(r => r.Id)
                    .ToListAsync();

            return new EmployeeAccess
            {
                EmployeeId = empId.Value,
                MappedRoleIds = roleIds,
                OwnRights = await GetOwnRightsAsync(ctx, empId.Value)
            };
        }

        /// <summary>Rows of BgEmployeeMenuRight; null when the table does not exist yet (script not run).</summary>
        public static async Task<List<RightRow>?> GetOwnRightsAsync(BapldmsvadContext ctx, int employeeId)
        {
            try
            {
                return await ctx.Database
                    .SqlQuery<RightRow>($"SELECT MenuId, SubMenuId, Permission FROM dbo.BgEmployeeMenuRight WHERE BgEmployeeId = {employeeId} AND Permission > 0")
                    .ToListAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>The employee rows as RoleWiseMenuRight objects (same shape the menu / guard code already uses).</summary>
        public static List<RoleWiseMenuRight> AsRoleRights(IEnumerable<RightRow> rows) =>
            rows.Select(r => new RoleWiseMenuRight
            {
                RoleId = "employee",
                MenuId = r.MenuId,
                SubMenuId = r.SubMenuId,
                Permission = r.Permission,
                CreatedBy = "employee",
                CreatedDate = DateTime.Now
            }).ToList();
    }
}
