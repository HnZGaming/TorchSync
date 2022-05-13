using System.Collections.Concurrent;
namespace GarbageSync.Shared.Utils;

public static class CollectionExtensions
{
    public static T TakeOrCreate<T>(this ConcurrentBag<T> bag) where T : class, new()
    {
        bag.TryTake(out var element);
        return element ?? new();
    }
}