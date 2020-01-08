using System.Threading.Tasks;

namespace Kiukie
{
    public interface IQueue<T>
    {
        Task<T> DequeueAsync();
    }
}