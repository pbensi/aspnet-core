namespace Web.Host.HttpContexts
{
    internal static class RequestManager
    {
        private const string AuthorizationHeader = "Authorization";

        public static string GetAuthorizationHeader(HttpContext httpContext)
        {
            if (httpContext?.Request?.Headers.ContainsKey(AuthorizationHeader) == true)
            {
                var authHeader = httpContext.Request.Headers[AuthorizationHeader].ToString();

                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return authHeader.Substring("Bearer ".Length).Trim();
                }
            }
            return string.Empty;
        }

        public static void SetAuthorizationHeader(HttpContext httpContext, string token)
        {
            if (httpContext != null)
            {
                httpContext.Request.Headers[AuthorizationHeader] = "Bearer " + token;
            }
        }

        public static void RemoveAuthorizationHeader(HttpContext httpContext)
        {
            if (httpContext != null && httpContext.Request.Headers.ContainsKey(AuthorizationHeader))
            {
                httpContext.Request.Headers.Remove(AuthorizationHeader);
            }
        }

        public static string GetRequestHeader(HttpContext httpContext, string requestName)
        {
            if (httpContext != null && !string.IsNullOrWhiteSpace(requestName))
            {
                return httpContext.Request.Headers[requestName].ToString();
            }

            return string.Empty;
        }

        public static void SetRequestHeader(HttpContext httpContext, string requestName, string value)
        {
            if (httpContext != null && !string.IsNullOrEmpty(requestName))
            {
                httpContext.Request.Headers[requestName] = value;
            }
        }

        public static void RemoveRequestHeader(HttpContext httpContext, string requestName)
        {
            if (httpContext != null && !string.IsNullOrWhiteSpace(requestName))
            {
                httpContext.Request.Headers.Remove(requestName);
            }
        }
    }
}
