using System.Threading.Tasks;

namespace TorchSync.Http
{
    public interface ISyncHttpServerEndpoint
    {
        Task<SyncHttpResult> TryProcess(string path, string body);
    }
}