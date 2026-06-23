namespace LinkUpPro.Core.Application.Interfaces.Services
{
    public class ServiceResult
    {
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static ServiceResult Success() => new() { Succeeded = true };
        public static ServiceResult Failure(string error) => new() { Succeeded = false, ErrorMessage = error };
    }
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };
        public static new ServiceResult<T> Failure(string error) => new() { Succeeded = false, ErrorMessage = error };
    }
}
