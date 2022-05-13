using GarbageSync.Host.Patches;
using GarbageSync.Shared;
using GarbageSync.Shared.Managers;
using GarbageSync.Shared.Managers.Network;
using GarbageSync.Shared.Messages;
using LiteNetLib;
using Torch.API;
using Torch.Managers;
namespace GarbageSync.Host.Managers;

public class ServerInfoManager : Manager
{
    [Dependency]
    private readonly IHostNetworkManager _networkManager = null!;
    
    public ServerInfoManager(ITorchBase torchInstance) : base(torchInstance)
    {
    }

    public override void Attach()
    {
        _networkManager.AddHandler<ServerInfoMessage>(SetServerInfoRpc);
    }

    [RpcHandlerId((uint)RpcHandlers.ServerInfo)]
    private void SetServerInfoRpc(ServerInfoMessage message, NetPeer peer)
    {
        SteamPlayerDataPatch.OnPlayersData(message.Players);
    }
}
