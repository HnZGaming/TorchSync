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

        public static bool Enabled { get; set; }

        public static void Patch(PatchContext ctx)
        {
            var patchee = typeof(MyDedicatedServerBase).GetMethod(nameof(UpdateSteamServerData), BindingFlags.Instance | BindingFlags.NonPublic);
            var patcher = typeof(MyDedicatedServerBase_UpdateSteamServerData).GetMethod(nameof(UpdateSteamServerData), BindingFlags.Static | BindingFlags.NonPublic);
            ctx.GetPattern(patchee).Prefixes.Add(patcher);
        }

        public static void UpdateRemotePlayerCollection(IEnumerable<RemotePlayer> remotePlayers)
        {
            _remotePlayerInfos.ClearList();

            foreach (var remotePlayer in remotePlayers)
            {
                _remotePlayerInfos.Add(remotePlayer);
            }

            _remotePlayerInfos.ApplyChanges();
            Log.Info($"remote players: {_remotePlayerInfos.ToStringSeq()}");

            _remoteDataDirty = true;
        }

        static bool UpdateSteamServerData(MyDedicatedServerBase __instance, ref bool __field_m_gameServerDataDirty)
        {
            if (!Enabled) return true;

            if (__field_m_gameServerDataDirty)
            {
                __field_m_gameServerDataDirty = false;
                MyGameService.GameServer.SetMapName(__instance.WorldName);
                MyGameService.GameServer.SetMaxPlayerCount(__instance.MemberLimit);
            }

            if (!_remoteDataDirty) return false;
            _remoteDataDirty = false;

            foreach (var remotePlayer in _remotePlayerInfos)
            {
                var (steamId, name) = (remotePlayer.SteamId, remotePlayer.Name);
                MyGameService.GameServer.BrowserUpdateUserData(steamId, name, 0);
            }

            foreach (var steamId in __instance.Members)
            {
                var name = __instance.GetMemberName(steamId);
                MyGameService.GameServer.BrowserUpdateUserData(steamId, name, 0);
            }

            MyGameService.GameServer.SetBotPlayerCount(_remotePlayerInfos.Count);

            return false;
        }
    }
}