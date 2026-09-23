using Epocha.Application.Abstractions;
using Epocha.Application.Ingestion;
using Epocha.Infrastructure.Persistence;
using Epocha.Infrastructure.Sources.ArticApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Epocha.Infrastructure;

/// <summary>
/// Clean-architecture convention: each outer layer exposes a single
/// <c>AddXyz(IServiceCollection)</c> extension method that registers everything it owns.
/// Program.cs then reads as a short list of "add API, add infrastructure, add
/// application" instead of knowing the internal details of what Infrastructure needs.
/// This is dependency injection at the composition-root level — nothing here is new
/// DI machinery, just where the registration calls happen to live.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        // AddDbContext registers EpochaDbContext with a Scoped lifetime by default: one
        // instance per HTTP request (or per background job iteration), which is what you
        // want — it lets each request track its own set of changes without stepping on
        // another request's.
        services.AddDbContext<EpochaDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Application code asks for IEpochaDbContext; hand it the same scoped
        // EpochaDbContext instance so both names share one set of tracked changes.
        services.AddScoped<IEpochaDbContext>(sp => sp.GetRequiredService<EpochaDbContext>());

        // AddHttpClient<TInterface, TImplementation> registers the museum client and
        // gives it a pre-configured, connection-pooled HttpClient.
        services.AddHttpClient<IArtworkSource, ArticArtworkSource>(client =>
        {
            client.BaseAddress = new Uri("https://api.artic.edu/api/v1/");
            // Polite to identify yourself to a public API.
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Epocha/1.0 (portfolio project)");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
