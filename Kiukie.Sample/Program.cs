using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Kiukie.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("appsettings.json");
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddOptions();

                    var config = hostingContext.Configuration;
                    var connString = config.GetConnectionString("Kiukie");
                    services.AddTransient<IDbConnection>((provider) => new SqlConnection(connString));

                    services.Configure<QueueProcessorConfig>(config.GetSection("QueueProcessor"));
                    services.AddTransient<IPayloadHandler<StringItem>, StringItemPayloadHandler>();
                    services.AddTransient<IQueue<StringItem>, DefaultQueue<StringItem>>();
                    services.AddTransient<IQueueProcessor, QueueProcessor<StringItem>>();

                    services.AddHostedService<QueueItemProducer>();
                    services.AddHostedService<QueueProcessorHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }
    }
}
