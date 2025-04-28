using app.migrator.Contexts;
using app.shared;
using Web.Host.HttpContexts;
using Web.Host.Models;

namespace Web.Host.Middlewares
{
    internal sealed class RequestContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext,
            RequestContext requestContext)
        {
            var corsAllowedOrigins = EnvironmentManager.APP_CORS_ALLOWED_ORIGINS?.Split(',') ?? Array.Empty<string>();
            var corsServerOrigins = EnvironmentManager.APP_CORS_SERVER_ORIGINS?.Split(',') ?? Array.Empty<string>();

            string originRequest = RequestManager.GetRequestHeader(httpContext, RequestNames.Origin);

            if (corsAllowedOrigins.Length == 0 || corsServerOrigins.Length == 0)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new ResponseModel
                {
                    Message = "The client or server CORS origin should not be empty.",
                });
                return;
            }

            string accountUserGuid = string.Empty;
            string pageName = string.Empty;
            AllowedRole allowedRole = AllowedRole.None;

            if (corsAllowedOrigins.Contains(originRequest) && !corsServerOrigins.Contains(originRequest))
            {
                accountUserGuid = RequestManager.GetRequestHeader(httpContext, RequestNames.Account);
                pageName = RequestManager.GetRequestHeader(httpContext, RequestNames.Page);
                allowedRole = AllowedRole.Client;
            }
            else if (corsAllowedOrigins.Contains(originRequest) && corsServerOrigins.Contains(originRequest) || string.IsNullOrEmpty(originRequest))
            {
                accountUserGuid = SessionManager.GetSessionString(httpContext, SessionNames.Account);
                pageName = SessionManager.GetSessionString(httpContext, SessionNames.Page);
                allowedRole = AllowedRole.Server;
            }

            if (allowedRole == AllowedRole.None)
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(new ResponseModel
                {
                    Message = "The client or server CORS origin is incorrectly configured.",
                });
                return;
            }

            Guid.TryParse(accountUserGuid, out Guid userGuid);
            requestContext.UserGuid = userGuid;
            requestContext.PageName = pageName;
            requestContext.AllowedRole = allowedRole;

            await _next(httpContext);
        }
    }
}
