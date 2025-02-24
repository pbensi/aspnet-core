using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Host.Middlewares
{
    internal sealed class SQLHealthCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HealthCheckService _healthCheckService;

        public SQLHealthCheckMiddleware(RequestDelegate next, HealthCheckService healthCheckService)
        {
            _next = next;
            _healthCheckService = healthCheckService;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var results = await _healthCheckService.CheckHealthAsync();
            if (results.Status != HealthStatus.Healthy)
            {
                httpContext.Response.ContentType = "text/html";
                await httpContext.Response.SendFileAsync(UrlResources.UnhealthyUrl);
                return;
            }

            await _next(httpContext);
        }
    }
}
