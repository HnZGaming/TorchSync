using System.Threading.Tasks;

namespace TorchSync.Http
{
    public interface ISyncHttpServerEndpoint
    {
        Task<SyncHttpResult> Respond(string path, string body);
    }
}