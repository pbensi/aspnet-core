using Web.Host.Middlewares;

namespace Web.Host.Extensions.ApplicationBuilderExtensions
{
    public static class ApplicationBuilder
    {
        public static IApplicationBuilder UseHealthCheckApplication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HealthCheckMiddleWare>();
        }
        public static IApplicationBuilder UseSwaggerAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerAuthenticationMiddleware>();
        }
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
