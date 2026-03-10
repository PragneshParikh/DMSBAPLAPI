using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text;

namespace DMS_BAPL_Utils.Middleware
{
    public class ApiAuditMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiAuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, BapldmsvadContext db)
        {
            var request = context.Request;

            // Only capture body for POST or PUT
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

            // Replace response body to capture it
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            await _next(context); // call the next middleware / controller
            watch.Stop();

            // Read the response body
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Only store logs for POST/PUT
            if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put)
            {
                string responseForDb = responseBodyText;
                var statusText = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300 ? "Success" : "Failed";

                var endpoint = context.GetRouteValue("controller")?.ToString() ?? "Unknown";
                var action = context.GetRouteValue("action")?.ToString() ?? "Unknown";

                var log = new Apitracking
                {
                    Endpoint = $"{endpoint}",
                    Dateofhit = DateTime.UtcNow,
                    Payload = payload,
                    Status = statusText,
                    Response = responseForDb
                };

                db.Apitrackings.Add(log);
                await db.SaveChangesAsync();
            }

            // Copy response back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}