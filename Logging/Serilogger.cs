using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;

namespace Common.Logging
{
    public static class Serilogger
    {
        public static void ConfigureLogger(LoggerConfiguration loggerConfig, IConfiguration configuration)
        {
            string? logPath = configuration["FilePaths:LogFileRootDirectory"];
            string? elasticUri = configuration["ElasticConfiguration:Uri"];
            string? elasticUserName = configuration["ElasticConfiguration:UserName"];
            string? elasticPassword = configuration["ElasticConfiguration:Password"];
            string? environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

            loggerConfig
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error)
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(path: Path.Combine(string.IsNullOrEmpty(logPath) ? "logs" : logPath, "webapi-.log"), rollingInterval: RollingInterval.Hour, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .Enrich.WithProperty("Environment", environmentName);

            // Elasticsearch is optional: only configured when a URI is provided (credentials from environment variables).
            if (!string.IsNullOrEmpty(elasticUri))
            {
                loggerConfig.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    IndexFormat = $"applogs-{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{environmentName.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}",
                    AutoRegisterTemplate = true,
                    NumberOfShards = 2,
                    NumberOfReplicas = 1,
                    ModifyConnectionSettings = x =>
                    {
                        x.BasicAuthentication(elasticUserName, elasticPassword);
                        // TLS certificate validation stays ON. If Elasticsearch uses an internal CA, install that
                        // CA certificate on the server instead of disabling validation.
                        return x;
                    }
                });
            }
        }
    }
}
