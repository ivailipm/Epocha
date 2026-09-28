using Epocha.Application.Ingestion;
using Epocha.Application.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Epocha.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var imageCacheDirectory = configuration["ImageCache:Directory"] ?? "/data/images";

        services.AddScoped<ArtworkIngestionService>();
        services.AddScoped<ArtworkSearchService>();
        services.AddScoped<Details.ArtworkDetailService>();
        services.AddScoped(sp => new Images.ArtworkImageService(
            sp.GetRequiredService<Abstractions.IEpochaDbContext>(),
            sp.GetRequiredService<Images.IExternalImageFetcher>(),
            imageCacheDirectory));
        services.AddScoped<Auth.AuthService>();
        services.AddScoped<Collections.CollectionService>();
        return services;
    }
}
