namespace USSDMiddleware.Core.Exceptions
{
    public class OperationFailedException : Exception
    {
        public OperationFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
