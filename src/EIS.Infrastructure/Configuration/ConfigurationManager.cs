using System.Net.Mime;
using System.IO;
using System.Reflection.Metadata;
using System;
using System.Reflection.Emit;
using System.Reflection;
using EIS.Application.Interfaces;
using Microsoft.Extensions.Logging;
using EIS.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace EIS.Infrastructure.Configuration;

public class ConfigurationManager : IConfigurationManager
{
    private bool isDisposed = false;
    private readonly ILogger<ConfigurationManager> _log;
    private BrokerConfiguration _brokerConfiguration;
    private ApplicationSettings _appSettings;
    private readonly IConfiguration _configuration;
    private string _sourceSystemName;
    private bool _messageSubscription;

    public ConfigurationManager(ILogger<ConfigurationManager> log, IConfiguration configuration)
    {
        _log = log;
        _log.LogInformation("Configuration manager constructor");
        _configuration = configuration;
        BindAppSettingsToObjects();
    }

    private void BindAppSettingsToObjects()
    {
        _log.LogInformation("Loading application configurations");

        var assembly = Assembly.GetExecutingAssembly();
        var brokerConfigurationClass = new BrokerConfiguration();
        var applicationSettingsList = new List<ApplicationSettings>();

        // If any environment profiles are set, bind settings from that file.
        var environment = _configuration["environment:profile"];
        _sourceSystemName = _configuration["eis:source-system-name"];
        _messageSubscription = _configuration["eis:messageSubscription"] == null ? true : _configuration["eis:messageSubscription"].Equals("true");

        string eisSettingsFileName = "EISCore.eissettings.json";
        _log.LogInformation("Loading assemblies");
        _log.LogInformation("Settings loading from {a}", eisSettingsFileName);
        Stream stream = AssemblyBuilder.GetManifestResourceStream(eisSettingsFileName);
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonStream(stream);
        var configSectionFromFile = configurationBuilder.Build();
        configSectionFromFile.GetSection("ApplicationSettings").Bind(applicationSettingsList);
        _appSetting = GetAppSettingsFromList(applicationSettingsList);

        GlobalVariables.IsMessageQueueSubscribed = _messageSubscription;

        if (environment != null)
        {
            eissettingsFileName = eisSettingsFileName.Replace(".json", string.Empty) + "-" + Environment.ToLower() + ".json";
            _log.LogInformation("Loading : {n}", eissettingsFileName);
            stream = assembly.GetManifestResourceStream(eissettingsFileName);
            configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(stream);
            configSectionFromFile = configurationBuilder.Build();
            configSectionFromFile.GetSection("BrokerConfiguration").Bind(brokerConfigurationClass); // Bind broker settings to BrokerConfiguration class.
            _brokerConfiguration = brokerConfigurationClass;
        }
        else
        {
            _log.LogInformation("Environment is null, loading from : {n}", eissettingsFileName);
            stream = assembly.GetManifestResourceStream(eissettingsFileName);
            configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(stream);
            configSectionFromFile = configurationBuilder.Build();
            configSectionFromFile.GetSection("BrokerConfiguration").Bind(brokerConfigurationClass); // Bind broker settings to BrokerConfiguration class.
            _brokerConfiguration = brokerConfigurationClass;
        }
        stream.Close();
        _log.LogInformation("Consumer connection Quartz Job... Cron: [" + _brokerConfiguration.CronExpression + "]");
        _log.LogInformation("Inbox - Outbox      Quarts Job... Cron: [" + _brokerConfiguration.InboxOutboxTimePeriod + "]");
    }

    public string GetBrokerUrl()
    {
        if (_brokerConfiguration != null)
        {
            return _brokerConfiguration.Url;
        }

        return null;
    }

    public bool GetMessageSubscriptionStatus() 
    {
        return _messageSubscription;
    }

    public BrokerConfiguration GetBrokerConfiguration()
    {
        return _brokerConfiguration;
    }

    public ApplicationSettings GetAppSettings()
    {
        return _appSettings;
    }

    public string GetSourceSystemName()
    {
        return _sourceSystemName;
    }

    #region 
    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
        }
    }

    public string GetBrokeUrl()
    {
        throw new NotImplementedException();
    }

    public bool GetMessageSubcriptionStatus()
    {
        throw new NotImplementedException();
    }
    #endregion

}