using EIS.Domain.Entities;
using System.Threading.Tasks;

namespace EIS.Application.Interfaces;

public interface IMessageProcessor
{
    Task Process(Payload payload, string eventType);
}