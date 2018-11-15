using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace hjudgeWeb.Hubs
{
    public class ChatHub : Hub
    {
        public async Task BroadcastMessage(string userId, string userName, DateTime sendTime, string content)
        {
            await Clients.All.SendAsync("ChatMessage", userId, userName, $"{sendTime.ToShortDateString()} {sendTime.ToLongTimeString()}", content);
        }
    }
}
