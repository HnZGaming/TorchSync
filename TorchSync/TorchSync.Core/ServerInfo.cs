using Newtonsoft.Json;

namespace TorchSync.Core
{
    public sealed class ServerInfo
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name;

        [JsonProperty("httpAddress", Required = Required.Always)]
        public IpPort HttpAddress;

        [JsonProperty("gameAddress", Required = Required.Always)]
        public IpPort GameAddress;
    }
}