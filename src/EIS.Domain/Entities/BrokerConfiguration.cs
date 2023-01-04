namespace EIS.Domain.Entities;

public class BrokerConfiguration
{
    public string Url { get; set;}
    public string Username { get; set;}
    public string Password { get; set;}
    public string CronExpression { get; set;}
    public int RefreshInterval { get; set;}
    public string InboxOutboxTimmerPeriod { get; set;}
}