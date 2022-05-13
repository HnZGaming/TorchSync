using System.Net;
using GarbageSync.Shared.Managers;
using GarbageSync.Shared.Managers.Network;
namespace GarbageSync.Client.Managers;

public interface IClientNetworkManager : INetworkManagerBase
{
    void SendMessage<TMessage>(TMessage message, uint handlerId)
        where TMessage : new();

    IResponseTask<TResponse> SendResponseMessage<TMessage, TResponse>(TMessage message, uint handlerId)
        where TMessage : new()
        where TResponse : new();
}

public interface IClientNetworkConfig : INetworkConfig
{
    IPAddress TargetIp { get; }
}
