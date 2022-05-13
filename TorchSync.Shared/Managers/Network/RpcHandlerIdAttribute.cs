using System;

namespace TorchSync.Shared.Managers.Network;

[AttributeUsage(AttributeTargets.Method)]
public class RpcHandlerIdAttribute : Attribute
{
    public uint Id { get; }

    public RpcHandlerIdAttribute(uint id)
    {
        Id = id;
    }
}