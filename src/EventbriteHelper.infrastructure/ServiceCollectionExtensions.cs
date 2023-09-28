using EventbriteHelper.infrastructure.Api;
using EventbriteHelper.infrastructure.Azure;
using EventbriteHelper.infrastructure.Configuration;
using EventbriteHelper.infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventbriteHelper.infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<EventbriteApiConfiguration>(configuration.GetSection(EventbriteApiConfiguration.ConfigurationMappingName));
        services.Configure<TableStorageConfiguration>(configuration.GetSection(TableStorageConfiguration.ConfigurationMappingName));
        services.AddSingleton<IEventbriteClient, EventbriteClient>();
        services.AddSingleton<IAttendeeService, AttendeeService>();
        services.AddSingleton<ITableStorageClient, TableStorageClient>();

        return services;
    }
}