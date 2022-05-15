using System;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;

namespace TorchSync.Http
{
    public sealed class SyncHttpClient
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly HttpClient _client;

        public SyncHttpClient()
        {
            _client = new HttpClient();
        }

        public void Close()
        {
            _client.Dispose();
        }

        public async Task<SyncHttpResult> SendRequest(int port, string path, string body)
        {
            try
            {
                Log.Debug($"send request: {port} {path} {body}");

                var url = new Uri(SyncHttpServer.MakeUrl(port, path));
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                req.Content = new StringContent(body);
                var res = await _client.SendAsync(req);

                var success = (int)res.StatusCode == 200;
                var resBody = await res.Content.ReadAsStringAsync();
                return new SyncHttpResult(success, resBody);
            }
            catch (Exception e)
            {
                return SyncHttpResult.FromException(e);
            }
        }
    }
}