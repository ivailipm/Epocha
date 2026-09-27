using Elastic.Clients.Elasticsearch;
using Epocha.Application.Abstractions;
using Epocha.Application.Auth;
using Epocha.Application.Images;
using Epocha.Application.Ingestion;
using Epocha.Application.Search;
using Epocha.Infrastructure.Auth;
using Epocha.Infrastructure.Persistence;
using Epocha.Infrastructure.Search;
using Epocha.Infrastructure.Sources.ArticApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Epocha.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        // Scoped by default: one DbContext per request (or per ingestion page).
        services.AddDbContext<EpochaDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Same scoped instance under the interface Application depends on.
        services.AddScoped<IEpochaDbContext>(sp => sp.GetRequiredService<EpochaDbContext>());

        services.AddHttpClient<IArtworkSource, ArticArtworkSource>(client =>
        {
            client.BaseAddress = new Uri("https://api.artic.edu/api/v1/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Epocha/1.0 (portfolio project)");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IExternalImageFetcher, ArticImageFetcher>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Epocha/1.0 (portfolio project)");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        var elasticUrl = configuration["Elasticsearch:Url"]
            ?? throw new InvalidOperationException("Setting 'Elasticsearch:Url' is not configured.");

        // The client is thread-safe and pools connections, so one instance serves the whole app.
        services.AddSingleton(new ElasticsearchClient(new Uri(elasticUrl)));
        services.AddScoped<IArtworkSearchIndexer, ElasticArtworkSearchIndexer>();
        services.AddScoped<IArtworkSearcher, ElasticArtworkSearcher>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
