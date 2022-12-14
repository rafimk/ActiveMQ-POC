using System.Net.Mime;
using System;
using System.Net.NetworkInformation;
using System.Buffers;
using System.Security.Authentication.ExtendedProtection;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Net.Security;
using System.ComponentModel.Design;
namespace EIS.Infrastructure.Configuration;

public static class EisStartup
{
    public static IServiceCollection AddEISServices(this IServiceCollection services)
    {
        services.AddSingleton<IConfigurationManager, ConfigurationManager>();
        services.AddSingleton<IEventInboxOutboxDbContext, EventInboxOutboxDbContext>();
        services.AddSingleton<ICompetingConsumerDbContext, CompetingConsumerDbContext>();
        services.AddSingleton<IDatabaseBootstrap, DatabaseBootstrap>();
        services.AddSingleton<BrokerConfiguration>();
        services.AddSingleton<EventHandlerRegistry>();
        services.AddSingleton<IBrokerConnectionFactory, BrokerConnectionFactory>();
        services.AddSingleton<IMessageQueueManager, MessageQueueManager>();
        services.AddSingleton<IEventPublisherService, EventPublisherService>();
        services.AddSingleton<IJobFactory, JobFactory>();

        var properties = new NameValueCollection
        {
            ["quartz.scheduler.instance"] = "ConsumerQuartzScheduler",
            ["quartz.scheduler.instanceId"] = "ConsumerQuartzInstance"
        };

        services.AddSingleton<IScheduleFactory>(sf = new StdSchedulerFactory(properties));
        services.AddHostedService<QuartzHostedService>();

        services.Configure<QuartzOptions>(options => 
        {
            options.SchedulerName = "Quartz ASP.Net Core Eis Scheduler";
            options.Scheduling.IgnoreDuplicates = true; // default : false;
            options.Scheduling.OverWriteExistingData = true; // default : true;
        });

        // Add the required Quartz.Net services.
        services.AddSingleton<ConsumerKeepAliveEntryPollerJob>();
        services.AddSingleton<InboxOutboxPollerJob>();
        services.AddSingleton<IJobSchedule, ConsumerKeepAliveJobSchedule>();
        services.AddSingleton<IJobSchedule, InboxOutboxPollerJobSchedule>();
        return service;
    }

    public static IApplicationBuilder AddEISProcessor<T>(this IApplicationBuilder app) where T : IMessageProcessor
    {
        if (Nullable.GetUnderlyingType(typeof(T)) != null || typeof(T).IsClass) 
        {
            var scopedFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            var scope = scopedFactory?.CreateScope();
            var eventProcessor = (T)scope?.ServiceProvider?.GetRequiredService<IMessageProcessor>();
            EventHandlerRegistry eventHandlerRegistry = app.ApplicationService.GetService<EventHandlerRegistry>();
            eventHandlerRegistry?.AddMessageProcessor(eventProcessor);
        }
        return AddEISProcessor(app);
    }

    public static IApplicationBuilder AddEISProcessor(this IApplication app)
    {
        app.ApplicationService.GetRequiredService<IMessageQueueManager>();
        app.ApplicationService.GetRequiredService<IDatabaseBootstrap>();

        return app;
    }
}