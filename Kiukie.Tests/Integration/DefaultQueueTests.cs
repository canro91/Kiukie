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
    public class DefaultQueueTests
    {
        [Test]
        public async Task Dequeue_TwoConcurrentCalls_DequeueDifferentItems()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("An item"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Another item"));

                var queue1 = new DefaultQueue<StringItem>(connection);
                var queue2 = new DefaultQueue<StringItem>(connection);

                var t1 = queue1.DequeueAsync();
                var t2 = queue1.DequeueAsync();

                var items = await Task.WhenAll(t1, t2);
                Assert.IsTrue(items.First().Payload != items.Last().Payload);
            }
        }

        [Test]
        public async Task Dequeue_TwoItemsInQueue_DequeueItemsInOrder()
        {
            using (var scope = new IsolationScope(TestFixtureContext.Provider))
            {
                var connection = scope.Provider.GetRequiredService<IDbConnection>();
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item1"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(Payload) VALUES(@Payload)", new StringItem("Item2"));

                var queue = new DefaultQueue<StringItem>(connection);

                var item1 = await queue.DequeueAsync();
                Assert.AreEqual("Item1", item1.Payload);

                var item2 = await queue.DequeueAsync();
                Assert.AreEqual("Item2", item2.Payload);
            }
        }
    }

    public class StringItem : IQueueItem<string>
    {
        public StringItem(string payload)
        {
            Payload = payload;
        }

        public int StatusId { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

}
