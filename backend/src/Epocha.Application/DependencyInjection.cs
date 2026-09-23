using Epocha.Application.Ingestion;
using Microsoft.Extensions.DependencyInjection;

namespace Epocha.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ArtworkIngestionService>();
        return services;
    }
}
