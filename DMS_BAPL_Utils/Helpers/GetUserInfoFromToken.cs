using Microsoft.AspNetCore.Http;
using System;
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
                var role = context.User?.FindFirst(ClaimTypes.Role)?.Value;

                return role != null &&
               (role.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
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
    }
}