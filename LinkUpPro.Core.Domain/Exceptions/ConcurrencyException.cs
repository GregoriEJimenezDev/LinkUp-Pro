namespace LinkUpPro.Core.Domain.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message = "La entidad ha sido modificada por otro usuario. porfavor, intentelo de nuevo mas tarde.")
            : base(message) { }

        public ConcurrencyException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
