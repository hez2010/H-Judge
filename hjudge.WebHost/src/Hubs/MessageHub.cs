using hjudge.WebHost.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace hjudge.WebHost.Hubs
{
    public class MessageHub : Hub<IMessageHub>
    {
        public Task SubscribeMessageHub()
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"message_{Context.UserIdentifier}");
        }
    }
}
