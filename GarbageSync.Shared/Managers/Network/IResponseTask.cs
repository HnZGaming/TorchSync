namespace GarbageSync.Shared.Managers.Network;

public interface IResponseTask<T> : IResponseTask where T : new()
{
    public Task<T> Task { get; }
}