namespace LinkUpPro.Core.Domain.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message = "The entity was modified by another user. Please retry.")
            : base(message) { }

        public ConcurrencyException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
