using System.Threading.Tasks;

namespace Kiukie
{
    public interface IQueueProcessor
    {
        Task<bool> ProcessAsync();
    }
}