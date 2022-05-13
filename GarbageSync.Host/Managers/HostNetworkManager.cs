using GarbageSync.Shared.Managers;
using GarbageSync.Shared.Managers.Network;
using GarbageSync.Shared.Utils;
using LiteNetLib;
using NLog;
using Torch.API;
namespace GarbageSync.Host.Managers;

public class HostNetworkManager : NetworkManagerBase, IHostNetworkManager
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public HostNetworkManager(ITorchBase torchInstance, INetworkConfig config) : base(torchInstance, config)
    {
        Listener.ConnectionRequestEvent += ListenerOnConnectionRequest;
    }
    
    private void ListenerOnConnectionRequest(ConnectionRequest request)
    {
        Logger.Info("Connection request from {0}", request.RemoteEndPoint);
        request.Accept();
    }

    public override void Attach()
    {
        base.Attach();
        if (!NetworkManager.Start(Config.Port))
            throw new($"Unable to bind socket to port {Config.Port}, maybe port already in use");
        if (NetworkManager.IsRunning)
            Logger.Info("Server listening on {0}", Config.Port);
    }
    
    public void SendMessage<TMessage>(TMessage message, uint handlerId, NetPeer peer) where TMessage : new()
    {
        var writer = DataWriterPool.TakeOrCreate();
        SerializeToNetWriter(message, writer, handlerId, false);
        peer.Send(writer, DeliveryMethod.ReliableUnordered);
        writer.Reset();
        DataWriterPool.Add(writer);
    }

    public void SendMessage<TMessage>(TMessage message, uint handlerId, int peerId) where TMessage : new()
    {
        if (NetworkManager.GetPeerById(peerId) is not { } peer)
            throw new KeyNotFoundException($"Peer with given id ({peerId}) not found");
        SendMessage(message, handlerId, peer);
    }

    public void SendMessage<TMessage>(TMessage message, uint handlerId) where TMessage : new()
    {
        var writer = DataWriterPool.TakeOrCreate();
        SerializeToNetWriter(message, writer, handlerId, false);
        NetworkManager.SendToAll(writer, DeliveryMethod.ReliableUnordered);
        writer.Reset();
        DataWriterPool.Add(writer);
    }

    public IResponseTask<TResponse> SendResponseMessage<TMessage, TResponse>(TMessage message, uint handlerId, NetPeer destination) where TMessage : new() where TResponse : new()
    {
        var writer = DataWriterPool.TakeOrCreate();
        var responseMessageId = IdGenerator.NextId();
        SerializeToNetWriter(message, writer, handlerId, true, responseMessageId: responseMessageId);
        destination.Send(writer, DeliveryMethod.ReliableUnordered);
        writer.Reset();
        DataWriterPool.Add(writer);
        var task = new ResponseTask<TResponse>();
        ResponseTasks[responseMessageId] = task;
        return task;
    }

    public IResponseTask<TResponse> SendResponseMessage<TMessage, TResponse>(TMessage message, uint handlerId, int destinationId) where TMessage : new() where TResponse : new()
    {
        if (NetworkManager.GetPeerById(destinationId) is not { } peer)
            throw new KeyNotFoundException($"Peer with given id ({destinationId}) not found");
        return SendResponseMessage<TMessage, TResponse>(message, handlerId, peer);
    }
}
