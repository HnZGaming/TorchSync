using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TorchSync.Http
{
    public sealed class SyncHttpError
    {
        [JsonProperty("message", Required = Required.Always)]
        public string Message;
    }
}