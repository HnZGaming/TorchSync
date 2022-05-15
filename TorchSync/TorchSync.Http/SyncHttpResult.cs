using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TorchSync.Http
{
    public record SyncHttpResult(bool Success, string Body)
    {
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