using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DMS_BAPL_Data.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _secretKey;
        private readonly List<string> _validApiKeys;
        private readonly BapldmsvadContext _bapldmsvadContext;

        public AuthorizationMiddleware(RequestDelegate next, IConfiguration configuration, BapldmsvadContext bapldmsvadContext)
        {
            _next = next;
            _secretKey = configuration["Jwt:Key"]
                 ?? throw new ArgumentNullException("Jwt:Key missing in configuration");
            //_validApiKeys = configuration.GetSection("ApiKeys").Get<List<string>>() ?? new List<string>();
            _bapldmsvadContext = bapldmsvadContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (context.Request.Path.StartsWithSegments("/api/auth"))
            {
                await _next(context);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/api/vehicle-dispatch")
                && endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                if (!context.Request.Headers.TryGetValue("x-api-key", out var extractedApiKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("x-api-key header missing");
                    return;
                }

                if (!_validApiKeys.Contains(extractedApiKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid API Key");
                    return;
                }

                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Authorization header missing");
                return;
            }

            var token = authHeader.ToString().Replace("Bearer ", "");

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "DMS_BAPL_Api",
                    ValidAudience = "DMS_BAPL_Angular",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ??
                                  principal.FindFirst("userId");
                var userId = userIdClaim?.Value;

                context.Items["UserId"] = userId;

                await _next(context);
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired token");
            }
        }
    }
}
