namespace Epocha.Ingestion;

/// <summary>
/// Strongly-typed view of the "Ingestion" section of appsettings.json. This is the
/// .NET "options pattern": instead of reading config strings by key all over the
/// code, you bind a section to a class once and inject it wherever it's needed.
/// </summary>
public class IngestionOptions
{
    public const string SectionName = "Ingestion";

    /// <summary>Artworks per request. The Art Institute API allows at most 100.</summary>
    public int PageSize { get; set; } = 100;

    /// <summary>Upper bound on pages per run, so a dev run doesn't pull all ~130k artworks.</summary>
    public int MaxPages { get; set; } = 5;

    /// <summary>Pause between requests, to be a polite API client.</summary>
    public int DelayBetweenPagesMs { get; set; } = 500;
}
