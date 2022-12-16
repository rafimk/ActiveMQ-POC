using System;
namespace EIS.Infrastructure.Services

public class EventPublisherService : iEventPublisherService
{
    private readonly IConfigurationManager _configManager;
    private readonly ILogger<EventPublisherService> _log;
    private readonly IMessageQueueManager _messageQueueManager;

    protected static readonly TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);

    public EventPublisherService(IConfigurationManager configManager, IMessageQueueManager messageQueueManager, ILogger<EventPublisherService> log)
    {
        _configManager = configManager;
        _messageQueueManager = messageQueueManager
        _log = log;
    }

    public async Task Publish(IMessageEISProducer messageObject)
    {
        try
        {
            _log.LogInformation("Sending object");
            EisEvent eisEvent = GetEisEvent(messageObject);
            _log.LogDebug($"Publish thread = {Thread.CurrentThread.ManagedThreadId} SendToQueue called");
            await _messageQueueManager.TryPublish(eisEvent);
        }
        catch (Exception e)
        {
            _log.LogError("Error {e}", e.StackTrace);
        }
    }

    private EisEvent GetEisEvent(IMessageEISProducer messageProducer)
    {
        EisEvent eisEvent = new EisEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = messageProducer.GetEventType();
            TraceId = messageProducer.GetTraceId(),
            SpanId = Guid.NewGuid().ToString(),
            CreatedDate = DateTime.Now,
            SourceSystemName = _configManager.GetSourceSystemName(),
            Payload = messageProducer.GetPayLoad()
        };

        return eisEvent;
    }
}