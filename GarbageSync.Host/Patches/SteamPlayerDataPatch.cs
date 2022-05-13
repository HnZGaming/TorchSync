using System.Reflection;
using GarbageSync.Shared.Messages;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Networking;
using Torch.Managers.PatchManager;
using Torch.Utils;
namespace GarbageSync.Host.Patches;

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

        foreach (var (clientId, name) in _playerInfos.Select(b => (b.ClientId, b.Name))
                     .Concat(__instance.Members.Select(b => (b, __instance.GetMemberName(b)))))
        {
            MyGameService.GameServer.BrowserUpdateUserData(clientId, name, 0);
        }

        MyGameService.GameServer.SetBotPlayerCount(_playerInfos.Count());

        return false;
    }
}
