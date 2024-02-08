using System.Text.Json.Serialization;

namespace Common.Events
{
    public class Event<T>
    {
        public string EventType { get; private set; }
        [JsonPropertyName("Data")]
        public T Payload { get; private set; }

        public Event(T payload)
        {
            EventType = typeof(T).Name;
            Payload = payload;
        }
    }
}
