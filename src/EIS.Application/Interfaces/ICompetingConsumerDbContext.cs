namespace EIS.Application.Interfaces
{
    public interface ICompetingConsumerDbContext
    {
        Task<int> InsertEntry(string eisGroupKey);
        Task<int> KeepAliveEntry(bool isStarted, string eisGroupKey);
        Task<int> DeleteStaleEntry(string eisGroupKey, int eisGroupRefreshInterval);

        void SetHostIpAddress(string hostIp);
    }
}