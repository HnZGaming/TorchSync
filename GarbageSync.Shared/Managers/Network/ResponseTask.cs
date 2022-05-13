﻿using ProtoBuf.Meta;
namespace GarbageSync.Shared.Managers.Network;

public class ResponseTask<T> : IResponseTask<T> where T : new() {
    public void OnResponse(ReadOnlySpan<byte> data)
    {
        _source.SetResult(RuntimeTypeModel.Default.Deserialize<T>(data));
    }

    private readonly TaskCompletionSource<T> _source = new();

    public Task<T> Task => _source.Task;
}