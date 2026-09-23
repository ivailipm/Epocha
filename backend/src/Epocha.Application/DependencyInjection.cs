using Epocha.Application.Ingestion;
using Microsoft.Extensions.DependencyInjection;

namespace Epocha.Application;

/// <summary>Registers everything the Application layer owns. Mirrors Infrastructure's AddInfrastructure.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped, because it depends on IEpochaDbContext which is itself scoped.
        // A service may not outlive the services it depends on.
        services.AddScoped<ArtworkIngestionService>();
        return services;
    }
}
