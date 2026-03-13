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
                return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing token: {ex.Message}");
                return null;
            }
        }
    }
}
