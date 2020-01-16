using System;
using System.Threading.Tasks;

namespace Kiukie
{
    public interface IStatefulQueue<T>
    {
        Task<IQueueItem<T>> DequeueAsync();

        Task UpdateAsync(IQueueItem<T> item, Exception e = null);
    }
}
