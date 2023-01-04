using EIS.Domain.Entities;
using System.Threading.Tasks;

namespace EIS.Application.Interfaces;

public interface IMessageQueueManager
{
    Task ConsumerEvent(EisEvent eisEvent, string queueName);
    Task InboxOutboxPollerTask();
    Task TryPublish(EisEvent eisEvent);
    Task ConsumerKeepAliveTask();
}