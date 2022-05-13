using LiteNetLib;
namespace GarbageSync.Shared.Managers.Network;

public delegate TResponse ResponseMessageHandler<in TMessage, out TResponse>(TMessage message, NetPeer peer);