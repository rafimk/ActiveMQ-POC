using System;

namespace EIS.Application.Exceptions;

public class MessagePublishException : Exception
{
    public MessagePublishException(string message, Exception exception) : base(message, exception)
    {
    }

    public MessagePublishException(string message) : base(message)
    {
    }
}