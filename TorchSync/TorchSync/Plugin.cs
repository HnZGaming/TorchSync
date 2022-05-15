using System.ComponentModel;
using System.Windows.Controls;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using TorchSync.Core;
using Utils.General;
using Utils.Torch;

namespace TorchSync
{
    public sealed class Plugin : TorchPluginBase, IWpfPlugin
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        Persistent<Config> _config;
        ConfigControl _control;

        public SyncCore Core { get; private set; }

        UserControl IWpfPlugin.GetControl()
        {
            return _control ??= new ConfigControl();
        }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.OnSessionStateChanged(TorchSessionState.Loaded, OnSessionLoaded);
            this.OnSessionStateChanged(TorchSessionState.Unloading, OnSessionUnloading);

            ReloadConfig();
        }

        public void ReloadConfig()
        {
            _config?.Dispose();
            _config = Persistent<Config>.Load(this.MakeFilePath("TorchSync.cfg"));
            Config.Instance = _config.Data;
            _config.Data.PropertyChanged += OnConfigChanged;
            OnConfigChanged(null, null);
        }

        void OnConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            Log.Info($"config changed: {e?.PropertyName ?? "<init>"}");

            var restartHttp = e == null || e.PropertyName == nameof(Config.Port);
            Core?.OnConfigChanged(restartHttp);
        }

        void OnSessionLoaded()
        {
            var chatManager = Torch.CurrentSession.Managers.GetManager<IChatManagerServer>();
            chatManager.ThrowIfNull(nameof(chatManager));

            Core = new SyncCore();
            Core.Start(chatManager);
        }

        void OnSessionUnloading()
        {
            Core?.Close();
        }

        public override void Update()
        {
            Core?.Update();
        }
    }
}