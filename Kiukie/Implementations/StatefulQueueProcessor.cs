using System;
using System.Threading.Tasks;

namespace Kiukie
{
    public class StatefulQueueProcessor<T> : IQueueProcessor
    {
        private readonly IStatefulQueue<T> Queue;
        private readonly IPayloadHandler<T> PayloadHandler;

        public StatefulQueueProcessor(IStatefulQueue<T> queue, IPayloadHandler<T> handler)
        {
            Queue = queue;
            PayloadHandler = handler;
        }

        public async Task<bool> ProcessAsync()
        {
            var queueItem = await Queue.DequeueAsync();
            if (queueItem == null)
            {
                return false;
            }

            Exception exception = null;
            try
            {
                await PayloadHandler.ProcessAsync(queueItem.Payload);
            }
            catch (Exception e)
            {
                exception = e;
                throw;
            }
            finally
            {
                await Queue.UpdateAsync(queueItem, exception);
            }
            return true;
        }
    }
}
