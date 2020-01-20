using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Kiukie.Tests.Unit
{
    [TestFixture]
    public class QueueProcessorTests
    {
        [Test]
        public async Task ProcessAsync_EmptyQueue_ReturnsFalse()
        {
            var queue = new EmptyQueue();
            var handler = new FakePayloadHandler();
            var queueProcessor = new QueueProcessor<string>(queue, handler);

            var processed = await queueProcessor.ProcessAsync();

            Assert.IsFalse(processed);
        }

        [Test]
        public async Task ProcessAsync_AnItemInQueue_ReturnsTrue()
        {
            var queue = new SingleItemQueue(new StringItem("An item"));
            var handler = new FakePayloadHandler();
            var queueProcessor = new QueueProcessor<string>(queue, handler);

            var processed = await queueProcessor.ProcessAsync();

            Assert.IsTrue(processed);
        }

        [Test]
        public void ProcessAsync_ExceptionWhileProcessingItem_ThrowException()
        {
            var queue = new SingleItemQueue(new StringItem("An item"));
            var handler = new ThrowExceptionPayloadHandler(new Exception("An exception"));
            var queueProcessor = new QueueProcessor<string>(queue, handler);

            Assert.ThrowsAsync<Exception>(() => queueProcessor.ProcessAsync());
        }
    }

    public class StringItem : IQueueItem<string>
    {
        public StringItem(string payload)
        {
            Payload = payload;
        }

        public int Id { get; set; }
        public int StatusId { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class SingleItemQueue : IQueue<string>
    {
        public StringItem Item;

        public SingleItemQueue(StringItem item)
        {
            Item = item;
        }

        public Task<IQueueItem<string>> DequeueAsync()
        {
            return Task.FromResult((IQueueItem<string>)Item);
        }
    }

    public class EmptyQueue : IQueue<string>
    {
        public Task<IQueueItem<string>> DequeueAsync()
        {
            return Task.FromResult((IQueueItem<string>)null);
        }
    }

    public class FakePayloadHandler : IPayloadHandler<string>
    {
        public Task ProcessAsync(string queueItem)
        {
            return Task.CompletedTask;
        }
    }

    public class ThrowExceptionPayloadHandler : IPayloadHandler<string>
    {
        public Exception Exception;

        public ThrowExceptionPayloadHandler(Exception exception)
        {
            Exception = exception;
        }

        public Task ProcessAsync(string queueItem)
        {
            return Task.FromException(Exception);
        }
    }
}