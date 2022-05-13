using System.Net;
using TorchSync.Shared.Managers;
using TorchSync.Shared.Managers.Network;
using TorchSync.Shared.Utils;
using LiteNetLib;
using Torch.API;
namespace TorchSync.Client.Managers;

public class ClientNetworkManager : NetworkManagerBase, IClientNetworkManager
{
    private NetPeer _peer = null!;
    private new IClientNetworkConfig Config { get; }

    public ClientNetworkManager(ITorchBase torchInstance, IClientNetworkConfig config) : base(torchInstance, config)
    {
        Config = config;
    }

    public override void Attach()
    {
        base.Attach();
        NetworkManager.Start();
        Connect();
    }

    private void Connect()
    {
        _peer = NetworkManager.Connect(new IPEndPoint(Config.TargetIp, Config.Port), string.Empty);
    }

    protected override void ListenerOnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.ListenerOnPeerDisconnected(peer, disconnectInfo);
        Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith(_ => Connect());
    }

    public void SendMessage<TMessage>(TMessage message, uint handlerId) where TMessage : new()
    {
        if (_peer is null)
            throw new InvalidOperationException();

        var writer = DataWriterPool.TakeOrCreate();
        SerializeToNetWriter(message, writer, handlerId, false);
        _peer.Send(writer, DeliveryMethod.ReliableUnordered);
        writer.Reset();
        DataWriterPool.Add(writer);
    }

    public IResponseTask<TResponse> SendResponseMessage<TMessage, TResponse>(TMessage message, uint handlerId) where TMessage : new() where TResponse : new()
    {
        var writer = DataWriterPool.TakeOrCreate();
        var responseMessageId = IdGenerator.NextId();
        SerializeToNetWriter(message, writer, handlerId, true, responseMessageId: responseMessageId);
        _peer!.Send(writer, DeliveryMethod.ReliableUnordered);
        writer.Reset();
        DataWriterPool.Add(writer);
        var task = new ResponseTask<TResponse>();
        ResponseTasks[responseMessageId] = task;
        return task;
    }
}
