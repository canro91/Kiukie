﻿using Insight.Database;
using System.Data;
using System.Threading.Tasks;

namespace Kiukie
{
    public class DefaultQueue<T> : IQueue<T>
    {
        private readonly IDbConnection Connection;

        public DefaultQueue(IDbConnection connection)
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
    ORDER BY Id 
)
DELETE FROM CTE
OUTPUT deleted.Payload";
            return await Connection.SingleSqlAsync<QueueItem<T>>(sql: sql);
        }
    }
}
