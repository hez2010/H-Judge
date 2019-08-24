using System.Threading.Tasks;
using hjudge.WebHost.Models.Judge;
using Microsoft.AspNetCore.SignalR;

namespace hjudge.WebHost.Hubs
{
    public interface IJudgeHub
    {
        Task JudgeCompleteSignalReceived(int resultId);
    }
    public class JudgeHub : Hub<IJudgeHub>
    {
        public Task SubscribeJudgeResult(int resultId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"result_{resultId}");
        }
    }
}