
// dotnet add package Microsoft.Extensions.Hosting.WindowsServices --version 6.0.0
using Microsoft.Extensions.Hosting.WindowsServices;

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

var errorManager = new ErrorManager();

services.AddSingleton<IErrorManager>(errorManager);

services.AddMvcCore().WithMultiParameterModelBinding();

var app = builder.Build();

app.UseRouting();

errorManager.Info("Startup");

app.UseResponseCompression();

// Catches all server errors, do some error logging
app.UseExceptionHandler("/internalerror");

//app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseStaticFiles();

app.MapControllers();

app.Run();
