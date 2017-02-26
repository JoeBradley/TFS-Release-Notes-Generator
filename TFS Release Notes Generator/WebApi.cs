using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator
{
    public static class WebApi
    {
        private const string accessToken = "jhpqurmgtcvsp6ofoyrzldj2lm4wif6lz7qu7gln5cvijjpgmska";

        public static string Get(string url)
        {
            using (var client = new WebClient())
            {
                
                client.Headers.Add(HttpRequestHeader.Authorization, GetAuthHeader().ToString());

                return client.DownloadString(url);
            }
        }

        public static string Post(string url, string data)
        {
            using (var client = new WebClient())
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{accessToken}"));
                var authHeader = new AuthenticationHeaderValue("Basic", credentials);

                client.Headers.Add(HttpRequestHeader.Authorization, authHeader.ToString());
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.AcceptCharset, "utf-8");

                return client.UploadString(url, data);
            }
        }

        private static AuthenticationHeaderValue GetAuthHeader() {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{accessToken}"));
            var authHeader = new AuthenticationHeaderValue("Basic", credentials);
            return authHeader;
        }

    }
}
