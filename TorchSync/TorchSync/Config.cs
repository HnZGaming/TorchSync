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

        bool _countRemotePlayerCount;
        int _port = 8100;

        public Config()
        {
            CollectionChangedEventManager.AddHandler(OtherPorts, OnOtherPortsCollectionChanged);
        }

        void OnOtherPortsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(OtherPorts));

            foreach (var port in OtherPorts)
            {
                PropertyChangedEventManager.RemoveHandler(port, OnOtherPortPropertyChanged, nameof(OtherPort.Number));
                PropertyChangedEventManager.AddHandler(port, OnOtherPortPropertyChanged, nameof(OtherPort.Number));
            }
        }

        void OnOtherPortPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(OtherPorts));
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
        public ObservableCollection<OtherPort> OtherPorts { get; } = new(); // parser will merge stuff
    }

    public class OtherPort : ViewModel
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