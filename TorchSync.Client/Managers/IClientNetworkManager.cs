using System.Net;
using TorchSync.Shared.Managers;
using TorchSync.Shared.Managers.Network;
namespace TorchSync.Client.Managers;

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
