using System.Collections.Generic;
using ProtoBuf;
namespace TorchSync.Shared.Messages;

[ProtoContract]
public struct ServerInfoMessage
{
    [ProtoMember(1)]
    public List<PlayerInfo> Players { get; set; } = new();

    public ServerInfoMessage(){}
}

[ProtoContract]
public struct PlayerInfo
{
    [ProtoMember(1)]
    public ulong ClientId { get; set; }
    [ProtoMember(2)]
    public string Name { get; set; }

    public void Deconstruct(out ulong clientId, out string name)
    {
        clientId = ClientId;
        name = Name;
    }
}
