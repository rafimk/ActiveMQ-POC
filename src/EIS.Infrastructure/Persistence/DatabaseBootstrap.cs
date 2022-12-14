using System.Xml.Linq;
using System.IO;
using System.Reflection;
namespace EIS.Infrastructure.Persistence;

public class DatabaseBootstrap : IDatabaseBootstrap
{
    private readonly string _databaseName;
    private readonly IConfiguration _configuration;
    private readonly IEventInboxOutboxDbContext _eventINOUTDbContext;
    private readonly string _OutboundTopic;
    private readonly string _InboundQueue;

    public DatabaseBootstrap(IConfiguration configuration, IConfigurationManager configManager, IEventInboxOutboxDbContext eventINOUTDbContext)
    {
        _configuration = configuration;
        _eventINOUTDbContext = eventINOUTDbContext;
        _databaseName = configuration.GetConnectionString("DefaultConnection");
        Setup();
        _OutboundTopic = configuration.GetAppSettings().OutboundTopic;
        _InboundQueue = configuration.GetAppSettings().InboundQueue;

        InitiateUnProcessedInOutMessage();
    }

    public void Setup()
    {
        Console.WriteLine("Setup....");
        using (car connection = new SqlConnection(_databaseName))
        {
            try
            {
                Console.WriteLine("Before executing schema. sql");

                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "EISCore.schema.sql";

                string result = "";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName));

                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                connection.ExecuteAsync(result);

                Console.WriteLine($"Created database {_databaseName}");
                GlobalVariables.IsDatabaseTablesInitialized = true;
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith("There is already an object named"))
                {
                    GlobalVariables.IsDatabaseTablesInitialized = true;
                }
                else if (e.Message.StartsWith("Database Error:Invalid object name"))
                {
                    GlobalVariables.IsDatabaseTablesInitialized = true;
                }
                else
                {
                    GlobalVariables.IsDatabaseTablesInitialized = false;
                }
                Console.WriteLine("Exception creating database: " + e.Message);
            }
        }
    }

    public async void InitiateUnprocessedINOUTMessages()
    {
        List<EisEventInboxOutbox> inboxEventList = await _eventINOUTDbContext.GetAllUnprocessedEvents(AtleastOnceDeliveryDirection.IN, _InboundQueue);
        List<EisEventInboxOutbox> outboxEventList = await _eventINOUTDbContext.GetAllUnprocessedEvents(AtleastOnceDeliveryDirection.IN, _OutboundQueue);

        if (inboxEventList != null && inboxEventList.Count > 0)
        {
            GlobalVariables.IsUnprocessedInMessagePresentInDB = true;
        }
        else
        {
            GlobalVariables.IsUnprocessedInMessagePresentInDB = false;
        }

         if (outboxEventList != null && outboxEventList.Count > 0)
        {
            GlobalVariables.IsUnprocessedOutMessagePresentInDB = true;
        }
        else
        {
            GlobalVariables.IsUnprocessedOutMessagePresentInDB = false;
        }

    }

}