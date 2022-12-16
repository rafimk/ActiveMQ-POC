namespace EIS.Infrastructure

public class MessageQueueManager : IMessageQueueManager
{
    private readonly IBrokerConnectionFactory _brokerConnectionFactory;
    private readonly IEventInboxOutboxDbContext = _eventInboxOutboxDbContext;
    private readonly ICompetingConsumerDbContext = _competingConsumerDbContext;
    private readonly IConfigurationManager _configManager;
    private readonly ILogger<MessageQueueManager> _log;
    private readonly EventHandlerRegistry _eventRegistry;
    private readonly string eisHostIp;
    private readonly _OutboundTopic;
    private readonly _InboundQueue;

    public MessageQueueManager(Parameters)
    {
        
    }

}