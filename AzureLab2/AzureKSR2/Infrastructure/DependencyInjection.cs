using Core.Application;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = new AzureStorageSettings();
        configuration.GetSection("AzureStorage").Bind(settings);
        services.AddSingleton(settings);

        services.AddSingleton<IFileStorage, BlobFileStorage>();
        services.AddSingleton<IJobQueue, AzureJobQueue>();

        return services;
    }
}