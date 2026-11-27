using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DMS_BAPL_Utils.Helpers
{
    public static class GetUserInfoFromToken
    {
        public static string GetUserIdFromToken(HttpContext context)
        {
            try
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return userId;

                return "CUS0345A";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing token: {ex.Message}");
                return null;
            }
        }

        // ── existing method (kept for any existing callers) ──────────────
        public static string GetDealerCode(HttpContext context)
        {
            var username = context.User?.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(username))
                return username;

            return string.Empty;   // empty = no restriction (admin user)
        }

        public static bool GetUserGroup(HttpContext context)
        {
            try
            {
                // 26.11.2026 - a user can have more than one role (e.g. Dealer + SuperAdmin).
                // FindFirst only looked at the first role claim, so such a SuperAdmin was treated
                // as a dealer and every report was cut to his own dealer code. Check all role claims.
                var roles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

                return roles.Any(role =>
                    role.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                    role.Equals("superadmin", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing token: {ex.Message}");
                return false;
            }
        }

        // ── NEW: called by ReportController ─────────────────────────────
        public static string GetDealerCodeFromToken(HttpContext context)
        {
            // First try the dedicated "DealerCode" claim (set at login for dealer users)
            var dealerCode = context.User?.FindFirst("DealerCode")?.Value;
            if (!string.IsNullOrEmpty(dealerCode))
                return dealerCode;

            // Fallback: try ClaimTypes.Name (your existing logic)
            var username = context.User?.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(username))
                return username;

            // Empty = admin user, no dealer restriction applied
            return string.Empty;
        }

        // ── 26.11.2026 dealer scope (BG employees carry several dealer codes, comma separated) ──

        /// <summary>Dealer codes the logged-in user may see. Empty list = no restriction (admin / superadmin).</summary>
        public static List<string> GetAllowedDealerCodes(HttpContext context)
        {
            if (GetUserGroup(context)) return new List<string>();
            return SplitCodes(GetDealerCodeFromToken(context));
        }

        public static List<string> SplitCodes(string? codes) =>
            (codes ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(c => !string.Equals(c, "ALL", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(c, "undefined", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(c, "null", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        /// <summary>
        /// The dealer filter a report should use:
        ///  - admin / superadmin: what was asked for (blank = all dealers)
        ///  - dealer / employee: what was asked for when every asked code is one of theirs,
        ///    otherwise all of their own dealer codes (comma separated for an employee with several dealers).
        /// </summary>
        public static string? ScopeDealerCode(HttpContext context, string? requested)
        {
            if (GetUserGroup(context)) return requested;

            var allowed = SplitCodes(GetDealerCodeFromToken(context));
            if (allowed.Count == 0) return requested;

            var asked = SplitCodes(requested);
            if (asked.Count > 0 && asked.All(a => allowed.Contains(a, StringComparer.OrdinalIgnoreCase)))
                return string.Join(",", asked);

            return string.Join(",", allowed);
        }

        public static string GetLocationCodeFromToken(HttpContext httpContext)
        {
            return httpContext.User?.FindFirst("LocationCode")?.Value ?? string.Empty;
        }
    }
}