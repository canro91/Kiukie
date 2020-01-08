using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Kiukie.Tests
{
    [TestFixture]
    public class QueueProcessorTests
    {
        [Test]
        public async Task ProcessAsync_EmptyQueue_ReturnsFalse()
        {
            var queue = new EmptyQueue();
            var handler = new FakePayloadHandler();
            var queueProcessor = new QueueProcessor<StringItem>(queue, handler);

            var processed = await queueProcessor.ProcessAsync();

            Assert.IsFalse(processed);
        }

        [Test]
        public async Task ProcessAsync_AnItemInQueue_ReturnsTrue()
        {
            var queue = new SingleItemQueue(new StringItem("An item"));
            var handler = new FakePayloadHandler();
            var queueProcessor = new QueueProcessor<StringItem>(queue, handler);

            var processed = await queueProcessor.ProcessAsync();

            Assert.IsTrue(processed);
        }

        [Test]
        public void ProcessAsync_ExceptionWhileProcessingItem_ThrowException()
        {
            var queue = new SingleItemQueue(new StringItem("An item"));
            var handler = new TrhowExceptionPayloadHandler(new Exception("An exception"));
            var queueProcessor = new QueueProcessor<StringItem>(queue, handler);

            Assert.ThrowsAsync<Exception>(() => queueProcessor.ProcessAsync());
        }
    }

    public class StringItem : IQueueItem<string>
    {
        public StringItem() { }

        public StringItem(string payload)
        {
            Payload = payload;
        }

        public int StatusId { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class SingleItemQueue : IQueue<StringItem>
    {
        public StringItem Item;

        public SingleItemQueue(StringItem item)
        {
            Item = item;
        }

        public Task<StringItem> DequeueAsync()
        {
            return Task.FromResult(Item);
        }
    }

    public class EmptyQueue : IQueue<StringItem>
    {
        public Task<StringItem> DequeueAsync()
        {
            return Task.FromResult<StringItem>(null);
        }
    }

    public class FakePayloadHandler : IPayloadHandler<StringItem>
    {
        public Task ProcessAsync(StringItem queueItem)
        {
            return Task.CompletedTask;
        }
    }

    public class TrhowExceptionPayloadHandler : IPayloadHandler<StringItem>
    {
        public Exception Exception;

        public TrhowExceptionPayloadHandler(Exception exception)
        {
            Exception = exception;
        }

        public Task ProcessAsync(StringItem queueItem)
        {
            return Task.FromException(Exception);
        }
    }
}