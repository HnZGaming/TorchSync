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

        public int OtherPortsSelectedIndex { get; set; } = -1;

        void OnOtherPortsAddClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.OtherPorts.Add(new OtherPort { Number = 0 });
        }

        void OnOtherPortsRemoveClick(object sender, RoutedEventArgs e)
        {
            Config.Instance.OtherPorts.RemoveAt(OtherPortsSelectedIndex);
        }
    }
}