using System.Windows;
using NLog;
using Utils.General;

namespace TorchSync
{
    public partial class ConfigControl
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public ConfigControl()
        {
            Initialize();
        }

        public int RemotePortsSelectedIndex { get; set; }

        public int RemoteChatAuthorSelectedIndex { get; set; }

        public void OnReload()
        {
            Dispatcher.Invoke(Initialize);
        }

        void Initialize()
        {
            DataContext = Config.Instance;
            InitializeComponent();
        }

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