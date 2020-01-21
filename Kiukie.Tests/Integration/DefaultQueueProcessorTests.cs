using Insight.Database;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Data;
using System.Threading.Tasks;

namespace Kiukie.Tests.Integration
{
    [TestFixture]
    public class DefaultQueueProcessorTests
    {
        [Test]
        public async Task ProcessAsync_SingleItemInQueue_ProcessesAndEmptiesQueue()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("An item"));

                var queue = new DefaultQueue<string>(connection);
                var handler = new FakePayloadHandler();
                var queueProcessor = new DefaultQueueProcessor<string>(queue, handler);

                var processed = await queueProcessor.ProcessAsync();
                Assert.IsTrue(processed);

                processed = await queueProcessor.ProcessAsync();
                Assert.IsFalse(processed);
            }
        }
    }

    public class FakePayloadHandler : IPayloadHandler<string>
    {
        public Task ProcessAsync(string queueItem)
        {
            return Task.CompletedTask;
        }
    }
}
