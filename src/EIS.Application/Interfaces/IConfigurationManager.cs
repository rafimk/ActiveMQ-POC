using EIS.Domain.Entities;

namespace EIS.Application.Interfaces;

public interface IConfigurationManager
{
    string GetBrokeUrl();
    ApplicationSettings GetAppSettings();
    BrokerConfiguration GetBrokerConfiguration();
    void Dispose();

    string GetSourceSystemName();
    bool GetMessageSubcriptionStatus();
    
}