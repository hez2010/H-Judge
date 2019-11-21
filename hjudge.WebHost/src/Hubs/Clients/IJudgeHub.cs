using System.Threading.Tasks;

namespace hjudge.WebHost.Hubs.Clients
{
    public interface IJudgeHub
    {
        Task JudgeCompleteSignalReceived(int resultId);
    }
}
