using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Abstractions.Storage;
using Shared.Infrastructure.Configs.Storage;
using Shared.Infrastructure.Storage;

namespace Shared.Infrastructure.Extensions;

public static class StorageServiceExtensions
{
    public static IServiceCollection AddStorageService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureOptions<StorageConfigSetup>();
        services.AddSingleton<IStorageService, MinioStorageService>();

        return services;
    }
}