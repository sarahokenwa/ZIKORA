namespace USSDMiddleware.Core.Exceptions
{
    public class NotSuccessfulException : Exception
    {
        public NotSuccessfulException(string message) : base(message) { }
    }
}
