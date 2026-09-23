namespace Epocha.Ingestion;

/// <summary>Bound from the "Ingestion" section of appsettings.json.</summary>
public class IngestionOptions
{
    public const string SectionName = "Ingestion";

    /// <summary>Artworks per request. The Art Institute API allows at most 100.</summary>
    public int PageSize { get; set; } = 100;

    /// <summary>Upper bound on pages per run, so a dev run doesn't pull the whole collection.</summary>
    public int MaxPages { get; set; } = 5;

    /// <summary>Pause between requests, to be a polite API client.</summary>
    public int DelayBetweenPagesMs { get; set; } = 500;
}
