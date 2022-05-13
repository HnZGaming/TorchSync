using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using GarbageSync.Shared.Utils;
using Humanizer;
using LiteNetLib;
using LiteNetLib.Utils;
using NLog;
using ProtoBuf;
using ProtoBuf.Meta;
using Torch.API;
using Torch.Managers;
using VRage.Library.Algorithms;
namespace GarbageSync.Shared.Managers.Network;

public abstract class NetworkManagerBase : Manager, INetworkManagerBase, INetworkCallback
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    
    protected readonly EventBasedNetListener Listener = new();

    protected NetManager NetworkManager = null!;

    protected readonly ConcurrentBag<NetDataWriter> DataWriterPool = new();

    protected readonly SequenceIdGenerator IdGenerator = SequenceIdGenerator.CreateWithStopwatch(TimeSpan.FromSeconds(4));
    
    protected readonly ConcurrentDictionary<uint, IResponseTask> ResponseTasks = new();
    protected readonly INetworkConfig Config;

    private readonly Dictionary<uint, NetHandlerExpression> _handlers = new();

    public override void Attach()
    {
        NetworkManager = new(Listener)
        {
            UnsyncedEvents = true,
            IPv6Enabled = IPv6Mode.Disabled
        };
    }
    
    protected virtual void ListenerOnPeerConnected(NetPeer peer)
    {
        Logger.Info("Peer connected {0} ({1})", peer.EndPoint, peer.Id);
    }

    protected virtual void ListenerOnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Logger.Info("Peer disconnected {0} ({1}) reason: {2}", peer.EndPoint, peer.Id,
            disconnectInfo.Reason.Humanize().Transform(To.LowerCase));
    }

    private void ListenerOnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        var id = reader.GetUInt();
        var isResponse = reader.GetBool();
        try
        {
            if (deliveryMethod != DeliveryMethod.ReliableUnordered)
                throw new InvalidOperationException($"Invalid delivery method: {deliveryMethod.Humanize().Transform(To.LowerCase)}");

            uint messageId = default;
            var isResponsible = reader.GetBool();
            if (isResponsible)
                messageId = reader.GetUInt();

            var data = (ReadOnlySpan<byte>)reader.RawData.AsSpan(reader.Position, reader.AvailableBytes);

            if (isResponse)
            {
                if (ResponseTasks.TryRemove(messageId, out var task))
                {
                    task.OnResponse(data);
                    IdGenerator.Return(messageId);
                }
                else
                    Logger.Warn("Unexpected response ({0}+{1}) responsible-{2} response-{3}", id, messageId, isResponsible, isResponse);
                return;
            }

            var netHandlerExpression = _handlers[id];
            netHandlerExpression.Invoke(messageId, data, peer, this);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error while processing received packet ({0}+{1}). Peer {2} ({3}) ", id, isResponse, peer.EndPoint, peer.Id);
            peer.Disconnect();
        }
    }

    public void AddHandlerDescriptor<TMessage>(NetworkHandlerDescriptor<TMessage> descriptor) where TMessage : new()
    {
        var (del, responseMessageType) = descriptor;
        var hash = HashDelegate(del);
        _handlers.Add(hash, CreateNetHandler<TMessage>(del, hash, responseMessageType));
    }
    
    private static uint HashDelegate(Delegate del)
    {
        return del.Method.GetCustomAttribute<RpcHandlerIdAttribute>()?.Id ??
               throw new InvalidOperationException(
                   $"Missing {nameof(RpcHandlerIdAttribute)} on method {del.Method.DeclaringType?.FullName}+{del.Method.Name}");
    }
    
    #region CallSites

    private static NetHandlerExpression CreateNetHandler<TMessage>(Delegate handler, uint? responseId = null, Type? responseType = null) where TMessage : new()
    {
        var messageIdParameter = Expression.Parameter(typeof(uint), "responseMessageId");
        var dataParameter = Expression.Parameter(typeof(ReadOnlySpan<byte>), "data");
        var peerParameter = Expression.Parameter(typeof(NetPeer), "peer");
        var callbackParameter = Expression.Parameter(typeof(INetworkCallback), "callback");

        var targetVariable = Expression.Constant(handler.Target);

        var deserializeMethod = typeof(Serializer).GetMethods().First(b =>
            b.Name == "Deserialize" && b.GetParameters()[0].ParameterType == typeof(ReadOnlySpan<byte>));

        var deserializeCall = Expression.Call(deserializeMethod.MakeGenericMethod(typeof(TMessage)), dataParameter,
            Expression.Default(typeof(TMessage)), Expression.Constant(null));

        Expression body = Expression.Call(targetVariable, handler.Method,
            deserializeCall, peerParameter);

        if (responseType is { } && responseId is { } id)
        {
            var idVariable = Expression.Constant(id, typeof(uint));
            body = Expression.Call(callbackParameter,
                typeof(INetworkCallback).GetMethod(nameof(NetResponseCallback))?.MakeGenericMethod(responseType) ??
                throw new InvalidOperationException(), body, idVariable, messageIdParameter, peerParameter);
        }

        return Expression.Lambda<NetHandlerExpression>(body, messageIdParameter, dataParameter, peerParameter, callbackParameter).Compile();
    }

    public void NetResponseCallback<TResponse>(TResponse value, uint id, uint responseMessageId, NetPeer peer)
    {
        var netWriter = DataWriterPool.TakeOrCreate();
        SerializeToNetWriter(value, netWriter, id, true, true, responseMessageId);
        peer.Send(netWriter, DeliveryMethod.ReliableUnordered);
        netWriter.Reset();
        DataWriterPool.Add(netWriter);
    }

#endregion
    
    protected void SerializeToNetWriter<T>(T obj, NetDataWriter writer, uint handlerId, bool isResponsible, bool isResponse = false, uint? responseMessageId = default)
    {
        using var stream = new MemoryStream();
        RuntimeTypeModel.Default.Serialize(stream, obj);

        writer.Put(handlerId);
        writer.Put(isResponse);
        writer.Put(isResponsible);
        if (responseMessageId.HasValue)
            writer.Put(responseMessageId.Value);
        writer.Put(stream.GetBuffer(), 0, (int)stream.Length);
    }
    
    private delegate void NetHandlerExpression(
        uint responseMessageId,
        ReadOnlySpan<byte> data,
        NetPeer peer,
        INetworkCallback callback);
    protected NetworkManagerBase(ITorchBase torchInstance, INetworkConfig config) : base(torchInstance)
    {
        Config = config;
        Listener.PeerConnectedEvent += ListenerOnPeerConnected;
        Listener.PeerDisconnectedEvent += ListenerOnPeerDisconnected;
        Listener.NetworkReceiveEvent += ListenerOnNetworkReceive;
    }
}