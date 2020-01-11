using Insight.Database;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
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
            using (var scope = new IsolationScope(TestContext.Provider))
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
    }
}
