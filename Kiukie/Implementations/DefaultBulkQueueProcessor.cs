using System.Linq;
using System.Threading.Tasks;

namespace Kiukie
{
    public class DefaultBulkQueueProcessor<T> : IQueueProcessor
    {
        private readonly IBulkQueue<T> Queue;
        private readonly IPayloadHandler<T> PayloadHandler;

        public DefaultBulkQueueProcessor(IBulkQueue<T> queue, IPayloadHandler<T> payloadHandler)
        {
            this.Queue = queue;
            this.PayloadHandler = payloadHandler;
        }

        public async Task<bool> ProcessAsync()
        {
            var items = await Queue.DequeueAsync();
            if (!items.Any())
            {
                return false;
            }

            await Task.WhenAll(items.Select(item => PayloadHandler.ProcessAsync(item.Payload)));
            return true;
        }
    }
}
