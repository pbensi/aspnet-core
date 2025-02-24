using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetEscapades.AspNetCore.SecurityHeaders;
using System.Text;

namespace Web.Host
{
    public sealed class NonceAttribute : ActionFilterAttribute
    {
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var response = context.HttpContext.Response;

            if (context.Result is FileContentResult || context.Result is FileStreamResult ||
                context.Result is JsonResult || context.Result is ObjectResult)
            {
                await next();
                return;
            }

            var originalBodyStream = response.Body;

            using (var newBodyStream = new MemoryStream())
            {
                response.Body = newBodyStream;

                await next();

                response.Body.Seek(0, SeekOrigin.Begin);

                var responseBody = await new StreamReader(response.Body).ReadToEndAsync();

                var requestSpecificNonce = context.HttpContext.GetNonce();

                var nonceInjectedBody = responseBody
                    .Replace("<script>", $"<script nonce=\"{requestSpecificNonce}\">", StringComparison.OrdinalIgnoreCase)
                    .Replace("<style>", $"<style nonce=\"{requestSpecificNonce}\">", StringComparison.OrdinalIgnoreCase);

                response.Body = originalBodyStream;

                await response.WriteAsync(nonceInjectedBody, Encoding.UTF8);
            }
        }
    }
}
