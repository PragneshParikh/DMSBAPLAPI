using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.Helpers
{
    public static class GetUserInfoFromToken
    {
        public static string GetUserIdFromToken(HttpContext context)
        {
            try
            {
                //return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }

                return "CUS0345A";

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing token: {ex.Message}");
                return null;
            }
        }

        public static string GetDealerCode(HttpContext context)
        {
            var username = context.User?.FindFirstValue(ClaimTypes.Name);
            //return username;
            if (!string.IsNullOrEmpty(username))
                return username;

            return "CUS0345U";
        }
    }
}
