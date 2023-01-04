namespace EIS.Application.Constants;

public class GlobalVariables
{
    private static readonly object _stopLock = new object();
    private static readonly object _transportLock = new object();
    private static bool _isUnprocessedOutMessagePresentInDB = true;
    private static bool _isUnprocessedInMessagePresentInDB = true;
    private static bool _isTransportInterrupted = true;
    private static bool _isCurrentIpLockedForConsumer = false;
    private static bool _isDatabaseTablesInitialized = false;
    private static bool _isMessageQueueSubscribed = false;
    private static string _applicationName = "MessageCore";

    public static bool IsMessageQueueSubscribed
    {
        get 
        {
            lock (_stopLock)
            {
                return _isMessageQueueSubscribed;
            }
        }

        set
        {
            lock (_stopLock)
            {
                _isMessageQueueSubscribed = value;
            }
        }
    }

    public static bool IsDatabaseTablesInitialized
    {
        get 
        {
            lock (_stopLock)
            {
                return _isDatabaseTablesInitialized;
            }
        }

        set
        {
            lock (_stopLock)
            {
                _isDatabaseTablesInitialized = value;
            }
        }
    }

    public static bool isUnprocessedOutMessagePresentInDB
    {
        get 
        {
            lock (_stopLock)
            {
                return _isUnprocessedOutMessagePresentInDB;
            }
        }

        set
        {
            lock (_stopLock)
            {
                _isUnprocessedOutMessagePresentInDB = value;
            }
        }
    }

    public static bool IsUnprocessedInMessagePresentInDB
    {
        get 
        {
            lock (_stopLock)
            {
                return _isUnprocessedInMessagePresentInDB;
            }
        }

        set
        {
            lock (_stopLock)
            {
                _isUnprocessedInMessagePresentInDB = value;
            }
        }
    }

    public static bool IsTransportInterrupted
    {
        get 
        {
            lock (_transportLock)
            {
                return _isTransportInterrupted;
            }
        }

        set
        {
            lock (_transportLock)
            {
                _isTransportInterrupted = value;
            }
        }
    }

    public static bool IsCurrentIpLockedForConsumer
    {
        get 
        {
            lock (_transportLock)
            {
                return _isCurrentIpLockedForConsumer;
            }
        }

        set
        {
            lock (_transportLock)
            {
                _isCurrentIpLockedForConsumer = value;
            }
        }
    }

    public static string ApplicationName
    {
        get 
        {
            return _applicationName;
        }
        set
        {
            _applicationName = value;
        }
    }

}