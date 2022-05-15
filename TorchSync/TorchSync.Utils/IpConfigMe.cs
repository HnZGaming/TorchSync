using System;
using System.IO;
using System.Net;

namespace TorchSync.Utils
{
    public static class IpConfigMe
    {
        // https://stackoverflow.com/questions/3253701
        public static string GetPublicIpAddress()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");
            request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command
            request.Method = "GET";

            using var response = request.GetResponse();
            var responseStream = response.GetResponseStream() ?? throw new InvalidOperationException("response fail");
            using var reader = new StreamReader(responseStream);
            var publicIPAddress = reader.ReadToEnd();
            return publicIPAddress.Replace("\n", "");
        }
    }
}