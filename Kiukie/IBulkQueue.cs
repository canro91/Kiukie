using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kiukie
{
    public interface IBulkQueue<T>
    {
        Task<IEnumerable<IQueueItem<T>>> DequeueAsync();
    }
}
