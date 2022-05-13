using TorchSync.Host.Patches;
using TorchSync.Shared;
using TorchSync.Shared.Managers.Network;
using TorchSync.Shared.Messages;
using LiteNetLib;
using Torch.API;
using Torch.Managers;

namespace TorchSync.Host.Managers;

public class ServerInfoManager : Manager
{
    [Dependency]
    private readonly IHostNetworkManager _networkManager = null!;

    public ServerInfoManager(ITorchBase torchInstance) : base(torchInstance) { }

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