using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Serilog;

namespace Utility.Services.HttpClientService
{
    public interface IHttpClientExtensions
    {
        Task<ResponseModel> PostRequest(string uri, string content, Dictionary<string, string>? headers = null, bool is_timeout = false, int timeout_seconds = 30, bool is_rcvc_true = false);
        Task<ResponseModel> PostXMLRequest(string uri, string content, Dictionary<string, string>? headers = null, bool is_timeout = false, int timeout_seconds = 30, bool is_rcvc_true = false);
        Task<ResponseModel> GetRequest(string uri, Dictionary<string, string>? headers = null, string request_body = "", bool is_timeout = false, int timeout_seconds = 30, bool is_rcvc_true = false);
        Task<ResponseModel> PostHttpFormURLEncodedRequest(string uri, Dictionary<string, string> urlEncodedItems, Dictionary<string, string>? headers = null, bool is_timeout = false, int timeout_seconds = 30, bool is_rcvc_true = false);
    }
    public class HttpClientExtensions : IHttpClientExtensions
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public HttpClientExtensions(ILogger logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<ResponseModel> PostRequest(string uri, string content, Dictionary<string, string>? headers = null, bool is_timeout = false, int timeout_seconds = 30, bool is_rcvc_true = false)
        {
            var responseModel = new ResponseModel();
            string responseBody = string.Empty;
            try
            {
                if (is_rcvc_true)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new
                    System.Net.Security.RemoteCertificateValidationCallback
                    (
                       delegate { return true; }
                    );
                }
                using var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (is_timeout)
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout_seconds);
                }
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                var serialized = new StringContent(content, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PostAsync(uri, serialized);
                responseModel.response_body = await response.Content.ReadAsStringAsync();
                responseModel.response_code = (int)response.StatusCode;
                responseModel.is_success = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                responseModel.response_body = ex.Message;
                _logger.Error(responseModel.response_body);
            }
            return responseModel;
        }
        public async Task<ResponseModel> GetRequest(string uri, Dictionary<string, string>? headers = null, string request_body = "", bool is_timeout = false, int timeout_seconds = 30, bool is_rcvc_true = false)
        {
            string responseBody = string.Empty;
            var responseModel = new ResponseModel();
            try
            {
                if (is_rcvc_true)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new
                    System.Net.Security.RemoteCertificateValidationCallback
                    (
                       delegate { return true; }
                    );
                }
                using var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                if (headers != null)
                {
                    if (is_timeout)
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeout_seconds);
                    }
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                HttpRequestMessage httpRequestMessage = new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri),
                    Content = !string.IsNullOrWhiteSpace(request_body) ? new StringContent(request_body, Encoding.UTF8, "application/json") : null
                };
                using HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
                responseModel.response_body = await response.Content.ReadAsStringAsync();
                responseModel.response_code = (int)response.StatusCode;
                responseModel.is_success = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                responseModel.response_body = ex.Message;
                _logger.Error(responseModel.response_body);
            }
            return responseModel;
        }
        public async Task<ResponseModel> PostHttpFormURLEncodedRequest(string uri, Dictionary<string, string> urlEncodedItems, Dictionary<string, string>? headers = null, bool is_timeout = false, int timeout_seconds = 30, bool is_rcvc_true = false)
        {
            var responseModel = new ResponseModel();
            try
            {
                if (is_rcvc_true)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new
                    System.Net.Security.RemoteCertificateValidationCallback
                    (
                       delegate { return true; }
                    );
                }
                using var client = _httpClientFactory.CreateClient();
                if (is_timeout)
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout_seconds);
                }
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                using HttpResponseMessage response = await client.PostAsync(uri, new FormUrlEncodedContent(urlEncodedItems));
                responseModel.response_body = await response.Content.ReadAsStringAsync();
                responseModel.response_code = (int)response.StatusCode;
                responseModel.is_success = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                responseModel.response_body = ex.Message;
                _logger.Error(responseModel.response_body);
            }
            return responseModel;
        }
        public async Task<ResponseModel> PostXMLRequest(string uri, string content, Dictionary<string, string>? headers = null, bool is_timeout = false, int timeout_seconds = 30, bool is_rcvc_true = false)
        {
            var responseModel = new ResponseModel();
            string responseBody = string.Empty;
            try
            {
                if (is_rcvc_true)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new
                    System.Net.Security.RemoteCertificateValidationCallback
                    (
                       delegate { return true; }
                    );
                }
                using var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (is_timeout)
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout_seconds);
                }
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                var serialized = new StringContent(content, Encoding.UTF8, "text/xml");
                using HttpResponseMessage response = await client.PostAsync(uri, serialized);
                responseModel.response_body = await response.Content.ReadAsStringAsync();
                responseModel.response_code = (int)response.StatusCode;
                responseModel.is_success = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                responseModel.response_body = ex.Message;
                _logger.Error(responseModel.response_body);
            }
            return responseModel;
        }
    }
    public class ResponseModel
    {
        public int response_code { get; set; } = 500;
        public string response_body { get; set; } = string.Empty;
        public bool is_success { get; set; } = false;
    }
}
