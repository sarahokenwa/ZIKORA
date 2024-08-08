using Microsoft.Extensions.Logging;
using System.Text;
using Aornis;
using USSDMiddleware.Core.Interfaces.Component;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace USSDMiddleware.Core.Utilities
{
    public class HttpServiceUtil : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpServiceUtil> _log;

        public HttpServiceUtil(ILogger<HttpServiceUtil> log, HttpClient httpClient)
        {
            _log = log;
            _httpClient = httpClient;
        }

        public async Task<T> Get<T>(string url, IDictionary<string, string>? headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var response = await _httpClient.GetAsync(url);
            var rsp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(rsp);
            _httpClient.Dispose();
            return result;
        }

        public async Task<T> Post<T>(string url, IDictionary<string, string>? headers, string jsonContent)
        {
            if (headers !=null)
            {
                foreach (var header in headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            string rsp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(rsp);
            _httpClient.Dispose();
            return result;
        }

        public async Task<T> Post<T>(string url, HttpContent content, string token = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                HttpResponseMessage httpResponseMessage = await client.PostAsync(url, content);
                string response = await httpResponseMessage.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(response);
                client.Dispose();
                return result;
            };
        }

        public async Task<T> Get<T>(string url, string token = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                HttpResponseMessage httpResponseMessage = await client.GetAsync(url);
                string response = await httpResponseMessage.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(response);
                return result;
            };
        }

       }
}