using System.Windows;
using NLog;
using TorchSync.Core;

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
            Config.Instance.RemoteIps.Add(new IpPort { Port = 0 });
        }

        void OnRemotePortsRemoveClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.RemoteIps.RemoveAt(RemotePortsSelectedIndex);
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