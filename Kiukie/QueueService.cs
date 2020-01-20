using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kiukie
{
    public class QueueService : BackgroundService
    {
        private readonly IServiceProvider Services;
        private readonly QueueProcessorConfig Config;
        private readonly ILogger<QueueService> Logger;

        public QueueService(IServiceProvider services,
                            IOptions<QueueProcessorConfig> configuration,
                            ILogger<QueueService> logger)
        {
            Services = services;
            Config = configuration.Value;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = Services.CreateScope())
                    {
                        var queueProcessor = scope.ServiceProvider.GetRequiredService<IQueueProcessor>();
                        var itemProcessed = await queueProcessor.ProcessAsync();
                        if (!itemProcessed)
                        {
                            await Task.Delay(Config.PollIntervalMilliseconds, stoppingToken);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occured while processing an item");
                }
            }
        }
    }
}