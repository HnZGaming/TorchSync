using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;
using NLog;
using Torch;

namespace TorchSync
{
    public sealed class Config : ViewModel
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public static Config Instance { get; set; }

        readonly HashSet<int> _remotePortsSet;
        bool _countRemotePlayerCount;
        int _port = 8100;

        public Config()
        {
            _remotePortsSet = new HashSet<int>();
            CollectionChangedEventManager.AddHandler(RemotePorts, OnRemotePortsCollectionChanged);
        }

        void OnRemotePortsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnRemotePortPropertyChanged(null, null);

            foreach (var remotePort in RemotePorts)
            {
                PropertyChangedEventManager.RemoveHandler(remotePort, OnRemotePortPropertyChanged, nameof(RemotePort.Number));
                PropertyChangedEventManager.AddHandler(remotePort, OnRemotePortPropertyChanged, nameof(RemotePort.Number));
            }
        }

        void OnRemotePortPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _remotePortsSet.Clear();
            foreach (var remotePort in RemotePorts)
            {
                _remotePortsSet.Add(remotePort.Number);
            }

            OnPropertyChanged(nameof(RemotePorts));
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
        public ObservableCollection<RemotePort> RemotePorts { get; } = new(); // parser will merge stuff

        // use this instead
        [XmlIgnore]
        public IEnumerable<int> RemotePortsSet => _remotePortsSet;
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
}