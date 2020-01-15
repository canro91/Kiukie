using System;
using System.Threading.Tasks;

namespace Kiukie
{
    public interface IStatefulQueue<T>
    {
        Task<T> DequeueAsync();

        Task UpdateAsync(T item, Exception e = null);
    }
}
