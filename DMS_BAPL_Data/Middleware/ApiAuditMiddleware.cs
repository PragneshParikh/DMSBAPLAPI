using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace DMS_BAPL_Data.Middleware
{
    public class ApiAuditMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiAuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            var request = context.Request;
            string payload = string.Empty;

            if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put)
            {
                request.EnableBuffering();
                if (request.ContentLength > 0)
                {
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, false, leaveOpen: true);
                    payload = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                }
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context); // The controller logic runs here
            }
            finally
            {
                // 1. Capture response text regardless of success/failure
                responseBody.Seek(0, SeekOrigin.Begin);
                string responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put)
                {
                    // 2. Create a NEW SCOPE to get a clean DbContext
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<BapldmsvadContext>();

                        var statusText = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300 ? "Success" : "Failed";
                        var endpoint = context.GetRouteValue("controller")?.ToString() ?? "Unknown";

                        var log = new Apitracking
                        {
                            Endpoint = endpoint,
                            Dateofhit = DateTime.UtcNow,
                            Payload = payload,
                            Status = statusText,
                            Response = responseBodyText
                        };

                        db.Apitrackings.Add(log);
                        await db.SaveChangesAsync(); // This will now work!
                    }
                }

                // 3. Copy back to original stream so the client gets the response
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}