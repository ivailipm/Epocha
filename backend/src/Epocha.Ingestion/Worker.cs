using Epocha.Application.Ingestion;
using Epocha.Application.Search;
using Microsoft.Extensions.Options;

namespace Epocha.Ingestion;

/// <summary>
/// Runs one ingestion pass (fetch pages, upsert into Postgres, index into Elasticsearch,
/// then index any backlog) and shuts the process down.
/// </summary>
public class Worker(
    IServiceScopeFactory scopeFactory,
    IOptions<IngestionOptions> options,
    IHostApplicationLifetime lifetime,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        var totalCreated = 0;
        var totalMatched = 0;
        var totalIndexed = 0;

        try
        {
            using (var setupScope = scopeFactory.CreateScope())
            {
                var indexer = setupScope.ServiceProvider.GetRequiredService<IArtworkSearchIndexer>();
                await indexer.EnsureIndexAsync(stoppingToken);
            }

            for (var page = 1; page <= settings.MaxPages; page++)
            {
                // The worker lives for the whole process but DbContext must be short-lived, so
                // each page gets its own scope (and therefore its own DbContext).
                using var scope = scopeFactory.CreateScope();
                var source = scope.ServiceProvider.GetRequiredService<IArtworkSource>();
                var service = scope.ServiceProvider.GetRequiredService<ArtworkIngestionService>();

                logger.LogInformation("Fetching page {Page}/{MaxPages}", page, settings.MaxPages);
                var artworkPage = await source.GetPageAsync(page, settings.PageSize, stoppingToken);

                var result = await service.UpsertAsync(artworkPage.Records, stoppingToken);
                totalCreated += result.Created;
                totalMatched += result.Matched;
                totalIndexed += result.Indexed;

                if (!artworkPage.HasMore)
                {
                    break;
                }

                await Task.Delay(settings.DelayBetweenPagesMs, stoppingToken);
            }

            using (var backlogScope = scopeFactory.CreateScope())
            {
                var service = backlogScope.ServiceProvider.GetRequiredService<ArtworkIngestionService>();
                var backlog = await service.IndexBacklogAsync(settings.PageSize, stoppingToken);
                totalIndexed += backlog;
                logger.LogInformation("Backlog pass indexed {Count} previously un-indexed artworks", backlog);
            }

            logger.LogInformation(
                "Ingestion finished: {Created} created, {Matched} already existed, {Indexed} indexed",
                totalCreated, totalMatched, totalIndexed);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Ingestion cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ingestion failed");
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
