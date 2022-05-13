namespace GarbageSync.Shared.Managers.Network;

public record NetworkHandlerDescriptor<TMessage>(Delegate Handler, Type? ResponseMessageType = null);