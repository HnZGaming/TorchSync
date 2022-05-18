using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace TorchSync.Http
{
    public sealed class SyncHttpServer
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        readonly HttpListener _listener;
        readonly ISyncHttpServerEndpoint _endpoint;
        string _prefix;

        public static Uri MakeUrl(string ip, int port, string path)
        {
            var p = string.IsNullOrEmpty(path) ? "/" : path;
            var u = $"http://{ip}:{port}{p}";
            try
            {
                return new Uri(u);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(u, e);
            }
        }

        public SyncHttpServer(ISyncHttpServerEndpoint endpoint)
        {
            _endpoint = endpoint;
            _listener = new HttpListener();
        }

        public void SetPrefix(string ip, int port)
        {
            var prefix = MakeUrl(ip, port, "").AbsoluteUri;
            Log.Debug($"prefix: {prefix}");

            if (_prefix != null)
            {
                _listener.Prefixes.Remove(_prefix);
            }

            _prefix = prefix;
            _listener.Prefixes.Add(prefix);
        }

        public void Close()
        {
            _listener.Close();
        }

        public async Task Start()
        {
            _listener.Start();

            while (_listener.IsListening)
            {
                var ctx = await _listener.GetContextAsync();
                await Respond(ctx);
            }
        }

        async Task Respond(HttpListenerContext ctx)
        {
            try
            {
                var path = ctx.Request.RawUrl.Split('?')[0];
                var reqContent = ReadBody(ctx.Request);
                Log.Debug($"request: {path} {reqContent}");

                var (success, data) = await _endpoint.Respond(path, reqContent);

                WriteBody(ctx.Response, data);
                ctx.Response.StatusCode = success ? 200 : 500;
                ctx.Response.Close();

                Log.Debug($"response: {success}, {data}");
            }
            catch (Exception e)
            {
                Log.Error(e);

                var error = SyncHttpResult.FromException(e);
                WriteBody(ctx.Response, JsonConvert.SerializeObject(error));
                ctx.Response.StatusCode = 500;
                ctx.Response.Close();
            }
        }

        static string ReadBody(HttpListenerRequest req)
        {
            using var bodyReader = new StreamReader(req.InputStream, req.ContentEncoding);
            var body = bodyReader.ReadToEnd();
            return body;
        }

        static void WriteBody(HttpListenerResponse res, string body)
        {
            var contentBytes = Encoding.UTF8.GetBytes(body);
            res.ContentType = "application/json";
            res.OutputStream.Write(contentBytes, 0, contentBytes.Length);
        }
    }
}