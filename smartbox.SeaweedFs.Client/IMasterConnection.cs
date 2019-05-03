using smartbox.SeaweedFs.Client.Core;

namespace smartbox.SeaweedFs.Client
{
    public interface IMasterConnection
    {
        Connection GetConnection();
    }
}