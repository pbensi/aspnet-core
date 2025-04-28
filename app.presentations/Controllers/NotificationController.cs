using app.interfaces;
using app.shared.Dto;
using app.shared.Dto.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.presentations.Controllers
{
    [Route($"api/{PresentationAssemblyReference.Name}/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IServicesManager<INotificationService> _notification;
        private readonly ILogger<NotificationController> _logger;
        public NotificationController(
            IServicesManager<INotificationService> notification,
            ILogger<NotificationController> logger)
        {
            _notification = notification;
            _logger = logger;
        }

        [HttpGet("NotificationCountIsSeenAsync")]
        public async Task<int> NotificationCountIsSeenAsync()
        {
            try
            {
                int result = await _notification.Service.NotificationCountIsSeenAsync();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        [HttpGet("NotificationReadAsync")]
        public async Task<ResultDto> NotificationReadAsync(int id)
        {
            try
            {
                return await _notification.Service.NotificationReadAsync(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        [HttpGet("GetUserNotificationsAsync")]
        public async Task<PaginatedKeySetResultDto<UserNotificationDto>> GetUserNotificationsAsync([FromQuery] KeysetQueryDto keysetQuery)
        {
            try
            {
                var result = await _notification.Service.GetUserNotificationsAsync(keysetQuery);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }
    }
}
