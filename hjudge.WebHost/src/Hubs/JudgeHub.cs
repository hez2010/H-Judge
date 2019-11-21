using System.Threading.Tasks;
using hjudge.WebHost.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace hjudge.WebHost.Hubs
{
    public class JudgeHub : Hub<IJudgeHub>
    {
        public Task SubscribeJudgeResult(int resultId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"result_{resultId}");
        }
    }
}