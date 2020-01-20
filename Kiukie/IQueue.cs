using System.Threading.Tasks;

namespace Kiukie
{
    public interface IQueue<T>
    {
        Task<IQueueItem<T>> DequeueAsync();
    }
}