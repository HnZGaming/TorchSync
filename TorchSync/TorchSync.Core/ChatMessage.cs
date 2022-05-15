using Newtonsoft.Json;

namespace TorchSync.Core
{
    public sealed class ChatMessage
    {
        [JsonProperty("header", Required = Required.Always)]
        public string Header;

        [JsonProperty("name", Required = Required.Always)]
        public string Name;

        [JsonProperty("message", Required = Required.Always)]
        public string Message;

        public override string ToString()
        {
            return $"{nameof(Header)}: {Header}, {nameof(Name)}: {Name}, {nameof(Message)}: {Message}";
        }
    }
}