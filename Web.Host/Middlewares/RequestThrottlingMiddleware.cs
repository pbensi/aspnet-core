using app.migrator.Contexts;
using System.Collections.Concurrent;
using System.Text;
using Web.Host.Models;

namespace Web.Host.Middlewares
{
    internal sealed class RequestThrottlingMiddleware
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly RequestDelegate _next;

        public RequestThrottlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext,
            RequestContext requestContext)
        {
            string clientIp = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
                     ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

            string key = GenerateKey(httpContext.Request.Path, clientIp, requestContext.UserGuid, httpContext.Request.Query, httpContext.GetRouteData());

            var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            if (!await semaphore.WaitAsync(0))
            {
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await httpContext.Response.WriteAsJsonAsync(new ResponseModel
                {
                    Message = "Too many requests. Please try again later",
                });
                return;
            }

            try
            {
                await _next(httpContext);
            }
            finally
            {
                semaphore.Release();

                if (semaphore.CurrentCount == 1)
                {
                    _locks.TryRemove(key, out _);
                    semaphore.Dispose();
                }
            }
        }

        private string GenerateKey(string path,string clientIp, Guid userGuid, IQueryCollection query, RouteData routeData)
        {
            var keyBuilder = new StringBuilder($"{path}_{clientIp}_{userGuid}");

            if (query.Count > 0)
            {
                var sortedQuery = query.OrderBy(q => q.Key);
                keyBuilder.Append($"_{string.Join("&", sortedQuery.Select(q => $"{q.Key}={q.Value}"))}");
            }

            if (routeData.Values.Count > 0)
            {
                var sortedRoute = routeData.Values.OrderBy(r => r.Key);
                keyBuilder.Append($"_{string.Join("&", sortedRoute.Select(r => $"{r.Key}={r.Value}"))}");
            }

            return keyBuilder.ToString();
        }
    }
}
