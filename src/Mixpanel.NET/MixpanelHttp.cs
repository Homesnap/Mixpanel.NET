using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mixpanel.NET
{
  /// <summary>
  ///     This helper class and interface largly exists to improve readability and testability since there is
  ///     no way to do that with the WebRequest class cleanly.
  /// </summary>
  public interface IMixpanelHttp
    {
        string Get(string uri, string query);
        string Post(string uri, string body);
        Task<string> GetAsync(string uri, string query);
        Task<string> PostAsync(string uri, string body);
    }

    public class MixpanelHttp : IMixpanelHttp
    {
        private static readonly HttpClient client;

        static MixpanelHttp()
        {
            var httpClientHandler = new HttpClientHandler()
            {
                //Proxy = new WebProxy("tgoff201910", 5555)
            };
            
            client = new HttpClient(httpClientHandler);
        }


        public string Get(string uri, string query)
        {
            var request = WebRequest.Create(uri + "?" + query);
            request.Proxy = new WebProxy("tgoff201910", 5555);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            return responseStream == null
                ? null
                : new StreamReader(responseStream).ReadToEnd();
        }

        public async Task<string> GetAsync(string uri, string query)
        {
            using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                    throw new WebException($"Unable to get URL: {response.ReasonPhrase}");

                using (var content = response.Content)
                {
                    return await content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        public string Post(string uri, string body)
        {
            var request = WebRequest.Create(uri);
            //request.Proxy = new WebProxy("tgoff201910", 5555);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            request.GetRequestStream().Write(bodyBytes, 0, bodyBytes.Length);
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            return responseStream == null
                ? null
                : new StreamReader(responseStream).ReadToEnd();
        }

        public async Task<string> PostAsync(string uri, string body)
        {
            using (var requestContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {
                using (var response = await client.PostAsync(uri, requestContent).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                        throw new WebException($"Unable to get URL: {response.ReasonPhrase}");

                    using (var content = response.Content)
                    {
                        return await content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}