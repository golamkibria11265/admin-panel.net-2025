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
                .WriteTo.File(path: $"{logPath}.log", rollingInterval: RollingInterval.Hour, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticUri!))
                {
                    IndexFormat = $"applogs-{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{environmentName.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}",
                    AutoRegisterTemplate = true,
                    NumberOfShards = 2,
                    NumberOfReplicas = 1,
                    ModifyConnectionSettings = x =>
                    {
                        x.BasicAuthentication(elasticUserName, elasticPassword);
                        x.ServerCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) => true);
                        return x;
                    }
                })
                .Enrich.WithProperty("Environment", environmentName)
                .ReadFrom.Configuration(configuration);
        }
    }
}
