using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace hjudgeWeb.Hubs
{
    public class ChatHub : Hub
    {
        public async Task BroadcastMessage(int id, string userId, string userName, DateTime sendTime, string content, int replyId)
        {
            await Clients.All.SendAsync("ChatMessage", id, userId, userName, $"{sendTime.ToShortDateString()} {sendTime.ToLongTimeString()}", content, replyId);
        }
    }
}
