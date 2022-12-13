namespace EIS.Domain.Entities
{
    public class EisEvent
    {
        public string EventId { get; set;}
        public string EventType { get; set;}
        public DateTime CreatedDate { get; set;}
        public string SourceSystemName { get; set;}
        public string TraceId { get; set;}
        public string SpanId { get; set;}
        public Payload Payload { get; set;}
    }
}