using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using NLog;
using Torch;
using TorchSync.Core;
using Utils.Torch;
using VRage.Collections;

namespace TorchSync
{
    public sealed class Config : ViewModel
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public static Config Instance { get; set; }

        readonly ConcurrentCachingHashSet<IpPort> _remoteIps;
        readonly ConcurrentCachingHashSet<string> _remoteChatAuthors;
        bool _countRemotePlayerCount;
        int _port = 8100;
        string _name = "Example";

        public Config()
        {
            _remoteIps = new ConcurrentCachingHashSet<IpPort>();
            CollectionChangedEventManager.AddHandler(RemoteIps, (_, _) =>
            {
                _remoteIps.Clear();
                foreach (var remotePort in RemoteIps)
                {
                    _remoteIps.Add(remotePort);
                }

                _remoteIps.ApplyChanges();

                OnPropertyChanged(nameof(RemoteIps));
            });

            _remoteChatAuthors = new ConcurrentCachingHashSet<string>();
            CollectionChangedEventManager.AddHandler(RemoteChatAuthors, (_, _) =>
            {
                _remoteChatAuthors.Clear();
                foreach (var chatAuthor in RemoteChatAuthors)
                {
                    _remoteChatAuthors.Add(chatAuthor.Name);
                }

                _remoteChatAuthors.ApplyChanges();

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
        public ViewModelCollection<IpPort> RemoteIps { get; } = new(); // parser will merge stuff

        [XmlArray]
        public ViewModelCollection<ChatAuthor> RemoteChatAuthors { get; } = new(); // parser will merge stuff

        // use this instead
        [XmlIgnore]
        public IEnumerable<IpPort> RemoteIpsSet => _remoteIps;

        [XmlElement]
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        // use this instead
        [XmlIgnore]
        public IEnumerable<string> RemoteChatAuthorSet => _remoteChatAuthors;
    }
}