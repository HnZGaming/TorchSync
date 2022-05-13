using TorchSync.Shared.Managers;
using TorchSync.Shared.Managers.Network;
using LiteNetLib;
namespace TorchSync.Host.Managers;

public interface IHostNetworkManager : INetworkManagerBase
{
    void SendMessage<TMessage>(TMessage message, uint handlerId, NetPeer peer)
        where TMessage : new();
    void SendMessage<TMessage>(TMessage message, uint handlerId, int peerId)
        where TMessage : new();

    /// <summary>
    /// Broadcasts message to all connected peers
    /// </summary>
    /// <param name="message">ProtoBuf serializable message object</param>
    /// <param name="handlerId">Id of destination rpc handler</param>
    /// <typeparam name="TMessage">ProtoBuf serializable type</typeparam>
    void SendMessage<TMessage>(TMessage message, uint handlerId)
        where TMessage : new();

    IResponseTask<TResponse> SendResponseMessage<TMessage, TResponse>(TMessage message, uint handlerId, NetPeer destination)
        where TMessage : new()
        where TResponse : new();
    IResponseTask<TResponse> SendResponseMessage<TMessage, TResponse>(TMessage message, uint handlerId, int destinationId)
        where TMessage : new()
        where TResponse : new();
}
