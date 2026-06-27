using LinkUpPro.Core.Application.Interfaces.IServices;
using Supabase;

namespace LinkUpPro.Infrastructure.Shared.Services
{
    public class SupabaseStorageService : IFileStorageService
    {
        private readonly Client _supabaseClient;

        public SupabaseStorageService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string bucketName, string contentType)
        {
            var options = new Supabase.Storage.FileOptions
            {
                ContentType = contentType,
                Upsert = true
            };

            await _supabaseClient.Storage.From(bucketName).Upload(fileBytes, fileName, options);

            return _supabaseClient.Storage.From(bucketName).GetPublicUrl(fileName);
        }

        public async Task DeleteFileAsync(string fileUrl, string bucketName)
        {
            var uri = new Uri(fileUrl);
            var pathAndQuery = uri.PathAndQuery;
            // The public URL looks like: https://.../storage/v1/object/public/bucketName/fileName
            var pathSegments = pathAndQuery.Split('/');
            var fileName = pathSegments.Last();

            await _supabaseClient.Storage.From(bucketName).Remove(new List<string> { fileName });
        }
    }
}
