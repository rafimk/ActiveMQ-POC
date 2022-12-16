using System;
using System.Threading.Tasks;
using System.Threading;
namespace EIS.Infrastructure.Services

public class QuartzHostedService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IJobFactory _jobFactory;
    private readonly IEnumerable<IJobSchedule> _jobSchedules;

    public QuartzHostedService(ISchedulerFactory schedulerFactory, IEnumerable<IJobSchedule> jobSchedules, IJobFactory jobFactory)
    {
        _schedulerFactory = schedulerFactory;  
        _jobFactory = jobFactory;
        _jobSchedules = jobSchedules;
    }

    public IScheduler Scheduler { get; set;}

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        TaskScheduler.JobFactory = _jobFactory;

        foreach (var jobSchedule in _jobSchedules)
        {
            var job = CreateJob(jobSchedule);
            var trigger = CreateTrigger(jobSchedule);
            await Scheduler.ScheduleJob(job, trigger, cancellationToken);
        }

        await Scheduler.Start(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Scheduler?.Shutdown(cancellationToken);
    }

    private static ITrigger CreateTrigger(IJobSchedule schedule)
    {
        return TriggerBuilder.Create()
                             .WithIdentity($"{schedule.JobType.FullName}.trigger")
                             .WithCronSchedule(schedule.GetCronExpression(), x => x.WithMisfireHandlingInstructionDoNothing())
                             .StartAt(DateTime.Now.AddDays(-1))
                             .StartNow()
                             .WithDescription(schedule.GetCronExpression())
                             .Build();
    }

    public static IJobDetails CreateJob(IJobSchedule schedule)
    {
        var jobType = schedule.JobType;
        return JobBuilder.Create(jobType)
                         .WithIdentity(jobType.FullName)
                         .WithDescription(jobType.Name)
                         .Build();
    }
}