using USSDMiddleware.Core.Enums;

namespace USSDMiddleware.Core.Exceptions;

public class UssdMiddlewareException : Exception
{
    public ExceptionType ExceptionType { get; set; }

    public UssdMiddlewareException(ExceptionType exceptionType, string? message) : base(message)
    {
        ExceptionType = exceptionType;
    }
}