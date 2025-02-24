using app.migrator.Contexts;
using Web.Host.HttpContexts;
using Web.Host.Models;

namespace Web.Host.Middlewares
{
    internal sealed class SwaggerAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext,
            RequestContext requestContext)
        {
            switch (requestContext.AllowedRole)
            {
                case AllowedRole.Client:
                    //
                    break;
                case AllowedRole.Server:
                    bool isSigninRequest = httpContext.Request.Path.StartsWithSegments(UrlResources.RootUrl, StringComparison.OrdinalIgnoreCase) ||
                                                           httpContext.Request.Path.StartsWithSegments(UrlResources.HomeUrl, StringComparison.OrdinalIgnoreCase);
                    bool isSwaggerRequest = httpContext.Request.Path.StartsWithSegments(UrlResources.SwaggerUrl, StringComparison.OrdinalIgnoreCase);
                    bool isApiRequest = httpContext.Request.Path.StartsWithSegments(UrlResources.APIUrl, StringComparison.OrdinalIgnoreCase);
                    bool isAccountActive = requestContext.UserGuid != Guid.Empty;

                    bool isRedirectToRootUrl = (isSwaggerRequest && !isSigninRequest && !isAccountActive);
                    if (isRedirectToRootUrl)
                    {
                        httpContext.Response.Redirect(UrlResources.RootUrl, true);
                        return;
                    }

                    bool isRedirectToSwaggerUrl = (!isSwaggerRequest  && isSigninRequest && isAccountActive);
                    if (isRedirectToSwaggerUrl)
                    {
                        httpContext.Response.Redirect(UrlResources.SwaggerUrl, true);
                        return;
                    }

                    bool isRedirectToPageNotFound = (!isApiRequest && !isSigninRequest  && !isAccountActive);
                    if (isRedirectToPageNotFound)
                    {
                        httpContext.Response.ContentType = "text/html";
                        await httpContext.Response.SendFileAsync(UrlResources.PageNotFound);
                        return;
                    }

                    //bool isSessionExpired = (isApiRequest && !isAccountActive);
                    //if (isSessionExpired)
                    //{
                    //    httpContext.Response.Redirect(UrlResources.RootUrl, true);
                    //    return;
                    //}

                    break;
                case AllowedRole.None:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await httpContext.Response.WriteAsJsonAsync(new ResponseModel
                    {
                        Message = $"Invalid Access Role: {requestContext.AllowedRole}. Access Role should be one of the defined roles (Client, Server)",
                    });
                    return;
            }

            await _next(httpContext);
        }
    }
}
