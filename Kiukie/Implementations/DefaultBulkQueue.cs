using Insight.Database;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Kiukie
{
    public class DefaultBulkQueue<T> : IBulkQueue<T>
    {
        private readonly IDbConnection Connection;
        private readonly int BulkSize;

        public DefaultBulkQueue(IDbConnection connection, int bulkSize)
        {
            this.Connection = connection;
            this.BulkSize = bulkSize;
        }

        public async Task<IEnumerable<IQueueItem<T>>> DequeueAsync()
        {
            var sql = @"
WITH CTE AS
(
    SELECT TOP(@BulkSize) *
    FROM Kiukie.Queue 
    ORDER BY Id 
)
DELETE FROM CTE
OUTPUT deleted.Payload";
            return await Connection.QuerySqlAsync<QueueItem<T>>(sql: sql, new { BulkSize });
        }
    }
}
