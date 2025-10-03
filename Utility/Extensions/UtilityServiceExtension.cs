using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utility.Services.HttpClientService;
using Utility.Services.HttpClientService.Policy;
using Utility.Services.RabbitMqService;
using Utility.Services.RedisService;

namespace Utility.Extensions
{
    public static class UtilityServiceExtension
    {
        public static IServiceCollection AddUtilityServices(this IServiceCollection _iServiceCollection, IConfiguration _iConfiguration)
        {
            _iServiceCollection.AddScoped<IHttpClientExtensions, HttpClientExtensions>();
            _iServiceCollection.AddHttpClient();

            #region HttpClient registration along with policy
            _iServiceCollection.AddHttpClient("QuoteClient")
                .AddPolicyHandler(HttpPolicy.RetryPolicy())
                .AddPolicyHandler(HttpPolicy.CircuitBreakerPolicy());
            #endregion

            #region Redis registration
            var redisEnabled = _iConfiguration.GetValue<bool>("Redis:Enabled", false);
            if(redisEnabled)
            {
                _iServiceCollection.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = _iConfiguration.GetValue<string>("Redis:Server");
                    options.InstanceName = _iConfiguration.GetValue<string>("Redis:Database");
                });
                _iServiceCollection.AddScoped<IRedisService, RedisService>();
            }
            else
            {
                _iServiceCollection.AddScoped<IRedisService, OpRedisService>();
            }
            #endregion

            #region RabbitMQ registration
            _iServiceCollection.AddScoped<IRabbitMQMessageSender, RabbitMQMessageSender>();
            #endregion

            #region Handler registration
            #endregion

            return _iServiceCollection;
        }   
    }
}
