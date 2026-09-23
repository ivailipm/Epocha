using Elastic.Clients.Elasticsearch;
using Epocha.Application.Search;
using Microsoft.Extensions.Logging;

namespace Epocha.Infrastructure.Search;

internal sealed class ElasticArtworkSearchIndexer(
    ElasticsearchClient client,
    ILogger<ElasticArtworkSearchIndexer> logger) : IArtworkSearchIndexer
{
    public const string IndexName = "artworks";

    public async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        var exists = await client.Indices.ExistsAsync(IndexName, cancellationToken);
        if (exists.Exists)
        {
            return;
        }

        // Explicit mapping rather than dynamic. "text" fields are analysed for full-text search;
        // "keyword" fields hold exact values for filters, sorting and facets. Title and artist
        // name are both, via a ".keyword" sub-field.
        var response = await client.Indices.CreateAsync<ArtworkSearchDocument>(IndexName, c => c
            .Mappings(m => m
                .Properties(p => p
                    .IntegerNumber(d => d.Id)
                    .Text(d => d.Title, t => t
                        .Analyzer("english")
                        .Fields(f => f.Keyword("keyword", k => k.IgnoreAbove(256))))
                    .Text(d => d.Description, t => t.Analyzer("english"))
                    .Text(d => d.ArtistName, t => t
                        .Fields(f => f.Keyword("keyword", k => k.IgnoreAbove(256))))
                    .Keyword(d => d.Movements)
                    .Keyword(d => d.Era)
                    .Keyword(d => d.MediumCategory)
                    .Text(d => d.MediumDisplay, t => t.Analyzer("english"))
                    .Keyword(d => d.Department)
                    .Keyword(d => d.DateDisplay, k => k.Index(false))
                    .IntegerNumber(d => d.DateStartYear)
                    .IntegerNumber(d => d.DateEndYear)
                    .Keyword(d => d.SourceSystem)
                    // Display-only fields: kept in _source but not indexed.
                    .Keyword(d => d.SourceUrl, k => k.Index(false))
                    .Keyword(d => d.ImageUrl, k => k.Index(false))
                    .Keyword(d => d.ThumbnailUrl, k => k.Index(false))
                    .Boolean(d => d.IsPublicDomain))),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Could not create Elasticsearch index '{IndexName}': {response.DebugInformation}");
        }

        logger.LogInformation("Created Elasticsearch index '{Index}'", IndexName);
    }

    public async Task<IReadOnlyCollection<int>> IndexAsync(
        IReadOnlyCollection<ArtworkSearchDocument> documents,
        CancellationToken cancellationToken)
    {
        if (documents.Count == 0)
        {
            return [];
        }

        // One bulk request per page. The Postgres Id is the document id, so this is an upsert.
        var response = await client.BulkAsync(b => b
            .Index(IndexName)
            .IndexMany(documents, (op, doc) => op.Id(doc.Id)),
            cancellationToken);

        if (!response.IsValidResponse && response.Items.Count == 0)
        {
            throw new InvalidOperationException($"Elasticsearch bulk request failed: {response.DebugInformation}");
        }

        // A bulk request can succeed overall while individual items fail.
        var succeeded = new List<int>(documents.Count);
        foreach (var item in response.Items)
        {
            if (item.Error is null && item.Id is not null)
            {
                succeeded.Add(int.Parse(item.Id));
            }
            else
            {
                logger.LogWarning("Failed to index artwork {Id}: {Reason}", item.Id, item.Error?.Reason);
            }
        }

        return succeeded;
    }
}
