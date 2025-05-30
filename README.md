# Spark.CodeBoost.MinIO for .NET

A robust and extensible MinIO integration service for .NET applications, offering file upload, download, URL generation, and deletion features with error handling and logging.

## Features

- ✅ Upload files with `IFormFile` support  
- ✅ Generate pre-signed file URLs  
- ✅ Download as stream or base64  
- ✅ Delete files from bucket  
- ✅ Auto-create buckets if missing  
- ✅ Built-in exception logging via `ILogger`  

---

### Installation

1. Add the [Minio .NET SDK](https://www.nuget.org/packages/Minio):

```bash
dotnet add package Minio

```
### Register dependencies in Program.cs:
```csharp
builder.Services.AddMinIO(options =>
{
    options.EndPoint = "localhost:9000";
    options.AccessKey = "minio-access-key";
    options.SecretKey = "minio-secret-key";
});

```
## Basic Usage
### Uploading Files
```csharp
bool result = await _minioService.UploadFile(
    file: formFile,
    key: "documents/invoice.pdf",
    bucketName: "my-bucket"
);

```
### Generating Pre-signed URL
```csharp
string? url = await _minioService.GetUrl(
    bucketName: "my-bucket",
    key: "documents/invoice.pdf"
);

```
### Generating Pre-signed URL
```csharp
string base64 = await _minioService.GetFileAsBase64String(
    bucketName: "my-bucket",
    key: "images/logo.png"
);

```
### Downloading as Stream
```csharp
MemoryStream? stream = await _minioService.GetFileAsStream(
    bucketName: "my-bucket",
    key: "backups/db.bak"
);

```
### Deleting Files
```csharp
bool deleted = await _minioService.DeleteFile(
    bucketName: "my-bucket",
    key: "temp/file-to-delete.txt"
);

```
## Extension Method Usage
### AddMinIO via Dependency Injection
```csharp
builder.Services.AddMinIO(options =>
{
    options.EndPoint = "localhost:9000";
    options.AccessKey = "minio-access-key";
    options.SecretKey = "minio-secret-key";
});

```
## Best Practices
### 1. Use Cancellation Tokens
```csharp
await _minioService.UploadFile(file, "key", "bucket", cancellationToken);

```
### 2. Validate Bucket and Key
```csharp
The service throws ArgumentNullException if bucketName or key is missing. Always validate these before making calls.

```
### 3. Use Logging for Troubleshooting
```csharp
All exceptions are logged with ILogger<MinIOService>. You can monitor MinIO-related errors through your logging infrastructure.

```
## License
```bash
MIT — Free for personal and commercial use.

