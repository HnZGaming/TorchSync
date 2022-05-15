using System.Windows;
using NLog;

namespace TorchSync
{
    public partial class ConfigControl
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public ConfigControl()
        {
            DataContext = Config.Instance;
            InitializeComponent();
        }

        public int RemotePortsSelectedIndex { get; set; }

        void OnRemotePortsAddClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.RemotePorts.Add(new RemotePort { Number = 0 });
        }

        void OnRemotePortsRemoveClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.RemotePorts.RemoveAt(RemotePortsSelectedIndex);
        }
    }
}