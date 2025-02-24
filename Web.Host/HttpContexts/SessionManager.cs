namespace Web.Host.HttpContexts
{
    internal static class SessionManager
    {
        public static void SetSessionString(HttpContext httpContext, string sessionName, string value)
        {
            if (httpContext != null && !string.IsNullOrWhiteSpace(sessionName))
            {
                httpContext.Session.SetString(sessionName, value);
            }
        }

        public static string GetSessionString(HttpContext httpContext, string sessionName)
        {
            if (httpContext != null && !string.IsNullOrWhiteSpace(sessionName))
            {
                return httpContext.Session.GetString(sessionName) ?? string.Empty;
            }
            return string.Empty;
        }

        public static void RemoveSession(HttpContext httpContext, string sessionName)
        {
            if (httpContext != null && !string.IsNullOrWhiteSpace(sessionName))
            {
                httpContext.Session.Remove(sessionName);
            }
        }
    }
}
