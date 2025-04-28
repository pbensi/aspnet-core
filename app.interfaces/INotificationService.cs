using app.shared.Dto;
using app.shared.Dto.Account;

namespace app.interfaces
{
    public interface INotificationService
    {
        Task<int> NotificationCountIsSeenAsync();
        Task<ResultDto> NotificationReadAsync(int id);
        Task<PaginatedKeySetResultDto<UserNotificationDto>> GetUserNotificationsAsync(KeysetQueryDto keysetQuery);
    }
}
