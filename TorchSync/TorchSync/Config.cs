using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using NLog;
using Torch;
using Utils.Torch;

namespace TorchSync
{
    public sealed class Config : ViewModel
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public static Config Instance { get; set; }

        readonly HashSet<int> _remotePorts;
        readonly HashSet<string> _remoteChatAuthors;
        bool _countRemotePlayerCount;
        int _port = 8100;
        string _chatHeader = "Example";

        public Config()
        {
            _remotePorts = new HashSet<int>();
            CollectionChangedEventManager.AddHandler(RemotePorts, (_, _) =>
            {
                _remotePorts.Clear();
                foreach (var remotePort in RemotePorts)
                {
                    _remotePorts.Add(remotePort.Number);
                }

                OnPropertyChanged(nameof(RemotePorts));
            });

            _remoteChatAuthors = new HashSet<string>();
            CollectionChangedEventManager.AddHandler(RemoteChatAuthors, (_, _) =>
            {
                _remoteChatAuthors.Clear();
                foreach (var chatAuthor in RemoteChatAuthors)
                {
                    _remoteChatAuthors.Add(chatAuthor.Name);
                }

                OnPropertyChanged(nameof(RemoteChatAuthors));
            });
        }

        [XmlElement]
        public bool CountRemotePlayerCount
        {
            get => _countRemotePlayerCount;
            set => SetValue(ref _countRemotePlayerCount, value);
        }

        [XmlElement]
        public int Port
        {
            get => _port;
            set => SetValue(ref _port, value);
        }

        [XmlArray]
        public ViewModelCollection<RemotePort> RemotePorts { get; } = new(); // parser will merge stuff

        [XmlArray]
        public ViewModelCollection<ChatAuthor> RemoteChatAuthors { get; } = new(); // parser will merge stuff

        // use this instead
        [XmlIgnore]
        public IEnumerable<int> RemotePortsSet => _remotePorts;

        [XmlElement]
        public string ChatHeader
        {
            get => _chatHeader;
            set => SetValue(ref _chatHeader, value);
        }

        // use this instead
        [XmlIgnore]
        public IEnumerable<string> RemoteChatAuthorSet => _remoteChatAuthors;
    }

    public class RemotePort : ViewModel
    {
        int _number;

        [XmlAttribute]
        public int Number
        {
            get => _number;
            set => SetValue(ref _number, value);
        }
    }

    public class ChatAuthor : ViewModel
    {
        string _name;

        [XmlAttribute]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }
    }
}