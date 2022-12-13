namespace EIS.Application.Interfaces
{
    public interface IEventInboxOutboxDbContext
    {
        Task<List<EisEventInboxOutbox>> GetAllUnprocessedEvents(string direction, string topicQueueName);
        Task<int> TryEventInsert(EisEvent eisEvent, string topicQueueName, string direction);
        Task<int> UpdateEventStatus(string evenitId, string topicQueueName, string eventStatus, string direction);
    }
}