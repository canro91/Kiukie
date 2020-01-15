using Insight.Database;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Data;
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
                var queue = new StatefulQueue<StringItem>(connection);

                var item = await queue.DequeueAsync();

                Assert.IsNull(item);
            }
        }

        [Test]
        public async Task Dequeue_PendingItem_ReturnsItem()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "An item"));

                var queue = new StatefulQueue<StringItem>(connection);

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
                var processed = await connection.InsertSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(status, "A processed item"));
                var pending = await connection.InsertSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "A pending item"));

                var queue = new StatefulQueue<StringItem>(connection);

                var item = await queue.DequeueAsync();

                Assert.IsNotNull(item);
                Assert.AreEqual(pending.Id, item.Id);
            }
        }

        [Test]
        public async Task Dequeue_PendingItem_ReturnsItemInProcessingStatus()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "An item"));

                var queue = new StatefulQueue<StringItem>(connection);

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
                var pending1 = await connection.InsertSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Item1"));
                var pending2 = await connection.InsertSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Item2"));

                var queue = new StatefulQueue<StringItem>(connection);

                var item1 = await queue.DequeueAsync();
                Assert.IsNotNull(item1);
                Assert.AreEqual(pending1.Id, item1.Id);

                var item2 = await queue.DequeueAsync();
                Assert.IsNotNull(item2);
                Assert.AreEqual(pending2.Id, item2.Id);
            }
        }

        [Test]
        public async Task Dequeue_TwoPendingItemsInConcurrentCall_DequeueItemsInOrder()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                var pending1 = await connection.InsertSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Item1"));
                var pending2 = await connection.InsertSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) OUTPUT Inserted.Id VALUES(@StatusId, @Payload)", new StringItem(ItemStatus.Pending, "Item2"));

                var queue = new StatefulQueue<StringItem>(connection);

                var t1 = queue.DequeueAsync();
                var t2 = queue.DequeueAsync();
                var items = await Task.WhenAll(t1, t2);

                Assert.IsTrue(items.First().Id != items.Last().Id);
            }
        }
    }
}
