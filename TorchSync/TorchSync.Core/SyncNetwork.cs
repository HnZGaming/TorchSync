using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using Sandbox.ModAPI;
using TorchSync.Http;
using Utils.General;
using VRage.Game.ModAPI;

namespace TorchSync.Core
{
    public sealed class SyncNetwork : ISyncHttpServerEndpoint
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        SyncHttpServer _httpServer;
        SyncHttpClient _httpClient;

        public void Close()
        {
            _httpServer?.Close();
            _httpClient?.Close();
        }

        public void Start()
        {
            _httpServer = new SyncHttpServer(Config.Instance.Port, this);
            _httpServer.Start().Forget(Log);

            _httpClient = new SyncHttpClient();
        }

        public void UpdateConfig()
        {
            _httpServer?.Close();
            _httpServer = new SyncHttpServer(Config.Instance.Port, this);
            _httpServer.Start().Forget(Log);

            _httpClient?.Close();
            _httpClient = new SyncHttpClient();
        }

        public async Task<RemotePlayer[]> GetRemotePlayers(int port)
        {
            var (success, body) = await _httpClient.SendRequest(port, "/v1/remote_players", "{}");
            if (!success) return Array.Empty<RemotePlayer>();

            Log.Info($"get remote players: {body}");
            return JToken.Parse(body).Value<RemotePlayer[]>("remote_players");
        }

        async Task<SyncHttpResult> ISyncHttpServerEndpoint.TryProcess(string path, string body)
        {
            var pathComps = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var version = pathComps[0];
            if (version != "v1")
            {
                return SyncHttpResult.FromError($"invalid version: {version}");
            }

            switch (pathComps[1])
            {
                case "remote_players":
                {
                    Log.Info("remote players");

                    var onlinePlayers = new List<IMyPlayer>();
                    MyAPIGateway.Players.GetPlayers(onlinePlayers);

                    var remotePlayers = onlinePlayers.Select(p => new RemotePlayer
                    {
                        SteamId = p.SteamUserId,
                        Name = p.DisplayName,
                    });

                    return SyncHttpResult.FromSuccess("remote_players", remotePlayers.ToArray());
                }
                default:
                {
                    return SyncHttpResult.FromError($"api not found for path: {path}");
                }
            }
        }
    }
}