using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace app.services.SignalR
{
    public class SignalRHub : Hub
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

        public override async Task OnConnectedAsync()
        {
            var userGuid = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userGuid))
            {
                AddConnection(userGuid, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public void AddConnection(string userGuid, string connectionId)
        {
            _userConnections.AddOrUpdate(userGuid,
                _ => new HashSet<string> { connectionId },
                (_, connectionIds) =>
                {
                    connectionIds.Add(connectionId);
                    return connectionIds;
                });
        }

        public void RemoveConnection(string userGuid, string connectionId)
        {
            if (_userConnections.TryGetValue(userGuid, out var connectionIds))
            {
                connectionIds.Remove(connectionId);

                if (connectionIds.Count == 0)
                {
                    _userConnections.TryRemove(userGuid, out _);
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userGuid = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userGuid))
            {
                RemoveConnection(userGuid, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
