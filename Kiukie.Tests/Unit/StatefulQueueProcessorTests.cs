using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Kiukie.Tests.Unit
{
    [TestFixture]
    public class StatefulQueueProcessorTests
    {
        [Test]
        public async Task ProcessAsync_EmptyQueue_ReturnsFalse()
        {
            var queue = new EmptyStatefulQueue();
            var handler = new FakePayloadHandler();
            var queueProcessor = new StatefulQueueProcessor<string>(queue, handler);

            var processed = await queueProcessor.ProcessAsync();

            Assert.IsFalse(processed);
        }

        [Test]
        public async Task ProcessAsync_AnItemInQueue_ReturnsTrue()
        {
            var queue = new SingleItemStatefulQueue(new StringItem("An item"));
            var handler = new FakePayloadHandler();
            var queueProcessor = new StatefulQueueProcessor<string>(queue, handler);

            var processed = await queueProcessor.ProcessAsync();

            Assert.IsTrue(processed);
        }

        [Test]
        public void ProcessAsync_ExceptionWhileProcessingItem_ThrowExceptionAndUpdatesItem()
        {
            var queue = new SingleItemStatefulQueue(new StringItem("An item"));
            var handler = new ThrowExceptionPayloadHandler(new Exception("An exception"));
            var queueProcessor = new StatefulQueueProcessor<string>(queue, handler);

            Assert.ThrowsAsync<Exception>(() => queueProcessor.ProcessAsync());
            Assert.IsTrue(queue.UpdateWasCalled);
        }

        [Test]
        public async Task ProcessAsync_NoExceptionWhileProcessingItem_UpdatesItem()
        {
            var queue = new SingleItemStatefulQueue(new StringItem("An item"));
            var handler = new FakePayloadHandler();
            var queueProcessor = new StatefulQueueProcessor<string>(queue, handler);

            await queueProcessor.ProcessAsync();

            Assert.IsTrue(queue.UpdateWasCalled);
        }
    }

    public class EmptyStatefulQueue : IStatefulQueue<string>
    {
        public Task<IQueueItem<string>> DequeueAsync()
        {
            return Task.FromResult((IQueueItem<string>)null);
        }

        public Task UpdateAsync(IQueueItem<string> item, Exception e = null)
        {
            return Task.CompletedTask;
        }
    }

    public class SingleItemStatefulQueue : IStatefulQueue<string>
    {
        public StringItem Item;
        public bool UpdateWasCalled;

        public SingleItemStatefulQueue(StringItem item)
        {
            Item = item;
        }

        public Task<IQueueItem<string>> DequeueAsync()
        {
            return Task.FromResult((IQueueItem<string>)Item);
        }

        public Task UpdateAsync(IQueueItem<string> item, Exception e = null)
        {
            UpdateWasCalled = true;
            return Task.CompletedTask;
        }
    }
}
