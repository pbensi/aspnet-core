using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Host.Middlewares
{
    public sealed class HealthCheckMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly HealthCheckService _healthCheckService;

        public HealthCheckMiddleWare(RequestDelegate next, HealthCheckService healthCheckService)
        {
            _next = next;
            _healthCheckService = healthCheckService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var results = await _healthCheckService.CheckHealthAsync();
            if (results.Status != HealthStatus.Healthy)
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync("wwwroot/Unhealthy.html");
                return;
            }

            await _next(context);
        }
    }
}
