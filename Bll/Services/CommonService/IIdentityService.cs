using Dll.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Utility;
using Utility.Exceptions;
using Dll.Models.Common;
using Dll.Data.Entities;

namespace Bll.Services.CommonService
{
    public interface IIdentityService
    {
        Task<CommonResponse> GetAspNetUser();
        Task<CommonResponse> GetAspNetUserById(string userId);
        Task<CommonResponse> GetUserRoles(string userId);
        Task<CommonResponse> UpdateAspNetUser(object request);
        Task<CommonResponse> GetAspNetUserByMobile(string mobile);
        Task<CommonResponse> GetAspNetUserByMobile(string mobile, string authHeader, CancellationToken cancellationToken = default);
    }
    public class IdentityService : IIdentityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Serilog.ILogger _logger;
        private readonly IHttpClientFactory _iHttpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IdentityService
        (
            IUnitOfWork unitOfWork,
            Serilog.ILogger logger,
            IHttpClientFactory iHttpClientFactory,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _iHttpClientFactory = iHttpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<CommonResponse> GetAspNetUser()
        {
            CommonResponse commonResponse = new();
            string uri = "https://localhost:8090/api/account/getAspNetUserInfo";
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new CustomApplicationException("uri is not found");
            }
            var client = _iHttpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            if (_httpContextAccessor?.HttpContext?.Request?.Headers is null)
            {
                throw new CustomApplicationException("request headers are not available");
            }
            string? token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(token))
            {
                commonResponse.code = StatusCodes.Status401Unauthorized;
                commonResponse.messages = new List<string> { "authorization token is missing" };
                return commonResponse;
            }
            HttpRequestMessage httpRequestMessage = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers = { { "Authorization", $"Bearer {token}" } }
            };
            string responseBody;
            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(httpRequestMessage);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (TimeoutException)
            {
                commonResponse.code = StatusCodes.Status408RequestTimeout;
                commonResponse.messages = new List<string> { "request timed out. please try again later." };
                return commonResponse;
            }
            catch (Exception ex)
            {
                commonResponse.code = StatusCodes.Status500InternalServerError;
                commonResponse.messages = new List<string> { $"something went wrong: {ex.Message}" };
                return commonResponse;
            }
            if (!response.IsSuccessStatusCode)
            {
                commonResponse.code = (int)response.StatusCode;
                commonResponse.messages = new List<string> { $"request failed with status code {response.StatusCode}." };
                return commonResponse;
            }
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                throw new CustomApplicationException("response body is empty");
            }
            JObject jObjectData;
            try
            {
                jObjectData = JObject.Parse(responseBody);
            }
            catch (JsonException)
            {
                throw new CustomApplicationException("invalid json response format");
            }
            int code = jObjectData["code"]?.Value<int>() ?? 0;
            List<string> messages = jObjectData["messages"] is JArray messagesArray ? messagesArray.Select(m => m.ToString()).ToList() : new List<string>();
            if (code != StatusCodes.Status200OK)
            {
                return new CommonResponse { code = code, messages = messages };
            }
            if (jObjectData["data"] is not JObject userData)
            {
                commonResponse.code = StatusCodes.Status500InternalServerError;
                commonResponse.messages = new List<string> { "invalid response: user data is missing" };
                return commonResponse;
            }
            AspNetUser userInfo = new()
            {
                UserId = userData["userId"]?.Value<int>() ?? 0,
                Name = userData["name"]?.ToString() ?? string.Empty,
                UserName = userData["userName"]?.ToString() ?? string.Empty,
                Email = userData["email"]?.ToString() ?? string.Empty,
                PhoneNumber = userData["phoneNumber"]?.ToString() ?? string.Empty,
                EmailConfirmed = userData["emailConfirmed"]?.Value<bool>() ?? false,
                PhoneNumberConfirmed = userData["phoneNumberConfirmed"]?.Value<bool>() ?? false,
                LockoutEnabled = userData["lockoutEnabled"]?.Value<bool>() ?? false,
                AccessFailedCount = userData["accessFailedCount"]?.Value<int>() ?? 0,
                Roles = userData["role"] is JArray rolesArray ? rolesArray.Select(r => r.ToString()).ToList() : new List<string>(),
                TrxLimitPackageId = userData["trxLimitPackageId"]?.Value<long>() ?? 0,
                TrxFeePackageId = userData["trxFeePackageId"]?.Value<long>() ?? 0,
                IsActive = userData["isActive"]?.Type == JTokenType.Null ? false : userData["isActive"]?.Value<bool>() ?? false,
                Status = userData["status"]?.ToString() ?? string.Empty
            };
            commonResponse.code = StatusCodes.Status200OK;
            commonResponse.messages = new List<string> { "Information retrieved successfully." };
            commonResponse.data = userInfo;
            return commonResponse;
        }
        public Task<CommonResponse> GetAspNetUserById(string userId)
        {
            throw new NotImplementedException();
        }
        public Task<CommonResponse> GetUserRoles(string userId)
        {
            throw new NotImplementedException();
        }
        public Task<CommonResponse> UpdateAspNetUser(object request)
        {
            throw new NotImplementedException();
        }
        public async Task<CommonResponse> GetAspNetUserByMobile(string mobile)
        {
            CommonResponse commonResponse = new();

            if (string.IsNullOrWhiteSpace(mobile) || string.IsNullOrEmpty(mobile))
            {
                commonResponse.code = StatusCodes.Status400BadRequest;
                commonResponse.messages = new List<string> { "mobile number is required" };
                return commonResponse;
            }
            string uri = "https://localhost:8090/api/account/getAspNetUserInfoByMobile";
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new CustomApplicationException("uri is not found");
            }
            var client = _iHttpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            if (_httpContextAccessor?.HttpContext?.Request?.Headers is null)
            {
                throw new CustomApplicationException("request headers are not available");
            }
            string? token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(token))
            {
                commonResponse.code = StatusCodes.Status401Unauthorized;
                commonResponse.messages = new List<string> { "authorization token is missing" };
                return commonResponse;
            }
            var values = new Dictionary<string, string>
            {
                { "Mobile", mobile }
            };
            using var content = new FormUrlEncodedContent(values);

            HttpRequestMessage httpRequestMessage = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri),
                Headers = { { "Authorization", $"Bearer {token}" } },
                Content = content
            };
            string responseBody;
            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(httpRequestMessage);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (TimeoutException)
            {
                commonResponse.code = StatusCodes.Status408RequestTimeout;
                commonResponse.messages = new List<string> { "request timed out. please try again later." };
                return commonResponse;
            }
            catch (Exception ex)
            {
                commonResponse.code = StatusCodes.Status500InternalServerError;
                commonResponse.messages = new List<string> { $"something went wrong: {ex.Message}" };
                return commonResponse;
            }
            if (!response.IsSuccessStatusCode)
            {
                commonResponse.code = (int)response.StatusCode;
                commonResponse.messages = new List<string> { $"request failed with status code {response.StatusCode}." };
                return commonResponse;
            }
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                throw new CustomApplicationException("response body is empty");
            }
            JObject jObjectData;
            try
            {
                jObjectData = JObject.Parse(responseBody);
            }
            catch (JsonException)
            {
                throw new CustomApplicationException("invalid json response format");
            }
            int code = jObjectData["code"]?.Value<int>() ?? 0;
            List<string> messages = jObjectData["messages"] is JArray messagesArray ? messagesArray.Select(m => m.ToString()).ToList() : new List<string>();
            if (code != StatusCodes.Status200OK)
            {
                return new CommonResponse { code = code, messages = messages };
            }
            if (jObjectData["data"] is not JObject userData)
            {
                commonResponse.code = StatusCodes.Status500InternalServerError;
                commonResponse.messages = new List<string> { "invalid response: user data is missing" };
                return commonResponse;
            }
            AspNetUser userInfo = new()
            {
                UserId = userData["userId"]?.Value<int>() ?? 0,
                Name = userData["name"]?.ToString() ?? string.Empty,
                UserName = userData["userName"]?.ToString() ?? string.Empty,
                Email = userData["email"]?.ToString() ?? string.Empty,
                PhoneNumber = userData["phoneNumber"]?.ToString() ?? string.Empty,
                EmailConfirmed = userData["emailConfirmed"]?.Value<bool>() ?? false,
                PhoneNumberConfirmed = userData["phoneNumberConfirmed"]?.Value<bool>() ?? false,
                LockoutEnd = userData["lockoutEnd"]?.Type == JTokenType.Null ? (DateTimeOffset?)null : userData["lockoutEnd"]?.ToObject<DateTimeOffset>(),
                LockoutEnabled = userData["lockoutEnabled"]?.Value<bool>() ?? false,
                AccessFailedCount = userData["accessFailedCount"]?.Value<int>() ?? 0,
                Roles = userData["role"] is JArray rolesArray ? rolesArray.Select(r => r.ToString()).ToList() : new List<string>(),
                TrxLimitPackageId = userData["trxLimitPackageId"]?.Value<long>() ?? 0,
                TrxFeePackageId = userData["trxFeePackageId"]?.Value<long>() ?? 0,
                IsActive = userData["isActive"]?.Type == JTokenType.Null ? false : userData["isActive"]?.Value<bool>() ?? false,
                Status = userData["status"]?.ToString() ?? string.Empty
            };
            commonResponse.code = StatusCodes.Status200OK;
            commonResponse.messages = new List<string> { "Information retrieved successfully." };
            commonResponse.data = userInfo;
            return commonResponse;
        }
        public async Task<CommonResponse> GetAspNetUserByMobile(string mobile, string authToken, CancellationToken cancellationToken = default)
        {
            CommonResponse commonResponse = new();

            if (string.IsNullOrWhiteSpace(mobile) || string.IsNullOrEmpty(mobile))
            {
                commonResponse.code = StatusCodes.Status400BadRequest;
                commonResponse.messages = new List<string> { "mobile number is required" };
                return commonResponse;
            }
            string uri = "https://localhost:8090/api/account/getAspNetUserInfoByMobile";
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new CustomApplicationException("uri is not found");
            }
            var client = _iHttpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            string? token = authToken;
            if (string.IsNullOrEmpty(token))
            {
                commonResponse.code = StatusCodes.Status401Unauthorized;
                commonResponse.messages = new List<string> { "authorization token is missing" };
                return commonResponse;
            }
            var values = new Dictionary<string, string>
            {
                { "Mobile", mobile }
            };
            using var content = new FormUrlEncodedContent(values);

            HttpRequestMessage httpRequestMessage = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri),
                Headers = { { "Authorization", $"Bearer {token}" } },
                Content = content
            };
            string responseBody;
            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(httpRequestMessage, cancellationToken);
                responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                commonResponse.code = StatusCodes.Status499ClientClosedRequest;
                commonResponse.messages = new List<string> { "request was cancelled" };
                return commonResponse;
            }
            catch (TimeoutException)
            {
                commonResponse.code = StatusCodes.Status408RequestTimeout;
                commonResponse.messages = new List<string> { "request timed out. please try again later." };
                return commonResponse;
            }
            catch (Exception ex)
            {
                commonResponse.code = StatusCodes.Status500InternalServerError;
                commonResponse.messages = new List<string> { $"something went wrong: {ex.Message}" };
                return commonResponse;
            }
            if (!response.IsSuccessStatusCode)
            {
                commonResponse.code = (int)response.StatusCode;
                commonResponse.messages = new List<string> { $"request failed with status code {response.StatusCode}." };
                return commonResponse;
            }
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                throw new CustomApplicationException("response body is empty");
            }
            JObject jObjectData;
            try
            {
                jObjectData = JObject.Parse(responseBody);
            }
            catch (JsonException)
            {
                throw new CustomApplicationException("invalid json response format");
            }
            int code = jObjectData["code"]?.Value<int>() ?? 0;
            List<string> messages = jObjectData["messages"] is JArray messagesArray ? messagesArray.Select(m => m.ToString()).ToList() : new List<string>();
            if (code != StatusCodes.Status200OK)
            {
                return new CommonResponse { code = code, messages = messages };
            }
            if (jObjectData["data"] is not JObject userData)
            {
                commonResponse.code = StatusCodes.Status500InternalServerError;
                commonResponse.messages = new List<string> { "invalid response: user data is missing" };
                return commonResponse;
            }
            AspNetUser userInfo = new()
            {
                UserId = userData["userId"]?.Value<int>() ?? 0,
                Name = userData["name"]?.ToString() ?? string.Empty,
                UserName = userData["userName"]?.ToString() ?? string.Empty,
                Email = userData["email"]?.ToString() ?? string.Empty,
                PhoneNumber = userData["phoneNumber"]?.ToString() ?? string.Empty,
                EmailConfirmed = userData["emailConfirmed"]?.Value<bool>() ?? false,
                PhoneNumberConfirmed = userData["phoneNumberConfirmed"]?.Value<bool>() ?? false,
                LockoutEnd = userData["lockoutEnd"]?.Type == JTokenType.Null ? (DateTimeOffset?)null : userData["lockoutEnd"]?.ToObject<DateTimeOffset>(),
                LockoutEnabled = userData["lockoutEnabled"]?.Value<bool>() ?? false,
                AccessFailedCount = userData["accessFailedCount"]?.Value<int>() ?? 0,
                Roles = userData["role"] is JArray rolesArray ? rolesArray.Select(r => r.ToString()).ToList() : new List<string>(),
                TrxLimitPackageId = userData["trxLimitPackageId"]?.Value<long>() ?? 0,
                TrxFeePackageId = userData["trxFeePackageId"]?.Value<long>() ?? 0,
                IsActive = userData["isActive"]?.Type == JTokenType.Null ? false : userData["isActive"]?.Value<bool>() ?? false,
                Status = userData["status"]?.ToString() ?? string.Empty
            };
            commonResponse.code = StatusCodes.Status200OK;
            commonResponse.messages = new List<string> { "Information retrieved successfully." };
            commonResponse.data = userInfo;
            return commonResponse;
        }
    }
}
