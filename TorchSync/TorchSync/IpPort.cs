using System.Xml.Serialization;
using Newtonsoft.Json;
using Torch;

namespace TorchSync
{
    public class IpPort : ViewModel
    {
        [JsonProperty("ip", Required = Required.Always)]
        string _ip;

        [JsonProperty("port", Required = Required.Always)]
        int _port;

        [JsonConstructor]
        public IpPort()
        {
        }

        public IpPort(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        [XmlAttribute]
        public string Ip
        {
            get => _ip;
            set => SetValue(ref _ip, value);
        }

        [XmlAttribute]
        public int Port
        {
            get => _port;
            set => SetValue(ref _port, value);
        }

        public override string ToString()
        {
            return $"{nameof(Ip)}: {Ip}, {nameof(Port)}: {Port}";
        }
    }
}