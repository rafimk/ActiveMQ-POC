namespace EIS.Infrastructure.Services

public class EventConsumerService
{
    private readonly IEventInboxOutboxDbContext _eventInboxOutboxDbContext;
    private readonly string _inboundQueue;

    public EventConsumerService(IEventInboxOutboxDbContext eventInboxOutboxDbContext, IConfigurationManager configManager)
    {
        _eventInboxOutboxDbContext = eventInboxOutboxDbContext;
        _inboundQueue = configManager.GetAppSettings().InboundQueue;
    }

    public async Task<int> UpdateProcessEventStatus(EisEvent eisEvent, string eventStatus)
    {
        return await _eventInboxOutboxDbContext.UpdateProcessEventStatus(eisEvent.EventId, _inboundQueue, eventStatus, AtleastOnceDeliveryDirection.IN);
    }

}