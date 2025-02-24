using Web.Host.Middlewares;

namespace Web.Host.Extensions
{
    internal static class AppBuilder
    {
        public static IApplicationBuilder UseSqlServerHealthCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SQLHealthCheckMiddleware>();
        }
        public static IApplicationBuilder UseRequestThrottling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestThrottlingMiddleware>();
        }
        public static IApplicationBuilder UseSwaggerAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerAuthenticationMiddleware>();
        }
        public static IApplicationBuilder UseRolePermission(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RolePermissionMiddleware>();
        }
        public static IApplicationBuilder UseRequestContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestContextMiddleware>();
        }

        //public static IApplicationBuilder UseSignalRHub(this IApplicationBuilder builder)
        //{
        //    builder.UseEndpoints(endpoints =>
        //    {
        //        endpoints.MapHub<SignalRHub>(UrlResources.SignalRHubUrl);
        //    });

        //    return builder;
        //}
    }
}
