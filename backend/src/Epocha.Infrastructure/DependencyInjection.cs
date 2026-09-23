using Epocha.Infrastructure.Persistence;
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

        return services;
    }
}
