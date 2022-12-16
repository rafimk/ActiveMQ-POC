using System.Threading.Tasks;
namespace EIS.Infrastructure.Scheduler.Jobs

public class InboxOutboxPollerJob : IJob
{
    private readonly IMessageQueueManager _messageQueueManager;
    private readonly ILogger<InboxOutboxPollerJob> _log;

    public InboxOutboxPollerJob(IMessageQueueManager _messageQueueManager, ILogger<InboxOutboxPollerJob> _log)
    {
        _messageQueueManager = messageQueueManager;
        _log = log;
    }

    public async Task Execute(IJobExecution context)
    {
        _log.LogInformation("QuartzInboxOutboxPollerJob >> Executing Task...");

        if (GlobalVariables.IsDatabaseTableInitialized)
        {
            _messageQueueManager.InboxOutboxPollerTask();
        }

        return Task.CompletedTask;
    }
}