using Insight.Database;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Data;
using System.Threading.Tasks;

namespace Kiukie.Tests.Integration
{
    [TestFixture]
    public class DefaultBulkQueueProcessorTests
    {
        [Test]
        public async Task ProcessAsync_NotEmptyQueue_ProcessesAndEmptiesQueue()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item1"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item2"));

                var queue = new DefaultBulkQueue<string>(connection, bulkSize: 2);
                var handler = new FakePayloadHandler();
                var queueProcessor = new DefaultBulkQueueProcessor<string>(queue, handler);

                var processed = await queueProcessor.ProcessAsync();
                Assert.IsTrue(processed);

                processed = await queueProcessor.ProcessAsync();
                Assert.IsFalse(processed);
            }
        }
    }
}
