using System.IO;
using System.Windows.Controls;
using GarbageSync.Client.Managers;
using GarbageSync.Shared.Utils;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Views;

namespace GarbageSync.Client;

public class Plugin : TorchPluginBase, IWpfPlugin
{
    private Persistent<Config> _config = null!;

    public override void Init(ITorchBase torch)
    {
        base.Init(torch);
        _config = Persistent<Config>.Load(Path.Combine(StoragePath, "GarbageSync.Client.cfg"));
        var sessionManager = Torch.Managers.GetManager<ITorchSessionManager>();
        
        sessionManager.AddFactory(s => new ClientNetworkManager(s.Torch, _config.Data));
        sessionManager.AddFactory(s => new ServerInfoManager(s.Torch));
    }

    public UserControl GetControl() => new PropertyGrid
    {
        Margin = new(3),
        DataContext = _config.Data
    };
}
