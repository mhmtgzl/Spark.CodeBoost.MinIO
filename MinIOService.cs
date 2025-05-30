using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Spark.CodeBoost.MinIO;

public class MinIOService : IMinIOService
{

    private readonly IMinioClient minioClient;
    private readonly ILogger<MinIOService> logger;

    public MinIOService(IMinioClient minioClient, ILogger<MinIOService> logger)
    {
        this.minioClient = minioClient;
        this.logger = logger;
    }

    public async Task<bool> UploadFile(IFormFile file, string key, string bucketName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bucketName))
            throw new ArgumentNullException(nameof(bucketName));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        try
        {
            BucketExistsArgs bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
            bucketExistsArgs.IsBucketCreationRequest = true;

            if (!await minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken))
            {
                MakeBucketArgs makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);

                await minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
            }


            PutObjectArgs putObjectArgs = new PutObjectArgs()
                                           .WithBucket(bucketName)
                                           .WithObject(key)
                                           .WithContentType(file.ContentType)
                                           .WithStreamData(file.OpenReadStream())
                                           .WithObjectSize(file.OpenReadStream().Length);

            var result = await minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            if (result.Size > 0)
            {
                return true;
            }

        }
        catch (MinioException ex)
        {
            logger.LogError(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        return false;
    }

    public async Task<string?> GetUrl(string bucketName, string key, int timeoutDuration = 60 * 10, string endpoint = "")
    {

        if (string.IsNullOrEmpty(bucketName))
            throw new ArgumentNullException(nameof(bucketName));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        string urlString = "";

        try
        {

            if (!string.IsNullOrEmpty(endpoint))
            {
                using (IMinioClient preClient = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(minioClient.Config.AccessKey, minioClient.Config.SecretKey)
                    .Build())
                {


                    PresignedGetObjectArgs request = new PresignedGetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(key)
                        .WithExpiry(timeoutDuration);

                    urlString = await preClient.PresignedGetObjectAsync(request);

                }

            }
            else
            {
                PresignedGetObjectArgs request = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(key)
                    .WithExpiry(timeoutDuration);

                urlString = await minioClient.PresignedGetObjectAsync(request);
            }
        }
        catch (MinioException e)
        {
            logger.LogError("Error encountered on server. Message:'{0}' when writing an object", e.Message);
        }
        catch (Exception e)
        {
            logger.LogError("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
        }

        return urlString;
    }

    public async Task<string> GetFileAsBase64String(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var stream = await GetFileAsStream(bucketName, key, cancellationToken);

            if (stream is not null)
                return Convert.ToBase64String(stream.ToArray());
        }
        catch (MinioException e)
        {
            logger.LogError("Error encountered on server. Message:'{0}' when writing an object", e.Message);
        }
        catch (Exception e)
        {
            logger.LogError("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
        }

        return string.Empty;
    }

    public async Task<MemoryStream?> GetFileAsStream(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {

            using (MemoryStream memoryStream = new MemoryStream())
            {

                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                          .WithBucket(bucketName)
                                          .WithObject(key);

                await minioClient.StatObjectAsync(statObjectArgs, cancellationToken);

                GetObjectArgs getObjectArgs = new GetObjectArgs()
                                                  .WithBucket(bucketName)
                                                  .WithObject(key)
                                                  .WithCallbackStream((stream) =>
                                                       {
                                                           stream.CopyTo(memoryStream);
                                                       });

                await minioClient.GetObjectAsync(getObjectArgs, cancellationToken);

                return memoryStream;
            }


        }
        catch (MinioException e)
        {
            logger.LogError("Error encountered on server. Message:'{0}' when writing an object", e.Message);
        }
        catch (Exception e)
        {
            logger.LogError("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
        }

        return null;
    }
    public async Task<bool> DeleteFile(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bucketName))
            throw new ArgumentNullException(nameof(bucketName));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));


        try
        {

            RemoveObjectArgs request = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(key);

            await minioClient.RemoveObjectAsync(request, cancellationToken);

            return true;

        }
        catch (MinioException e)
        {
            logger.LogError("Error encountered on server. Message:'{0}' when deleting an object", e.Message);
        }
        catch (Exception e)
        {
            logger.LogError("Unknown encountered on server. Message:'{0}' when deleting an object", e.Message);
        }

        return false;
    }
}
