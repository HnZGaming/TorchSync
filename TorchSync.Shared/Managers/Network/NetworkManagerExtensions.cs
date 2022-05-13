namespace TorchSync.Shared.Managers.Network;

public static class NetworkManagerExtensions
{
    public static void AddHandler<TMessage>(this INetworkManagerBase manager, MessageHandler<TMessage> handler) where TMessage : new()
    {
        manager.AddHandlerDescriptor<TMessage>(new(handler));
    }
    
    public static void AddHandler<TMessage, TResponse>(this INetworkManagerBase manager, ResponseMessageHandler<TMessage, TResponse> handler) 
        where TMessage : new()
        where TResponse : new()
    {
        manager.AddHandlerDescriptor<TMessage>(new(handler, typeof(TResponse)));
    }
}