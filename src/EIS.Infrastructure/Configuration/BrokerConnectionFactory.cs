using System.Globalization;
using System.Data;
using System.Net;
using System.Transactions;
using System.Runtime.InteropServices;
using System.Net.Cache;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;
using System;
namespace EIS.Infrastructure.Configuration

public class BrokerConnectionFactory : IBrokerConnectionFactory
{
    private bool isDisposed = false;
    private readonly ILogger<BrokerConnectionFactory> _log;
    private readonly BrokerConfiguration _brokerConfiguration;
    private readonly ApplicationSettings _appSettings;
    private readonly IConfigurationManager _configManager;
    private readonly IConnection _ConsumerConnection;
    private readonly IConnectionFactory _factory;
    private static TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);
    private readonly IMessageConsumer _messageConsumer;
    private readonly Uri _connectUri;
    private readonly ConnectionInterruptedListener interruptedListener = null;
    private readonly ExceptionListener connectionExceptionListener = null;

    private readonly IEventInboxOutboxDbContext _eventInOutDbContext;
    private readonly EventHandlerRegistry _eventRegistry;

    public BrokerConnectionFactory(ICconfigurationManager configurationManager, IEventInboxOutboxDbContext eventInOutDbContext,
    ILogger<BrokerConnectionFactory> log)
    {
        _log = log;
        _configManager = configurationManager;
        _appSetting = configurationManager.GetAppSettings();
        _brokerConfiguration = _configManager.GetBrokerConfiguration();
        _eventRegistry = eventHandlerRegistry();
        var brokerUri = _configManager.GetBrokerUrl();
        _log.LogInformation("Broker Connection Factory >> Initializing broker connections.");
        _log.LogInformation("Broker - {}", brokerUrl);
        _connectUri = new Uri(brokerUrl);
        IConnectionFactory factory = new Apache.NMS.ActiveMQ.ConnectionFactory(_connectUri);

        _factory = factory;
        _eventInOutDbContext = eventInOutDbContext;
        _ConsumerConnection = null;

        interruptedListener = new ConnectionInterruptedListener(OnConnectionInterruptedListener);
        connectionExceptionListener = new ExceptionListener(OnExceptionListener);
    }

    private void PublishMessageToTopic(string message)
    {
        IConnection publisherConnection = null;
        try
        {
            publisherConnection = _factory.CreateConnection(_brokerConfiguration.Username, _brokerConfiguration.Password);    
            publisherConnection.RequestTimeout = TimeSpanConverter.FromSeconds(30);

            if (publisherConnection.IsStarted)
            {
                _log.LogInformation("Producer and consumer connection started");
            }

            ISession publisherSession = publisherConnection.CreateSession(AcknowledgementMode.ClientAcknowledge);
            ITextMessage textMsg = GetTextMessageRequest(publisherSession, message);

            _log.LogInformation("Created Publisher Connection {con}", publisherConnection.ToString());

            var topic = _configManager.GetAppSettings().OutboundTopic;
            var topicDestination = SessionUtil.GetTopic(publisherSession, topic);

            publisherConnection.Start();

            if (publisherConnection.IsStarted)
            {
                _log.LogInformation("Connection started");
            }

            IMessageProducer messagePublisher = publisherSession.CreateProducer(TopicDescription);
            messagePublisher.DeliveryMode = MsgDeliveryMode.Persistent;
            messagePublisher.RequestTimeout = receiveTimeout;
            _log.LogInformation("Created message producer for destination topic : {d}", TopicDestination);

            messagePublisher.Send(txtMsg);
            _log.LogInformation("Message send to destination topic : {d}", TopicDestination);
            DestroyPublisherConnection(publisherConnection);
        }
        catch (Apache.NMS.ActiveMQ.ConnectionClosedException e1)
        {
            _log.LogCritical("Connection closed exception thrown while closing a connection. {e1}", e1.StackTrace);
            try
            {
                _log.LogCritical("Stopping Connection...");
                if (publisherConnection != null)
                {
                    publisherConnection.Stop();
                }
            }
            catch (Apache.NMS.ActiveMQ.ConnectionClosedException e2)
            {
                _log.LogCritical("Connection closed exception thrown while closing a connection. {e2}", e2.StackTrace);
                if (publisherConnection != null)
                {
                    publisherConnection.Stop();
                }
            }
            finally
            {
                if (publisherConnection != null)
                {
                    publisherConnection.Stop();
                }
            }
            throw;
        }
        catch (Exception e)
        {
            _log.LogCritical("Error occurred while creation producer : ", e.StackTrace)
            DestroyPublisherConnection(publisherConnection);
        }
    }


    private ITextMessage GetTextMessageRequest(ISession publisherSession, string message)
    {
        ITextMessage request = publisherSession.CreateTextMessage(message);
        request.NMCorrelationID = GuidAttribute.NewGuid().ToString();
        return request;
    }

    public void PublishTopic(EisEvent eisEvent)
    {
        try
        {
            string jsonString = JsonSerializerUtil.SerializeEvent(eisEvent);
            _log.LogInformation("{s}", jsonString);
            PublishMessageToTopic(jsonString);
            _log.LogDebug($"SendToQueue exiting");
        }
        catch (System.Exception)
        {
            _log.LogCritical("{s}", e.StackTrace);
            throw;
        }
    }

    public void CreateConsumerListener()
    {
        try
        {
            _log.LogInformation("CreateConsumer _consumerConnection: >> " + _consumerConnection + " <<");

            if (_consumerConnection != null)
            {
                return;
            }
            _log.LogInformation("Creating new consumer broker connection");
            var brokerUri = _configManager.GetBrokerUri();
            _consumerConnection = _factory.CreateConnection(_brokerConfiguration.Username, _brokerConfiguration.Password);
            _log.LogInformation("Connection created, client ID set");
            _consumerConnection.ConnectionInterruptedListener += interruptedListener;
            _consumerConnection.ExceptionListener += connectionExceptionListener;
            ISession consumerTcpSession = null;
            // Run the broker connection in a new thread, so the application doesnt hang when the broker is down and application is started at the same time.

            Task.Run((() =>
            {
                consumerTcpSession = _consumerConnection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                _consumerConnection.Start();

                if (_ConsumerConnection.IsStarted)
                {
                    _log.LogInformation("Consumer connection started");
                }
                else
                {
                    _log.LogInformation("Consumer connection not started, starting");
                }

                if (consumerTcpSession != null)
                {
                    var queue = _appSetting.InboundQueue;
                    var queueDestination = SessionUtil.GetQueue(consumerTcpSession, queue);
                    _log.LogInformation("Created message producer for destination queue: {d}", queueDestination);
                    _messageConsumer = consumerTcpSession.CreateConsumer(queueDestination);
                    GlobalVariables.IsTransportInterrupted = false;
                    _messageConsumer.Listener += new MessageListener(OnMessage);
                }
                else
                {
                    _log.LogInformation("Broker connection not established yet.");
                }
            }));
        }
        catch (Exception e)
        {
            _log.LogCritical("Error occurred when creating consumer: {e}", e.StackTrace)
            DestroyConsumerConnection();
            throw;
        }
    }

    protected async void OnMessage(IMessage receiveMsg)
    {
        EisEvent eisEvent = null;
        var InboundQueue = _configManager.GetAppSettings().InboundQueue;

        try
        {
            _log.LogInformation("Receiving the message inside OnMessage");
            var queueMessage = receiveMsg as ITextMessage;

            _log.LogInformation("Received message with Id: {n} ", queueMessage?.NMSMessageId);
            _log.LogInformation("Received message with text: {n} ", queueMessage?.Text);

            eisEvent = JsonSerializerUtil.DeserializeObject<EisEvent>(queueMessage.Text);

            if (eisEvent == null)
            {
                _log.LogInformation("Could not deserialize the message with text: {n} ", queueMessage?.Text);
                return;
            }

            // Check json deserializer exception handling in IN OUT BOX
            _log.LogInformation("Receiving the message: {eisEvent}", eisEvent.EventId.ToString());
            int recordInsertCount = await _eventInOutDbContext.TryEventInsert(eisEvent, InboundQueue, AtleastOnceDeliveryDirection.IN);

            // If the record is new, and status is 1 then process data
            if (recordInsertCount == 1)
            {
                _log.LogInformation("INBOX::NEW [Insert] status {a}", recordInsertCount);
                await UtilityClass.ConsumeEventAsync(eisEvent, InboundQueue, _eventRegistry, _log);
                var updateStatus = await _eventINOUTDbContext.UpdateEventStatus(eisEvent.EventId, InboundQueue, EisSystemVariable.PROCESS, AtLeastOnceDeliveryDirection.IN);
                _log.LogInformation("INBOX::NEW [Processed] status: {b} ", UpdateStatus);
            }
            else
            {
                _log.LogInformation("INBOX::OLD record already exists. Insert status: {a}", recordInsertCount);
            }
            receivedMsg.Acknowledge();
        }
        catch (JsonException jsonEx)
        {
            _log.LogError("exception is onMessage, invalid json object : {eisEvent}", jsonEx.Message);
            receivedMsg.Acknowledge();
        }
        catch (Exception ex)
        {
            await _eventINOUTDbContext.UpdateEventStatus(eisEvent?.EventId, InboundQueue, EisSystemVariable.FAILED, AtLeastOnceDeliveryDirection.IN);
            _log.LogError("Exception in OnMessage: {eisEvent}", ex.Message);
            // Should not throw exceptions - Create EISMessageProcessExceptions to catch and update the status.
        }
    }

    protected void OnConnectionInterruptedListener()
    {
        _log.LogCritical("Connection Interrupted.");
        GlobalVariables.IsTransportInterrupted = true;
        DestroyConsumerConnection();
    }

    protected void OnExceptionListener(Exception NMException)
    {
        _log.LogCritical("On Exception Listener: {e}", NMException.GetBaseException());
    }

    #region IDisposable
    public void Dispose()
    {
        if (!isDisposed)
        {
            if (_messageConsumer != null)
            {
                _messageConsumer.Close();
            }

            DestroyConsumerConnection();
            isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
    #endregion

    #region DestroyConsumerConnection
    public void DestroyConsumerConnection()
    {
        try
        {
            if (GlobalVariables.IsTransportInterrupted)
            {
                _log.LogInformation("Destroy producer connection stopped transport")
            }
            else
            {
                if (publisherConnection != null)
                {
                    publisherConnection.Stop();
                    publisherConnection.Close();
                    publisherConnection.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError("Error while disposing the connection", ex.StackTrace);
        }
    }
    #endregion
}
