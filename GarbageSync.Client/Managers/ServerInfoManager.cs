using GarbageSync.Shared;
using GarbageSync.Shared.Messages;
using Sandbox.Engine.Multiplayer;
using Torch.API;
using Torch.API.Managers;
using Torch.Managers;
namespace GarbageSync.Client.Managers;

public class ServerInfoManager : Manager
{
    [Dependency]
    private readonly IClientNetworkManager _networkManager = null!;

    [Dependency]
    private readonly IMultiplayerManagerBase _multiplayerManager = null!;

    public ServerInfoManager(ITorchBase torchInstance) : base(torchInstance)
    {
    }

    public override void Attach()
    {
        _multiplayerManager.PlayerJoined += MultiplayerManagerOnPlayerChanged;
        _multiplayerManager.PlayerLeft += MultiplayerManagerOnPlayerChanged;
    }
    private void MultiplayerManagerOnPlayerChanged(IPlayer obj)
    {
        _networkManager.SendMessage(new ServerInfoMessage
        {
            Players = MyMultiplayer.Static.Members.Skip(1).Select(static b => new PlayerInfo
            {
                ClientId = b,
                Name = MyMultiplayer.Static.GetMemberName(b)
            }).ToList()
        }, (uint)RpcHandlers.ServerInfo);
    }
}
