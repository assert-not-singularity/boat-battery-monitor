using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BatMon
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = new HostBuilder()
                .ConfigureHostConfiguration(hostConfig =>
                {
                    hostConfig.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, appConfig) =>
                {
                    appConfig.SetBasePath(hostContext.HostingEnvironment.ContentRootPath);
                    appConfig.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureLogging((hostContext, loggingConfig) =>
                {
                    loggingConfig.AddConsole();
                })
                .ConfigureServices((hostContext, servicesConfig) =>
                {
                    servicesConfig.AddHostedService<BackgroundService>();
                    servicesConfig.AddSingleton<IConfiguration>(hostContext.Configuration);
                })
                .Build();

            await host.RunAsync();
        }
    }
}
