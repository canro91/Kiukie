using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kiukie.Sample
{
    public class StringItemPayloadHandler : IPayloadHandler<StringItem>
    {
        private readonly ILogger<StringItemPayloadHandler> Logger;

        public StringItemPayloadHandler(ILogger<StringItemPayloadHandler> logger)
        {
            Logger = logger;
        }

        public async Task ProcessAsync(StringItem queueItem)
        {
            Logger.LogInformation($"Processing item: {queueItem.Payload} at {DateTime.Now:yyyy-MM-dd hh:mm:ss}");
            await Task.Delay(1*1000);
        }
    }
}
