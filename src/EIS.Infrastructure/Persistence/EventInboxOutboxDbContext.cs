using System;
namespace EIS.Infrastructure.Persistence;

public class EventInboxOutboxDbContext : IEventInboxOutboxDbContext
{
    private readonly string _databaseName;
    private readonly ILogger<EventInboxOutboxDbContext> _log;
    private readonly IConfiguration _configuration;

    public EventInboxOutboxDbContext(IConfiguration configuration, ILogger<EventInboxOutboxDbContext> _log)
    {
        _log = log;
        _configuration = configuration;
        _databaseName = _configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<int> TryEventInsert(EisEvent eisEvent, string topicQueueName, string direction)
    {
        string sql = "INSERT INTO EIS_EVENT_INBOX_OUTBOX(ID, EVENT_ID, TOPIC_QUEUE_NAME, EIS_EVENT, EVENT_TIMESTAMP, IN_OUT)" +
        "(SELECT CAST(@Id AS VARCHAR(50)), CAST(@EventId AS VARCHAR(50)), CAST(@topicQueueName AS VARCHAR(50)), CAST(@objString AS VARCHAR(MAX)), GetDate(), CAST(@direction AS VARCHAR(3)) " +
        "WHERE NOT EXIST (SELECT 1 FROM EIS_EVENT_INBOX_OUTBOX WITH (nolock) WHERE EVENT_ID = @EventId AND TOPIC_QUEUE_NAME = @topicQueueName AND IN_OUT = @direction)) ";

        using (var connection = new SqlConnection(_databaseName))
        {
            try
            {
                string objString = JsonSerializerUtil.SerializeEvent(eisEvent);
                var Id = Guid.NewGuid().ToString();

                _log.LogDebug("Executing query: {sql} with variables [{Id}, {eisEvent.EventId}, {topicQueueName}, {objString}, {direction}]", sql, id, eisEvent.EventId, topicQueueName, objString, direction);

                return await connection.ExecuteAsync(sql, new { Id, eisEvent.EventId, topicQueName, objString, direction });
            }
            catch (Exception e)
            {
                _log.LogError("Database Error: {e}", e.Message);
                throw;
            }
        }
    }

    public async Task<int> UpdateEventStatus(string eventId, string topicQueueName, string eventStatus, string direction)
    {
        string sql = "UPDATE EIS_EVENT_INBOX_OUTBOX_ SET IS_EVENT_PROCESSED = @eventStatus WHERE EVENT_ID = @eventId AND TOPIC_QUEUE_NAME = @topicQueueName AND IN_OUT = @direction ";

        using (var connection = new SqlConnection(_databaseName))
        {
            try
            {
                _log.LogDebug("Executing query : {sql} with variables [{eventStatus}, {eventId}, {topicQueueName}, {direction}]"), sql, eventStatus, eventId, topicQueueName, direction);
                return await connection.ExecuteAsync(sql, new { eventStatus, eventId, topicQueueName, direction});
            }
            catch (Exception e)
            {
                _log.LogError("Database Error: {e}", e.Message);
                throw;
            }
        }
    }

    public async Task<List<EisEventInboxOutbox>> GetAllUnprocessedEvents(string direction, string topicQueueName)
    {
        string sql = "SELECT ID, EVENT_ID, AS EVENTID, TOPIC_QUEUE_NAME, EIS_EVENT AS EISEVENT, EVENT_TIMESTAMP AS EVENTTIMESTAMP, IS_EVENT_PROCESSED AS ISEVENTPROCESSED, IN_OUT AS INOUT FROM EIS_EVENT_INBOX_OUTBOX WITH (nolock) WHERE IS_EVENT_PROCESSED IS NULL " +
        "AND IN_OUT = @direction AND TOPIC_QUEUE_NAME = @ topicQueueName ORDER BY EVENT_TIMESTAMP ASC ";

        using (var connection = new SqlConnection(_databaseName))
        {
            try
            {
                _log.LogDebug("Executing query : {sql} with variables [{direction}, {topicQueueName}]", sql, direction, topicQueueName);
                var listOfEvents = await connection.QueryAsync<EisEventInboxOutbox>(sql, new {direction, topicQueueName});
                return listOfEvents.AsList();
            }
            catch (System.Exception)
            {
                _log.LogError("Database Error: {e}", e.Message);
                throw;
            }
        }
    }
}