using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kiukie.Sample
{
    public class StringItemPayloadHandler : IPayloadHandler<string>
    {
        private readonly ILogger<StringItemPayloadHandler> Logger;

        public StringItemPayloadHandler(ILogger<StringItemPayloadHandler> logger)
        {
            Logger = logger;
        }

        public async Task ProcessAsync(string payload)
        {
            Logger.LogInformation($"Processing item: {payload} at {DateTime.Now:yyyy-MM-dd hh:mm:ss}");
            await Task.Delay(1*1000);
        }
    }
}
