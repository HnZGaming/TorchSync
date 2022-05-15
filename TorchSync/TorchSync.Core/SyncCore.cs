using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Sandbox.Game.World;
using Utils.General;

namespace TorchSync.Core
{
    public sealed class SyncCore
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly SyncNetwork _network;

        public SyncCore(SyncNetwork network)
        {
            _network = network;
        }

        public void Close()
        {
        }

        public void UpdateConfig()
        {
            if (!Config.Instance.CountRemotePlayerCount)
            {
                MyDedicatedServerBase_UpdateSteamServerData.UpdateRemotePlayerCollection(Array.Empty<RemotePlayer>());
                Log.Info("cleared remote player list");
            }
        }

        public void Update()
        {
            if (Config.Instance.CountRemotePlayerCount)
            {
                if (MySession.Static.GameplayFrameCounter % (60 * 10) == 0)
                {
                    UpdateRemotePlayerCollection().Forget(Log);
                }
            }
        }

        async Task UpdateRemotePlayerCollection()
        {
            await TaskUtils.MoveToThreadPool();

            var results = new List<Task<RemotePlayer[]>>();
            foreach (var remotePort in Config.Instance.RemotePortsSet)
            {
                var r = _network.GetRemotePlayers(remotePort);
                results.Add(r);
            }

            var result = await Task.WhenAll(results);
            var remotePlayers = result.SelectMany(r => r);
            MyDedicatedServerBase_UpdateSteamServerData.UpdateRemotePlayerCollection(remotePlayers);
        }
    }
}