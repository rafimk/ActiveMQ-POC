namespace EIS.Infrastructure.Scheduler

public class ConsumerKeepAliveJobSchedule : IJobSchedule 
{
    private readonly IConfigurationManager _configManager;

    public ConsumerKeepAliveJobSchedule(IConfigurationManager configManager)
    {
        _configManager = configManager;
        JobType = typeof(ConsumerKeepAliveEntryPollerJob)

    }

    public Type JobType { get; }
    public string GetCronExpression
    {
        return _configManager.GetBrokerConfiguration().CronExpression;
    }
}