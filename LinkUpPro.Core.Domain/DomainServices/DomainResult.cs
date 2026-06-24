namespace LinkUpPro.Core.Domain.DomainServices
{
    public class DomainResult
    {
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static DomainResult Success() => new() { Succeeded = true };
        public static DomainResult Failure(string error) => new() { Succeeded = false, ErrorMessage = error };
    }
}
