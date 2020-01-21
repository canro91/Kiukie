using Insight.Database;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Kiukie
{
    public class StatefulQueue<T> : IStatefulQueue<T>
    {
        private readonly IDbConnection Connection;

        public StatefulQueue(IDbConnection connection)
        {
            Connection = connection;
        }

        public async Task<IQueueItem<T>> DequeueAsync()
        {
            var sql = @"
WITH CTE AS
(
    SELECT TOP(1) *
    FROM Kiukie.Queue 
    WHERE StatusId = 1
    ORDER BY Id
)
UPDATE CTE
SET StatusId = 2, UpdatedDate = GETDATE()
OUTPUT INSERTED.*";
            return await Connection.SingleSqlAsync<QueueItem<T>>(sql: sql);
        }

        public async Task UpdateAsync(IQueueItem<T> item, Exception e = null)
        {
            var status = (e == null) ? ItemStatus.Succeeded : ItemStatus.Failed;
            await Connection.ExecuteSqlAsync(@"
UPDATE Kiukie.Queue
SET StatusId = @StatusId
WHERE Id = @Id", new { StatusId = (int)status, Id = item.Id });
        }
    }
}
