using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var endpoint = context.GetEndpoint();
            var controllerAction = endpoint?.Metadata
                .GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();

            string controllerName = controllerAction?.ControllerName ?? "UnknownController";
            string actionName = controllerAction?.ActionName ?? "UnknownAction";

            string userName = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Anonymous";

            string userId = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                  ?? "UnknownUser"
                : "Anonymous";

            string method = context.Request.Method;

            var log = new ExceptionLog
            {
                UserName = userName,
                Controller = controllerName,
                Action = actionName,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                HttpMethod = method,
                ExceptionMessage = ex.ToString(),
                StackTrace = ex.StackTrace,
                OccureAt = DateTime.Now,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            using (var scope = context.RequestServices.CreateScope())
            {
                var dbcontext = scope.ServiceProvider.GetRequiredService<BapldmsvadContext>();

                dbcontext.ExceptionLogs.Add(log);
                await dbcontext.SaveChangesAsync();
            }

            _logger.LogError(ex, "Exception caught and stored in DB");

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = "Internal server error" });
        }
    }
}