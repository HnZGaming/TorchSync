using LiteNetLib;
namespace GarbageSync.Shared.Managers.Network;

public delegate void MessageHandler<in TMessage>(TMessage message, NetPeer peer);