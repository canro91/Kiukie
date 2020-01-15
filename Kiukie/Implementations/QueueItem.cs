using System;

namespace Kiukie
{
    public class QueueItem<T> : IQueueItem<T>
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public T Payload { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
