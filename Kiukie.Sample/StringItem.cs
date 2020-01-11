using System;

namespace Kiukie.Sample
{
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
}
