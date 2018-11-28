using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace hjudgeWeb.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.GetHttpContext().Request.Query["path"]);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.GetHttpContext().Request.Query["path"]);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
