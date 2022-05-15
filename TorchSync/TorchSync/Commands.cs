using System.Collections.Generic;
using System.Linq;
using Torch.Commands;
using Torch.Commands.Permissions;
using TorchSync.Core;
using Utils.Torch;
using VRage.Game.ModAPI;

namespace TorchSync
{
    [Category("sync")]
    public sealed class Commands : CommandModule
    {
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

        [Command("chat")]
        [Permission(MyPromoteLevel.Moderator)]
        public void SendChatMessageToOtherServer(int port, string message) => this.CatchAndReport(async () =>
        {
            await Plugin.Core.PostChatMessage(port, new ChatMessage
            {
                Header = Config.Instance.ChatHeader,
                Name = "Server",
                Message = message,
            });
        });
    }
}