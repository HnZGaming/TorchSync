using GarbageSync.Shared.Managers;
using GarbageSync.Shared.Managers.Network;
using Torch;
using Torch.Views;

namespace GarbageSync.Host;

public class Config : ViewModel, INetworkConfig
{
    [Display(Name = "Port")]
    public int Port { get; set; } = 4853;
}
