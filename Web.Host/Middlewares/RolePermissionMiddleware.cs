using app.interfaces;
using app.migrator.Contexts;
using Web.Host.Models;

namespace Web.Host.Middlewares
{
    internal sealed class RolePermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public RolePermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext,
            RequestContext requestContext,
            IServicesManager<IAccountService> accountService)
        {

            var result = await accountService.Service.CheckAccountPermissionAsync(requestContext.PageName,
                httpContext.Request.Method,
                httpContext.Request.Path,
                requestContext.AllowedRole.ToString(),
                requestContext.UserGuid);

            if (result.HasPermission == false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsJsonAsync(new ResponseModel
                {
                    Message = result.Message,
                });
                return;
            }

            await _next(httpContext);
        }
    }
}
