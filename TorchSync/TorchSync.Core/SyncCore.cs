using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Sandbox;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch.API.Managers;
using Torch.Mod;
using Torch.Mod.Messages;
using TorchSync.Http;
using TorchSync.Utils;
using Utils.General;
using Utils.Torch;
using VRage.Game.ModAPI;
using VRageMath;

namespace TorchSync.Core
{
    public sealed class SyncCore : ISyncHttpServerEndpoint
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        SyncHttpServer _httpServer;
        SyncHttpClient _httpClient;
        IChatManagerServer _chatManager;

        public void Start(IChatManagerServer chatManagerServer)
        {
            _chatManager = chatManagerServer;
            _chatManager.MessageRecieved += OnMessageReceived;

            _httpServer = new SyncHttpServer("localhost", Config.Instance.Port, this);
            _httpServer.Start().Forget(Log);

            _httpClient = new SyncHttpClient();
        }

        public void Close()
        {
            _httpServer?.Close();
            _httpClient?.Close();
        }

        public void OnConfigChanged(bool restartHttpServer)
        {
            if (restartHttpServer)
            {
                _httpServer?.Close();
                _httpServer = new SyncHttpServer("localhost", Config.Instance.Port, this);
                _httpServer.Start().Forget(Log);

                _httpClient?.Close();
                _httpClient = new SyncHttpClient();
            }

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

        void OnMessageReceived(TorchChatMessage msg, ref bool consumed)
        {
            if (msg.AuthorSteamId == null) // server message
            {
                if (!Config.Instance.RemoteChatAuthorSet.Contains(msg.Author))
                {
                    return;
                }
            }
            else if (msg.Channel != ChatChannel.Global)
            {
                return;
            }

            Log.Info($"chat message: {msg.Message}");

            var chatMessage = new ChatMessage
            {
                Header = Config.Instance.Name,
                Name = msg.Author,
                Message = msg.Message,
            };

            foreach (var remoteIp in Config.Instance.RemoteIpsSet)
            {
                PostChatMessage(remoteIp, chatMessage).Forget(Log);
            }
        }

        async Task UpdateRemotePlayerCollection()
        {
            await TaskUtils.MoveToThreadPool();

            var results = new List<Task<RemotePlayer[]>>();
            foreach (var remotePort in Config.Instance.RemoteIpsSet)
            {
                var r = GetRemotePlayers(remotePort);
                results.Add(r);
            }

            var result = await Task.WhenAll(results);
            var remotePlayers = result.SelectMany(r => r);
            MyDedicatedServerBase_UpdateSteamServerData.UpdateRemotePlayerCollection(remotePlayers);
        }

        async Task<RemotePlayer[]> GetRemotePlayers(IpPort remoteIp)
        {
            var (success, body) = await _httpClient.SendRequest(remoteIp, "/v1/get_remote_players", "{}");
            if (!success)
            {
                var error = JsonConvert.DeserializeObject<SyncHttpError>(body);
                Log.Warn($"{nameof(GetRemotePlayers)}() error: {error.Message}");
                return Array.Empty<RemotePlayer>();
            }

            Log.Debug($"get remote players: {body}");
            return JToken.Parse(body)["remote_players"].ToObject<RemotePlayer[]>();
        }

        public async Task PostChatMessage(IpPort remoteIp, ChatMessage chatMessage)
        {
            var reqBody = JsonConvert.SerializeObject(chatMessage);
            var (success, body) = await _httpClient.SendRequest(remoteIp, "/v1/post_chat_message", reqBody);
            if (!success)
            {
                var error = JsonConvert.DeserializeObject<SyncHttpError>(body);
                Log.Warn($"{nameof(PostChatMessage)}() error: {error.Message}");
            }
        }

        async Task<SyncHttpResult> ISyncHttpServerEndpoint.Respond(string path, string body)
        {
            var pathComps = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var version = pathComps[0];
            if (version != "v1")
            {
                return SyncHttpResult.FromError($"invalid version: {version}");
            }

            switch (pathComps[1])
            {
                case "get_remote_players":
                {
                    Log.Debug("get_remote_players");

                    await VRageUtils.MoveToGameLoop();

                    var localPlayers = GetLocalPlayers();

                    await VRageUtils.MoveToThreadPool();
                    return SyncHttpResult.FromSuccess("remote_players", localPlayers.ToArray());
                }
                case "post_chat_message":
                {
                    Log.Debug("post_chat_message");

                    var chatMessage = JsonConvert.DeserializeObject<ChatMessage>(body);
                    await VRageUtils.MoveToGameLoop();

                    var author = $"<{chatMessage.Header}> {chatMessage.Name}";
                    _chatManager.SendMessageAsOther(author, chatMessage.Message, Color.Orange);

                    await VRageUtils.MoveToThreadPool();
                    return SyncHttpResult.FromSuccess("{}");
                }
                case "get_info":
                {
                    Log.Debug("get_info");

                    var info = GetServerInfo();
                    var res = JsonConvert.SerializeObject(info);
                    Log.Info($"response: {res}");
                    return SyncHttpResult.FromSuccess(res);
                }
                default:
                {
                    return SyncHttpResult.FromError($"api not found for path: {path}");
                }
            }
        }

        ServerInfo GetServerInfo() => new()
        {
            Name = Config.Instance.Name,
            HttpAddress = new IpPort("localhost", Config.Instance.Port),
            GameAddress = new IpPort
            {
                Ip = IpConfigMe.GetPublicIpAddress(),
                Port = MySandboxGame.ConfigDedicated.ServerPort,
            },
        };

        public async Task<List<ServerInfo>> GetRemoteServerInfo()
        {
            var tasks = new List<Task<SyncHttpResult>>();
            foreach (var remoteIp in Config.Instance.RemoteIpsSet)
            {
                var task = _httpClient.SendRequest(remoteIp, "/v1/get_info", "{}");
                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);
            var outs = new List<ServerInfo>();
            foreach (var (success, body) in results)
            {
                if (!success)
                {
                    var error = JsonConvert.DeserializeObject<SyncHttpError>(body);
                    Log.Warn($"{nameof(GetRemoteServerInfo)}() error: {error.Message}");
                    continue;
                }

                var serverInfo = JsonConvert.DeserializeObject<ServerInfo>(body);
                outs.Add(serverInfo);
            }

            return outs;
        }

        static IEnumerable<RemotePlayer> GetLocalPlayers()
        {
            var onlinePlayers = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(onlinePlayers);

            return onlinePlayers.Select(p => new RemotePlayer
            {
                SteamId = p.SteamUserId,
                Name = p.DisplayName,
            });
        }

        public async Task Jump(ulong steamId, IpPort address)
        {
            Log.Info($"{nameof(Jump)}({steamId}, {address})");

            var steamAddress = $"steam://{address.Ip}:{address.Port}";
            Log.Info($"jumping: {steamAddress}");

            var msg = new JoinServerMessage(steamAddress);
            ModCommunication.SendMessageTo(msg, steamId);
        }
    }
}