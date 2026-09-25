using Epocha.Application.Ingestion;
using Epocha.Application.Search;
using Microsoft.Extensions.DependencyInjection;

namespace Epocha.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ArtworkIngestionService>();
        services.AddScoped<ArtworkSearchService>();
        services.AddScoped<Details.ArtworkDetailService>();
        services.AddScoped<Images.ArtworkImageService>();
        return services;
    }
}
