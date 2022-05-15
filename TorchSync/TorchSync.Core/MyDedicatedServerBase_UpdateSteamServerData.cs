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
            _remotePlayerInfos.ClearList();

            foreach (var remotePlayer in remotePlayers)
            {
                _remotePlayerInfos.Add(remotePlayer);
            }

            _remotePlayerInfos.ApplyChanges();
            Log.Debug($"remote players: {_remotePlayerInfos.ToStringSeq()}");

            _remoteDataDirty = true;
        }

        static bool UpdateSteamServerData(MyDedicatedServerBase __instance, ref bool __field_m_gameServerDataDirty, ref IDictionary __field_m_memberData)
        {
            // original code copied & pasted
            if (__field_m_gameServerDataDirty)
            {
                MyGameService.GameServer.SetMapName(__instance.WorldName);
                MyGameService.GameServer.SetMaxPlayerCount(__instance.MemberLimit);
                foreach (DictionaryEntry o in __field_m_memberData)
                {
                    var steamId = (ulong)o.Key;
                    var name = __instance.GetMemberName(steamId);
                    MyGameService.GameServer.BrowserUpdateUserData(steamId, name, 0);
                }

                __field_m_gameServerDataDirty = false;
            }

            // feed remote players if the collection was updated
            if (!_remoteDataDirty) return false;
            _remoteDataDirty = false;

            Log.Debug($"feeding remote player collection: {_remotePlayerInfos.Count}");

            // feed the remote player list
            foreach (var remotePlayer in _remotePlayerInfos)
            {
                var (steamId, name) = (remotePlayer.SteamId, remotePlayer.Name);
                MyGameService.GameServer.BrowserUpdateUserData(steamId, name, 0);
                Log.Debug($"<{steamId}>: \"{name}\"");
            }

            // remote players -> bots
            MyGameService.GameServer.SetBotPlayerCount(_remotePlayerInfos.Count);

            return false;
        }
    }
}