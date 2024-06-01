using api.shared.Utilities;
using System.Text;

namespace Web.Host.Middlewares
{
    public sealed class SwaggerAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TimeSpan _inactiveTimeout = TimeSpan.FromMinutes(2);

        public SwaggerAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Session.GetString("token");

            var lastActivityTimeString = context.Session.GetString("LastActivityTime");
            DateTime lastActivityTime;

            if (string.IsNullOrEmpty(lastActivityTimeString)
                || !DateTime.TryParse(lastActivityTimeString, out lastActivityTime))
            {
                lastActivityTime = DateTime.UtcNow;
                context.Session.SetString("LastActivityTime", lastActivityTime.ToString());
            }

            var currentTime = DateTime.UtcNow;
            var timeSinceLastActivity = currentTime - lastActivityTime;

            if (timeSinceLastActivity > _inactiveTimeout)
            {
                token = null;
                context.Session.SetString("token", string.Empty);
            }

            context.Session.SetString("LastActivityTime", currentTime.ToString());
            
            if (!string.IsNullOrEmpty(token))
            {
                AES aes = new AES();

                string decryptData = aes.DecryptData(token, Token.Key);

                if (decryptData.Equals(Token.UserId))
                {
                    if ((context.Request.Path.StartsWithSegments("/")
                    || context.Request.Path.StartsWithSegments("/login"))
                    && !string.IsNullOrEmpty(token))
                    {
                        context.Response.Redirect("/swagger");
                        return;
                    }
                }
            }

            if ((context.Request.Path.StartsWithSegments("/swagger")
                || IsSwaggerRequest(context.Request.Headers))
                && string.IsNullOrEmpty(token))
            {
                context.Session.Remove("token");
                context.Response.Redirect("/login");
                return;
            }

            await _next(context);
        }

        private bool IsSwaggerRequest(IHeaderDictionary headers)
        {
            return headers.ContainsKey("Referer") && headers["Referer"].ToString().Contains("/swagger");
        }
    }
}
