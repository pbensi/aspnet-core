using app.entities;
using app.interfaces;
using app.migrator.Contexts;
using app.repositories;
using app.repositories.Extensions;
using app.services.SignalR;
using app.shared.Dto;
using app.shared.Dto.Account;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static app.shared.EnumGroup;

namespace app.services.Services
{
    internal sealed class NotificationService : INotificationService
    {
        private readonly IRepository<UserNotification> _userNotification;
        private readonly IRepository<PersonalDetail> _personalDetail;
        private readonly IHubContext<SignalRHub> _signalRHub;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly RequestContext _requestContext;

        public NotificationService(
            IRepositoryManager repositoryManager,
            IHubContext<SignalRHub> signalRHub,
            IMapper mapper,
            RequestContext requestContext
            )
        {
            _userNotification = repositoryManager.Entity<UserNotification>();
            _personalDetail = repositoryManager.Entity<PersonalDetail>();
            _unitOfWork = repositoryManager.UnitOfWork;
            _signalRHub = signalRHub;
            _mapper = mapper;
            _requestContext = requestContext;
        }

        public async Task<int> NotificationCountIsSeenAsync()
        {
            try
            {
                int totalIsSeen = await _userNotification
                                        .Where(p => p.RecieverGuid == _requestContext.UserGuid &&
                                                     p.IsSeen == false &&
                                                     p.IsDelete == false)
                                        .Take(99)
                                        .CountAsync();

                return totalIsSeen;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<ResultDto> NotificationReadAsync(int id)
        {
            try
            {

                var notification = await _userNotification
                                        .FirstOrDefaultAsync(p => p.RecieverGuid == _requestContext.UserGuid && p.Id == id && p.IsDelete == false);

                if (notification == null)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "Notification not found.",
                        Type = MessageType.Warning
                    };
                }

                if (notification.IsRead)
                {
                    return new ResultDto
                    {
                        IsSuccess = true,
                        Message = "Notification is already marked as read.",
                        Type = MessageType.Information
                    };
                }

                notification.IsRead = true;

                _userNotification.Update(notification);

                bool isSaveChanges = await _unitOfWork.SaveChangesAsync();

                return new ResultDto
                {
                    IsSuccess = isSaveChanges,
                    Message = isSaveChanges ? "Notification marked as read successfully" : "Failed to update notification.",
                    Type = isSaveChanges ? MessageType.Success : MessageType.Error
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        #region UserNotificationKeysetResult
        private IQueryable<UserNotification> UserNotificationAsQueryable(KeysetQueryDto keysetQuery)
        {
            return _userNotification.AsQueryable()
                                           .Where(p => p.RecieverGuid == _requestContext.UserGuid && p.IsDelete == false);
        }

        public async Task<PaginatedKeySetResultDto<UserNotificationDto>> GetUserNotificationsAsync(KeysetQueryDto keysetQuery)
        {
            try
            {
                keysetQuery.SortColumn = "Id";
                var query = UserNotificationAsQueryable(keysetQuery);

                if (keysetQuery.HasNextPage && !string.IsNullOrEmpty(keysetQuery.NextCursor))
                {
                    query = query.Where(p => p.Id.CompareTo(int.Parse(keysetQuery.NextCursor)) < 0);
                }

                var notifications = await query.OrderByDescending(p => p.Id)
                     .ThenByDescending(p => p.IsSeen)
                     .ThenByDescending(p => p.CreatedAt)
                     .Take(keysetQuery.PageSize)
                .ToListAsync();

                if (!notifications.Any())
                {
                    return new PaginatedKeySetResultDto<UserNotificationDto>
                    {
                        Data = new List<UserNotificationDto>(),
                        SortColumn = keysetQuery.SortColumn,
                        SortDirection = keysetQuery.SortDirection,
                        Search = keysetQuery.Search,
                        PreviousCursor = string.Empty,
                        NextCursor = string.Empty,
                        HasPreviousPage = false,
                        HasNextPage = false
                    };
                }

                var personalInfos = await _personalDetail
                    .Where(p => p.UserGuid == _requestContext.UserGuid)
                    .ToDictionaryAsync(p => p.UserGuid, p => $"{p.FirstName} {p.LastName} {p.MiddleName}".Trim());

                var notificationDtos = notifications.Select(notification =>
                {
                    notification.IsSeen = true;
                    notification.SeenAt = DateTime.Now;

                    var senderName = personalInfos.TryGetValue(notification.SenderGuid, out var name)
                    ? name
                    : "Unknown Sender";

                    var notificationDto = _mapper.Map<UserNotificationDto>(notification);
                    notificationDto.SenderName = senderName;

                    return notificationDto;
                }).ToList();

                string nextCursor = notifications.LastOrDefault().GetPropertyValue("Id");

                var nextPageQuery = UserNotificationAsQueryable(keysetQuery);
                bool hasNextPage = await nextPageQuery
                     .Where(p => p.Id.CompareTo(int.Parse(nextCursor)) < 0)
                    .OrderByDescending(p => p.Id)
                    .ThenByDescending(p => p.IsSeen)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(1)
                    .AnyAsync();

                _userNotification.UpdateRange(notifications);

                await _unitOfWork.SaveChangesAsync();

                await _signalRHub.Clients.User(_requestContext.UserGuid.ToString()).SendAsync("countIsSeenAsync", await NotificationCountIsSeenAsync());

                return new PaginatedKeySetResultDto<UserNotificationDto>
                {
                    Data = notificationDtos,
                    SortColumn = keysetQuery.SortColumn,
                    SortDirection = keysetQuery.SortDirection,
                    Search = keysetQuery.Search,
                    NextCursor = nextCursor,
                    HasNextPage = hasNextPage
                };
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while fetching user notifications.", e);
            }
        }
        #endregion
    }
}
