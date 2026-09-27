using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Epocha.Application.Search;

namespace Epocha.Infrastructure.Search;

/// <summary>
/// Builds the search request as JSON and sends it through the client's transport. The query is
/// a nested bool/aggregation structure that reads more clearly as the Elasticsearch DSL itself
/// than through the client's fluent descriptors.
/// </summary>
internal sealed class ElasticArtworkSearcher(ElasticsearchClient client) : IArtworkSearcher
{
    private const string EraField = "era";
    private const string MediumField = "mediumCategory";
    private const string MovementField = "movements";
    private const string ArtistField = "artistName.keyword";

    private const int FacetSize = 30;
    // Artist names run into the hundreds, unlike the other facets, and the frontend needs the
    // full list client-side to power its searchable filter, not just the top few by count.
    private const int ArtistFacetSize = 2000;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] SourceFields =
        ["id", "title", "artistName", "dateDisplay", "dateStartYear", "era", "mediumCategory", "thumbnailUrl", "isPublicDomain"];

    public async Task<ArtworkSearchResult> SearchAsync(ArtworkSearchQuery q, CancellationToken cancellationToken)
    {
        var era = TermsFilter(EraField, q.Eras);
        var medium = TermsFilter(MediumField, q.Mediums);
        var movement = TermsFilter(MovementField, q.Movements);
        var artist = TermsFilter(ArtistField, q.Artists);
        var years = YearRange(q);

        // The text query decides which documents match. The filters go in post_filter so each
        // facet aggregation can be computed with its OWN filter left out: ticking "Baroque"
        // still shows counts for the other eras, which a multi-select filter UI needs.
        var body = new JsonObject
        {
            ["from"] = (q.Page - 1) * q.PageSize,
            ["size"] = q.PageSize,
            ["track_total_hits"] = true,
            ["_source"] = new JsonArray(SourceFields.Select(f => (JsonNode?)JsonValue.Create(f)).ToArray()),
            ["query"] = TextQuery(q.Text),
            ["post_filter"] = Combine(era, medium, movement, artist, years),
            ["sort"] = SortFor(q),
            ["aggs"] = new JsonObject
            {
                ["era"] = Facet(EraField, Combine(medium, movement, artist, years)),
                ["medium"] = Facet(MediumField, Combine(era, movement, artist, years)),
                ["movement"] = Facet(MovementField, Combine(era, medium, artist, years)),
                ["artist"] = Facet(ArtistField, Combine(era, medium, movement, years), ArtistFacetSize),
            }
        };

        var response = await client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.POST,
            $"/{ElasticArtworkSearchIndexer.IndexName}/_search",
            PostData.String(body.ToJsonString()),
            cancellationToken: cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw new InvalidOperationException($"Elasticsearch search failed: {response.ApiCallDetails.DebugInformation}");
        }

        using var doc = JsonDocument.Parse(response.Body);
        var root = doc.RootElement;

        var hits = root.GetProperty("hits");
        var total = hits.GetProperty("total").GetProperty("value").GetInt64();

        var items = hits.GetProperty("hits").EnumerateArray()
            .Select(h => h.GetProperty("_source").Deserialize<ArtworkSummary>(JsonOptions)!)
            .ToList();

        var aggs = root.GetProperty("aggregations");
        var facets = new Dictionary<string, IReadOnlyList<FacetBucket>>
        {
            ["era"] = Buckets(aggs, "era"),
            ["medium"] = Buckets(aggs, "medium"),
            ["movement"] = Buckets(aggs, "movement"),
            ["artist"] = Buckets(aggs, "artist"),
        };

        return new ArtworkSearchResult(items, total, q.Page, q.PageSize, facets);
    }

    private static JsonNode TextQuery(string? text) =>
        text is null
            ? new JsonObject { ["match_all"] = new JsonObject() }
            : new JsonObject
            {
                ["multi_match"] = new JsonObject
                {
                    ["query"] = text,
                    ["fields"] = new JsonArray("title^3", "artistName^2", "movements^2", "description", "mediumDisplay"),
                    ["fuzziness"] = "AUTO"
                }
            };

    private static JsonNode? TermsFilter(string field, IReadOnlyList<string> values) =>
        values.Count == 0
            ? null
            : new JsonObject
            {
                ["terms"] = new JsonObject
                {
                    [field] = new JsonArray(values.Select(v => (JsonNode?)JsonValue.Create(v)).ToArray())
                }
            };

    // Filters on the start year, the same value Era and date sorting use. Matching on the full
    // date span instead would let a dynasty-long object (1644-1911) match almost any range.
    // Undated artworks are excluded whenever a range is given.
    private static JsonNode? YearRange(ArtworkSearchQuery q)
    {
        if (q.YearFrom is null && q.YearTo is null)
        {
            return null;
        }

        var bounds = new JsonObject();
        if (q.YearFrom is { } from)
        {
            bounds["gte"] = from;
        }

        if (q.YearTo is { } to)
        {
            bounds["lte"] = to;
        }

        return new JsonObject { ["range"] = new JsonObject { ["dateStartYear"] = bounds } };
    }

    // A JsonNode can only have one parent, so shared filters are cloned into each combination.
    private static JsonNode Combine(params JsonNode?[] filters)
    {
        var present = filters.Where(f => f is not null).Select(f => f!.DeepClone()).ToArray();

        return present.Length == 0
            ? new JsonObject { ["match_all"] = new JsonObject() }
            : new JsonObject { ["bool"] = new JsonObject { ["filter"] = new JsonArray(present) } };
    }

    private static JsonObject Facet(string field, JsonNode otherFilters, int size = FacetSize) =>
        new()
        {
            ["filter"] = otherFilters,
            ["aggs"] = new JsonObject
            {
                ["values"] = new JsonObject { ["terms"] = new JsonObject { ["field"] = field, ["size"] = size } }
            }
        };

    private static JsonArray SortFor(ArtworkSearchQuery q)
    {
        JsonObject primary = q.Sort switch
        {
            ArtworkSort.DateAsc => SortBy("dateStartYear", "asc", missingLast: true),
            ArtworkSort.DateDesc => SortBy("dateStartYear", "desc", missingLast: true),
            ArtworkSort.TitleAsc => SortBy("title.keyword", "asc"),
            ArtworkSort.TitleDesc => SortBy("title.keyword", "desc"),
            _ => SortBy("_score", "desc")
        };

        // Tie-breaker so paging is stable when the primary sort values are equal.
        return new JsonArray(primary, SortBy("id", "asc"));
    }

    private static JsonObject SortBy(string field, string order, bool missingLast = false)
    {
        var options = new JsonObject { ["order"] = order };
        if (missingLast)
        {
            options["missing"] = "_last";
        }

        return new JsonObject { [field] = options };
    }

    private static IReadOnlyList<FacetBucket> Buckets(JsonElement aggregations, string name) =>
        aggregations.GetProperty(name).GetProperty("values").GetProperty("buckets").EnumerateArray()
            .Select(b => new FacetBucket(b.GetProperty("key").GetString() ?? string.Empty, b.GetProperty("doc_count").GetInt64()))
            .ToList();
}
