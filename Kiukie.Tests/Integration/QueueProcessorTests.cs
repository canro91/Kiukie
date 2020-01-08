using Insight.Database;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;

namespace Kiukie.Tests.Integration
{
    [TestFixture]
    public class QueueProcessorTests
    {
        [Test]
        public async Task ProcessAsync_SingleItemInQueue_ProcessesAndEmptiesQueue()
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Kiukie;Integrated Security=True;");
                await connection.ExecuteSqlAsync("INSERT INTO Kiukie.Queue(StatusId, Payload) VALUES(@StatusId, @Payload)", new StringItem("An item"));

                var queue = new DefaultQueue<string>(connection);
                var handler = new FakePayloadHandler();
                var queueProcessor = new QueueProcessor<string>(queue, handler);

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
