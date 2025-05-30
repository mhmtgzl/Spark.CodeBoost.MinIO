using Microsoft.AspNetCore.Http;

namespace Spark.CodeBoost.MinIO;

public interface IMinIOService
{
    Task<bool> UploadFile(IFormFile file, string key, string bucketName, CancellationToken cancellationToken = default);
    Task<string?> GetUrl(string bucketName, string key, int timeoutDuration = 10, string endpoint = "");
    Task<bool> DeleteFile(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<string> GetFileAsBase64String(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<MemoryStream?> GetFileAsStream(string bucketName, string key, CancellationToken cancellationToken = default);
}
