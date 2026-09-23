using Epocha.Application.Ingestion;
using Microsoft.Extensions.Options;

namespace Epocha.Ingestion;

/// <summary>
/// The ingestion job. It is a <see cref="BackgroundService"/>: the .NET generic host
/// starts it on launch and calls <see cref="ExecuteAsync"/>. For now it runs ONE pass
/// (fetch pages -> upsert into Postgres) and then shuts the process down, which suits
/// a job you run on demand or on a schedule. Elasticsearch indexing comes next.
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

        try
        {
            for (var page = 1; page <= settings.MaxPages; page++)
            {
                // A hosted service lives for the whole process (it's effectively a
                // singleton), but the DbContext is scoped and must be short-lived: a
                // long-lived DbContext keeps every entity it has ever loaded in
                // memory. So we open a fresh DI scope PER PAGE, which gives us a
                // fresh DbContext, and it is disposed as soon as the page is saved.
                using var scope = scopeFactory.CreateScope();
                var source = scope.ServiceProvider.GetRequiredService<IArtworkSource>();
                var service = scope.ServiceProvider.GetRequiredService<ArtworkIngestionService>();

                logger.LogInformation("Fetching page {Page}/{MaxPages}", page, settings.MaxPages);
                var artworkPage = await source.GetPageAsync(page, settings.PageSize, stoppingToken);

                var result = await service.UpsertAsync(artworkPage.Records, stoppingToken);
                totalCreated += result.Created;
                totalMatched += result.Matched;

                if (!artworkPage.HasMore)
                {
                    break;
                }

                await Task.Delay(settings.DelayBetweenPagesMs, stoppingToken);
            }

            logger.LogInformation("Ingestion finished: {Created} created, {Matched} already existed", totalCreated, totalMatched);
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
            // Without this the host would keep running forever after the work is done.
            lifetime.StopApplication();
        }
    }
}
