﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kiukie
{
    public class QueueService : BackgroundService
    {
        private readonly IQueueProcessor QueueProcessor;
        private readonly QueueProcessorConfig Config;
        private readonly ILogger<QueueService> Logger;

        public QueueService(IQueueProcessor queueProcessor,
                                           IOptions<QueueProcessorConfig> configuration,
                                           ILogger<QueueService> logger)
        {
            QueueProcessor = queueProcessor;
            Config = configuration.Value;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // TODO Use scope here...
                    var itemProcessed = await QueueProcessor.ProcessAsync();
                    if (!itemProcessed)
                    {
                        await Task.Delay(Config.PollIntervalMilliseconds, stoppingToken);
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