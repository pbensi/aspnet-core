using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api.web.host.presentation.token
{
    public static class JwtToken
    {
        public static string GenerateJwtToken(IConfiguration configuration)
        {
            try
            {
                string secretKey = configuration["Jwt:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("Missing Secret Key Configuration");
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var signinCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var tokenOptions = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: new List<Claim>(),
                    expires: DateTime.Now.AddMinutes(1),
                    signingCredentials: signinCredentials
                );

                return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred during JWT token generation: {ex.Message}");
                throw;
            }
        }

        public static bool ValidateJwtToken(IConfiguration configuration, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = Encoding.ASCII.GetBytes(configuration["Jwt:SecretKey"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
