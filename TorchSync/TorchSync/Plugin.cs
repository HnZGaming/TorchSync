using System.ComponentModel;
using System.Windows.Controls;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.API.Session;
using TorchSync.Core;
using Utils.Torch;

namespace TorchSync
{
    public sealed class Plugin : TorchPluginBase, IWpfPlugin
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        Persistent<Config> _config;
        ConfigControl _control;
        SyncCore _core;
        SyncNetwork _network;

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

            if (e == null || e.PropertyName == nameof(Config.Port))
            {
                _network?.UpdateConfig();
            }

            _core?.UpdateConfig();
        }

        void OnSessionLoaded()
        {
            _network = new SyncNetwork();
            _network.Start();

            _core = new SyncCore(_network);
        }

        void OnSessionUnloading()
        {
            _network?.Close();
            _core?.Close();
        }

        public override void Update()
        {
            _core?.Update();
        }
    }
}