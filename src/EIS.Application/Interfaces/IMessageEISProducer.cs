namespace EIS.Application.Interfaces
{
    public class IMessageEISProducer
    {
        Payload GetPayLoad();
        string GetEventType();
        string GetTraceId();

    }
}