using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storage.AWS3.Services;

namespace Storage.AWS3;
public static class ConfigureServices
{
    public static IServiceCollection AddAmazonS3(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IStorageService, StorageService>();

        return services;
    }
}
