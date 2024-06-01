using api.repository.Exceptions;
using Web.Host.Models;

namespace Web.Host.Middlewares
{
    internal sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                await HandleExecptionAsync(context, e);
            }
        }

        private static async Task HandleExecptionAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType="application/json";
            httpContext.Response.StatusCode = exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            AddLogErrorToFile(exception);

            await httpContext.Response.WriteAsync(new ErrorResponse(exception.Message).ToString());
        }

        private static void AddLogErrorToFile(Exception exception)
        {
            string logFilePath = Path.Combine("App_Data", "logs.txt");
            string rootPath = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(rootPath, logFilePath);

            if(Path.Exists(fullPath))
            {
                File.AppendAllText(fullPath, $"{DateTime.Now}: {exception.Message}\n");
            }
        }
    }
}
