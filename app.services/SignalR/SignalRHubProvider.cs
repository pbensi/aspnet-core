using System.Web;
using app.shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace app.services.SignalR
{
    [Authorize]
    public class SignalRProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var httpContext = connection.GetHttpContext();
            if (httpContext == null) return null;

            string queryString = httpContext.Request.QueryString.Value!;
            var queryParams = HttpUtility.ParseQueryString(queryString);
            string httpContextAuthorization = queryParams["access_token"] ?? string.Empty;

            Guid userGuid = JwtToken.GetUserGuidWithValidateJwtToken(httpContextAuthorization);

            return userGuid != Guid.Empty ? userGuid.ToString() : null;
        }
    }
}
