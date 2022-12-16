using System.Threading.Tasks;
namespace EIS.Infrastructure.Scheduler.Jobs

[DisallowConcurrentExecution]
public class ConsumerKeepAliveEntryPollerJob : IJob
{
    private readonly IMessageQueueManager _messageQueueManager;

    public ConsumerKeepAliveEntryPollerJob(IMessageQueueManager messageQueueManager)
    {
        _messageQueueManager = messageQueueManager;
    }

    public await Task Execute(IJobExecutionContext context)
    {
        if (GlobalVariables.IsDatabaseTablesInitialized && GlobalVariables.IsMessageQueueSubscribed)
        {
            _messageQueueManager.ConsumerKeepAliveTask();
        }

        return Task.CompletedTask;
    }
}