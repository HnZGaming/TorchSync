using LiteNetLib;
namespace TorchSync.Shared.Managers.Network;

public interface INetworkCallback
{
    void NetResponseCallback<TResponse>(TResponse value, uint id, uint responseMessageId, NetPeer peer);
}