using Api.Middlewares;
using Bll.Extensions;
using Dll.Extensions;
using Utility.Extensions;
using Serilog;
using System.Reflection;
using Bll.Services.Queue;
using Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

#region Serilog Configuration For Sending Log To Elastic Search
string? logpath = builder.Configuration["FilePaths:LogFileRootDirectory"];
Serilog.Debugging.SelfLog.Enable(Console.Out);
builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    configuration.Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error)
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(path: $"{logpath}.log", rollingInterval: RollingInterval.Hour, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(builder.Configuration["ElasticConfiguration:Uri"]!))
    {
        IndexFormat = $"applogs-{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{hostContext.HostingEnvironment.EnvironmentName.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}",
        AutoRegisterTemplate = true,
        NumberOfShards = 2,
        NumberOfReplicas = 1,
        ModifyConnectionSettings = x =>
        {
            x.BasicAuthentication(builder.Configuration["ElasticConfiguration:UserName"]!, builder.Configuration["ElasticConfiguration:Password"]!);
            x.ServerCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) => true);
            return x;
        }
    })
    .Enrich.WithProperty("Environment", hostContext.HostingEnvironment.EnvironmentName)
    .ReadFrom.Configuration(hostContext.Configuration);
});
#endregion

builder.Services.AddBusinessLogicLayerServices(builder.Configuration);
builder.Services.AddDataLogicLayerServices(builder.Configuration);
builder.Services.AddUtilityServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

#region Queue Services
builder.Services.AddSingleton<MonitorLoop>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(_ =>
{
    if (!int.TryParse(builder.Configuration["QueueCapacity"], out var queueCapacity))
    {
        queueCapacity = 1;
    }
    return new DefaultBackgroundTaskQueue(queueCapacity);
});
#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapGet("/", async context =>
{
    await context.Response.SendFileAsync("welcome.html");
});

app.MapControllers();

app.Run();
