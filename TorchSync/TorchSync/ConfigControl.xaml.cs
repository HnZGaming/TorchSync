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

        public int RemoteChatAuthorSelectedIndex { get; set; }

        void OnRemotePortsAddClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.RemotePorts.Add(new RemotePort { Number = 0 });
        }

        void OnRemotePortsRemoveClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.RemotePorts.RemoveAt(RemotePortsSelectedIndex);
        }

        void OnRemoteChatAuthorAddClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.RemoteChatAuthors.Add(new ChatAuthor { Name = "" });
        }

        void OnRemoteChatAuthorRemoveClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.RemoteChatAuthors.RemoveAt(RemoteChatAuthorSelectedIndex);
        }
    }
}