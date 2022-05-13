using LiteNetLib;
namespace TorchSync.Shared.Managers.Network;

public delegate void MessageHandler<in TMessage>(TMessage message, NetPeer peer);