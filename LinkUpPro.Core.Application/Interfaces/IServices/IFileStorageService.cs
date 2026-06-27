namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string bucketName, string contentType);
        Task DeleteFileAsync(string fileUrl, string bucketName);
    }
}
