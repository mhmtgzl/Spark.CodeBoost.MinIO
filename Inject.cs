using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Spark.CodeBoost.MinIO;
public static class Inject
{
    public static IServiceCollection AddMinIO(this IServiceCollection services, Action<MinIOOptions> configureOptions)
    {

        MinIOOptions options = new MinIOOptions();

        configureOptions(options);

        string endpoint = options?.EndPoint ?? "";
        string accessKey = options?.AccessKey ?? "";
        string secretKey = options?.SecretKey ?? "";

        services.AddMinio(configureClient => configureClient
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false));

        services.AddSingleton<IMinIOService, MinIOService>();

        return services;
    }

}
