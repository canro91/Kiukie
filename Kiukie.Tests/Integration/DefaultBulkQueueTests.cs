using Insight.Database;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Kiukie.Tests.Integration
{
    [TestFixture]
    public class DefaultBulkQueueTests
    {
        [Test]
        public async Task Dequeue_EmptyQueue_ReturnsEmpty()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                var queue = new DefaultBulkQueue<string>(connection, bulkSize: 2);

                var items = await queue.DequeueAsync();

                Assert.IsEmpty(items);
            }
        }

        [Test]
        public async Task Dequeue_NotEmptyQueue_DequeueItemsInOrder()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item1"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item2"));
                var queue = new DefaultBulkQueue<string>(connection, bulkSize: 2);

                var items = await queue.DequeueAsync();

                Assert.AreEqual(2, items.Count());
            }
        }

        [Test]
        public async Task Dequeue_MoreItemsThanBulkSize_DequeueRightCountOfItems()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item1"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item2"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item3"));
                var queue = new DefaultBulkQueue<string>(connection, bulkSize: 2);

                var items = await queue.DequeueAsync();

                Assert.AreEqual(2, items.Count());
            }
        }
    }
}