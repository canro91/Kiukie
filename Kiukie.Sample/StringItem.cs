using System;

namespace Kiukie.Sample
{
    public class StringItem : IQueueItem<string>
    {
        public StringItem() { }

        public StringItem(ItemStatus status, string payload)
        {
            StatusId = (int)status;
            Payload = payload;
        }

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
}
