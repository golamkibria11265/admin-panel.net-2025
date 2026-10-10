using Bll.Extensions;
using Dll.Extensions;
using Utility.Extensions;
using Serilog;
using Bll.Services.CommonService;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApi.Filters;

var builder = WebApplication.CreateBuilder(args);
bool isDevelopment = builder.Environment.IsDevelopment();

#region Serilog Configuration
// Log folder comes from configuration (FilePaths:LogFileRootDirectory, or the environment variable
// FilePaths__LogFileRootDirectory). Falls back to a "logs" folder next to the application.
string logDirectory = builder.Configuration["FilePaths:LogFileRootDirectory"] is { Length: > 0 } configuredLogDirectory
    ? configuredLogDirectory
    : Path.Combine(AppContext.BaseDirectory, "logs");
if (isDevelopment)
{
    Serilog.Debugging.SelfLog.Enable(Console.Error);
}
builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    configuration.Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error)
    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                        theme: AnsiConsoleTheme.Code
                    )
    .WriteTo.File(path: Path.Combine(logDirectory, "webapi-.log"), rollingInterval: RollingInterval.Hour, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    // Elasticsearch: if you enable it, keep certificate validation ON and read the credentials from
    // environment variables / a secret store, never from appsettings.json.
    .Enrich.WithProperty("Environment", hostContext.HostingEnvironment.EnvironmentName)
    .ReadFrom.Configuration(hostContext.Configuration);
});
#endregion

builder.Services.AddBusinessLogicLayerServices(builder.Configuration);
builder.Services.AddDataLogicLayerServices(builder.Configuration);
builder.Services.AddUtilityServices(builder.Configuration);
builder.Services.AddMemoryCache();

builder.Services.AddControllersWithViews(options =>
{
    // Every action requires a signed-in, active user unless marked [AllowAnonymous].
    options.Filters.Add<CheckSessionWithRedirect>();
    // Every POST/PUT/PATCH/DELETE must carry a valid anti-forgery token (form field or header).
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

CookieSecurePolicy cookieSecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken"; // sent by wwwroot/js/CustomScripts/security.js
    options.Cookie.Name = ".AdminPanel.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

builder.Services.AddSession(options =>
{
    // PCI-DSS 8.2.8: re-authenticate after at most 15 minutes of inactivity.
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.Name = ".AdminPanel.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
});

// Keys that protect the session, anti-forgery and TempData cookies. Without a persistent folder they are
// regenerated on every container restart and everyone is logged out. Set DataProtection__KeysDirectory to a
// mounted volume that only the application user can read.
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("AdminPanel");
if (builder.Configuration["DataProtection:KeysDirectory"] is { Length: > 0 } keysDirectory)
{
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));
}

// Behind nginx: trust X-Forwarded-For / X-Forwarded-Proto only from known proxies, so the app sees the real
// client IP (login lockout, logs) and knows the original request was HTTPS.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Loopback (nginx on the same host) is trusted by default. Add more, e.g. the Docker bridge network:
    // ForwardedHeaders__KnownNetworks=172.16.0.0/12
    foreach (var entry in (builder.Configuration["ForwardedHeaders:KnownNetworks"] ?? string.Empty)
                 .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        var parts = entry.Split('/');
        if (parts.Length == 2 && IPAddress.TryParse(parts[0], out var prefix) && int.TryParse(parts[1], out var length))
        {
            options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, length));
        }
    }
    foreach (var entry in (builder.Configuration["ForwardedHeaders:KnownProxies"] ?? string.Empty)
                 .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (IPAddress.TryParse(entry, out var proxy))
        {
            options.KnownProxies.Add(proxy);
        }
    }
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

#region Data seeding (one-off)
// To seed a fresh database: set the environment variable Seed__AdminPassword (at least 12 characters,
// letters and digits), uncomment this block, run once, then comment it again and remove the variable.
// Seeding is skipped automatically if the "admin" user already exists.
//using (var scope = app.Services.CreateScope())
//{
//    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeedingService>();
//    await seeder.SeedDataAsync();
//}
#endregion

// Basic security headers for every response.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
