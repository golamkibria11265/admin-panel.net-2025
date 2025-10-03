using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Api.Extensions
{
    public static class ApiServiceExtension
    {
        public static IServiceCollection AddApiServices(this IServiceCollection _iServiceCollection, IConfiguration _iConfiguration)
        {
            _iServiceCollection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.Authority = _iConfiguration.GetValue<string>("ExternalServer:IdentityServer:Url:BaseUrl");

                #region aud should be in the token. if not, set it in claims during token generation for both client and user token validation
                options.Audience = "offline_access";
                #endregion

                options.RequireHttpsMetadata = false;
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var httpContext = context.HttpContext;
                        string? token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                        if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token))
                        {
                            context.Fail("Fail");
                            return;
                        }
                        var clientFactory = httpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                        using var client = clientFactory.CreateClient();
                        var requestBody = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("token", token),
                            new KeyValuePair<string, string>("client_id", _iConfiguration.GetValue<string>("ExternalServer:IdentityServer:Credential:ClientId")!),
                            new KeyValuePair<string, string>("client_secret", _iConfiguration.GetValue<string>("ExternalServer:IdentityServer:Credential:ClientSecret")!)
                        });
                        var response = await client.PostAsync(_iConfiguration.GetValue<string>("ExternalServer:IdentityServer:Url:IntrospectUrl"), requestBody);
                        if (!response.IsSuccessStatusCode)
                        {
                            context.Fail("Fail");
                            return;
                        }
                        var introspectResponse = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(introspectResponse) || string.IsNullOrWhiteSpace(introspectResponse))
                        {
                            context.Fail("Fail");
                            return;
                        }
                        var jsonData = JObject.Parse(introspectResponse);
                        if (!jsonData.ContainsKey("active") || jsonData["active"]?.Value<bool>() != true)
                        {
                            context.Fail("Fail");
                            return;
                        }
                    },
                    OnChallenge = async context =>
                    {
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";

                            var defaultResponse = new
                            {
                                code = 401,
                                messages = new[] { "Unauthorized" },
                                data = "N/A"
                            };
                            var jsonResponse = JsonConvert.SerializeObject(defaultResponse);
                            await context.Response.WriteAsync(jsonResponse, Encoding.UTF8);
                        }
                        context.HandleResponse();
                    }
                };
            });
            return _iServiceCollection;
        }
    }
}
