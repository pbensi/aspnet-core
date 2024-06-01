using api.service.abstractions;
using api.shared.Dto;
using api.web.host.presentation.token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace api.web.host.presentation.Controllers
{
    [Route($"api/{PresentationAssemblyReference.Version}/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public UserController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        [HttpGet("GetUserPermissionsAsync")]
        public async Task<IActionResult> GetUserPermissionsAsync(int id)
        {
            var result = await _servicesManager.UserService.GetUserPermissionAsync(id);
            return Ok(result);
        }

        [HttpGet("GetAllUserAsync")]
        public async Task<IActionResult> GetAllUserAsync(string search, int pageNumber, int pageSize)
        {
            var result = await _servicesManager.UserService.GetAllUsersAsync(search, pageNumber, pageSize);
            return Ok(new { UsersDto = result.UsersDto, TotalPages = result.totalPages });
        }

        [HttpGet("FindUserByIdAsync")]
        public async Task<IActionResult> FindUserByIdAsync(int id)
        {
            var result = await _servicesManager.UserService.FindUserByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("UpdateUserAsync")]
        public async Task<IActionResult> UpdateUserAsync(UserDto user)
        {
            var isUserExist = await _servicesManager.UserService.FindUserByIdAsync(user.Id);

            if (isUserExist == null)
            {
                return Ok(isUserExist);
            }

            var result = await _servicesManager.UserService.UpdateUserAsync(user);

            return Ok(result);
        }

        [HttpPost("CreateUserAsync")]
        public async Task<IActionResult> CreateUserAsync(UserDto user)
        {
            var result = await _servicesManager.UserService.CreateUserAsync(user);
            return Ok(result);
        }

        [HttpDelete("DeleteUserAsync")]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            var result = await _servicesManager.UserService.DeleteUserAsync(id);
            return Ok(result);
        }

        [HttpGet("GetAuthorize")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAuthorize([FromServices] IConfiguration configuration, string accountName, [DataType(DataType.Password)] string password)
        {
            var result = await _servicesManager.UserService.SignInUserAsync(accountName, password);
            string jwtToken = string.Empty;

            if (result.user != null && result.user.IsAdmin)
            {
                jwtToken = JwtToken.GenerateJwtToken(configuration);
            }

            return Ok(new { Token = jwtToken });
        }
    }
}
