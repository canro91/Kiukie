using System.Threading.Tasks;

namespace Kiukie
{
    public interface IPayloadHandler<T>
    {
        Task ProcessAsync(T queueItem);
    }
}