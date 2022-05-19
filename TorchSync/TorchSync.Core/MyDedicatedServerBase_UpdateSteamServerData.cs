using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Networking;
using Torch.Managers.PatchManager;
using Utils.General;
using VRage.Collections;

namespace TorchSync.Core
{
    [PatchShim]
    public static class MyDedicatedServerBase_UpdateSteamServerData
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        static readonly ConcurrentCachingList<RemotePlayer> _remotePlayerInfos;
        static bool _remoteDataDirty;

        static MyDedicatedServerBase_UpdateSteamServerData()
        {
            _remotePlayerInfos = new ConcurrentCachingList<RemotePlayer>();
        }

        public static void Patch(PatchContext ctx)
        {
            var patchee = typeof(MyDedicatedServerBase).GetMethod(nameof(UpdateSteamServerData), BindingFlags.Instance | BindingFlags.NonPublic);
            var patcher = typeof(MyDedicatedServerBase_UpdateSteamServerData).GetMethod(nameof(UpdateSteamServerData), BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(patchee).Prefixes.Add(patcher);
        }

        public static void UpdateRemotePlayerCollection(IEnumerable<RemotePlayer> remotePlayers)
        {
            var oldSet = new HashSet<ulong>();
            var newSet = new HashSet<ulong>();

            foreach (var a in _remotePlayerInfos)
            {
                oldSet.Add(a.SteamId);
            }

            _remotePlayerInfos.ClearList();

            foreach (var remotePlayer in remotePlayers)
            {
                _remotePlayerInfos.Add(remotePlayer);
                newSet.Add(remotePlayer.SteamId);
            }

            _remotePlayerInfos.ApplyChanges();

            _remoteDataDirty = !newSet.SetEquals(oldSet);
            Log.Debug($"remote players (changed: {_remoteDataDirty}): {_remotePlayerInfos.ToStringSeq()}");
        }

        static bool UpdateSteamServerData(MyDedicatedServerBase __instance, ref bool __field_m_gameServerDataDirty, ref IDictionary __field_m_memberData)
        {
            if (!__field_m_gameServerDataDirty && !_remoteDataDirty) return false;

            Log.Debug($"updating steam server data; local changed: {__field_m_gameServerDataDirty}, remote changed: {_remoteDataDirty}");

            MyGameService.GameServer.SetMapName(__instance.WorldName);
            MyGameService.GameServer.SetMaxPlayerCount(__instance.MemberLimit);

            foreach (DictionaryEntry o in __field_m_memberData)
            {
                var steamId = (ulong)o.Key;
                var name = __instance.GetMemberName(steamId);
                MyGameService.GameServer.BrowserUpdateUserData(steamId, name, 0);
                Log.Debug($"added to steam server data (local): <{steamId}> \"{name}\"");
            }

            Log.Debug($"feeding remote player collection: {_remotePlayerInfos.Count}");

            // feed the remote player list
            foreach (var remotePlayer in _remotePlayerInfos)
            {
                var (steamId, name) = (remotePlayer.SteamId, remotePlayer.Name);
                MyGameService.GameServer.BrowserUpdateUserData(steamId, name, 0);
                Log.Debug($"added to steam server data (remote): <{steamId}> \"{name}\"");
            }

            // remote players -> bots
            MyGameService.GameServer.SetBotPlayerCount(_remotePlayerInfos.Count);

            __field_m_gameServerDataDirty = false;
            _remoteDataDirty = false;

            return false;
        }
    }
}