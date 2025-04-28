using System.Security.Claims;
using app.interfaces;
using app.shared;
using app.shared.Crypto.Dto;
using app.shared.Dto.Account;
using app.shared.Securities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.presentations.Controllers
{
    [Route($"api/{PresentationAssemblyReference.Name}/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IServicesManager<IAccountService> _account;
        private readonly ILogger<PersonalDetailController> _logger;

        public AccountController(
              IServicesManager<IAccountService> account,
              ILogger<PersonalDetailController> logger
            )
        {
            _account = account;
            _logger = logger;
        }

        [HttpGet("GetAccountAsync")]
        public async Task<AccountDto> GetAccountAsync(Guid userGuid)
        {
            try
            {
                return await _account.Service.GetAccountAsync(userGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet("SignInAccountAsync")]
        [AllowAnonymous]
        public async Task<SignInResultDto> SignInAccountAsync([FromQuery] DataRequestDto request)
        {
            try
            {
                var data = Asymmetric.ProcessSecureData<SignInDto>(request);

                var (account, message) = await _account.Service.SignInAccountAsync(data);

                if (account == null)
                {
                    return new SignInResultDto
                    {
                        Authorization = null,
                        Message = message
                    };
                }

                var claims = new[]
                 {
                    new Claim(ClaimTypes.NameIdentifier, $"{account.UserGuid}"),
                    new Claim(ClaimTypes.Name, $"{account.UserName}"),
                    new Claim(ClaimTypes.Role, "Client")
                };

                string userJwt = JwtToken.GenerateJwtToken(claims);
                return new SignInResultDto
                {
                    Authorization = userJwt,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet("CheckAccountPermissionAsync")]
        public async Task<PermissionCheckDto> CheckAccountPermissionAsync(string permissionName, string requestMethod)
        {
            try
            {
                return await _account.Service.CheckAccountPermissionAsync(permissionName, requestMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        [HttpGet("CheckPermissionPageNamesAsync")]
        public async Task<List<AccountRoleDto>> CheckPermissionPageNamesAsync([FromQuery] List<string> permissionNames)
        {
            try
            {
                return await _account.Service.CheckPermissionPageNamesAsync(permissionNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
