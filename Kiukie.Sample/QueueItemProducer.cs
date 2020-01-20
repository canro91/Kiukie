using Insight.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kiukie.Sample
{
    internal class QueueItemProducer : BackgroundService
    {
        private readonly IDbConnection Connection;
        private readonly ILogger<QueueItemProducer> Logger;

        public QueueItemProducer(IDbConnection connection, ILogger<QueueItemProducer> logger)
        {
            Connection = connection;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Connection.ExecuteSql("TRUNCATE TABLE Kiukie.Queue");

            foreach (var bulk in Enumerable.Range(0, 2))
            {
                Logger.LogInformation($"Creating Bulk {bulk}");

                foreach (var item in Enumerable.Range(0, 100))
                {
                    Connection.ExecuteSql("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem($"Item-{bulk}-{item}"));
                }
                await Task.Delay(1 * 1000);
            }
            Logger.LogInformation("Done creating items");
        }
    }
}
