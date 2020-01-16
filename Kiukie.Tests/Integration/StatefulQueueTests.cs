using Insight.Database;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Kiukie.Tests.Integration
{
    [TestFixture]
    public class StatefulQueueTests
    {
        [Test]
        public async Task Dequeue_EmptyQueue_ReturnsNull()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                var queue = CreateQueue(connection);

                var item = await queue.DequeueAsync();

                Assert.IsNull(item);
            }
        }

        private static StatefulQueue<string> CreateQueue(IDbConnection connection)
        {
            return new StatefulQueue<string>(connection);
        }

        [Test]
        public async Task Dequeue_PendingItem_ReturnsItem()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "An item"));

                var queue = CreateQueue(connection);
                var item = await queue.DequeueAsync();

                Assert.IsNotNull(item);
            }
        }

        [Test]
        [TestCase(ItemStatus.Succeeded)]
        [TestCase(ItemStatus.Failed)]
        public async Task Dequeue_PendingAndAlreadyProcessedItem_ReturnsPendingItem(ItemStatus status)
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(status, "Processed"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Pending"));

                var queue = CreateQueue(connection);
                var item = await queue.DequeueAsync();

                Assert.IsNotNull(item);
                Assert.AreEqual("Pending", item.Payload);
            }
        }

        [Test]
        public async Task Dequeue_PendingItem_ReturnsItemInProcessingStatus()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "An item"));

                var queue = CreateQueue(connection);
                var item = await queue.DequeueAsync();

                Assert.IsNotNull(item);
                Assert.AreEqual((int)ItemStatus.Processing, item.StatusId);
            }
        }

        [Test]
        public async Task Dequeue_TwoPendingItems_DequeueItemsInOrder()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Item1"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Item2"));

                var queue = CreateQueue(connection);

                var item1 = await queue.DequeueAsync();
                Assert.IsNotNull(item1);
                Assert.AreEqual("Item1", item1.Payload);

                var item2 = await queue.DequeueAsync();
                Assert.IsNotNull(item2);
                Assert.AreEqual("Item2", item2.Payload);
            }
        }

        [Test]
        public async Task Dequeue_TwoPendingItemsInConcurrentCall_DequeueItemsInOrder()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Item1"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Item2"));

                var queue1 = CreateQueue(connection);
                var queue2 = CreateQueue(connection);

                var t1 = queue1.DequeueAsync();
                var t2 = queue2.DequeueAsync();
                var items = await Task.WhenAll(t1, t2);

                Assert.IsTrue(items.First().Payload != items.Last().Payload);
            }
        }

        [Test]
        public async Task Update_ProcessingItemAndNoException_UpdatesEventToProcessed()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                var pending = await connection.InsertSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Processing, "An Item"));

                var queue = CreateQueue(connection);
                await queue.UpdateAsync(pending);

                var updatedEvent = await connection.SingleSqlAsync<StringItem>("SELECT * FROM Kiukie.Queue WHERE Id = @Id", new { pending.Id });
                Assert.AreEqual((int)ItemStatus.Succeeded, updatedEvent.StatusId);
            }
        }

        [Test]
        public async Task Update_ProcessingItemAndAnyException_UpdatesEventToFailed()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                var pending = await connection.InsertSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Processing, "An Item"));

                var queue = CreateQueue(connection);
                await queue.UpdateAsync(pending, new Exception("Any Exception"));

                var updatedEvent = await connection.SingleSqlAsync<StringItem>("SELECT * FROM Kiukie.Queue WHERE Id = @Id", new { pending.Id });
                Assert.AreEqual((int)ItemStatus.Failed, updatedEvent.StatusId);
            }
        }
    }
}
