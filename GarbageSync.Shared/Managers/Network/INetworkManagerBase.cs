using Torch.API.Managers;
namespace GarbageSync.Shared.Managers.Network;

public interface INetworkManagerBase : IManager
{
    void AddHandlerDescriptor<TMessage>(NetworkHandlerDescriptor<TMessage> descriptor) where TMessage : new();
}
public interface IResponseTask
{
    void OnResponse(ReadOnlySpan<byte> data);
}