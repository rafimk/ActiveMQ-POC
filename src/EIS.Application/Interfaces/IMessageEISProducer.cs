using EIS.Domain.Entities;

namespace EIS.Application.Interfaces;

public interface IMessageEISProducer
{
    Payload GetPayLoad();
    string GetEventType();
    string GetTraceId();

}