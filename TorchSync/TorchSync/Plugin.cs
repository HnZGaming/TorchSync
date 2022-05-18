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
        FileLoggingConfigurator _fileLogger;

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

            _fileLogger = new FileLoggingConfigurator(
                nameof(TorchSync),
                new[] { $"{nameof(TorchSync)}.*" },
                Config.DefaultPath);

            _fileLogger.Initialize();

            ReloadConfig();
        }

        public void ReloadConfig()
        {
           // _config?.Dispose(); // this saves the file >:(
            _config = Persistent<Config>.Load(this.MakeFilePath("TorchSync.cfg"));
            Config.Instance = _config.Data;
            PropertyChangedEventManager.AddHandler(_config.Data, OnConfigChanged, "");
            _fileLogger.Configure(Config.Instance);
            _control?.OnReload();
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