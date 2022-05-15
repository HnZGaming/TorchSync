using Newtonsoft.Json;

namespace TorchSync.Http
{
    public sealed class SyncHttpError
    {
        [JsonProperty("message", Required = Required.Always)]
        public string Message;

        public override string ToString()
        {
            return $"{nameof(Message)}: {Message}";
        }
    }
}