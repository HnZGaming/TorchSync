using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TorchSync.Shared.Messages;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Networking;
using Torch.Managers.PatchManager;
using Torch.Utils;
namespace TorchSync.Host.Patches;

[PatchShim]
public static class SteamPlayerDataPatch
{
    [ReflectedMethodInfo(typeof(MyDedicatedServerBase), "UpdateSteamServerData")]
    private static readonly MethodInfo UpdateDataMethod = null!;

    [ReflectedMethodInfo(typeof(SteamPlayerDataPatch), nameof(Prefix))]
    private static readonly MethodInfo PrefixMethod = null!;

    private static bool _dataDirty;
    private static IEnumerable<PlayerInfo>? _playerInfos;

    public static void Patch(PatchContext context)
    {
        context.GetPattern(UpdateDataMethod).Prefixes.Add(PrefixMethod);
    }

    public static void OnPlayersData(IEnumerable<PlayerInfo> players)
    {
        _dataDirty = true;
        _playerInfos = players;
    }

    private static bool Prefix(MyDedicatedServerBase __instance, ref bool __field_m_gameServerDataDirty)
    {
        if (__field_m_gameServerDataDirty)
        {
            __field_m_gameServerDataDirty = false;
            MyGameService.GameServer.SetMapName(__instance.WorldName);
            MyGameService.GameServer.SetMaxPlayerCount(__instance.MemberLimit);
        }

        if (!_dataDirty || _playerInfos is null)
            return false;

        _dataDirty = false;

        var playerInfos = new List<PlayerInfo>();
        playerInfos.AddRange(_playerInfos);

        foreach (var memberId in __instance.Members)
        {
            playerInfos.Add(new PlayerInfo
            {
                ClientId = memberId,
                Name = __instance.GetMemberName(memberId),
            });
        }

        foreach (var (clientId, name) in playerInfos)
        {
            MyGameService.GameServer.BrowserUpdateUserData(clientId, name, 0);
        }

        MyGameService.GameServer.SetBotPlayerCount(_playerInfos.Count());

        return false;
    }
}
