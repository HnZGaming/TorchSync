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
            MyDedicatedServerBase_UpdateSteamServerData.Enabled = Config.Instance.CountRemotePlayerCount;
        }

        public void Update()
        {
            if (Config.Instance.CountRemotePlayerCount)
            {
                if (MySession.Static.GameplayFrameCounter % 600 == 0)
                {
                    UpdateRemotePlayerCollection().Forget(Log);
                }
            }
        }

        async Task UpdateRemotePlayerCollection()
        {
            await TaskUtils.MoveToThreadPool();

            Log.Info($"{nameof(UpdateRemotePlayerCollection)}()");

            var results = new List<Task<RemotePlayer[]>>();
            foreach (var otherPort in Config.Instance.OtherPorts)
            {
                var r = _network.GetRemotePlayers(otherPort.Number);
                results.Add(r);
            }

            var result = await Task.WhenAll(results);
            var remotePlayers = result.SelectMany(r => r);
            MyDedicatedServerBase_UpdateSteamServerData.UpdateRemotePlayerCollection(remotePlayers);
        }
    }
}