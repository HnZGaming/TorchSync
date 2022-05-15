using Newtonsoft.Json;

namespace TorchSync.Core
{
    public sealed class RemotePlayer
    {
        [JsonProperty("steamId", Required = Required.Always)]
        public ulong SteamId;

        [JsonProperty("name", Required = Required.Always)]
        public string Name;

        public override string ToString()
        {
            return $"{nameof(SteamId)}: {SteamId}, {nameof(Name)}: {Name}";
        }
    }
}