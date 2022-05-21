using System;
using System.Reflection;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Torch.Managers.PatchManager;
using Utils.General;

namespace TorchSync.Core
{
    [PatchShim]
    public static class Foo
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public static void Patch(PatchContext ctx)
        {
            ctx
                .GetPattern(typeof(MyDisconnectHelper).GetMethod(nameof(MyDisconnectHelper.Disconnect), BindingFlags.Instance | BindingFlags.Public))
                .Prefixes
                .Add(typeof(Foo).GetMethod(nameof(Disconnect), BindingFlags.Static | BindingFlags.NonPublic));

            LangUtils.TryFindType("Sandbox.Game.Entities.MyGridSkeleton", out var typeMyGridSkeleton);
            typeMyGridSkeleton.ThrowIfNull("type not found: MyGridSkeleton");

            ctx
                .GetPattern(typeMyGridSkeleton.GetMethod("RemoveUnusedBones", BindingFlags.Public | BindingFlags.Instance))
                .Prefixes
                .Add(typeof(Foo).GetMethod(nameof(RemoveUnusedBones), BindingFlags.Static | BindingFlags.NonPublic));
        }

        static void Disconnect(MyDisconnectHelper __instance, MyCubeGrid grid, MyCubeGrid.MyTestDisconnectsReason reason, MySlimBlock testBlock, bool testDisconnect)
        {
            Log.Info($"Disconnect({grid?.DisplayName}, {reason}, {testBlock?.BlockDefinition?.Id.TypeId}, {testDisconnect})");
        }

        static void RemoveUnusedBones(object __instance, MyCubeGrid grid)
        {
            Log.Info($"RemoveUnusedBones({grid?.DisplayName}");
        }
    }
}