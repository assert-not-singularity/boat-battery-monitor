using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BatMon
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(hostContext.HostingEnvironment.ContentRootPath);
                    configApp.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddSerilog(
                        new LoggerConfiguration()
                            .ReadFrom.Configuration(hostContext.Configuration)
                            .CreateLogger(),
                        dispose: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<BackgroundService>();
                    services.AddSingleton<IConfiguration>(hostContext.Configuration);
                })
                .Build();

            await host.RunAsync();
        }
    }
}
