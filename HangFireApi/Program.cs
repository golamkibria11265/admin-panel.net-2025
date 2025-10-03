using Hangfire;
using Hangfire.SqlServer;
using HangFireApi.Models.CommonModels;
using HangFireApi.Models.ViewModels;
using HangFireApi.Services.Common;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<SchedulerViewModel>(builder.Configuration.GetSection("SchedulerConfiguration"));
builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration["ConnectionStrings:DefaultConnection"], new SqlServerStorageOptions
                {
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true
                }));
builder.Services.AddHangfireServer();

builder.Services.AddTransient<ISchedulerService, SchedulerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var schedulers = app.Services.GetRequiredService<IOptions<SchedulerViewModel>>().Value;
using var scopeProvider = app.Services.CreateAsyncScope();
var recurringJobManager = scopeProvider.ServiceProvider.GetRequiredService<IRecurringJobManager>();
string? queue_name = "default";
if (schedulers != null && schedulers.Schedulers.Any(x => x.Enabled))
{
    foreach (var item in schedulers.Schedulers.Where(x => x.Enabled))
    {
        if (item != null && item.Name == SchedulerEnum.SchedulerNames.HangFireScheduler.ToString())
        {
            #pragma warning disable CS0618
                RecurringJob.AddOrUpdate<ISchedulerService>(
                    item.Name,
                    x => x.HangFireScheduler(),
                    item.CronExpression,
                    TimeZoneInfo.Local,
                    queue_name
                );
            #pragma warning restore CS0618
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.Run();
