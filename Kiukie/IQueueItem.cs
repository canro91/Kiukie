using System;

namespace Kiukie
{
    public interface IQueueItem<T>
    {
        int StatusId { get; set; }

        T Payload { get; set; }

        DateTime CreatedDate { get; set; }

        DateTime UpdatedDate { get; set; }
    }
}