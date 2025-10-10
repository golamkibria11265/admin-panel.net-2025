using Bll.Extensions;
using Dll.Extensions;
using Utility.Extensions;
using Serilog;
using System.Reflection;
using Bll.Services.CommonService;

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
    #region uncomment if elastic search is implemented
    //.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(builder.Configuration["ElasticConfiguration:Uri"]!))
    //{
    //    IndexFormat = $"applogs-{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{hostContext.HostingEnvironment.EnvironmentName.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}",
    //    AutoRegisterTemplate = true,
    //    NumberOfShards = 2,
    //    NumberOfReplicas = 1,
    //    ModifyConnectionSettings = x =>
    //    {
    //        x.BasicAuthentication(builder.Configuration["ElasticConfiguration:UserName"]!, builder.Configuration["ElasticConfiguration:Password"]!);
    //        x.ServerCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) => true);
    //        return x;
    //    }
    //})
    #endregion
    .Enrich.WithProperty("Environment", hostContext.HostingEnvironment.EnvironmentName)
    .ReadFrom.Configuration(hostContext.Configuration);
});
#endregion

builder.Services.AddBusinessLogicLayerServices(builder.Configuration);
builder.Services.AddDataLogicLayerServices(builder.Configuration);
builder.Services.AddUtilityServices(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

#region uncomment this block to seed data. after you are done with data seeding, comment this block again. else duplicate data will be inserted.
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var seeder = services.GetRequiredService<IDataSeedingService>();
//    seeder.SeedData();
//}
#endregion

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
