using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TorchSync.Http
{
    public sealed class SyncHttpResult
    {
        [JsonProperty("success", Required = Required.Always)]
        public bool Success;

        [JsonProperty("body", Required = Required.Always)]
        public string Body;

        public SyncHttpResult(bool success, string body)
        {
            Success = success;
            Body = body;
        }

        public void Deconstruct(out bool success, out string body)
        {
            success = Success;
            body = Body;
        }

        public static SyncHttpResult FromSuccess(string body)
        {
            return new SyncHttpResult(true, body);
        }

        public static SyncHttpResult FromSuccess(string key, object data)
        {
            var col = new Dictionary<string, object> { { key, data } };
            return new SyncHttpResult(true, JsonConvert.SerializeObject(col));
        }

        public static SyncHttpResult FromError(string message)
        {
            var error = new SyncHttpError { Message = message };
            return new SyncHttpResult(false, JsonConvert.SerializeObject(error));
        }

        public static SyncHttpResult FromException(Exception e)
        {
            var msg = (e.InnerException ?? e).Message;
            var error = new SyncHttpError { Message = msg };
            return new SyncHttpResult(false, JsonConvert.SerializeObject(error));
        }
    }
}