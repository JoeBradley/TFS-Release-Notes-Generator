using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator
{
    public static class WebApi
    {
        private static string accessToken => ConfigurationManager.AppSettings["TfsAccessToken"];
        
        public static string Get(string url)
        {
            using (var client = new WebClient())
            {                
                client.Headers.Add(HttpRequestHeader.Authorization, GetAuthHeader().ToString());

                return client.DownloadString(url);
            }
        }

        /// <summary>
        /// <example>POST https://{instance}/DefaultCollection/[{project}/]_apis/wit/wiql?api-version={version}</example>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Post(string url, string data)
        {
            try
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
            catch (Exception ex) {
                throw;
            }
        }

        private static AuthenticationHeaderValue GetAuthHeader() {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{accessToken}"));
            var authHeader = new AuthenticationHeaderValue("Basic", credentials);
            return authHeader;
        }

    }
}
