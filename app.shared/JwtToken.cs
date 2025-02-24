using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace app.shared
{
    public static class JwtToken
    {
        public static string GenerateJwtToken(Claim[] claims)
        {
            try
            {
                var jwtTokenConfig = new
                {
                    SecretKey = SecurityUtils.PublicDecrypt(EnvironmentManager.SECRET_KEY),
                    Issuer = EnvironmentManager.ISSUER,
                    Audience = EnvironmentManager.AUDIENCE,
                    Claims = claims,
                    ExpireDate = DateTime.UtcNow.AddSeconds(15)
                };

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenConfig.SecretKey));
                var signinCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var tokenOptions = new JwtSecurityToken(
                    issuer: jwtTokenConfig.Issuer,
                    audience: jwtTokenConfig.Audience,
                    claims: jwtTokenConfig.Claims,
                    expires: jwtTokenConfig.ExpireDate,
                    signingCredentials: signinCredentials
                );

                return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception occurred during GenerateJwtToken: {ex.Message}");
            }
        }

        public static ClaimsPrincipal? ValidateJwtToken(string httpContextAuthorization)
        {
            try
            {
                var jwtTokenConfig = new
                {
                    SecretKey = SecurityUtils.PublicDecrypt(EnvironmentManager.SECRET_KEY),
                    Issuer = EnvironmentManager.ISSUER,
                    Audience = EnvironmentManager.AUDIENCE
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = Encoding.ASCII.GetBytes(jwtTokenConfig.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtTokenConfig.Issuer,
                    ValidAudience = jwtTokenConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                return tokenHandler.ValidateToken(httpContextAuthorization, validationParameters, out validatedToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Guid GetUserGuidWithValidateJwtToken(string httpContextAuthorization)
        {
            if (string.IsNullOrEmpty(httpContextAuthorization))
            {
                return Guid.Empty;
            }

            var principal = ValidateJwtToken(httpContextAuthorization);

            if (principal != null)
            {
                var userGuidClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userGuidClaim) && Guid.TryParse(userGuidClaim, out Guid userGuid))
                {
                    return userGuid;
                }
            }

            return Guid.Empty;
        }
    }
}
