using System;

namespace EIS.Application.Exceptions;

public class EisMessageProcessException : Exception
{
    public EisMessageProcessException(string message, Exception exception) : base(message, exception)
    {
    }

    public EisMessageProcessException(string message) : base(message)
    {
    }
}