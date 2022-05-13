using TorchSync.Shared.Managers;
using TorchSync.Shared.Managers.Network;
using Torch;
using Torch.Views;

namespace TorchSync.Host;

public class Config : ViewModel, INetworkConfig
{
    [Display(Name = "Port")]
    public int Port { get; set; } = 4853;
}
