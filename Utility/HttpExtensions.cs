using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Utility.Models.ResponseModels;

namespace Utility
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, PaginationHeaderResponse header)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            #region previous peace of code
            //response.Headers.Append("Pagination", JsonSerializer.Serialize(header, jsonOptions));
            //response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
            #endregion
            response.Headers.Append("Pagination", JsonSerializer.Serialize(header, jsonOptions));
            response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
        }
        public static HeaderInfo GetHeaderInfo(this HttpRequest request)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var header in request.Headers)
            {
                headers[header.Key] = header.Value.FirstOrDefault() ?? string.Empty;
            }
            return new HeaderInfo
            {
                DeviceId = headers.GetValueOrDefault("DeviceId"),
                DeviceName = headers.GetValueOrDefault("DeviceName"),
                DeviceModel = headers.GetValueOrDefault("DeviceModel"),
                DeviceToken = headers.GetValueOrDefault("DeviceToken"),
                AuthToken = GetHeaderAuthToken(request)
            };
        }
        public static string? GetHeaderAuthToken(this HttpRequest request)
        {
            if (request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                string token = authHeader.FirstOrDefault()?.Replace("Bearer ", "").Trim() ?? string.Empty;
                return string.IsNullOrEmpty(token) ? null : token;
            }
            return string.Empty;
        }
    }
    public class HeaderInfo
    {
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceModel { get; set; }
        public string? DeviceToken { get; set; }
        public string? AuthToken { get; set; }
    }
}
