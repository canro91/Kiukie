using Insight.Database;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Kiukie.Tests.Integration
{
    [TestFixture]
    public class QueueTests
    {
        [Test]
        public async Task Dequeue_TwoConcurrentCalls_DequeueDifferentItems()
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Kiukie;Integrated Security=True;MultipleActiveResultSets=True");
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem("An item"));
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem("Another item"));

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
