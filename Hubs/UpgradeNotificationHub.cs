using Microsoft.AspNetCore.SignalR;

namespace UpgradeNotificationSystem.Hubs
{
    public class UpgradeNotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public override async Task OnConnectedAsync()
        {
            // Optional: Log connection
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Optional: Log disconnection
            await base.OnDisconnectedAsync(exception);
        }
    }
}