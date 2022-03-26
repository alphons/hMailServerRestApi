
// dotnet add package Microsoft.Extensions.Hosting.WindowsServices --version 6.0.0
using Microsoft.Extensions.Hosting.WindowsServices;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using hMailServerConnector.CoreWeb.MiddleWare;

using hMailServerConnector.CoreWeb.DataService;

AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    File.WriteAllText(@"c:\temp\UnhandledException.txt", ((Exception)e.ExceptionObject).Message);
};

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory
});

builder.Host.UseWindowsService();

var services = builder.Services;

if (WindowsServiceHelpers.IsWindowsService())
{
    services.AddSingleton<IHostLifetime, WindowsServiceLifetime>();
    builder.Logging.AddEventLog(settings =>
    {
        if (string.IsNullOrEmpty(settings.SourceName))
        {
            settings.SourceName = builder.Environment.ApplicationName;
        }
    });
}

//services.Configure<KestrelServerOptions>(options =>
//{
//    options.Listen(System.Net.IPAddress.Any, builder.Configuration.GetValue<int>("Port"));
//});

services.AddSingleton<IConfiguration>(builder.Configuration);

//services.AddSingleton<ILifeTime,LifeTime>();

var errorManager = new ErrorManager();

services.AddSingleton<IErrorManager>(errorManager);

services.AddHttpContextAccessor();

services.AddControllers(); // Only for views or pages and using swagger

services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache, needed also for (Add)Session

services.AddSession();

var staticFileProvider = new StaticFileProvider(builder.Environment.WebRootPath);

services.AddSingleton<IStaticFileProvider>(staticFileProvider);

services.AddMvcCoreCorrected();

services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

errorManager.Info("Startup");

app.UseResponseCompression();

// Catches all server errors, do some error logging
app.UseExceptionHandler("/internalerror");

//app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();// After UseRouting and Before UseEndpoints

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseDefaultFiles();

app.UseStaticFileProvider(staticFileProvider);

// Start SingleTon, when not called in a controller
//app.Services.GetRequiredService<ILifeTime>();

app.Run();
