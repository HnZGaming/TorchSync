using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Torch.Commands;
using Torch.Commands.Permissions;
using Utils.General;
using Utils.Torch;
using VRage.Game.ModAPI;

namespace TorchSync
{
    [Category("sync")]
    public sealed class Commands : CommandModule
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        Plugin Plugin => (Plugin)Context.Plugin;

        public static IEnumerable<CommandAttribute> GetAllCommands()
        {
            return CommandModuleUtils
                .GetCommandMethods(typeof(Commands))
                .Select(p => p.Command);
        }

        [Command("commands", "Show all available commands for player")]
        [Permission(MyPromoteLevel.None)]
        public void ShowAllCommands() => this.CatchAndReport(() =>
        {
            this.ShowCommands();
        });

        [Command("configs", "Show all available config properties for player")]
        [Permission(MyPromoteLevel.None)]
        public void ShowAllConfigs() => this.CatchAndReport(() =>
        {
            this.GetOrSetProperty(Config.Instance);
        });

        [Command("reload", "Reload configs from the disk")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ReloadConfigs() => this.CatchAndReport(() =>
        {
            Plugin.ReloadConfig();
        });

        [Command("jump", "Jump to other server")]
        [Permission(MyPromoteLevel.None)]
        public void Jump(string destination = null) => this.CatchAndReport(async () =>
        {
            Context.Respond("Fetching server list...");
            var infoList = await Plugin.Core.GetRemoteServerInfo();
            if (infoList.Count == 0)
            {
                Context.Respond("No online servers found");
                return;
            }

            foreach (var serverInfo in infoList)
            {
                if (serverInfo.Name == destination)
                {
                    if (Context.Player is not { } player)
                    {
                        Context.Respond("must be a player");
                        return;
                    }

                    Context.Respond("Jumping...");
                    Plugin.Core.Jump(player.SteamUserId, serverInfo.GameAddress).Forget(Log);
                    return;
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Destinations: ");

            foreach (var serverInfo in infoList)
            {
                var name = serverInfo.Name;
                sb.AppendLine($"> {name}");
            }

            sb.Append("Type !sync jump <destination>");

            Context.Respond(sb.ToString());
        });
    }
}