using Insight.Database;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Kiukie.Tests.Integration
{
    [TestFixture]
    public class StatefulQueueProcessorTests
    {
        [Test]
        public async Task ProcessAsync_SinglePendingItemInQueue_ProcessesAndEmptiesQueue()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(QueueItemStatus.Pending, "An item"));

                var queue = new StatefulQueue<string>(connection);
                var handler = new FakePayloadHandler();
                var queueProcessor = new StatefulQueueProcessor<string>(queue, handler);

                var processed = await queueProcessor.ProcessAsync();
                Assert.IsTrue(processed);

                processed = await queueProcessor.ProcessAsync();
                Assert.IsFalse(processed);
            }
        }

        [Test]
        public async Task ProcessAsync_SinglePendingItemInQueue_ProcessesAndUpdatesItem()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(QueueItemStatus.Pending, "An item"));

                var queue = new StatefulQueue<string>(connection);
                var handler = new FakePayloadHandler();
                var queueProcessor = new StatefulQueueProcessor<string>(queue, handler);

                var processed = await queueProcessor.ProcessAsync();
                Assert.IsTrue(processed);

                var item = await connection.SingleSqlAsync<StringItem>("SELECT TOP 1 * FROM Kiukie.Queue");
                Assert.IsNotNull(item);
                Assert.AreEqual((int)QueueItemStatus.Succeeded, item.StatusId);
            }
        }

        [Test]
        public async Task ProcessAsync_SinglePendingItemInQueueAndExceptionWhileProcessingIt_UpdatesItem()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(QueueItemStatus.Pending, "An item"));

                var queue = new StatefulQueue<string>(connection);
                var handler = new ThrowExceptionPayloadHandler(new Exception("Any exception"));
                var queueProcessor = new StatefulQueueProcessor<string>(queue, handler);

                Assert.ThrowsAsync<Exception>(() => queueProcessor.ProcessAsync());

                var item = await connection.SingleSqlAsync<StringItem>("SELECT TOP 1 * FROM Kiukie.Queue");
                Assert.IsNotNull(item);
                Assert.AreEqual((int)QueueItemStatus.Failed, item.StatusId);
            }
        }
    }

    public class ThrowExceptionPayloadHandler : IPayloadHandler<string>
    {
        public Exception Exception;

        public ThrowExceptionPayloadHandler(Exception exception)
        {
            Exception = exception;
        }

        public Task ProcessAsync(string queueItem)
        {
            return Task.FromException(Exception);
        }
    }
}
