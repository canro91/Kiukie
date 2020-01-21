using System.Threading.Tasks;

namespace Kiukie
{
    public class DefaultQueueProcessor<T> : IQueueProcessor
    {
        private readonly IQueue<T> Queue;
        private readonly IPayloadHandler<T> PayloadHandler;

        public DefaultQueueProcessor(IQueue<T> queue, IPayloadHandler<T> payloadHandler)
        {
            this.Queue = queue;
            this.PayloadHandler = payloadHandler;
        }

        public async Task<bool> ProcessAsync()
        {
            var queueItem = await Queue.DequeueAsync();
            if (queueItem == null)
            {
                return false;
            }

            await PayloadHandler.ProcessAsync(queueItem.Payload);
            return true;
        }
    }
}