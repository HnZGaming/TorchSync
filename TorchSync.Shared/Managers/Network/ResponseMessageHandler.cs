using LiteNetLib;
namespace TorchSync.Shared.Managers.Network;

public delegate TResponse ResponseMessageHandler<in TMessage, out TResponse>(TMessage message, NetPeer peer);