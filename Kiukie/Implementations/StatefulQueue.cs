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

        public async Task<T> DequeueAsync()
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
            return await Connection.SingleSqlAsync<T>(sql: sql);
        }

        public Task UpdateAsync(T item, Exception e = null)
        {
            return Task.CompletedTask;
        }
    }

    public enum ItemStatus
    {
        Pending = 1,
        Processing,
        Succeeded,
        Failed
    }
}
