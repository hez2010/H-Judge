using hjudge.WebHost.Models.Message;
using System.Threading.Tasks;

namespace hjudge.WebHost.Hubs.Clients
{
    public interface IMessageHub
    {
        Task MessageReceived(MessageModel message);
    }
}
