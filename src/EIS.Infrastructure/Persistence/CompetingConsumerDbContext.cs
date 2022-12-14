using System.Security.Cryptography.X509Certificates;
using System;
using System.Data;
namespace EIS.Infrastructure.Persistence;

public class CompetingConsumerDbContext : ICompetingConsumerDbContext
{
    private readonly string _databaseName;
    private readonly ILogger<CompetingConsumerDbContext> _log;
    private readonly IConfiguration _configuration;
    private string HostIp;

    public CompetingConsumerDbContext(ILogger<CompetingConsumerDbContext> log, IConfiguration configuration)
    {
        _log = log;
        _configuration = configuration;
        _databaseName = _configuration.GetConnectionString("DefaultConnection");
        HostIp = _configuration["environment:profile"];
    }

    public void setHostIpAddress(string hostIp)
    {
        HostIp = hostIp;
    }

    public async Task<int> InsertEntry(string eisGroupKey)
    {
        using (var connection = new SqlConnection(_databaseName))
        {
            try
            {
                var id = Guid.NewGuid().ToString();

                string sql = "INSERT INTO EIS_COMPETING_CONSUMER_GROUP(ID, GROUP_KEY, HOST_IP_ADDRESS, LAST_ACCESSED_TIMESTAMP) " +
                "(SELECT CAST(@id AS VARCHAR(50)), CAST(@eisGroupKey AS VARCHAR(50)) CAST(@HostIp AS VARCHAR(255)), GetDate() " +
                "WHERE NOT EXISTS (SELECT 1 FROM EIS_COMPETING_CONSUMER_GROUP WITH (nolock) WHERE GROUP_KEY = @eisGroupKey))";

                _log.LogDebug("Executing query: {sql} with variables [{id}, {eisGroupKey}, {HostIp]}]", sql, id, eisGroupKey, HostIp);
                return await Connection.ExecuteAsync(sql, new { id, eisGroupKey, HostIp});
            }
            catch (Exception e)
            {
                _log.LogError("Database Error: {e}", e.Message)
                throw;
            }
        }
    }

    public async Task<int> KeepAliveEntry(bool IsStarted, string eisGroupKey)
    {
        try
        {
            int startStatus = isStared ? 1 : 0;
            _log.LogInformation("Keep alive entry....");

            string sql = "UPDATE EIS_COMPETING_CONSUMER_GROUP SET LAST_ACCESSED_TIMESTAMP = GetDate() WHERE GROUP_KEY = @eisGroupKey AND " +
            "HOST_IP_ADDRESS = @HostIp AND 1 = @startStatus ";

            _log.LogDebug("Executing query: {sql} with variables [{eisGroupJey}, {HostIp}, {startStatus}] ", sql, eisGroupKey, startStatus);

            return await connection.ExecuteAsync(sql, new {eisGroupKey, HostIp, startStatus});
        }
        catch (Exception e)
        {
            _log.LogError("Database error: {e}", e.Message);
            throw;
        }
    }

    public async Task<int> DeleteStateEntry(string eisGroupKey, int eisGroupRefreshInterval)
    {
        using (var connection = new SqlConnection(_databaseName))
        {
            try
            {
                string sql = "DELETE FROM EIS_COMPETING_CONSUMER_GROUP WHERE " +
                "DATEDIFF(MINUTE, LAST_ACCESSED_TIMESTAMP, GETDATE()) > @eisGroupRefreshInterval " +
                "AND GROUP_KEY = @eisGroupKey";
                _log.LogDebug("Executing query: {sql} with variables {eisGroupRefreshInterval}, {eisGroupKey}", sql, eisGroupRefreshInterval, eisGroupKey);

                return await connection.ExecuteAsync(sql, new {eisGroupRefreshInterval, eisGroupKey});
            }
            catch (Exception e)
            {
                _log.LogError("Database Error: {e}", e.Message);
                throw;
            }
        }
    }

    public string GetIpAddressOfServer(string eisGroupKey, int eisGroupRefreshInterval)
    {
        using (var connection = new SqlConnection(_databaseName))
        {
            try
            {
                string sql = "SELECT HOST_IP_ADDRESS FROM EIS_COMPETING_CONSUMER_GROUP WITH (nolock) WHERE GROUP_KEY = @eisGroupKey " +
                "AND DATEDIFF(MINUTE, GETDATE(), LAST_ACCESSED_TIMESTAMP) <= @eisGroupRefreshInterval";

                _LOG.LogDebug("Executing query: {sql} with variables {eisGroupKey}, {eisGroupRefreshInterval}", sql, eisGroupKey, eisGroupRefreshInterval);

                string result = connection.QuerySingleOrDefault<string>(sql, new {eisGroupKey, eisGroupRefreshInterval});
                _log.LogDebug("IP address from query: [ {result} ]", result);
                return result;
            }
            catch (Exception e)
            {
                _log.LogError("Database error: {e}", e.Message);
                throw;
            }
        }
    }
}

